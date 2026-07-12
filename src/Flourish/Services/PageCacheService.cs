using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class PageCacheService : INavigationPageProvider, IPageCacheService
{
    private readonly object gate = new();
    private readonly IPageFactory pageFactory;
    private readonly NavigationRouteRegistry? routeRegistry;
    private readonly FlourishShellOptions options;
    private readonly Dictionary<Type, Page> cachedPages = [];
    private readonly Dictionary<Type, FlourishPageCacheMode> cacheModesByPageType;
    private long version;

    public PageCacheService(
        IServiceProvider serviceProvider,
        FlourishShellOptions options,
        NavigationRouteRegistry routeRegistry
    ) : this(new ServiceProviderPageFactory(serviceProvider), options, routeRegistry) { }

    internal PageCacheService(IPageFactory pageFactory, FlourishShellOptions options)
        : this(pageFactory, options, routeRegistry: null) { }

    private PageCacheService(
        IPageFactory pageFactory,
        FlourishShellOptions options,
        NavigationRouteRegistry? routeRegistry
    )
    {
        this.pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.routeRegistry = routeRegistry;
        cacheModesByPageType = new Dictionary<Type, FlourishPageCacheMode>(
            options.PageCacheModesByPageType
        );
        if (routeRegistry is not null)
        {
            routeRegistry.Changed += RouteRegistry_Changed;
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
        Page page;
        lock (gate)
        {
            if (
                cacheModesByPageType.TryGetValue(sourcePageType, out var cacheMode)
                && cacheMode == FlourishPageCacheMode.Enabled
            )
            {
                if (!cachedPages.TryGetValue(sourcePageType, out page!))
                {
                    page = CreatePage(sourcePageType);
                    cachedPages[sourcePageType] = page;
                    version++;
                    snapshot = CreateSnapshot();
                }
            }
            else
            {
                page = CreatePage(sourcePageType);
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
        if (routeRegistry is not null && routeRegistry.TryGet(pageType, out var route))
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
            options.PageCacheModesByPageType[pageType] = cacheMode;
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

    private Page CreatePage(Type sourcePageType)
    {
        if (routeRegistry is not null)
        {
            return routeRegistry.CreatePage(sourcePageType, pageFactory);
        }

        return pageFactory.Create(sourcePageType) as Page
            ?? throw new InvalidOperationException(
                $"{sourcePageType.FullName} must derive from System.Windows.Controls.Page."
            );
    }

    private void RouteRegistry_Changed(
        object? sender,
        FlourishNavigationRoutesChangedEventArgs e
    )
    {
        Type? affectedPageType = e.Route?.PageType ?? e.PreviousRoute?.PageType;
        FlourishPageCacheSnapshot snapshot;
        lock (gate)
        {
            if (
                e.PreviousRoute is { } previous
                && (e.Route is null || e.Route.PageType != previous.PageType)
            )
            {
                cacheModesByPageType.Remove(previous.PageType);
                cachedPages.Remove(previous.PageType);
            }

            if (e.Route is { } current)
            {
                cacheModesByPageType[current.PageType] = current.CacheMode;
                if (
                    current.CacheMode == FlourishPageCacheMode.Disabled
                    || e.ChangeKind == FlourishRuntimeChangeKind.Updated
                )
                {
                    cachedPages.Remove(current.PageType);
                }
            }

            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishPageCacheChangedEventArgs(
                snapshot,
                e.ChangeKind,
                affectedPageType
            )
        );
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
