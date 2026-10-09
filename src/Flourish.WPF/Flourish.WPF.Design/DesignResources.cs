using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ThemeMode = ArkheideSystem.Flourish.WPF.Abstract.ThemeMode;
using Microsoft.Win32;

namespace ArkheideSystem.Flourish.WPF;

/// <summary>Optional native paint resources; Framework owns control templates and geometry.</summary>
public static class DesignResources
{
    /// <summary>Creates a palette snapshot suitable for Application.Resources.MergedDictionaries.</summary>
    /// <remarks>Use Apply for a live scope that follows Windows appearance changes.</remarks>
    public static ResourceDictionary Load(ThemeMode mode = ThemeMode.System, string? primary = null, string? accent = null)
    {
        ValidateTheme(mode);
        var palette = AppearancePalette.Create(primary ?? AppearancePalette.DefaultPrimary, accent ?? AppearancePalette.DefaultAccent);
        var resources = new ResourceDictionary();
        Populate(resources, Resolve(mode), palette);
        return resources;
    }

    /// <summary>Applies live Design resources to one native visual scope.</summary>
    /// <remarks>The returned owner supports hot theme/color changes and removes its dictionary on disposal.</remarks>
    public static ThemeSession Apply(FrameworkElement scope, ThemeMode mode = ThemeMode.System, string? primary = null, string? accent = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        scope.Dispatcher.VerifyAccess();
        ValidateTheme(mode);
        var palette = AppearancePalette.Create(primary ?? AppearancePalette.DefaultPrimary, accent ?? AppearancePalette.DefaultAccent);
        return new ThemeSession(scope, mode, palette);
    }

    internal static void ValidateTheme(ThemeMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
    }

    internal static ThemeMode Resolve(ThemeMode mode)
    {
        if (mode != ThemeMode.System) return mode;
        try
        {
            var value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", null);
            if (value is int setting) return setting == 0 ? ThemeMode.Dark : ThemeMode.Light;
        }
        catch (Exception error) when (error is SecurityException or UnauthorizedAccessException or IOException)
        {
            // The system palette remains usable when registry access is unavailable.
        }
        var window = SystemColors.WindowColor;
        return .2126 * window.R + .7152 * window.G + .0722 * window.B < 128 ? ThemeMode.Dark : ThemeMode.Light;
    }

    internal static void Populate(ResourceDictionary resources, ThemeMode theme, AppearancePalette palette)
    {
        var highContrast = SystemParameters.HighContrast;
        var dark = theme == ThemeMode.Dark;
        var surface = dark ? "#263A30" : "#FFFFFF";
        var primary = dark ? palette.DarkPrimary : palette.Primary;
        var colors = new Dictionary<string, string>
        {
            ["Primary"] = primary,
            ["Accent"] = dark ? palette.DarkAccent : palette.Accent,
            ["Canvas"] = dark ? "#18231D" : "#F3F5F5",
            ["SurfaceAlternate"] = dark ? "#18231D" : "#F3F5F5",
            ["Surface"] = surface,
            ["SurfaceLight"] = "#FFFFFF",
            ["Text"] = dark ? "#E7EFEA" : "#112924",
            ["Muted"] = dark ? "#B8C9BF" : "#4F645D",
            ["Border"] = dark ? "#526F60" : "#9FAEA9",
            ["DisplayBoard"] = dark ? "#31483B" : "#E5E8EB",
            ["SurfaceHover"] = dark ? "#395A48" : "#C9DFDA",
            ["SurfacePressed"] = dark ? "#335141" : "#B5C9C4",
            ["PrimaryHover"] = dark ? "#D5E5DE" : "#2F5049",
            ["PrimaryPressed"] = dark ? "#C0CEC8" : "#2A4842",
            ["Danger"] = dark ? "#FFB4AB" : "#9D322D",
            ["DangerHover"] = dark ? "#8C3430" : "#7E2824",
            ["DangerPressed"] = dark ? "#7E2F2B" : "#712420",
            ["Information"] = dark ? "#8CB8FF" : "#1565C0",
            ["Warning"] = dark ? "#E5A047" : "#F2A33A",
            ["WarningInk"] = dark ? surface : "#112924",
            ["PrimaryInk"] = surface,
            ["AccentInk"] = surface,
            ["Chrome"] = dark ? "#18231D" : primary,
            ["ChromeHover"] = dark ? "#395A48" : "#2F5049",
            ["ChromePressed"] = dark ? "#335141" : "#2A4842",
            ["ChromeInk"] = dark ? "#E7EFEA" : surface,
            ["Overlay"] = primary
        };
        foreach (var pair in colors)
        {
            var color = (Color)ColorConverter.ConvertFromString(pair.Value);
            if (pair.Key == "Overlay") color.A = 102; // Blazor's modal backdrop uses Primary at 40%.
            resources["Flourish.Brush." + pair.Key] = Brush(highContrast ? ContrastColor(pair.Key) : color);
        }
        resources["Flourish.FontFamily"] = new FontFamily("Segoe UI");
        resources["Flourish.Motion.Reduced"] = !SystemParameters.ClientAreaAnimation || highContrast;
        resources["Flourish.Shadow.Control"] = Shadow(primary, 2, 6, highContrast);
        resources["Flourish.Shadow.Popup"] = Shadow(primary, 16, 48, highContrast);
        resources["Flourish.Shadow.Card"] = Shadow(primary, 18, 52, highContrast);
    }

    private static Color ContrastColor(string role) => role switch
    {
        "Primary" or "Accent" or "Danger" or "DangerHover" or "DangerPressed" or "PrimaryHover" or "PrimaryPressed" or "Chrome" or "ChromeHover" or "ChromePressed"
            => SystemColors.HighlightColor,
        "PrimaryInk" or "AccentInk" or "ChromeInk" => SystemColors.HighlightTextColor,
        "SurfaceHover" or "SurfacePressed" => SystemColors.ControlColor,
        "Text" or "Border" or "Information" or "WarningInk" => SystemColors.WindowTextColor,
        "Muted" => SystemColors.GrayTextColor,
        // An opaque system canvas keeps the modal background readable in high contrast.
        _ => SystemColors.WindowColor
    };

    private static SolidColorBrush Brush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static DropShadowEffect Shadow(string primary, double depth, double blur, bool highContrast)
    {
        var shadow = new DropShadowEffect
        {
            Color = (Color)ColorConverter.ConvertFromString(primary),
            Direction = 270,
            ShadowDepth = depth,
            BlurRadius = blur,
            Opacity = highContrast ? 0 : .16,
            RenderingBias = RenderingBias.Quality
        };
        shadow.Freeze();
        return shadow;
    }
}
