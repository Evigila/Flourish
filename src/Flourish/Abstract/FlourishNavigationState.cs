using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Represents current navigation and history state.</summary>
public sealed record FlourishNavigationState(
    string? NavigationKey,
    Type? PageType,
    object? Parameter,
    bool CanGoBack,
    bool CanGoForward,
    FlourishNavigationRouteSnapshot Routes,
    FlourishNavigationMenuSnapshot Menu,
    FlourishNavigationPanelState Panel,
    FlourishPageCacheSnapshot Cache,
    long Version
);
