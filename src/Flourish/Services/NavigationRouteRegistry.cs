using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class NavigationRouteRegistry : INavigationRouteRegistry
{
    private readonly Lock gate = new();
    private readonly IServiceProvider? serviceProvider;
    private readonly Dictionary<string, FlourishNavigationRoute> routes = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, FlourishNavigationRoute> routesByPageType = [];
    private readonly Dictionary<string, Guid> leases = new(StringComparer.Ordinal);
    private long version;

    public NavigationRouteRegistry(IServiceProvider serviceProvider, FlourishShellOptions options)
        : this(options)
    {
        this.serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    internal NavigationRouteRegistry(FlourishShellOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        foreach (var route in options.InitialNavigationRoutes)
        {
            ValidateRoute(route);
            EnsurePageTypeAvailable(route.PageType, exceptNavigationKey: null);
            routes.Add(route.NavigationKey, route);
            routesByPageType.Add(route.PageType, route);
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
            routesByPageType.Add(route.PageType, route);
            leases[route.NavigationKey] = lease;
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
            if (previous is not null && previous.PageType != route.PageType)
            {
                routesByPageType.Remove(previous.PageType);
            }

            routes[route.NavigationKey] = route;
            routesByPageType[route.PageType] = route;
            leases[route.NavigationKey] = lease;
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
            routesByPageType[current.PageType] = current;
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
            return routesByPageType.TryGetValue(pageType, out route!);
        }
    }

    internal Page CreatePage(Type pageType, IPageFactory fallbackFactory, out long routeVersion)
    {
        FlourishNavigationRoute? route;
        lock (gate)
        {
            routesByPageType.TryGetValue(pageType, out route);
            routeVersion = version;
        }

        if (route?.PageFactory is not null)
        {
            return route.PageFactory(
                serviceProvider
                    ?? throw new InvalidOperationException(
                        "The runtime route factory service provider is unavailable."
                    )
            );
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
                && (
                    !leases.TryGetValue(navigationKey, out var currentLease)
                    || currentLease != lease
                )
            )
            {
                return false;
            }

            if (!routes.Remove(navigationKey, out removed))
            {
                return false;
            }

            leases.Remove(navigationKey);
            routesByPageType.Remove(removed.PageType);
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
        if (
            routesByPageType.TryGetValue(pageType, out var existing)
            && !StringComparer.Ordinal.Equals(existing.NavigationKey, exceptNavigationKey)
        )
        {
            throw new InvalidOperationException(
                $"Page type '{pageType.FullName}' is already registered as route '{existing.NavigationKey}'."
            );
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
            throw new ArgumentOutOfRangeException(
                nameof(cacheMode),
                cacheMode,
                "Unknown cache mode."
            );
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
