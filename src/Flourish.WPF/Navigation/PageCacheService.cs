using System.Linq;

using System;
using System.Collections.Generic;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class PageCacheService : INavigationPageProvider
{
    private readonly Lock gate = new();
    private readonly IPageFactory pageFactory;
    private readonly NavigationRouteRegistry routeRegistry;
    private readonly Dictionary<Type, Page> cachedPages = [];
    private readonly Dictionary<Type, PageCacheMode> cacheModesByPageType = [];
    private readonly Dictionary<Type, NavigationRoute> routesByPageType = [];
    private PageCacheSnapshot current = null!;
    private long lastAppliedRouteVersion = -1;
    private long version;

    public PageCacheService(IServiceProvider serviceProvider, NavigationRouteRegistry routeRegistry)
        : this(new ServiceProviderPageFactory(serviceProvider), routeRegistry) { }

    internal PageCacheService(IPageFactory pageFactory, NavigationRouteRegistry routeRegistry)
    {
        this.pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
        this.routeRegistry =
            routeRegistry ?? throw new ArgumentNullException(nameof(routeRegistry));
        routeRegistry.Changed += RouteRegistry_Changed;
        lock (gate)
        {
            ApplyRouteSnapshotLocked(routeRegistry.Current, incrementVersion: false);
            current = CreateSnapshot();
        }
    }

    public event EventHandler<PageCacheChangedEventArgs>? Changed;

    public PageCacheSnapshot Current => Volatile.Read(ref current);

    public Page GetPage(Type sourcePageType)
    {
        ArgumentNullException.ThrowIfNull(sourcePageType);
        PageCacheSnapshot? snapshot = null;
        Page? page;
        lock (gate)
        {
            if (
                cacheModesByPageType.TryGetValue(sourcePageType, out var cacheMode)
                && cacheMode == PageCacheMode.Enabled
                && cachedPages.TryGetValue(sourcePageType, out page)
            )
            {
                return page;
            }
        }

        page = CreatePage(sourcePageType, out var creationRouteVersion);
        lock (gate)
        {
            if (
                cacheModesByPageType.TryGetValue(sourcePageType, out var currentCacheMode)
                && currentCacheMode == PageCacheMode.Enabled
                && creationRouteVersion == lastAppliedRouteVersion
            )
            {
                if (cachedPages.TryGetValue(sourcePageType, out var existingPage))
                {
                    page = existingPage;
                }
                else
                {
                    cachedPages[sourcePageType] = page;
                    version++;
                    snapshot = PublishSnapshot();
                }
            }
        }

        if (snapshot is not null)
        {
            Changed?.Invoke(
                this,
                new PageCacheChangedEventArgs(
                    snapshot,
                    CollectionChangeKind.Added,
                    sourcePageType
                )
            );
        }

        return page;
    }

    public void SetCacheMode(Type pageType, PageCacheMode cacheMode)
    {
        ValidatePageType(pageType);
        ValidateCacheMode(cacheMode);
        if (routeRegistry.TryGet(pageType, out var route))
        {
            routeRegistry.SetCacheMode(route.NavigationKey, cacheMode);
            return;
        }

        PageCacheSnapshot snapshot;
        lock (gate)
        {
            if (
                cacheModesByPageType.GetValueOrDefault(pageType, PageCacheMode.Disabled)
                == cacheMode
            )
            {
                return;
            }

            cacheModesByPageType[pageType] = cacheMode;
            if (cacheMode == PageCacheMode.Disabled)
            {
                cachedPages.Remove(pageType);
            }

            version++;
            snapshot = PublishSnapshot();
        }

        Changed?.Invoke(
            this,
            new PageCacheChangedEventArgs(
                snapshot,
                CollectionChangeKind.Updated,
                pageType
            )
        );
    }

    public bool Evict(Type pageType)
    {
        ValidatePageType(pageType);
        PageCacheSnapshot? snapshot = null;
        lock (gate)
        {
            if (!cachedPages.Remove(pageType))
            {
                return false;
            }

            version++;
            snapshot = PublishSnapshot();
        }

        Changed?.Invoke(
            this,
            new PageCacheChangedEventArgs(
                snapshot,
                CollectionChangeKind.Removed,
                pageType
            )
        );
        return true;
    }

    public void Clear()
    {
        PageCacheSnapshot snapshot;
        lock (gate)
        {
            if (cachedPages.Count == 0)
            {
                return;
            }

            cachedPages.Clear();
            version++;
            snapshot = PublishSnapshot();
        }

        Changed?.Invoke(
            this,
            new PageCacheChangedEventArgs(
                snapshot,
                CollectionChangeKind.Reset,
                pageType: null
            )
        );
    }

    public bool Contains(Type pageType)
    {
        ValidatePageType(pageType);
        lock (gate)
        {
            return cachedPages.ContainsKey(pageType);
        }
    }

    private Page CreatePage(Type sourcePageType, out long routeVersion)
    {
        return routeRegistry.CreatePage(sourcePageType, pageFactory, out routeVersion);
    }

    private void RouteRegistry_Changed(object? sender, NavigationRoutesChangedEventArgs e)
    {
        PageCacheSnapshot? snapshot;
        lock (gate)
        {
            if (!ApplyRouteSnapshotLocked(e.Current, incrementVersion: true))
            {
                return;
            }

            snapshot = PublishSnapshot();
        }

        Changed?.Invoke(
            this,
            new PageCacheChangedEventArgs(
                snapshot,
                e.ChangeKind,
                e.Route?.PageType ?? e.PreviousRoute?.PageType
            )
        );
    }

    private bool ApplyRouteSnapshotLocked(
        NavigationRouteSnapshot routeSnapshot,
        bool incrementVersion
    )
    {
        if (routeSnapshot.Version <= lastAppliedRouteVersion)
        {
            return false;
        }

        var currentRoutesByPageType = routeSnapshot.Routes.Values.ToDictionary(route =>
            route.PageType
        );
        foreach (var previousPageType in routesByPageType.Keys.Except(currentRoutesByPageType.Keys))
        {
            cacheModesByPageType.Remove(previousPageType);
            cachedPages.Remove(previousPageType);
        }

        foreach (var pair in currentRoutesByPageType)
        {
            var pageType = pair.Key;
            var route = pair.Value;
            var routeChanged =
                !routesByPageType.TryGetValue(pageType, out var previousRoute)
                || previousRoute != route;
            cacheModesByPageType[pageType] = route.CacheMode;
            if (routeChanged || route.CacheMode == PageCacheMode.Disabled)
            {
                cachedPages.Remove(pageType);
            }
        }

        routesByPageType.Clear();
        foreach (var pair in currentRoutesByPageType)
        {
            routesByPageType.Add(pair.Key, pair.Value);
        }

        lastAppliedRouteVersion = routeSnapshot.Version;
        if (incrementVersion)
        {
            version++;
        }

        return true;
    }

    private PageCacheSnapshot PublishSnapshot()
    {
        var snapshot = CreateSnapshot();
        Volatile.Write(ref current, snapshot);
        return snapshot;
    }

    private PageCacheSnapshot CreateSnapshot()
    {
        return new PageCacheSnapshot(
            new ReadOnlyDictionary<Type, PageCacheMode>(
                new Dictionary<Type, PageCacheMode>(cacheModesByPageType)
            ),
            Array.AsReadOnly(cachedPages.Keys.ToArray()),
            version
        );
    }

    private static void ValidatePageType(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException(
                $"{pageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(pageType)
            );
        }
    }

    private static void ValidateCacheMode(PageCacheMode cacheMode)
    {
        if (!Enum.IsDefined(cacheMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(cacheMode),
                cacheMode,
                "Unknown cache mode."
            );
        }
    }
}
