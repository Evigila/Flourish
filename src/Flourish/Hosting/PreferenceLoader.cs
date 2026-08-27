using System;
using System.Globalization;
using System.Linq;
using System.Windows;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Navigation;
using ArkheideSystem.Flourish.Windowing;
using Microsoft.Extensions.Configuration;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;

namespace ArkheideSystem.Flourish.Hosting;

internal static class PreferenceLoader
{
    public static void Apply(
        IConfiguration configuration,
        ApplicationDataOptions data,
        ApplicationOptions applicationOptions,
        MaterialEffectPlatform? materialPlatform = null
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(applicationOptions);

        if (
            data.UsePersistedLocale
            && configuration[PreferenceConfigurationKeys.Locale] is { } locale
            && LocalizationService.TryNormalizeLocale(locale, out var normalizedLocale)
        )
        {
            data.Locale = normalizedLocale;
        }

        ApplyWindow(configuration, applicationOptions.Window);
        ApplyNavigation(configuration, applicationOptions.Navigation);
        ApplyMotion(configuration, applicationOptions.Motion);
        ApplyAppearance(
            configuration,
            applicationOptions.Appearance,
            materialPlatform ?? MaterialEffectPlatform.Current
        );
        ApplyFontAndLayout(
            configuration,
            applicationOptions.Appearance,
            applicationOptions.Layout
        );

        if (
            applicationOptions.Layout.UsePersistedSmoothScroll
            && TryGetBoolean(configuration, PreferenceConfigurationKeys.SmoothScrolling, out var smooth)
        )
        {
            applicationOptions.Layout.IsSmoothScrollingEnabled = smooth;
        }

        if (
            applicationOptions.Profile.UsePersistedNameOrder
            && TryGetEnum(configuration, PreferenceConfigurationKeys.NameOrder, out NameOrder nameOrder)
        )
        {
            applicationOptions.Profile.NameOrder = nameOrder;
        }
    }

    private static void ApplyWindow(
        IConfiguration configuration,
        WindowOptions options
    )
    {
        if (
            options.UsePersistedWindowSize
            && TryGetDouble(
                configuration,
                $"{PreferenceConfigurationKeys.WindowSize}:Width",
                out var width
            )
            && TryGetDouble(
                configuration,
                $"{PreferenceConfigurationKeys.WindowSize}:Height",
                out var height
            )
            && IsPositiveFinite(width)
            && IsPositiveFinite(height)
        )
        {
            options.WindowWidth = Math.Clamp(width, options.WindowMinWidth, options.WindowMaxWidth);
            options.WindowHeight = Math.Clamp(height, options.WindowMinHeight, options.WindowMaxHeight);
        }

        if (
            options.UsePersistedWindowPosition
            && TryGetDouble(
                configuration,
                $"{PreferenceConfigurationKeys.WindowPosition}:Left",
                out var left
            )
            && TryGetDouble(
                configuration,
                $"{PreferenceConfigurationKeys.WindowPosition}:Top",
                out var top
            )
            && double.IsFinite(left)
            && double.IsFinite(top)
        )
        {
            options.WindowLeft = left;
            options.WindowTop = top;
            options.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        if (
            options.UsePersistedWindowState
            && TryGetEnum(configuration, PreferenceConfigurationKeys.WindowState, out WindowState state)
            && state is WindowState.Normal or WindowState.Maximized
        )
        {
            options.WindowState = state;
        }

        if (
            options.UsePersistedWindowTopmost
            && TryGetBoolean(configuration, PreferenceConfigurationKeys.WindowTopmost, out var topmost)
        )
        {
            options.WindowTopmost = topmost;
        }

        if (
            options.UsePersistedTrayExit
            && configuration[PreferenceConfigurationKeys.WindowCloseBehavior] is { } closeBehavior
            && Enum.TryParse(
                closeBehavior,
                ignoreCase: true,
                out WindowCloseBehavior parsedCloseBehavior
            )
            && Enum.IsDefined(parsedCloseBehavior)
        )
        {
            options.IsTrayExitEnabled = parsedCloseBehavior == WindowCloseBehavior.MinimizeToTray;
        }
    }

    private static void ApplyNavigation(
        IConfiguration configuration,
        NavigationOptions options
    )
    {
        if (
            options.UsePersistedNavigationDirection
            && TryGetEnum(
                configuration,
                $"{PreferenceConfigurationKeys.Navigation}:Direction",
                out NavigationPanelDirection direction
            )
        )
        {
            options.NavigationPanelDirection = direction;
        }

        if (
            options.UsePersistedNavigationOpenState
            && TryGetBoolean(
                configuration,
                $"{PreferenceConfigurationKeys.Navigation}:IsOpen",
                out var isOpen
            )
        )
        {
            options.IsNavigationPanelInitiallyOpen = isOpen;
        }

        if (
            options.UsePersistedNavigationWidth
            && TryGetDouble(
                configuration,
                $"{PreferenceConfigurationKeys.Navigation}:OpenWidth",
                out var openWidth
            )
            && double.IsFinite(openWidth)
        )
        {
            options.OpenPaneWidth = Math.Clamp(
                openWidth,
                options.NavigationPaneMinWidth,
                options.NavigationPaneMaxWidth
            );
            options.ClosedPaneWidth = Math.Min(options.ClosedPaneWidth, options.OpenPaneWidth);
        }

        if (
            options.UsePersistedLastNavigation
            && configuration[$"{PreferenceConfigurationKeys.Navigation}:LastKey"] is { } lastKey
            && options.InitialNavigationRoutes.FirstOrDefault(route =>
                string.Equals(route.NavigationKey, lastKey, StringComparison.Ordinal)
            )
                is { } route
        )
        {
            options.InitialNavigationKey = route.NavigationKey;
            options.InitialNavigationPageType = route.PageType;
        }
    }

    private static void ApplyMotion(
        IConfiguration configuration,
        MotionOptions motion
    )
    {
        if (
            motion.UsePersistedMotion
            && TryGetBoolean(
                configuration,
                $"{PreferenceConfigurationKeys.Motion}:Enabled",
                out var enabled
            )
        )
        {
            motion.IsEnabled = enabled;
        }

        if (
            motion.UsePersistedPageTransition
            && TryGetEnum(
                configuration,
                $"{PreferenceConfigurationKeys.Motion}:PageTransition:Transition",
                out PageTransition pageTransition
            )
            && TryGetDuration(
                configuration,
                $"{PreferenceConfigurationKeys.Motion}:PageTransition:DurationMilliseconds",
                out var pageDuration
            )
        )
        {
            motion.PageTransition = pageTransition;
            motion.PageTransitionDuration = pageDuration;
        }

        if (
            motion.UsePersistedNavigationPanelTransition
            && TryGetEnum(
                configuration,
                $"{PreferenceConfigurationKeys.Motion}:NavigationPanelTransition:Transition",
                out NavigationPanelTransition navigationTransition
            )
            && TryGetDuration(
                configuration,
                $"{PreferenceConfigurationKeys.Motion}:NavigationPanelTransition:DurationMilliseconds",
                out var navigationDuration
            )
        )
        {
            motion.NavigationPanelTransition = navigationTransition;
            motion.NavigationPanelTransitionDuration = navigationDuration;
        }

        if (
            motion.UsePersistedHoverReveal
            && TryGetBoolean(
                configuration,
                $"{PreferenceConfigurationKeys.Motion}:HoverReveal:Enabled",
                out var hoverEnabled
            )
            && TryGetDuration(
                configuration,
                $"{PreferenceConfigurationKeys.Motion}:HoverReveal:DurationMilliseconds",
                out var hoverDuration
            )
        )
        {
            motion.IsHoverRevealEnabled = hoverEnabled;
            motion.HoverRevealAnimationDuration = hoverDuration;
        }

        if (
            motion.UsePersistedReducedMotion
            && TryGetBoolean(
                configuration,
                $"{PreferenceConfigurationKeys.Motion}:RespectSystemReducedMotion",
                out var reducedMotion
            )
        )
        {
            motion.RespectSystemReducedMotion = reducedMotion;
        }
    }

    private static void ApplyAppearance(
        IConfiguration configuration,
        AppearanceOptions options,
        MaterialEffectPlatform materialPlatform
    )
    {
        if (
            options.UsePersistedMaterialEffect
            && TryGetBoolean(
                configuration,
                $"{PreferenceConfigurationKeys.Material}:Enabled",
                out var materialEnabled
            )
            && TryGetEnum(
                configuration,
                $"{PreferenceConfigurationKeys.Material}:Effect",
                out MaterialEffect material
            )
        )
        {
            options.MaterialEffect =
                materialEnabled && !materialPlatform.IsSupported(material)
                    ? MaterialEffect.Auto
                    : material;
            options.IsMaterialEffectEnabled =
                materialEnabled && options.MaterialEffect != MaterialEffect.None;
        }

        if (
            options.UsePersistedThemeColors
            && TryGetBoolean(
                configuration,
                $"{PreferenceConfigurationKeys.ThemeColors}:Enabled",
                out var colorsEnabled
            )
        )
        {
            if (!colorsEnabled)
            {
                options.ThemeColors = null;
            }
            else if (
                TryGetColor(
                    configuration,
                    $"{PreferenceConfigurationKeys.ThemeColors}:Primary",
                    out var primary
                )
                && TryGetColor(
                    configuration,
                    $"{PreferenceConfigurationKeys.ThemeColors}:Secondary",
                    out var secondary
                )
                && TryGetColor(
                    configuration,
                    $"{PreferenceConfigurationKeys.ThemeColors}:Accent",
                    out var accent
                )
                && primary.A == byte.MaxValue
                && secondary.A == byte.MaxValue
                && accent.A == byte.MaxValue
            )
            {
                options.ThemeColors = new ThemeColors(primary, secondary, accent);
            }
        }

        if (
            options.UsePersistedCornerRadius
            && TryGetBoolean(
                configuration,
                $"{PreferenceConfigurationKeys.CornerRadius}:Enabled",
                out var radiusEnabled
            )
        )
        {
            if (!radiusEnabled)
            {
                options.CornerRadius = null;
            }
            else if (
                TryGetDouble(
                    configuration,
                    $"{PreferenceConfigurationKeys.CornerRadius}:Value",
                    out var radius
                )
                && double.IsFinite(radius)
                && radius >= 0
            )
            {
                options.CornerRadius = radius;
            }
        }
    }

    private static void ApplyFontAndLayout(
        IConfiguration configuration,
        AppearanceOptions appearance,
        LayoutOptions layout
    )
    {
        if (appearance.UsePersistedFont)
        {
            var prefix = PreferenceConfigurationKeys.Font;
            var family = configuration[$"{prefix}:Family"];
            var iconFamily = configuration[$"{prefix}:IconFamily"];
            var hasStandardIcon = TryGetDouble(
                configuration,
                $"{prefix}:StandardIcon",
                out var standardIcon
            );
            var hasLegacyIcon = TryGetDouble(
                configuration,
                $"{prefix}:Icon",
                out var legacyIcon
            );
            var icon = hasStandardIcon
                ? standardIcon
                : legacyIcon == 22d
                    ? 14d
                    : legacyIcon;
            if (
                !string.IsNullOrWhiteSpace(family)
                && !string.IsNullOrWhiteSpace(iconFamily)
                && TryGetDouble(configuration, $"{prefix}:Small", out var small)
                && TryGetDouble(configuration, $"{prefix}:Standard", out var standard)
                && (hasStandardIcon || hasLegacyIcon)
                && TryGetDouble(configuration, $"{prefix}:Large", out var large)
                && TryGetDouble(configuration, $"{prefix}:ExtraLarge", out var extraLarge)
                && TryGetDouble(configuration, $"{prefix}:Header", out var header)
                && new[] { small, standard, icon, large, extraLarge, header }.All(IsPositiveFinite)
            )
            {
                appearance.FontFamily = family.Trim();
                appearance.IconFontFamily = iconFamily.Trim();
                appearance.FontSizeSmall = small;
                appearance.FontSizeStandard = standard;
                appearance.FontSizeIcon = icon;
                appearance.FontSizeLarge = large;
                appearance.FontSizeExtraLarge = extraLarge;
                appearance.FontSizeHeaderSize = header;
            }
        }

        if (
            layout.UsePersistedContentLayout
            && TryGetBoolean(
                configuration,
                $"{PreferenceConfigurationKeys.ContentLayout}:Enabled",
                out var centered
            )
            && TryGetDouble(
                configuration,
                $"{PreferenceConfigurationKeys.ContentLayout}:Width",
                out var contentWidth
            )
            && IsPositiveFinite(contentWidth)
        )
        {
            layout.IsCenterContentEnabled = centered;
            layout.CenterContentWidth = contentWidth;
        }
    }

    private static bool TryGetBoolean(IConfiguration configuration, string key, out bool value) =>
        bool.TryParse(configuration[key], out value);

    private static bool TryGetDouble(IConfiguration configuration, string key, out double value) =>
        double.TryParse(
            configuration[key],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value
        );

    private static bool TryGetDuration(IConfiguration configuration, string key, out TimeSpan value)
    {
        value = default;
        if (
            !TryGetDouble(configuration, key, out var milliseconds)
            || !IsPositiveFinite(milliseconds)
        )
        {
            return false;
        }

        value = TimeSpan.FromMilliseconds(milliseconds);
        return true;
    }

    private static bool TryGetEnum<TEnum>(IConfiguration configuration, string key, out TEnum value)
        where TEnum : struct, Enum =>
        Enum.TryParse(configuration[key], ignoreCase: true, out value) && Enum.IsDefined(value);

    private static bool TryGetColor(IConfiguration configuration, string key, out MediaColor color)
    {
        color = default;
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            if (MediaColorConverter.ConvertFromString(value) is MediaColor parsed)
            {
                color = parsed;
                return true;
            }
        }
        catch (FormatException) { }

        return false;
    }

    private static bool IsPositiveFinite(double value) => double.IsFinite(value) && value > 0;
}
