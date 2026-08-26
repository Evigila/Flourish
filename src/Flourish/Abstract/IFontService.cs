using System;
using System.Collections.Generic;

using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls global Flourish text and icon fonts and page-specific text overrides at runtime.
/// </summary>
public interface IFontService
{
    /// <summary>
    /// Gets an immutable snapshot of the current global fonts and page-specific overrides.
    /// </summary>
    FlourishFontState Current { get; }

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

    /// <summary>Sets or replaces the font override for a page type.</summary>
    void SetOverrideFont<TPage>(
        string fontFamily,
        double? smallFontSize,
        double? standardFontSize,
        double? iconFontSize,
        double? largeFontSize,
        double? extraLargeFontSize,
        double? headerSizeFontSize
    )
        where TPage : Page =>
        SetOverrideFont(
            typeof(TPage),
            fontFamily,
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );

    /// <summary>Removes the font override from a page type selected at runtime.</summary>
    /// <param name="pageType">The closed, concrete WPF page type whose override is removed.</param>
    /// <returns><see langword="true"/> when an override was removed; otherwise, <see langword="false"/>.</returns>
    bool RemoveOverrideFont(Type pageType);

    /// <summary>Removes the font override from a page type.</summary>
    bool RemoveOverrideFont<TPage>()
        where TPage : Page => RemoveOverrideFont(typeof(TPage));
}

/// <summary>Represents an immutable snapshot of the active runtime font configuration.</summary>
public sealed record FlourishFontState(
    string FontFamily,
    string IconFontFamily,
    double SmallFontSize,
    double StandardFontSize,
    double IconFontSize,
    double LargeFontSize,
    double ExtraLargeFontSize,
    double HeaderSizeFontSize,
    IReadOnlyDictionary<Type, FlourishPageFontOverride> PageOverrides
);
