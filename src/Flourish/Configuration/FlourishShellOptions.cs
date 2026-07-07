using System.Windows.Media;
using System.Windows;
using AckSS.Flourish.Abstract;

namespace AckSS.Flourish.Configuration;

internal sealed class FlourishShellOptions
{
    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = "WPF Application";

    public string PaneTitle { get; set; } = string.Empty;

    public string SearchPlaceholder { get; set; } = "Search";

    public string StatusText { get; set; } = string.Empty;

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

    public bool IsThemeEnabled { get; set; }

    public FlourishTheme DefaultTheme { get; set; } = FlourishTheme.System;

    public FlourishMotionOptions Motion { get; } = new();

    public FlourishTipOptions Tips { get; } = new();

    public bool WindowTopmost { get; set; }

    public bool WindowShowInTaskbar { get; set; } = true;

    public bool IsTrayExitEnabled { get; set; }

    public bool IsNavigationPanelEnabled { get; set; }

    public bool IsNavigationPanelInitiallyOpen { get; set; }

    public NavigationPanelDirection NavigationPanelDirection { get; set; } = NavigationPanelDirection.Left;

    public bool IsTitlebarEnabled { get; set; }

    public bool IsTitlebarSearchEnabled { get; set; }

    public bool IsTitlebarNavigationToggleEnabled { get; set; }

    public bool IsTitlebarLogoEnabled { get; set; }

    public bool IsTitlebarTitleEnabled { get; set; }

    public bool IsTitlebarSubtitleEnabled { get; set; }

    public bool IsTitlebarProfileEnabled { get; set; }

    public bool IsTitlebarThemeToggleEnabled { get; set; }

    public bool IsDynamicToolbarEnabled { get; set; }

    public bool IsBreadcrumbEnabled { get; set; }

    public bool IsStatusBarEnabled { get; set; }

    public BreadcrumbShowOption BreadcrumbShowOption { get; set; } = BreadcrumbShowOption.Auto;

    public double OpenPaneWidth { get; set; } = 220;

    public double ClosedPaneWidth { get; set; } = 48;

    public double NavigationPaneMinWidth { get; set; } = 160;

    public double NavigationPaneMaxWidth { get; set; } = 420;

    public string? InitialNavigationKey { get; set; }

    public Type? InitialNavigationPageType { get; set; }

    public Dictionary<Type, FlourishPageCacheMode> PageCacheModesByPageType { get; } = [];

    public List<FlourishNavigationGroup> NavigationGroups { get; } = [];

    public List<FlourishNavigationItem> FixedNavigationItemDefinitions { get; } = [];

    public List<FlourishNavigationItem> NavigationItems { get; } = [];

    public List<FlourishNavigationItem> FixedNavigationItems { get; } = [];

    public List<FlourishToolbarItem> ToolbarItems { get; } = [];

    public Dictionary<Type, IReadOnlyList<FlourishToolbarItem>> DynamicToolbarItems { get; } = [];

    public Dictionary<Type, bool> DynamicToolbarIconModes { get; } = [];

    public List<FlourishStatusItem> StatusItems { get; } = [];

    public List<FlourishRegionContent> RegionContents { get; } = [];
}
