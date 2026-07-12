using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

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
    /// Gets the current base font size.
    /// </summary>
    double FontSize { get; }

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
    /// Changes the text font family and base size together.
    /// </summary>
    void SetFont(string fontFamily, double fontSize);

    /// <summary>
    /// Changes the text font family.
    /// </summary>
    void SetFontFamily(string fontFamily);

    /// <summary>
    /// Changes the base font size.
    /// </summary>
    void SetFontSize(double fontSize);

    /// <summary>
    /// Changes the icon font family.
    /// </summary>
    void SetIconFontFamily(string fontFamily);

    /// <summary>
    /// Sets or replaces the font override for a page type.
    /// </summary>
    /// <typeparam name="TPage">The WPF page type that receives the override.</typeparam>
    /// <param name="fontFamily">The page-specific font family name.</param>
    /// <param name="fontSize">
    /// The page-specific base font size, or <see langword="null"/> to continue following the global size.
    /// </param>
    void SetOverrideFont<TPage>(string fontFamily, double? fontSize = null)
        where TPage : Page;

    /// <summary>
    /// Sets or replaces the font override for a page type selected at runtime.
    /// </summary>
    /// <param name="pageType">The closed, concrete WPF page type that receives the override.</param>
    /// <param name="fontFamily">The page-specific font family name.</param>
    /// <param name="fontSize">
    /// The page-specific base font size, or <see langword="null"/> to continue following the global size.
    /// </param>
    void SetOverrideFont(Type pageType, string fontFamily, double? fontSize = null);

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
