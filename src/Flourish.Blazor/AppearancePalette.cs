using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Derives contrasting text, focus and hover colors from explicit #RRGGBB tokens.</summary>
public sealed record AppearancePalette(string Primary, string Accent, string PrimaryInk, string AccentInk,
    string AccentText, string DarkAccentText, string AccentHover)
{
    public static AppearancePalette Create(string primary, string accent)
    {
        primary = Normalize(primary); accent = Normalize(accent);
        return new(primary, accent, Ink(primary), Ink(accent), Accessible(accent, "#F1F5F3", false),
            Accessible(accent, "#394148", true), Mix(accent, Ink(accent) == "#FFFFFF" ? "#000000" : "#FFFFFF", .16));
    }
    private static string Normalize(string color)
    {
        if (color is null || color.Length != 7 || color[0] != '#' || !int.TryParse(color.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            throw new ArgumentException("Colors must use #RRGGBB.", nameof(color));
        return color.ToUpperInvariant();
    }
    private static string Ink(string background)
    {
        if (Contrast(background, "#FFFFFF") >= 4.5) return "#FFFFFF";
        return Contrast(background, "#10241F") >= 4.5 ? "#10241F" : "#000000";
    }
    private static string Accessible(string color, string background, bool lighten)
    {
        for (var step = 0; step <= 20; step++)
        {
            var candidate = Mix(color, lighten ? "#FFFFFF" : "#000000", step / 20d);
            if (Contrast(candidate, background) >= 4.5) return candidate;
        }
        return lighten ? "#FFFFFF" : "#000000";
    }
    private static string Mix(string color, string other, double amount)
    {
        var result = "#";
        for (var index = 1; index < 7; index += 2)
        {
            var channel = Convert.ToInt32(color.Substring(index, 2), 16);
            var target = Convert.ToInt32(other.Substring(index, 2), 16);
            result += ((int)Math.Round(channel + (target - channel) * amount)).ToString("X2", CultureInfo.InvariantCulture);
        }
        return result;
    }
    public static double Contrast(string first, string second)
    {
        var a = Luminance(Normalize(first)); var b = Luminance(Normalize(second));
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }
    private static double Luminance(string color)
    {
        double Channel(int index) { var value = Convert.ToInt32(color.Substring(index, 2), 16) / 255d; return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4); }
        return .2126 * Channel(1) + .7152 * Channel(3) + .0722 * Channel(5);
    }
    internal string CssVariables => $"--f-primary:{Primary};--f-accent:{Accent};--f-primary-ink:{PrimaryInk};--f-accent-ink:{AccentInk};--f-accent-text-light:{AccentText};--f-accent-text-dark:{DarkAccentText};--f-accent-hover:{AccentHover}";
}