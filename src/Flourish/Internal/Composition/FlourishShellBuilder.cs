using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishShellBuilder(FlourishShellOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishShellBuilder
{
    public IFlourishShellBuilder UseTitleBar(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsTitlebarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseMultiProject(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsMultiProjectEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseNavigation(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsNavigationPanelEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseCenterContent(
        bool enabled,
        double contentWidth
    )
    {
        ThrowIfFrozen();
        ValidatePositiveFinite(contentWidth, nameof(contentWidth));
        options.IsCenterContentEnabled = enabled;
        options.CenterContentWidth = contentWidth;
        return this;
    }

    public IFlourishShellBuilder UseDynamicToolbar(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsDynamicToolbarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseTips(bool enabled = true, int delay = 200)
    {
        ThrowIfFrozen();
        if (delay < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay),
                delay,
                "Tooltip delay cannot be negative."
            );
        }

        options.Tips.InitialShowDelayMilliseconds = delay;
        options.Tips.SpawnableMargin = 5;
        options.IsTipsEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseMotion(bool enabled = true)
    {
        ThrowIfFrozen();
        options.Motion.IsEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseMaterialEffect(
        bool enabled = true,
        MaterialEffect effect = MaterialEffect.Mica
    )
    {
        ThrowIfFrozen();
        ValidateEnum(effect, nameof(effect));
        options.MaterialEffect = effect;
        options.IsMaterialEffectEnabled = enabled && effect != MaterialEffect.None;
        return this;
    }

    public IFlourishShellBuilder UseThemeColors(
        bool enabled,
        FlourishThemeColors colors
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(colors);
        options.ThemeColors = enabled ? colors : null;
        return this;
    }

    public IFlourishShellBuilder UseCornerRadius(bool enabled, double radius = 6)
    {
        ThrowIfFrozen();
        ValidateNonNegativeFinite(radius, nameof(radius));
        options.CornerRadius = enabled ? radius : null;
        return this;
    }

    public IFlourishShellBuilder UseSmoothScroll(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsSmoothScrollingEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder InitGlobalFont(
        string fontFamily = "Microsoft Yahei",
        double smallFontSize = 12,
        double standardFontSize = 14,
        double iconFontSize = 16,
        double largeFontSize = 16,
        double extraLargeFontSize = 24,
        double headerSizeFontSize = 32
    )
    {
        ThrowIfFrozen();
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
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
        return this;
    }

    public IFlourishShellBuilder InitOverrideFont<TPage>(
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
        ThrowIfFrozen();
        ValidatePageType(typeof(TPage), nameof(TPage));
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
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

        options.PageFontOverridesByPageType[typeof(TPage)] =
            new FlourishPageFontOverride(
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

    public IFlourishShellBuilder UseStatusBar(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsStatusBarEnabled = enabled;
        return this;
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than 0."
            );
        }
    }

    private static void ValidateNonNegativeFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be finite and non-negative."
            );
        }
    }

    private static void ValidateNullableSize(double? value, string parameterName)
    {
        if (value is { } size)
        {
            ValidatePositiveFinite(size, parameterName);
        }
    }

    private static void ValidateFontScale(
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        ValidatePositiveFinite(smallFontSize, nameof(smallFontSize));
        ValidatePositiveFinite(standardFontSize, nameof(standardFontSize));
        ValidatePositiveFinite(iconFontSize, nameof(iconFontSize));
        ValidatePositiveFinite(largeFontSize, nameof(largeFontSize));
        ValidatePositiveFinite(extraLargeFontSize, nameof(extraLargeFontSize));
        ValidatePositiveFinite(headerSizeFontSize, nameof(headerSizeFontSize));
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
    }

    private static void ValidatePageType(Type pageType, string parameterName)
    {
        if (pageType.IsAbstract || pageType.ContainsGenericParameters)
        {
            throw new ArgumentException(
                $"{pageType.FullName} must be a closed, concrete System.Windows.Controls.Page type.",
                parameterName
            );
        }
    }
}
