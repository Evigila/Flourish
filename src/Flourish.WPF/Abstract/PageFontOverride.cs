using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a font override applied to one WPF page type.
/// </summary>
public sealed record PageFontOverride
{
    /// <summary>
    /// Initializes a page-specific font and explicit text and icon size override.
    /// </summary>
    /// <param name="fontFamily">The page-specific font family name.</param>
    /// <param name="smallFontSize">The page-specific small size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="standardFontSize">The page-specific standard size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="iconFontSize">The page-specific standard icon size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="largeFontSize">The page-specific large size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="extraLargeFontSize">The page-specific extra-large size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="headerSizeFontSize">The page-specific header size, or <see langword="null"/> to follow the global size.</param>
    public PageFontOverride(
        string fontFamily,
        double? smallFontSize,
        double? standardFontSize,
        double? iconFontSize,
        double? largeFontSize,
        double? extraLargeFontSize,
        double? headerSizeFontSize
    )
    {
        if (string.IsNullOrWhiteSpace(fontFamily))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(fontFamily));
        }

        ValidateNullableSize(smallFontSize, nameof(smallFontSize));
        ValidateNullableSize(standardFontSize, nameof(standardFontSize));
        ValidateNullableSize(iconFontSize, nameof(iconFontSize));
        ValidateNullableSize(largeFontSize, nameof(largeFontSize));
        ValidateNullableSize(extraLargeFontSize, nameof(extraLargeFontSize));
        ValidateNullableSize(headerSizeFontSize, nameof(headerSizeFontSize));

        FontFamily = fontFamily;
        SmallFontSize = smallFontSize;
        StandardFontSize = standardFontSize;
        IconFontSize = iconFontSize;
        LargeFontSize = largeFontSize;
        ExtraLargeFontSize = extraLargeFontSize;
        HeaderSizeFontSize = headerSizeFontSize;
    }

    /// <summary>
    /// Gets the page-specific font family name.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets the page-specific small size, or <see langword="null"/> when the page follows the global size.
    /// </summary>
    public double? SmallFontSize { get; }

    /// <summary>
    /// Gets the page-specific standard size, or <see langword="null"/> when the page follows the global size.
    /// </summary>
    public double? StandardFontSize { get; }

    /// <summary>
    /// Gets the page-specific standard icon size, or <see langword="null"/> when the page follows the global size.
    /// </summary>
    public double? IconFontSize { get; }

    /// <summary>
    /// Gets the page-specific large size, or <see langword="null"/> when the page follows the global size.
    /// </summary>
    public double? LargeFontSize { get; }

    /// <summary>
    /// Gets the page-specific extra-large size, or <see langword="null"/> when the page follows the global size.
    /// </summary>
    public double? ExtraLargeFontSize { get; }

    /// <summary>
    /// Gets the page-specific header size, or <see langword="null"/> when the page follows the global size.
    /// </summary>
    public double? HeaderSizeFontSize { get; }

    private static void ValidateNullableSize(double? value, string parameterName)
    {
        if (value is { } size && (!double.IsFinite(size) || size <= 0))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                size,
                "Value must be a positive finite number."
            );
        }
    }

}
