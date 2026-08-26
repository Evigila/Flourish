using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Changes Flourish title bar content and visibility while the application is running.</summary>
public interface ITitleBarService
{
    /// <summary>Gets an immutable snapshot of the current title bar configuration.</summary>
    FlourishTitleBarState Current { get; }

    /// <summary>Occurs synchronously after title bar content or visibility changes.</summary>
    event EventHandler<FlourishStateChangedEventArgs<FlourishTitleBarState>>? Changed;

    /// <summary>Enables or disables the complete title bar surface.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Sets and shows the application title.</summary>
    /// <param name="title">The non-empty application title.</param>
    /// <exception cref="ArgumentException"><paramref name="title" /> is empty or whitespace.</exception>
    void SetApplicationTitle(string title);

    /// <summary>Sets the application subtitle displayed in the logo information surface.</summary>
    /// <param name="subtitle">The subtitle, or <see langword="null" /> to clear it.</param>
    void SetApplicationSubtitle(string? subtitle);

    /// <summary>Changes the application title and subtitle atomically.</summary>
    /// <param name="title">The non-empty application title.</param>
    /// <param name="subtitle">The application subtitle, or <see langword="null" /> to clear it.</param>
    /// <exception cref="ArgumentException"><paramref name="title" /> is empty or whitespace.</exception>
    void SetApplicationIdentity(string title, string? subtitle = null);

    /// <summary>Sets the display name used for a project that has no storage path or when no project is active.</summary>
    /// <param name="placeholder">The non-empty display name for an unpersisted project.</param>
    /// <exception cref="ArgumentException"><paramref name="placeholder" /> is empty or whitespace.</exception>
    void SetUnnamedProjectPlaceholder(string placeholder);

    /// <summary>Changes the logo source and information fields and makes the logo button visible.</summary>
    /// <param name="logoPath">The image path, or <see langword="null" /> to use the built-in logo.</param>
    /// <param name="fallbackText">The fallback text, or <see langword="null" /> to retain its current value.</param>
    /// <param name="showApplicationTitle">Whether the information surface displays the application title.</param>
    /// <param name="showApplicationSubtitle">Whether the information surface displays the application subtitle.</param>
    /// <param name="showProjectTitle">Whether the information surface displays the active project title.</param>
    void SetLogo(
        string? logoPath,
        string? fallbackText = null,
        bool showApplicationTitle = true,
        bool showApplicationSubtitle = true,
        bool showProjectTitle = false
    );

    /// <summary>Sets the placeholder displayed by the title bar search box.</summary>
    /// <param name="placeholder">The non-empty search placeholder.</param>
    /// <exception cref="ArgumentException"><paramref name="placeholder" /> is empty or whitespace.</exception>
    void SetSearchPlaceholder(string placeholder);

    /// <summary>Shows or hides the title bar search box.</summary>
    void SetSearchVisible(bool visible);

    /// <summary>Sets search text without invoking query subscribers.</summary>
    void SetSearchText(string text);

    /// <summary>Clears search text without invoking query subscribers.</summary>
    void ClearSearch();

    /// <summary>Requests keyboard focus for the title bar search box.</summary>
    void FocusSearch();

    /// <summary>Registers an asynchronous handler for user-entered search queries.</summary>
    IRegistration SubscribeSearch(
        Func<FlourishTitleBarSearchQuery, CancellationToken, ValueTask> handler
    );

    /// <summary>Shows or hides one title bar element without changing its content.</summary>
    /// <param name="element">The element to change.</param>
    /// <param name="visible"><see langword="true" /> to show the element; otherwise, <see langword="false" />.</param>
    /// <remarks>
    /// Multi-project mode keeps the title selector visible because it represents the active project or
    /// unnamed-project placeholder.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="element" /> is not defined.</exception>
    void SetElementVisible(TitleBarElement element, bool visible);

    /// <summary>Sets when breadcrumbs are displayed and synchronizes breadcrumb visibility.</summary>
    /// <param name="mode">The breadcrumb presentation mode.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode" /> is not defined.</exception>
    void SetBreadcrumbMode(BreadcrumbShowOption mode);
}
