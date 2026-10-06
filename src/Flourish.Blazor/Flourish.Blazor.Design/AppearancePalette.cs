using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Validates configurable light seeds and selects fixed dark defaults without deriving colors.</summary>
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

    public static double Contrast(string first, string second)
    {
        var a = Luminance(Normalize(first));
        var b = Luminance(Normalize(second));
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }

    private static double Luminance(string color)
    {
        double Channel(int index)
        {
            var value = Convert.ToInt32(color.Substring(index, 2), 16) / 255d;
            return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4);
        }
        return .2126 * Channel(1) + .7152 * Channel(3) + .0722 * Channel(5);
    }

    public string CssVariables =>
        $"--f-primary-light:{Primary};--f-accent-light:{Accent};--f-primary-dark:{DarkPrimary};--f-accent-dark:{DarkAccent}";
}
