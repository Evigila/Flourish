namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Changes Flourish title bar content and visibility while the application is running.</summary>
public interface ITitleBarService
{
    /// <summary>Gets an immutable snapshot of the current title bar configuration.</summary>
    FlourishTitleBarState Current { get; }

    /// <summary>Occurs synchronously after title bar content or visibility changes.</summary>
    event EventHandler<FlourishTitleBarChangedEventArgs>? Changed;

    /// <summary>Sets and shows the application title.</summary>
    /// <param name="title">The non-empty application title.</param>
    /// <exception cref="ArgumentException"><paramref name="title" /> is empty or whitespace.</exception>
    void SetApplicationTitle(string title);

    /// <summary>Sets the application subtitle displayed in the logo information surface.</summary>
    /// <param name="subTitle">The subtitle, or <see langword="null" /> to clear it.</param>
    void SetApplicationSubTitle(string? subTitle);

    /// <summary>Changes the application title and subtitle atomically.</summary>
    /// <param name="title">The non-empty application title.</param>
    /// <param name="subTitle">The application subtitle, or <see langword="null" /> to clear it.</param>
    /// <exception cref="ArgumentException"><paramref name="title" /> is empty or whitespace.</exception>
    void SetApplicationIdentity(string title, string? subTitle = null);

    /// <summary>Sets the title shown when multi-project mode has no active project.</summary>
    /// <param name="placeholder">The non-empty placeholder.</param>
    /// <exception cref="ArgumentException"><paramref name="placeholder" /> is empty or whitespace.</exception>
    void SetUnnamedProjectPlaceholder(string placeholder);

    /// <summary>Changes the logo source and information fields and makes the logo button visible.</summary>
    /// <param name="logoPath">The image path, or <see langword="null" /> to use the built-in logo.</param>
    /// <param name="fallbackText">The fallback text, or <see langword="null" /> to retain its current value.</param>
    /// <param name="showApplicationTitle">Whether the information surface displays the application title.</param>
    /// <param name="showApplicationSubTitle">Whether the information surface displays the application subtitle.</param>
    /// <param name="showProjectTitle">Whether the information surface displays the active project title.</param>
    void SetLogo(
        string? logoPath,
        string? fallbackText = null,
        bool showApplicationTitle = true,
        bool showApplicationSubTitle = true,
        bool showProjectTitle = false
    );

    /// <summary>Sets and shows the application title.</summary>
    /// <param name="title">The non-empty application title.</param>
    /// <exception cref="ArgumentException"><paramref name="title" /> is empty or whitespace.</exception>
    [Obsolete("Use SetApplicationTitle.")]
    void SetTitle(string title);

    /// <summary>Sets the application subtitle.</summary>
    /// <param name="subtitle">The subtitle, or <see langword="null" /> to clear it.</param>
    [Obsolete("Use SetApplicationSubTitle.")]
    void SetSubtitle(string? subtitle);

    /// <summary>Changes the application title and subtitle atomically.</summary>
    /// <param name="title">The non-empty application title.</param>
    /// <param name="subtitle">The subtitle, or <see langword="null" /> to clear it.</param>
    /// <exception cref="ArgumentException"><paramref name="title" /> is empty or whitespace.</exception>
    [Obsolete("Use SetApplicationIdentity.")]
    void SetIdentity(string title, string? subtitle = null);

    /// <summary>Sets the placeholder displayed by the title bar search box.</summary>
    /// <param name="placeholder">The non-empty search placeholder.</param>
    /// <exception cref="ArgumentException"><paramref name="placeholder" /> is empty or whitespace.</exception>
    void SetSearchPlaceholder(string placeholder);

    /// <summary>Shows or hides one title bar element without changing its content.</summary>
    /// <param name="element">The element to change.</param>
    /// <param name="visible"><see langword="true" /> to show the element; otherwise, <see langword="false" />.</param>
    /// <remarks>
    /// Multi-project mode keeps the title button visible because it represents the active project or
    /// unnamed-project placeholder. The obsolete <see cref="TitleBarElement.Subtitle" /> value changes
    /// subtitle visibility inside the logo information surface.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="element" /> is not defined.</exception>
    void SetElementVisible(TitleBarElement element, bool visible);

    /// <summary>Sets when breadcrumbs are displayed and synchronizes breadcrumb visibility.</summary>
    /// <param name="mode">The breadcrumb presentation mode.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode" /> is not defined.</exception>
    void SetBreadcrumbMode(BreadcrumbShowOption mode);
}
