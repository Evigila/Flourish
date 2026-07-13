using Color = System.Windows.Media.Color;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Defines the application brand colors used to derive Flourish theme resources.
/// </summary>
public sealed record FlourishThemeColors
{
    /// <summary>
    /// Initializes an opaque application color scheme.
    /// </summary>
    /// <param name="primary">The primary action and brand color.</param>
    /// <param name="secondary">The supporting brand color.</param>
    /// <param name="accent">The color used for highlights and emphasized details.</param>
    public FlourishThemeColors(Color primary, Color secondary, Color accent)
    {
        Primary = ValidateOpaque(primary, nameof(primary));
        Secondary = ValidateOpaque(secondary, nameof(secondary));
        Accent = ValidateOpaque(accent, nameof(accent));
    }

    /// <summary>Gets the primary action and brand color.</summary>
    public Color Primary { get; }

    /// <summary>Gets the supporting brand color.</summary>
    public Color Secondary { get; }

    /// <summary>Gets the color used for highlights and emphasized details.</summary>
    public Color Accent { get; }

    private static Color ValidateOpaque(Color color, string parameterName)
    {
        if (color.A != byte.MaxValue)
        {
            throw new ArgumentException(
                "Theme colors must be opaque so Flourish can preserve predictable contrast.",
                parameterName
            );
        }

        return color;
    }
}
