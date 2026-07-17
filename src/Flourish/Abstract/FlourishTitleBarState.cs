namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Represents an immutable snapshot of the current Flourish title bar state.</summary>
/// <param name="ApplicationTitle">The application title.</param>
/// <param name="ApplicationSubTitle">The application subtitle, or an empty string when unset.</param>
/// <param name="UnnamedProjectPlaceholder">The title shown when multi-project mode has no active project.</param>
/// <param name="SearchPlaceholder">The search-box placeholder.</param>
/// <param name="LogoPath">The configured logo path, or <see langword="null" /> when the built-in logo is used.</param>
/// <param name="LogoFallbackText">The text displayed when no logo image is available.</param>
/// <param name="ShowApplicationTitle">Whether the logo information surface shows the application title.</param>
/// <param name="ShowApplicationSubTitle">Whether the logo information surface shows the application subtitle.</param>
/// <param name="ShowProjectTitle">Whether the logo information surface shows the active project title.</param>
/// <param name="IsSearchVisible">Whether the search box is visible.</param>
/// <param name="IsBreadcrumbVisible">Whether the breadcrumb trail is visible.</param>
/// <param name="IsNavigationToggleVisible">Whether the navigation toggle is visible.</param>
/// <param name="IsLogoVisible">Whether the application logo button is visible.</param>
/// <param name="IsTitleVisible">Whether the application or project title button is visible.</param>
/// <param name="IsThemeToggleVisible">Whether the theme toggle is visible.</param>
/// <param name="IsProfileVisible">Whether the profile entry point is visible.</param>
/// <param name="BreadcrumbMode">The current breadcrumb presentation mode.</param>
public sealed record FlourishTitleBarState(
    string ApplicationTitle,
    string ApplicationSubTitle,
    string UnnamedProjectPlaceholder,
    string SearchPlaceholder,
    string? LogoPath,
    string LogoFallbackText,
    bool ShowApplicationTitle,
    bool ShowApplicationSubTitle,
    bool ShowProjectTitle,
    bool IsSearchVisible,
    bool IsBreadcrumbVisible,
    bool IsNavigationToggleVisible,
    bool IsLogoVisible,
    bool IsTitleVisible,
    bool IsThemeToggleVisible,
    bool IsProfileVisible,
    BreadcrumbShowOption BreadcrumbMode
)
{
    /// <summary>Creates a title bar snapshot using the legacy application identity shape.</summary>
    [Obsolete("Use the constructor that includes project and logo information settings.")]
    public FlourishTitleBarState(
        string title,
        string subtitle,
        string searchPlaceholder,
        string? logoPath,
        string logoFallbackText,
        bool isSearchVisible,
        bool isBreadcrumbVisible,
        bool isNavigationToggleVisible,
        bool isLogoVisible,
        bool isTitleVisible,
        bool isSubtitleVisible,
        bool isThemeToggleVisible,
        bool isProfileVisible,
        BreadcrumbShowOption breadcrumbMode
    )
        : this(
            title,
            subtitle,
            "Unnamed project",
            searchPlaceholder,
            logoPath,
            logoFallbackText,
            ShowApplicationTitle: true,
            ShowApplicationSubTitle: isSubtitleVisible,
            ShowProjectTitle: false,
            isSearchVisible,
            isBreadcrumbVisible,
            isNavigationToggleVisible,
            isLogoVisible,
            isTitleVisible,
            isThemeToggleVisible,
            isProfileVisible,
            breadcrumbMode
        ) { }

    /// <summary>Gets the application title.</summary>
    [Obsolete("Use ApplicationTitle.")]
    public string Title => ApplicationTitle;

    /// <summary>Gets the application subtitle.</summary>
    [Obsolete("Use ApplicationSubTitle.")]
    public string Subtitle => ApplicationSubTitle;

    /// <summary>Gets whether the application subtitle is visible in the logo information surface.</summary>
    [Obsolete("Application subtitles are displayed in the logo information surface.")]
    public bool IsSubtitleVisible => ShowApplicationSubTitle;

    /// <summary>Deconstructs the snapshot using the legacy application identity shape.</summary>
    [Obsolete("Use the deconstructor that includes project and logo information settings.")]
    public void Deconstruct(
        out string title,
        out string subtitle,
        out string searchPlaceholder,
        out string? logoPath,
        out string logoFallbackText,
        out bool isSearchVisible,
        out bool isBreadcrumbVisible,
        out bool isNavigationToggleVisible,
        out bool isLogoVisible,
        out bool isTitleVisible,
        out bool isSubtitleVisible,
        out bool isThemeToggleVisible,
        out bool isProfileVisible,
        out BreadcrumbShowOption breadcrumbMode
    )
    {
        title = ApplicationTitle;
        subtitle = ApplicationSubTitle;
        searchPlaceholder = SearchPlaceholder;
        logoPath = LogoPath;
        logoFallbackText = LogoFallbackText;
        isSearchVisible = IsSearchVisible;
        isBreadcrumbVisible = IsBreadcrumbVisible;
        isNavigationToggleVisible = IsNavigationToggleVisible;
        isLogoVisible = IsLogoVisible;
        isTitleVisible = IsTitleVisible;
        isSubtitleVisible = ShowApplicationSubTitle;
        isThemeToggleVisible = IsThemeToggleVisible;
        isProfileVisible = IsProfileVisible;
        breadcrumbMode = BreadcrumbMode;
    }
}
