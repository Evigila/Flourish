namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Represents an immutable snapshot of the current Flourish title bar state.</summary>
/// <param name="ApplicationTitle">The application title.</param>
/// <param name="ApplicationSubtitle">The application subtitle, or an empty string when unset.</param>
/// <param name="UnnamedProjectPlaceholder">The display name used for a project that has no storage path or when no project is active.</param>
/// <param name="SearchPlaceholder">The search-box placeholder.</param>
/// <param name="LogoPath">The configured logo path, or <see langword="null" /> when the built-in logo is used.</param>
/// <param name="LogoFallbackText">The text displayed when no logo image is available.</param>
/// <param name="ShowApplicationTitle">Whether the logo information surface shows the application title.</param>
/// <param name="ShowApplicationSubtitle">Whether the logo information surface shows the application subtitle.</param>
/// <param name="ShowProjectTitle">Whether the logo information surface shows the active project title.</param>
/// <param name="IsSearchVisible">Whether the search box is visible.</param>
/// <param name="IsBreadcrumbVisible">Whether the breadcrumb trail is visible.</param>
/// <param name="IsNavigationToggleVisible">Whether the navigation toggle is visible.</param>
/// <param name="IsLogoVisible">Whether the application logo button is visible.</param>
/// <param name="IsTitleVisible">Whether the application or project title selector is visible.</param>
/// <param name="IsThemeToggleVisible">Whether the theme toggle is visible.</param>
/// <param name="IsProfileVisible">Whether the profile entry point is visible.</param>
/// <param name="BreadcrumbMode">The current breadcrumb presentation mode.</param>
public sealed record FlourishTitleBarState(
    string ApplicationTitle,
    string ApplicationSubtitle,
    string UnnamedProjectPlaceholder,
    string SearchPlaceholder,
    string? LogoPath,
    string LogoFallbackText,
    bool ShowApplicationTitle,
    bool ShowApplicationSubtitle,
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
    /// <summary>Gets whether the complete title bar surface is enabled.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Gets the monotonic state version.</summary>
    public long Version { get; init; }

    /// <summary>Gets the current title bar search text.</summary>
    public string SearchText { get; init; } = string.Empty;

    /// <summary>Gets whether the search box has a pending focus request.</summary>
    public bool IsSearchFocusRequested { get; init; }
}
