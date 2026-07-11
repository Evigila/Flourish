namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a font override applied to one WPF page type.
/// </summary>
public sealed record FlourishPageFontOverride
{
    /// <summary>
    /// Initializes a page-specific font override.
    /// </summary>
    /// <param name="fontFamily">The page-specific font family name.</param>
    /// <param name="fontSize">
    /// The page-specific base font size, or <see langword="null"/> to continue following the global size.
    /// </param>
    public FlourishPageFontOverride(string fontFamily, double? fontSize)
    {
        if (string.IsNullOrWhiteSpace(fontFamily))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(fontFamily));
        }

        if (
            fontSize is { } size
            && (double.IsNaN(size) || double.IsInfinity(size) || size <= 0)
        )
        {
            throw new ArgumentOutOfRangeException(
                nameof(fontSize),
                size,
                "Value must be a positive finite number."
            );
        }

        FontFamily = fontFamily;
        FontSize = fontSize;
    }

    /// <summary>
    /// Gets the page-specific font family name.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets the page-specific base font size, or <see langword="null"/> when the page follows the global size.
    /// </summary>
    public double? FontSize { get; }
}
