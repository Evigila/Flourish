using System;
using System.Collections.Generic;

using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

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
