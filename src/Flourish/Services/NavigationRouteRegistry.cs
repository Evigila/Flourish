using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class NavigationRouteRegistry : INavigationRouteRegistry
{
    private readonly object gate = new();
    private readonly FlourishShellOptions options;
    private readonly IServiceProvider? serviceProvider;
    private readonly Dictionary<string, FlourishNavigationRoute> routes = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<string, Guid> leases = new(StringComparer.Ordinal);
    private long version;

    public NavigationRouteRegistry(IServiceProvider serviceProvider, FlourishShellOptions options)
        : this(options)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    internal NavigationRouteRegistry(FlourishShellOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        foreach (var pair in options.PageTypesByNavigationKey)
        {
            routes[pair.Key] = new FlourishNavigationRoute(
                pair.Key,
                pair.Value,
                options.PageCacheModesByPageType.GetValueOrDefault(
                    pair.Value,
                    FlourishPageCacheMode.Disabled
                )
            );
        }
    }

    public event EventHandler<FlourishNavigationRoutesChangedEventArgs>? Changed;

    public FlourishNavigationRouteSnapshot Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public INavigationRouteRegistration Register(FlourishNavigationRoute route)
    {
        ValidateRoute(route);
        var lease = Guid.NewGuid();
        FlourishNavigationRouteSnapshot snapshot;
        lock (gate)
        {
            if (routes.ContainsKey(route.NavigationKey))
            {
                throw new InvalidOperationException(
                    $"Navigation key '{route.NavigationKey}' is already registered."
                );
            }

            EnsurePageTypeAvailable(route.PageType, exceptNavigationKey: null);
            routes.Add(route.NavigationKey, route);
            leases[route.NavigationKey] = lease;
            SynchronizeOptions();
            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishNavigationRoutesChangedEventArgs(
                snapshot,
                FlourishRuntimeChangeKind.Added,
                previousRoute: null,
                route
            )
        );
        return new Registration(this, route.NavigationKey, lease);
    }

    public INavigationRouteRegistration Upsert(FlourishNavigationRoute route)
    {
        ValidateRoute(route);
        var lease = Guid.NewGuid();
        FlourishNavigationRoute? previous;
        FlourishNavigationRouteSnapshot snapshot;
        lock (gate)
        {
            routes.TryGetValue(route.NavigationKey, out previous);
            EnsurePageTypeAvailable(route.PageType, route.NavigationKey);
            routes[route.NavigationKey] = route;
            leases[route.NavigationKey] = lease;
            SynchronizeOptions();
            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishNavigationRoutesChangedEventArgs(
                snapshot,
                previous is null
                    ? FlourishRuntimeChangeKind.Added
                    : FlourishRuntimeChangeKind.Updated,
                previous,
                route
            )
        );
        return new Registration(this, route.NavigationKey, lease);
    }

    public bool Remove(string navigationKey)
    {
        navigationKey = ValidateKey(navigationKey, nameof(navigationKey));
        return RemoveCore(navigationKey, lease: null);
    }

    public void SetCacheMode(string navigationKey, FlourishPageCacheMode cacheMode)
    {
        navigationKey = ValidateKey(navigationKey, nameof(navigationKey));
        ValidateCacheMode(cacheMode);
        FlourishNavigationRoute previous;
        FlourishNavigationRoute current;
        FlourishNavigationRouteSnapshot snapshot;
        lock (gate)
        {
            if (!routes.TryGetValue(navigationKey, out previous!))
            {
                throw new KeyNotFoundException(
                    $"Navigation key '{navigationKey}' is not registered."
                );
            }

            if (previous.CacheMode == cacheMode)
            {
                return;
            }

            current = previous with { CacheMode = cacheMode };
            routes[navigationKey] = current;
            SynchronizeOptions();
            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishNavigationRoutesChangedEventArgs(
                snapshot,
                FlourishRuntimeChangeKind.Updated,
                previous,
                current
            )
        );
    }

    public bool Contains(string navigationKey)
    {
        navigationKey = ValidateKey(navigationKey, nameof(navigationKey));
        lock (gate)
        {
            return routes.ContainsKey(navigationKey);
        }
    }

    internal bool TryGet(string navigationKey, out FlourishNavigationRoute route)
    {
        lock (gate)
        {
            return routes.TryGetValue(navigationKey, out route!);
        }
    }

    internal bool TryGet(Type pageType, out FlourishNavigationRoute route)
    {
        lock (gate)
        {
            route = routes.Values.FirstOrDefault(value => value.PageType == pageType)!;
            return route is not null;
        }
    }

    internal Page CreatePage(Type pageType, IPageFactory fallbackFactory)
    {
        FlourishNavigationRoute? route;
        lock (gate)
        {
            route = routes.Values.FirstOrDefault(value => value.PageType == pageType);
        }

        if (route?.PageFactory is not null)
        {
            return route.PageFactory(serviceProvider
                ?? throw new InvalidOperationException(
                    "The runtime route factory service provider is unavailable."
                ));
        }

        return fallbackFactory.Create(pageType) as Page
            ?? throw new InvalidOperationException(
                $"{pageType.FullName} must derive from System.Windows.Controls.Page."
            );
    }

    private bool RemoveCore(string navigationKey, Guid? lease)
    {
        FlourishNavigationRoute? removed = null;
        FlourishNavigationRouteSnapshot? snapshot = null;
        lock (gate)
        {
            if (
                lease is not null
                && (!leases.TryGetValue(navigationKey, out var currentLease) || currentLease != lease)
            )
            {
                return false;
            }

            if (!routes.Remove(navigationKey, out removed))
            {
                return false;
            }

            leases.Remove(navigationKey);
            SynchronizeOptions();
            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishNavigationRoutesChangedEventArgs(
                snapshot,
                FlourishRuntimeChangeKind.Removed,
                removed,
                route: null
            )
        );
        return true;
    }

    private void EnsurePageTypeAvailable(Type pageType, string? exceptNavigationKey)
    {
        var existing = routes.Values.FirstOrDefault(route =>
            route.PageType == pageType
            && !StringComparer.Ordinal.Equals(route.NavigationKey, exceptNavigationKey)
        );
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Page type '{pageType.FullName}' is already registered as route '{existing.NavigationKey}'."
            );
        }
    }

    private void SynchronizeOptions()
    {
        options.PageTypesByNavigationKey.Clear();
        options.NavigationKeysByPageType.Clear();
        options.PageCacheModesByPageType.Clear();
        foreach (var route in routes.Values)
        {
            options.PageTypesByNavigationKey[route.NavigationKey] = route.PageType;
            options.NavigationKeysByPageType[route.PageType] = route.NavigationKey;
            options.PageCacheModesByPageType[route.PageType] = route.CacheMode;
        }
    }

    private FlourishNavigationRouteSnapshot CreateSnapshot()
    {
        return new FlourishNavigationRouteSnapshot(
            new Dictionary<string, FlourishNavigationRoute>(routes, StringComparer.Ordinal),
            version
        );
    }

    private static void ValidateRoute(FlourishNavigationRoute route)
    {
        ArgumentNullException.ThrowIfNull(route);
        ValidateKey(route.NavigationKey, nameof(route.NavigationKey));
        ArgumentNullException.ThrowIfNull(route.PageType);
        if (!typeof(Page).IsAssignableFrom(route.PageType))
        {
            throw new ArgumentException(
                $"{route.PageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(route)
            );
        }

        ValidateCacheMode(route.CacheMode);
    }

    private static string ValidateKey(string navigationKey, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(navigationKey))
        {
            throw new ArgumentException("A navigation key is required.", parameterName);
        }

        return navigationKey;
    }

    private static void ValidateCacheMode(FlourishPageCacheMode cacheMode)
    {
        if (!Enum.IsDefined(cacheMode))
        {
            throw new ArgumentOutOfRangeException(nameof(cacheMode), cacheMode, "Unknown cache mode.");
        }
    }

    private sealed class Registration(
        NavigationRouteRegistry owner,
        string navigationKey,
        Guid lease
    ) : INavigationRouteRegistration
    {
        private NavigationRouteRegistry? owner = owner;

        public string NavigationKey { get; } = navigationKey;

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.RemoveCore(NavigationKey, lease);
        }
    }

}
