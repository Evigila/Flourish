using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor;

public sealed record ThemePalette(
    string Primary,
    string Accent,
    string PrimaryInk,
    string AccentInk,
    string AccentText)
{
    public const string DefaultPrimary = "#153A32";
    public const string DefaultAccent = "#16745F";

    public string CssVariables => string.Create(
        CultureInfo.InvariantCulture,
        $"--surface-theme-primary:{Primary};--surface-theme-accent:{Accent};--surface-theme-primary-ink:{PrimaryInk};--surface-theme-accent-ink:{AccentInk};--surface-theme-accent-text:{AccentText};--surface-theme-focus:{AccentText};--focus:{AccentText}");

    public static ThemePalette FromColors(string? primary, string? accent)
    {
        var resolvedPrimary = Normalize(primary, DefaultPrimary);
        var resolvedAccent = Normalize(accent, DefaultAccent);
        return new(
            resolvedPrimary,
            resolvedAccent,
            ContrastingInk(resolvedPrimary),
            ContrastingInk(resolvedAccent),
            AccessibleAccentText(resolvedAccent));
    }

    private static string Normalize(string? value, string fallback)
    {
        if (value is null || value.Length != 7 || value[0] != '#'
            || !int.TryParse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
        {
            return fallback;
        }

        return value.ToUpperInvariant();
    }

    private static string ContrastingInk(string color)
    {
        var luminance = Luminance(color);
        var whiteContrast = 1.05 / (luminance + .05);
        var darkContrast = (luminance + .05) / .057;
        return whiteContrast >= darkContrast ? "#FFFFFF" : "#10241F";
    }

    private static string AccessibleAccentText(string color)
    {
        if (ContrastRatio(color, "#FFFFFF") >= 4.5)
        {
            return color;
        }

        var red = Convert.ToInt32(color.Substring(1, 2), 16);
        var green = Convert.ToInt32(color.Substring(3, 2), 16);
        var blue = Convert.ToInt32(color.Substring(5, 2), 16);
        for (var percentage = 95; percentage >= 0; percentage -= 5)
        {
            var candidate = $"#{red * percentage / 100:X2}{green * percentage / 100:X2}{blue * percentage / 100:X2}";
            if (ContrastRatio(candidate, "#FFFFFF") >= 4.5)
            {
                return candidate;
            }
        }

        return "#000000";
    }

    private static double ContrastRatio(string first, string second)
    {
        var firstLuminance = Luminance(first);
        var secondLuminance = Luminance(second);
        return (Math.Max(firstLuminance, secondLuminance) + .05)
            / (Math.Min(firstLuminance, secondLuminance) + .05);
    }

    private static double Luminance(string color)
    {
        var red = Convert.ToInt32(color.Substring(1, 2), 16) / 255d;
        var green = Convert.ToInt32(color.Substring(3, 2), 16) / 255d;
        var blue = Convert.ToInt32(color.Substring(5, 2), 16) / 255d;
        return .2126 * Linear(red) + .7152 * Linear(green) + .0722 * Linear(blue);
    }

    private static double Linear(double channel) =>
        channel <= .04045 ? channel / 12.92 : Math.Pow((channel + .055) / 1.055, 2.4);
}
