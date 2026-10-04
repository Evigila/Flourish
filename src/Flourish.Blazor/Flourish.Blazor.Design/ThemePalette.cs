using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor;

/// <summary>Resolves the configurable palette roles used by legacy theme hosts.</summary>
public sealed record ThemePalette(string Primary, string Accent, string DarkPrimary, string DarkAccent)
{
    public const string DefaultPrimary = AppearancePalette.DefaultPrimary;
    public const string DefaultAccent = AppearancePalette.DefaultAccent;
    public const string DefaultDarkPrimary = AppearancePalette.DefaultDarkPrimary;
    public const string DefaultDarkAccent = AppearancePalette.DefaultDarkAccent;

    public string CssVariables =>
        $"--f-primary-light:{Primary};--f-accent-light:{Accent};--f-primary-dark:{DarkPrimary};--f-accent-dark:{DarkAccent}";

    public static ThemePalette FromColors(string? primary, string? accent)
    {
        var resolvedPrimary = Normalize(primary, DefaultPrimary);
        var resolvedAccent = Normalize(accent, DefaultAccent);
        return new(resolvedPrimary, resolvedAccent,
            resolvedPrimary == DefaultPrimary ? DefaultDarkPrimary : resolvedPrimary,
            resolvedAccent == DefaultAccent ? DefaultDarkAccent : resolvedAccent);
    }

    private static string Normalize(string? value, string fallback)
    {
        if (value is null || value.Length != 7 || value[0] != '#'
            || !int.TryParse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            return fallback;
        return value.ToUpperInvariant();
    }
}
