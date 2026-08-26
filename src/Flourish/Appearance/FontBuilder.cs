using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Appearance;

internal sealed class FontBuilder(FlourishAppearanceOptions options)
    : FlourishBuilderMutationGuard,
        IFontBuilder
{
    public IFontBuilder SetFont(
        string fontFamily = "Microsoft Yahei",
        double smallFontSize = 12,
        double standardFontSize = 14,
        double iconFontSize = 22,
        double largeFontSize = 16,
        double extraLargeFontSize = 24,
        double headerSizeFontSize = 32,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        fontFamily = BuilderValidation.NotBlank(fontFamily, nameof(fontFamily));
        ValidateFontScale(
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );
        options.FontFamily = fontFamily;
        options.FontSizeSmall = smallFontSize;
        options.FontSizeStandard = standardFontSize;
        options.FontSizeIcon = iconFontSize;
        options.FontSizeLarge = largeFontSize;
        options.FontSizeExtraLarge = extraLargeFontSize;
        options.FontSizeHeaderSize = headerSizeFontSize;
        options.UsePersistedFont = usePersistedPreference;
        return this;
    }

    public IFontBuilder SetOverrideFont<TPage>(
        string fontFamily,
        double? smallFontSize = null,
        double? standardFontSize = null,
        double? iconFontSize = null,
        double? largeFontSize = null,
        double? extraLargeFontSize = null,
        double? headerSizeFontSize = null
    )
        where TPage : Page
    {
        ThrowIfFrozen();
        BuilderValidation.PageType(typeof(TPage), nameof(TPage));
        fontFamily = BuilderValidation.NotBlank(fontFamily, nameof(fontFamily));
        ValidateNullableSize(smallFontSize, nameof(smallFontSize));
        ValidateNullableSize(standardFontSize, nameof(standardFontSize));
        ValidateNullableSize(iconFontSize, nameof(iconFontSize));
        ValidateNullableSize(largeFontSize, nameof(largeFontSize));
        ValidateNullableSize(extraLargeFontSize, nameof(extraLargeFontSize));
        ValidateNullableSize(headerSizeFontSize, nameof(headerSizeFontSize));
        ValidateFontScale(
            smallFontSize ?? options.FontSizeSmall,
            standardFontSize ?? options.FontSizeStandard,
            iconFontSize ?? options.FontSizeIcon,
            largeFontSize ?? options.FontSizeLarge,
            extraLargeFontSize ?? options.FontSizeExtraLarge,
            headerSizeFontSize ?? options.FontSizeHeaderSize
        );

        options.PageFontOverridesByPageType[typeof(TPage)] = new FlourishPageFontOverride(
            fontFamily,
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );
        return this;
    }

    private static void ValidateNullableSize(double? value, string parameterName)
    {
        if (value is { } size)
        {
            BuilderValidation.PositiveFinite(size, parameterName);
        }
    }

    private static void ValidateFontScale(params double[] values)
    {
        var names = new[]
        {
            "smallFontSize",
            "standardFontSize",
            "iconFontSize",
            "largeFontSize",
            "extraLargeFontSize",
            "headerSizeFontSize",
        };
        for (var index = 0; index < values.Length; index++)
        {
            BuilderValidation.PositiveFinite(values[index], names[index]);
        }
    }
}
