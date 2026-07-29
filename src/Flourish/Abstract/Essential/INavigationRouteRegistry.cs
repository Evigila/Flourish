using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>
/// Registers and removes routes that can be used by <see cref="INavigationService" /> at runtime.
/// </summary>
public interface INavigationRouteRegistry
{
    /// <summary>Occurs after the route table changes.</summary>
    event EventHandler<FlourishNavigationRoutesChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of the current route table.</summary>
    FlourishNavigationRouteSnapshot Current { get; }

    /// <summary>Adds a route and returns a handle that removes it when disposed.</summary>
    INavigationRouteRegistration Append(FlourishNavigationRoute route);

    /// <summary>Adds a route or replaces the route with the same navigation key.</summary>
    INavigationRouteRegistration Set(FlourishNavigationRoute route);

    /// <summary>Removes a route by its case-sensitive navigation key.</summary>
    bool Remove(string navigationKey);

    /// <summary>Changes a route's page cache mode.</summary>
    void SetCacheMode(string navigationKey, FlourishPageCacheMode cacheMode);

    /// <summary>Gets a route by its case-sensitive navigation key, or <see langword="null" /> when it is not registered.</summary>
    FlourishNavigationRoute? Get(string navigationKey);
}

/// <summary>Controls the lifetime of a runtime navigation route.</summary>
public interface INavigationRouteRegistration : IDisposable
{
    /// <summary>Gets the case-sensitive navigation key.</summary>
    string NavigationKey { get; }
}

/// <summary>Describes a route that can create a WPF page.</summary>
public sealed record FlourishNavigationRoute
{
    /// <summary>Creates a route definition.</summary>
    public FlourishNavigationRoute(
        string navigationKey,
        Type pageType,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Disabled,
        Func<IServiceProvider, Page>? pageFactory = null
    )
    {
        NavigationKey = navigationKey;
        PageType = pageType;
        CacheMode = cacheMode;
        PageFactory = pageFactory;
    }

    /// <summary>Gets the case-sensitive navigation key.</summary>
    public string NavigationKey { get; init; }

    /// <summary>Gets the WPF page type associated with the route.</summary>
    public Type PageType { get; init; }

    /// <summary>Gets the page cache mode.</summary>
    public FlourishPageCacheMode CacheMode { get; init; }

    /// <summary>
    /// Gets the optional runtime page factory. When omitted, Flourish resolves or activates
    /// <see cref="PageType" /> using the application service provider.
    /// </summary>
    public Func<IServiceProvider, Page>? PageFactory { get; init; }
}

/// <summary>Represents an immutable route-table snapshot.</summary>
public sealed record FlourishNavigationRouteSnapshot(
    IReadOnlyDictionary<string, FlourishNavigationRoute> Routes,
    long Version
);

/// <summary>Provides data for <see cref="INavigationRouteRegistry.Changed" />.</summary>
public sealed class FlourishNavigationRoutesChangedEventArgs(
    FlourishNavigationRouteSnapshot current,
    FlourishRuntimeChangeKind changeKind,
    FlourishNavigationRoute? previousRoute,
    FlourishNavigationRoute? route
) : EventArgs
{
    /// <summary>Gets the current route table.</summary>
    public FlourishNavigationRouteSnapshot Current { get; } = current;

    /// <summary>Gets the mutation kind.</summary>
    public FlourishRuntimeChangeKind ChangeKind { get; } = changeKind;

    /// <summary>Gets the route before the mutation, if applicable.</summary>
    public FlourishNavigationRoute? PreviousRoute { get; } = previousRoute;

    /// <summary>Gets the route after the mutation, if applicable.</summary>
    public FlourishNavigationRoute? Route { get; } = route;
}
