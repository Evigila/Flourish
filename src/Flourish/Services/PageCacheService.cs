using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed class PageCacheService : INavigationPageProvider, IPageCacheService
{
    private readonly object gate = new();
    private readonly IPageFactory pageFactory;
    private readonly NavigationRouteRegistry routeRegistry;
    private readonly Dictionary<Type, Page> cachedPages = [];
    private readonly Dictionary<Type, FlourishPageCacheMode> cacheModesByPageType = [];
    private readonly Dictionary<Type, FlourishNavigationRoute> routesByPageType = [];
    private long lastAppliedRouteVersion = -1;
    private long version;

    public PageCacheService(
        IServiceProvider serviceProvider,
        NavigationRouteRegistry routeRegistry
    ) : this(new ServiceProviderPageFactory(serviceProvider), routeRegistry) { }

    internal PageCacheService(
        IPageFactory pageFactory,
        NavigationRouteRegistry routeRegistry
    )
    {
        this.pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
        this.routeRegistry =
            routeRegistry ?? throw new ArgumentNullException(nameof(routeRegistry));
        routeRegistry.Changed += RouteRegistry_Changed;
        lock (gate)
        {
            ApplyRouteSnapshotLocked(routeRegistry.Current, incrementVersion: false);
        }
    }

    public event EventHandler<FlourishPageCacheChangedEventArgs>? Changed;

    public FlourishPageCacheSnapshot Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public Page GetPage(Type sourcePageType)
    {
        ArgumentNullException.ThrowIfNull(sourcePageType);
        FlourishPageCacheSnapshot? snapshot = null;
        Page? page;
        lock (gate)
        {
            if (
                cacheModesByPageType.TryGetValue(sourcePageType, out var cacheMode)
                && cacheMode == FlourishPageCacheMode.Enabled
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
                && currentCacheMode == FlourishPageCacheMode.Enabled
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
                    snapshot = CreateSnapshot();
                }
            }
        }

        if (snapshot is not null)
        {
            Changed?.Invoke(
                this,
                new FlourishPageCacheChangedEventArgs(
                    snapshot,
                    FlourishRuntimeChangeKind.Added,
                    sourcePageType
                )
            );
        }

        return page;
    }

    public void SetCacheMode(Type pageType, FlourishPageCacheMode cacheMode)
    {
        ValidatePageType(pageType);
        ValidateCacheMode(cacheMode);
        if (routeRegistry.TryGet(pageType, out var route))
        {
            routeRegistry.SetCacheMode(route.NavigationKey, cacheMode);
            return;
        }

        FlourishPageCacheSnapshot snapshot;
        lock (gate)
        {
            if (
                cacheModesByPageType.GetValueOrDefault(
                    pageType,
                    FlourishPageCacheMode.Disabled
                ) == cacheMode
            )
            {
                return;
            }

            cacheModesByPageType[pageType] = cacheMode;
            if (cacheMode == FlourishPageCacheMode.Disabled)
            {
                cachedPages.Remove(pageType);
            }

            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishPageCacheChangedEventArgs(
                snapshot,
                FlourishRuntimeChangeKind.Updated,
                pageType
            )
        );
    }

    public bool Evict(Type pageType)
    {
        ValidatePageType(pageType);
        FlourishPageCacheSnapshot? snapshot = null;
        lock (gate)
        {
            if (!cachedPages.Remove(pageType))
            {
                return false;
            }

            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishPageCacheChangedEventArgs(
                snapshot,
                FlourishRuntimeChangeKind.Removed,
                pageType
            )
        );
        return true;
    }

    public void Clear()
    {
        FlourishPageCacheSnapshot snapshot;
        lock (gate)
        {
            if (cachedPages.Count == 0)
            {
                return;
            }

            cachedPages.Clear();
            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishPageCacheChangedEventArgs(
                snapshot,
                FlourishRuntimeChangeKind.Reset,
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

    private void RouteRegistry_Changed(
        object? sender,
        FlourishNavigationRoutesChangedEventArgs e
    )
    {
        FlourishPageCacheSnapshot? snapshot;
        lock (gate)
        {
            if (!ApplyRouteSnapshotLocked(e.Current, incrementVersion: true))
            {
                return;
            }

            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishPageCacheChangedEventArgs(
                snapshot,
                e.ChangeKind,
                e.Route?.PageType ?? e.PreviousRoute?.PageType
            )
        );
    }

    private bool ApplyRouteSnapshotLocked(
        FlourishNavigationRouteSnapshot routeSnapshot,
        bool incrementVersion
    )
    {
        if (routeSnapshot.Version <= lastAppliedRouteVersion)
        {
            return false;
        }

        var currentRoutesByPageType = routeSnapshot
            .Routes.Values.ToDictionary(route => route.PageType);
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
            if (routeChanged || route.CacheMode == FlourishPageCacheMode.Disabled)
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

    private FlourishPageCacheSnapshot CreateSnapshot()
    {
        return new FlourishPageCacheSnapshot(
            new Dictionary<Type, FlourishPageCacheMode>(cacheModesByPageType),
            cachedPages.Keys.ToArray(),
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

    private static void ValidateCacheMode(FlourishPageCacheMode cacheMode)
    {
        if (!Enum.IsDefined(cacheMode))
        {
            throw new ArgumentOutOfRangeException(nameof(cacheMode), cacheMode, "Unknown cache mode.");
        }
    }
}
