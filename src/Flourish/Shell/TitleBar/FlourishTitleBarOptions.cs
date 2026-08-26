using System;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.TitleBar;

internal sealed class FlourishTitleBarOptions
{
    public string ApplicationTitle { get; set; } = "MyApp";
    public string ApplicationSubtitle { get; set; } = "MyApp";
    public string SearchPlaceholder { get; set; } = "Search";
    public Action<IServiceProvider, string>? TitlebarSearchTextChanged { get; set; }
    public string? LogoPath { get; set; }
    public string LogoFallbackText { get; set; } = "F";
    public bool ShowApplicationTitleInLogoFlyout { get; set; } = true;
    public bool ShowApplicationSubtitleInLogoFlyout { get; set; } = true;
    public bool ShowProjectTitleInLogoFlyout { get; set; }
    public bool IsTitlebarEnabled { get; set; }
    public bool IsTitlebarSearchEnabled { get; set; }
    public bool IsTitlebarNavigationToggleEnabled { get; set; }
    public bool IsTitlebarLogoEnabled { get; set; }
    public bool IsTitlebarTitleEnabled { get; set; }
    public bool IsTitlebarProfileEnabled { get; set; }
    public bool IsTitlebarThemeToggleEnabled { get; set; }
    public bool IsBreadcrumbEnabled { get; set; }
    public BreadcrumbShowOption BreadcrumbShowOption { get; set; } = BreadcrumbShowOption.Auto;
}
