using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationOptions
{
    public bool IsNavigationPanelEnabled { get; set; }
    public bool IsNavigationPanelInitiallyOpen { get; set; }
    public NavigationPanelDirection NavigationPanelDirection { get; set; } = NavigationPanelDirection.Left;
    public double OpenPaneWidth { get; set; } = 250;
    public double ClosedPaneWidth { get; set; } = 64;
    public double NavigationPaneMinWidth { get; set; } = 180;
    public double NavigationPaneMaxWidth { get; set; } = 520;
    public string? InitialNavigationKey { get; set; }
    public Type? InitialNavigationPageType { get; set; }
    public List<NavigationRoute> InitialNavigationRoutes { get; } = [];
    public List<NavigationGroupDefinition> NavigationGroups { get; } = [];
    public List<NavigationItemDefinition> FixedNavigationItemDefinitions { get; } = [];
    public List<NavigationItemDefinition> NavigationItems { get; } = [];
    public List<NavigationItemDefinition> FixedNavigationItems { get; } = [];
    public bool UsePersistedNavigationDirection { get; set; } = true;
    public bool UsePersistedNavigationOpenState { get; set; } = true;
    public bool UsePersistedNavigationWidth { get; set; } = true;
    public bool UsePersistedLastNavigation { get; set; } = true;
}
