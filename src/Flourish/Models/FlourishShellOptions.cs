using System.Windows.Media;
using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Models;

internal sealed class FlourishShellOptions
{
    public string Title { get; set; } = "Flourish";

    public string Subtitle { get; set; } = "WPF Application";

    public string PaneTitle { get; set; } = "NAVIGATION";

    public string SearchPlaceholder { get; set; } = "Search";

    public string StatusText { get; set; } = "Ready";

    public ImageSource? LogoSource { get; set; }

    public string LogoFallbackText { get; set; } = "F";

    public bool IsNavigationPanelEnabled { get; set; } = true;

    public NavigationPanelDirection NavigationPanelDirection { get; set; } = NavigationPanelDirection.Left;

    public bool IsTitlebarSearchEnabled { get; set; } = true;

    public bool IsDynamicToolbarEnabled { get; set; }

    public bool IsBreadcrumbEnabled { get; set; }

    public BreadcrumbShowOption BreadcrumbShowOption { get; set; } = BreadcrumbShowOption.OnlyAvailable;

    public double OpenPaneWidth { get; set; } = 220;

    public double ClosedPaneWidth { get; set; } = 56;

    public string? InitialNavigationKey { get; set; }

    public Type? InitialNavigationPageType { get; set; }

    public List<FlourishNavigationItem> NavigationItems { get; } = [];

    public List<FlourishToolbarItem> ToolbarItems { get; } = [];

    public Dictionary<Type, IReadOnlyList<FlourishToolbarItem>> DynamicToolbarItems { get; } = [];

    public List<FlourishStatusItem> StatusItems { get; } = [];
}
