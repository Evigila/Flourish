using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Represents current navigation and history state.</summary>
public sealed record NavigationState(
    string? NavigationKey,
    Type? PageType,
    object? Parameter,
    bool CanGoBack,
    bool CanGoForward,
    NavigationRouteSnapshot Routes,
    NavigationMenuSnapshot Menu,
    NavigationPanelState Panel,
    PageCacheSnapshot Cache,
    long Version
);
