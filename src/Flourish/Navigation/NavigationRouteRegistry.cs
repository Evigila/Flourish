using System;
using System.Collections.Generic;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationRouteRegistry
{
    private readonly Lock gate = new();
    private readonly IServiceProvider? serviceProvider;
    private readonly Dictionary<string, FlourishNavigationRoute> routes = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, FlourishNavigationRoute> routesByPageType = [];
    private readonly Dictionary<string, Guid> leases = new(StringComparer.Ordinal);
    private RouteRegistrySnapshot current;
    private long version;

    public NavigationRouteRegistry(IServiceProvider serviceProvider, FlourishNavigationOptions options)
        : this(options)
    {
        this.serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    internal NavigationRouteRegistry(FlourishNavigationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        foreach (var route in options.InitialNavigationRoutes)
        {
            ValidateRoute(route);
            EnsurePageTypeAvailable(route.PageType, exceptNavigationKey: null);
            routes.Add(route.NavigationKey, route);
            routesByPageType.Add(route.PageType, route);
        }

        current = CreateSnapshot();
    }

    public event EventHandler<FlourishNavigationRoutesChangedEventArgs>? Changed;

    public FlourishNavigationRouteSnapshot Current => Volatile.Read(ref current).Routes;

    public IRegistration Append(FlourishNavigationRoute route)
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
            snapshot = PublishSnapshot();
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

    public IRegistration Set(FlourishNavigationRoute route)
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
            snapshot = PublishSnapshot();
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
            snapshot = PublishSnapshot();
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

    public FlourishNavigationRoute? Get(string navigationKey)
    {
        navigationKey = ValidateKey(navigationKey, nameof(navigationKey));
        return Volatile.Read(ref current).Routes.Routes.GetValueOrDefault(navigationKey);
    }

    internal bool TryGet(string navigationKey, out FlourishNavigationRoute route)
    {
        return Volatile.Read(ref current).Routes.Routes.TryGetValue(navigationKey, out route!);
    }

    internal bool TryGet(Type pageType, out FlourishNavigationRoute route)
    {
        return Volatile.Read(ref current).RoutesByPageType.TryGetValue(pageType, out route!);
    }

    internal Page CreatePage(Type pageType, IPageFactory fallbackFactory, out long routeVersion)
    {
        var snapshot = Volatile.Read(ref current);
        snapshot.RoutesByPageType.TryGetValue(pageType, out var route);
        routeVersion = snapshot.Routes.Version;

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
            snapshot = PublishSnapshot();
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

    private FlourishNavigationRouteSnapshot PublishSnapshot()
    {
        var snapshot = CreateSnapshot();
        Volatile.Write(ref current, snapshot);
        return snapshot.Routes;
    }

    private RouteRegistrySnapshot CreateSnapshot()
    {
        var routesSnapshot = new ReadOnlyDictionary<string, FlourishNavigationRoute>(
            new Dictionary<string, FlourishNavigationRoute>(routes, StringComparer.Ordinal)
        );
        return new RouteRegistrySnapshot(
            new FlourishNavigationRouteSnapshot(routesSnapshot, version),
            new ReadOnlyDictionary<Type, FlourishNavigationRoute>(
                new Dictionary<Type, FlourishNavigationRoute>(routesByPageType)
            )
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
    ) : IRegistration
    {
        private NavigationRouteRegistry? owner = owner;

        public string NavigationKey { get; } = navigationKey;

        public bool IsRegistered => Volatile.Read(ref owner) is not null;

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.RemoveCore(NavigationKey, lease);
        }
    }

    private sealed record RouteRegistrySnapshot(
        FlourishNavigationRouteSnapshot Routes,
        IReadOnlyDictionary<Type, FlourishNavigationRoute> RoutesByPageType
    );
}
