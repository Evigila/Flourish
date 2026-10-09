using System.Globalization;

namespace ArkheideSystem.Flourish.WPF;

/// <summary>Validated brand seeds with the same fixed dark defaults as Flourish.Blazor.</summary>
public sealed record AppearancePalette(string Primary, string Accent, string DarkPrimary, string DarkAccent)
{
    public const string DefaultPrimary = "#153A32";
    public const string DefaultAccent = "#16745F";
    public const string DefaultDarkPrimary = "#BBD7C9";
    public const string DefaultDarkAccent = "#75CBB2";

    public static AppearancePalette Create(string primary, string accent)
    {
        primary = Normalize(primary);
        accent = Normalize(accent);
        return new(primary, accent,
            primary == DefaultPrimary ? DefaultDarkPrimary : primary,
            accent == DefaultAccent ? DefaultDarkAccent : accent);
    }

    private static string Normalize(string color)
    {
        if (color is null || color.Length != 7 || color[0] != '#'
            || !int.TryParse(color.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            throw new ArgumentException("Colors must use #RRGGBB.", nameof(color));
        return color.ToUpperInvariant();
    }
}
