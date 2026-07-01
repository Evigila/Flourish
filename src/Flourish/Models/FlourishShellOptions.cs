using System.Windows.Media;
using System.Windows;
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

    public string FontFamily { get; set; } = "Segoe UI";

    public string IconFontFamily { get; set; } = "Segoe MDL2 Assets";

    public double FontSize { get; set; } = 13;

    public double WindowWidth { get; set; } = 1100;

    public double WindowHeight { get; set; } = 720;

    public double WindowMinWidth { get; set; } = 820;

    public double WindowMinHeight { get; set; } = 560;

    public double WindowMaxWidth { get; set; } = double.PositiveInfinity;

    public double WindowMaxHeight { get; set; } = double.PositiveInfinity;

    public double? WindowLeft { get; set; }

    public double? WindowTop { get; set; }

    public WindowStartupLocation WindowStartupLocation { get; set; } =
        WindowStartupLocation.CenterScreen;

    public WindowState WindowState { get; set; } = WindowState.Normal;

    public ResizeMode WindowResizeMode { get; set; } = ResizeMode.CanResize;

    public MaterialEffect MaterialEffect { get; set; }

    public bool WindowTopmost { get; set; }

    public bool WindowShowInTaskbar { get; set; } = true;

    public bool IsTrayExitEnabled { get; set; }

    public bool IsNavigationPanelEnabled { get; set; } = true;

    public bool IsNavigationPanelInitiallyOpen { get; set; } = true;

    public NavigationPanelDirection NavigationPanelDirection { get; set; } = NavigationPanelDirection.Left;

    public bool IsTitlebarSearchEnabled { get; set; } = true;

    public bool IsTitlebarNavigationToggleEnabled { get; set; } = true;

    public bool IsTitlebarLogoEnabled { get; set; } = true;

    public bool IsTitlebarTitleEnabled { get; set; } = true;

    public bool IsTitlebarSubtitleEnabled { get; set; } = true;

    public bool IsTitlebarProfileEnabled { get; set; } = true;

    public bool IsDynamicToolbarEnabled { get; set; }

    public bool IsBreadcrumbEnabled { get; set; } = true;

    public BreadcrumbShowOption BreadcrumbShowOption { get; set; } = BreadcrumbShowOption.Auto;

    public double OpenPaneWidth { get; set; } = 220;

    public double ClosedPaneWidth { get; set; } = 48;

    public string? InitialNavigationKey { get; set; }

    public Type? InitialNavigationPageType { get; set; }

    public List<FlourishNavigationItem> NavigationItems { get; } = [];

    public List<FlourishToolbarItem> ToolbarItems { get; } = [];

    public Dictionary<Type, IReadOnlyList<FlourishToolbarItem>> DynamicToolbarItems { get; } = [];

    public Dictionary<Type, bool> DynamicToolbarIconModes { get; } = [];

    public List<FlourishStatusItem> StatusItems { get; } = [];
}
