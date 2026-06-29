using System.Windows.Media;

namespace Flourish.Models;

public sealed class FlourishShellOptions
{
    public string Title { get; set; } = "Flourish";

    public string Subtitle { get; set; } = "WPF Application";

    public string PaneTitle { get; set; } = "NAVIGATION";

    public string SearchPlaceholder { get; set; } = "Search";

    public string StatusText { get; set; } = "Ready";

    public ImageSource? LogoSource { get; set; }

    public string LogoFallbackText { get; set; } = "F";

    public double OpenPaneWidth { get; set; } = 220;

    public double ClosedPaneWidth { get; set; } = 56;

    public string? InitialNavigationKey { get; set; }

    public IReadOnlyList<FlourishNavigationItem> NavigationItems { get; set; } = [];

    public IReadOnlyList<FlourishCommandItem> ToolbarItems { get; set; } = [];

    public IReadOnlyList<FlourishStatusItem> StatusItems { get; set; } = [];
}
