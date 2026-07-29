using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>Provides typed conveniences for runtime font configuration.</summary>
public static class FontServiceExtensions
{
    /// <summary>Sets or replaces the font override for a page type.</summary>
    public static void SetOverrideFont<TPage>(
        this IFontService service,
        string fontFamily,
        double? smallFontSize,
        double? standardFontSize,
        double? iconFontSize,
        double? largeFontSize,
        double? extraLargeFontSize,
        double? headerSizeFontSize
    )
        where TPage : Page
    {
        ArgumentNullException.ThrowIfNull(service);
        service.SetOverrideFont(
            typeof(TPage),
            fontFamily,
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );
    }

    /// <summary>Removes the font override for a page type.</summary>
    public static bool RemoveOverrideFont<TPage>(this IFontService service)
        where TPage : Page
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.RemoveOverrideFont(typeof(TPage));
    }
}
