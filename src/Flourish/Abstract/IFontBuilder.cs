using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Configures the global and page-specific font scales.</summary>
public interface IFontBuilder
{
    /// <summary>Sets the global font family and size scale.</summary>
    IFontBuilder SetFont(
        string fontFamily = "Microsoft Yahei",
        double smallFontSize = 11,
        double standardFontSize = 13,
        double iconFontSize = 14,
        double largeFontSize = 14,
        double extraLargeFontSize = 18,
        double headerSizeFontSize = 25,
        bool usePersistedPreference = true
    );

    /// <summary>Sets a page-specific font override.</summary>
    IFontBuilder SetOverrideFont<TPage>(
        string fontFamily,
        double? smallFontSize = null,
        double? standardFontSize = null,
        double? iconFontSize = null,
        double? largeFontSize = null,
        double? extraLargeFontSize = null,
        double? headerSizeFontSize = null
    )
        where TPage : Page;
}
