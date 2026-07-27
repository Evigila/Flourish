using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>
/// Controls global Flourish text and icon fonts and page-specific text overrides at runtime.
/// </summary>
public interface IFontService
{
    /// <summary>
    /// Gets the current text font family name.
    /// </summary>
    string FontFamily { get; }

    /// <summary>
    /// Gets the current icon font family name.
    /// </summary>
    string IconFontFamily { get; }

    /// <summary>
    /// Gets the current small font size.
    /// </summary>
    double SmallFontSize { get; }

    /// <summary>
    /// Gets the current standard font size.
    /// </summary>
    double StandardFontSize { get; }

    /// <summary>
    /// Gets the current icon font size.
    /// </summary>
    double IconFontSize { get; }

    /// <summary>
    /// Gets the current large font size.
    /// </summary>
    double LargeFontSize { get; }

    /// <summary>
    /// Gets the current extra-large font size.
    /// </summary>
    double ExtraLargeFontSize { get; }

    /// <summary>
    /// Gets the current header-size font size.
    /// </summary>
    double HeaderSizeFontSize { get; }

    /// <summary>
    /// Gets immutable snapshots of the font overrides configured for individual page types.
    /// </summary>
    IReadOnlyDictionary<Type, FlourishPageFontOverride> PageOverrides { get; }

    /// <summary>
    /// Raised after the runtime font settings change.
    /// </summary>
    /// <remarks>
    /// When the Flourish application resource scope is attached, the event is raised on
    /// that application's dispatcher after the corresponding resources are updated.
    /// </remarks>
    event EventHandler<FlourishFontChangedEventArgs>? Changed;

    /// <summary>
    /// Changes the text font family and the explicit text and icon size scale together.
    /// </summary>
    /// <param name="fontFamily">The font family name.</param>
    /// <param name="smallFontSize">The small font size.</param>
    /// <param name="standardFontSize">The standard font size.</param>
    /// <param name="iconFontSize">The icon font size.</param>
    /// <param name="largeFontSize">The large font size.</param>
    /// <param name="extraLargeFontSize">The extra-large font size.</param>
    /// <param name="headerSizeFontSize">The header-size font size.</param>
    void SetFont(
        string fontFamily,
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    );

    /// <summary>
    /// Changes the icon font family.
    /// </summary>
    void SetIconFontFamily(string fontFamily);

    /// <summary>
    /// Sets or replaces the font and explicit text and icon size override for a page type.
    /// </summary>
    /// <typeparam name="TPage">The WPF page type that receives the override.</typeparam>
    /// <param name="fontFamily">The page-specific font family name.</param>
    /// <param name="smallFontSize">The page-specific small size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="standardFontSize">The page-specific standard size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="iconFontSize">The page-specific icon size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="largeFontSize">The page-specific large size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="extraLargeFontSize">The page-specific extra-large size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="headerSizeFontSize">The page-specific header size, or <see langword="null"/> to follow the global size.</param>
    void SetOverrideFont<TPage>(
        string fontFamily,
        double? smallFontSize,
        double? standardFontSize,
        double? iconFontSize,
        double? largeFontSize,
        double? extraLargeFontSize,
        double? headerSizeFontSize
    )
        where TPage : Page;

    /// <summary>
    /// Sets or replaces the font and explicit text and icon size override for a page type selected at runtime.
    /// </summary>
    /// <param name="pageType">The closed, concrete WPF page type that receives the override.</param>
    /// <param name="fontFamily">The page-specific font family name.</param>
    /// <param name="smallFontSize">The page-specific small size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="standardFontSize">The page-specific standard size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="iconFontSize">The page-specific icon size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="largeFontSize">The page-specific large size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="extraLargeFontSize">The page-specific extra-large size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="headerSizeFontSize">The page-specific header size, or <see langword="null"/> to follow the global size.</param>
    void SetOverrideFont(
        Type pageType,
        string fontFamily,
        double? smallFontSize,
        double? standardFontSize,
        double? iconFontSize,
        double? largeFontSize,
        double? extraLargeFontSize,
        double? headerSizeFontSize
    );

    /// <summary>
    /// Removes the font override from a page type so it follows the global font again.
    /// </summary>
    /// <typeparam name="TPage">The WPF page type whose override is removed.</typeparam>
    /// <returns><see langword="true"/> when an override was removed; otherwise, <see langword="false"/>.</returns>
    bool ClearOverrideFont<TPage>() where TPage : Page;

    /// <summary>
    /// Removes the font override from a page type selected at runtime.
    /// </summary>
    /// <param name="pageType">The closed, concrete WPF page type whose override is removed.</param>
    /// <returns><see langword="true"/> when an override was removed; otherwise, <see langword="false"/>.</returns>
    bool ClearOverrideFont(Type pageType);
}
