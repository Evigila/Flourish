using System.Windows;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Internal.Configuration;

internal sealed class FlourishShellOptions
{
    public string ApplicationTitle { get; set; } = "MyApp";

    public string ApplicationSubtitle { get; set; } = "MyApp";

    public string UnnamedProjectPlaceholder { get; set; } = "Unnamed project";

    public string SearchPlaceholder { get; set; } = "Search";

    public Action<IServiceProvider, string>? TitlebarSearchTextChanged { get; set; }

    public string? LogoPath { get; set; }

    public string LogoFallbackText { get; set; } = "F";

    public bool ShowApplicationTitleInLogoFlyout { get; set; } = true;

    public bool ShowApplicationSubtitleInLogoFlyout { get; set; } = true;

    public bool ShowProjectTitleInLogoFlyout { get; set; }

    public string FontFamily { get; set; } = "Segoe UI";

    public string IconFontFamily { get; set; } = "Segoe MDL2 Assets";

    public double FontSizeSmall { get; set; } = 12;

    public double FontSizeStandard { get; set; } = 14;

    public double FontSizeIcon { get; set; } = 22;

    public double FontSizeLarge { get; set; } = 16;

    public double FontSizeExtraLarge { get; set; } = 24;

    public double FontSizeHeaderSize { get; set; } = 32;

    public FlourishThemeColors? ThemeColors { get; set; }

    public double? CornerRadius { get; set; }

    public Dictionary<Type, FlourishPageFontOverride> PageFontOverridesByPageType { get; } = [];

    public double WindowWidth { get; set; } = 1536;

    public double WindowHeight { get; set; } = 864;

    public double WindowMinWidth { get; set; } = 1280;

    public double WindowMinHeight { get; set; } = 720;

    public double WindowMaxWidth { get; set; } = double.PositiveInfinity;

    public double WindowMaxHeight { get; set; } = double.PositiveInfinity;

    public double? WindowLeft { get; set; }

    public double? WindowTop { get; set; }

    public WindowStartupLocation WindowStartupLocation { get; set; } =
        WindowStartupLocation.CenterScreen;

    public WindowState WindowState { get; set; } = WindowState.Normal;

    public ResizeMode WindowResizeMode { get; set; } = ResizeMode.CanResize;

    public MaterialEffect MaterialEffect { get; set; } = MaterialEffect.Mica;

    public bool IsMaterialEffectEnabled { get; set; }

    public bool IsThemeEnabled { get; set; }

    public FlourishTheme DefaultTheme { get; set; } = FlourishTheme.System;

    public FlourishMotionOptions Motion { get; } = new();

    public bool IsSmoothScrollingEnabled { get; set; } = true;

    public FlourishTipOptions Tips { get; } = new();

    public FlourishProfileOptions Profile { get; } = new();

    public bool IsTipsEnabled { get; set; }

    public bool WindowTopmost { get; set; }

    public bool WindowShowInTaskbar { get; set; } = true;

    public bool IsTrayExitEnabled { get; set; }

    public bool IsNavigationPanelEnabled { get; set; }

    public bool IsCenterContentEnabled { get; set; }

    public double CenterContentWidth { get; set; }

    public bool IsNavigationPanelInitiallyOpen { get; set; }

    public NavigationPanelDirection NavigationPanelDirection { get; set; } = NavigationPanelDirection.Left;

    public bool IsTitlebarEnabled { get; set; }

    public bool IsMultiProjectEnabled { get; set; }

    public bool IsProfileEnabled { get; set; }

    public bool IsTitlebarSearchEnabled { get; set; }

    public bool IsTitlebarNavigationToggleEnabled { get; set; }

    public bool IsTitlebarLogoEnabled { get; set; }

    public bool IsTitlebarTitleEnabled { get; set; }

    public bool IsTitlebarProfileEnabled { get; set; }

    public bool IsTitlebarThemeToggleEnabled { get; set; }

    public bool IsDynamicToolbarEnabled { get; set; }

    public bool IsBreadcrumbEnabled { get; set; }

    public bool IsStatusBarEnabled { get; set; }

    public bool IsLANConnectionStatusEnabled { get; set; }

    public bool IsPowerStatusEnabled { get; set; }

    public BreadcrumbShowOption BreadcrumbShowOption { get; set; } = BreadcrumbShowOption.Auto;

    public double OpenPaneWidth { get; set; } = 250;

    public double ClosedPaneWidth { get; set; } = 64;

    public double NavigationPaneMinWidth { get; set; } = 180;

    public double NavigationPaneMaxWidth { get; set; } = 520;

    public string? InitialNavigationKey { get; set; }

    public Type? InitialNavigationPageType { get; set; }

    public List<FlourishNavigationRoute> InitialNavigationRoutes { get; } = [];

    public List<FlourishNavigationGroup> NavigationGroups { get; } = [];

    public List<FlourishNavigationItem> FixedNavigationItemDefinitions { get; } = [];

    public List<FlourishNavigationItem> NavigationItems { get; } = [];

    public List<FlourishNavigationItem> FixedNavigationItems { get; } = [];

    public List<FlourishToolbarItem> ToolbarItems { get; } = [];

    public Dictionary<Type, IReadOnlyList<FlourishToolbarItem>> DynamicToolbarItems { get; } = [];

    public Dictionary<Type, bool> DynamicToolbarIconModes { get; } = [];

    public List<FlourishStatusItem> StatusItems { get; } = [];

    public List<FlourishRegionContent> RegionContents { get; } = [];

    public bool UsePersistedTheme { get; set; } = true;

    public bool UsePersistedWindowSize { get; set; } = true;

    public bool UsePersistedWindowPosition { get; set; } = true;

    public bool UsePersistedWindowState { get; set; } = true;

    public bool UsePersistedWindowTopmost { get; set; } = true;

    public bool UsePersistedTrayExit { get; set; } = true;

    public bool UsePersistedNavigationDirection { get; set; } = true;

    public bool UsePersistedNavigationOpenState { get; set; } = true;

    public bool UsePersistedNavigationWidth { get; set; } = true;

    public bool UsePersistedLastNavigation { get; set; } = true;

    public bool UsePersistedMotion { get; set; } = true;

    public bool UsePersistedSmoothScroll { get; set; } = true;

    public bool UsePersistedFont { get; set; } = true;

    public bool UsePersistedContentLayout { get; set; } = true;

    public bool UsePersistedMaterialEffect { get; set; } = true;

    public bool UsePersistedThemeColors { get; set; } = true;

    public bool UsePersistedCornerRadius { get; set; } = true;

    public bool UsePersistedNameOrder { get; set; } = true;
}
