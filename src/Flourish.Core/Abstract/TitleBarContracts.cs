using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Specifies when breadcrumb navigation is displayed.</summary>
public enum BreadcrumbShowOption
{
    /// <summary>Always displays breadcrumb navigation.</summary>
    Always,

    /// <summary>Displays breadcrumbs when navigation history is available.</summary>
    Auto,

    /// <summary>Hides breadcrumb navigation.</summary>
    Hidden,
}

/// <summary>Identifies a configurable title bar element.</summary>
public enum TitleBarElement
{
    /// <summary>The title bar search box.</summary>
    Search,

    /// <summary>The breadcrumb trail.</summary>
    Breadcrumb,

    /// <summary>The navigation panel toggle.</summary>
    NavigationToggle,

    /// <summary>The application logo or text fallback.</summary>
    Logo,

    /// <summary>The application or active-project title.</summary>
    Title,

    /// <summary>The theme toggle.</summary>
    ThemeToggle,

    /// <summary>The profile entry point.</summary>
    Profile,
}

/// <summary>Describes a title bar search query raised at runtime.</summary>
public readonly record struct TitleBarSearchQuery(string Text, long Sequence);

/// <summary>Represents an immutable title bar state snapshot.</summary>
public sealed record TitleBarState(
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

    /// <summary>Gets the current search text.</summary>
    public string SearchText { get; init; } = string.Empty;

    /// <summary>Gets whether the search box has a pending focus request.</summary>
    public bool IsSearchFocusRequested { get; init; }
}

/// <summary>Changes title bar content and visibility at runtime.</summary>
public interface ITitleBarService
{
    /// <summary>Gets the current immutable configuration.</summary>
    TitleBarState Current { get; }

    /// <summary>Occurs synchronously after state changes.</summary>
    event EventHandler<StateChangedEventArgs<TitleBarState>>? Changed;

    /// <summary>Enables or disables the complete title bar surface.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Sets and shows the application title.</summary>
    void SetApplicationTitle(string title);

    /// <summary>Sets or clears the application subtitle.</summary>
    void SetApplicationSubtitle(string? subtitle);

    /// <summary>Changes title and subtitle atomically.</summary>
    void SetApplicationIdentity(string title, string? subtitle = null);

    /// <summary>Sets the name displayed for an unpersisted project.</summary>
    void SetUnnamedProjectPlaceholder(string placeholder);

    /// <summary>Changes the logo source and information fields.</summary>
    void SetLogo(
        string? logoPath,
        string? fallbackText = null,
        bool showApplicationTitle = true,
        bool showApplicationSubtitle = true,
        bool showProjectTitle = false
    );

    /// <summary>Sets the search placeholder.</summary>
    void SetSearchPlaceholder(string placeholder);

    /// <summary>Shows or hides search.</summary>
    void SetSearchVisible(bool visible);

    /// <summary>Sets search text without invoking query subscribers.</summary>
    void SetSearchText(string text);

    /// <summary>Clears search text without invoking query subscribers.</summary>
    void ClearSearch();

    /// <summary>Requests keyboard focus for search.</summary>
    void FocusSearch();

    /// <summary>Registers an asynchronous query handler.</summary>
    IRegistration SubscribeSearch(
        Func<TitleBarSearchQuery, CancellationToken, ValueTask> handler
    );

    /// <summary>Shows or hides one element.</summary>
    void SetElementVisible(TitleBarElement element, bool visible);

    /// <summary>Sets breadcrumb presentation.</summary>
    void SetBreadcrumbMode(BreadcrumbShowOption mode);
}
