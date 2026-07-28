using System.Globalization;
using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;

namespace ArkheideSystem.Flourish.Internal.Configuration;

internal static class FlourishPreferenceKeys
{
    public const string Root = "Flourish:Preferences";
    public const string Locale = $"{Root}:Locale";
    public const string Theme = $"{Root}:Theme";
    public const string WindowSize = $"{Root}:Window:Size";
    public const string WindowPosition = $"{Root}:Window:Position";
    public const string WindowState = $"{Root}:Window:State";
    public const string WindowTopmost = $"{Root}:Window:Topmost";
    public const string WindowCloseBehavior = $"{Root}:Window:CloseBehavior";
    public const string Navigation = $"{Root}:Navigation";
    public const string Motion = $"{Root}:Motion";
    public const string SmoothScrolling = $"{Root}:Interaction:SmoothScrolling";
    public const string Font = $"{Root}:Typography:Global";
    public const string ContentLayout = $"{Root}:Layout:CenterContent";
    public const string Material = $"{Root}:Appearance:Material";
    public const string ThemeColors = $"{Root}:Appearance:ThemeColors";
    public const string CornerRadius = $"{Root}:Appearance:CornerRadius";
    public const string NameOrder = $"{Root}:Profile:NameOrder";
}

internal static class FlourishPreferenceConfiguration
{
    public static void Apply(
        IConfiguration configuration,
        FlourishDataOptions data,
        FlourishShellOptions shell
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(shell);

        if (
            data.UsePersistedLocale
            && configuration[FlourishPreferenceKeys.Locale] is { } locale
            && FlourishLocalizationService.TryNormalizeLocale(locale, out var normalizedLocale)
        )
        {
            data.Locale = normalizedLocale;
        }

        ApplyWindow(configuration, shell);
        ApplyNavigation(configuration, shell);
        ApplyMotion(configuration, shell);
        ApplyAppearance(configuration, shell);
        ApplyFontAndLayout(configuration, shell);

        if (
            shell.UsePersistedSmoothScroll
            && TryGetBoolean(configuration, FlourishPreferenceKeys.SmoothScrolling, out var smooth)
        )
        {
            shell.IsSmoothScrollingEnabled = smooth;
        }

        if (
            shell.UsePersistedNameOrder
            && TryGetEnum(configuration, FlourishPreferenceKeys.NameOrder, out NameOrder nameOrder)
        )
        {
            shell.Profile.NameOrder = nameOrder;
        }
    }

    private static void ApplyWindow(IConfiguration configuration, FlourishShellOptions shell)
    {
        if (
            shell.UsePersistedWindowSize
            && TryGetDouble(configuration, $"{FlourishPreferenceKeys.WindowSize}:Width", out var width)
            && TryGetDouble(
                configuration,
                $"{FlourishPreferenceKeys.WindowSize}:Height",
                out var height
            )
            && IsPositiveFinite(width)
            && IsPositiveFinite(height)
        )
        {
            shell.WindowWidth = Math.Clamp(width, shell.WindowMinWidth, shell.WindowMaxWidth);
            shell.WindowHeight = Math.Clamp(height, shell.WindowMinHeight, shell.WindowMaxHeight);
        }

        if (
            shell.UsePersistedWindowPosition
            && TryGetDouble(
                configuration,
                $"{FlourishPreferenceKeys.WindowPosition}:Left",
                out var left
            )
            && TryGetDouble(
                configuration,
                $"{FlourishPreferenceKeys.WindowPosition}:Top",
                out var top
            )
            && double.IsFinite(left)
            && double.IsFinite(top)
        )
        {
            shell.WindowLeft = left;
            shell.WindowTop = top;
            shell.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        if (
            shell.UsePersistedWindowState
            && TryGetEnum(configuration, FlourishPreferenceKeys.WindowState, out WindowState state)
            && state is WindowState.Normal or WindowState.Maximized
        )
        {
            shell.WindowState = state;
        }

        if (
            shell.UsePersistedWindowTopmost
            && TryGetBoolean(
                configuration,
                FlourishPreferenceKeys.WindowTopmost,
                out var topmost
            )
        )
        {
            shell.WindowTopmost = topmost;
        }

        if (
            shell.UsePersistedTrayExit
            && configuration[FlourishPreferenceKeys.WindowCloseBehavior] is { } closeBehavior
            && Enum.TryParse(
                closeBehavior,
                ignoreCase: true,
                out WindowCloseBehavior parsedCloseBehavior
            )
            && Enum.IsDefined(parsedCloseBehavior)
        )
        {
            shell.IsTrayExitEnabled =
                parsedCloseBehavior == WindowCloseBehavior.MinimizeToTray;
        }
    }

    private static void ApplyNavigation(IConfiguration configuration, FlourishShellOptions shell)
    {
        if (
            shell.UsePersistedNavigationDirection
            && TryGetEnum(
                configuration,
                $"{FlourishPreferenceKeys.Navigation}:Direction",
                out NavigationPanelDirection direction
            )
        )
        {
            shell.NavigationPanelDirection = direction;
        }

        if (
            shell.UsePersistedNavigationOpenState
            && TryGetBoolean(
                configuration,
                $"{FlourishPreferenceKeys.Navigation}:IsOpen",
                out var isOpen
            )
        )
        {
            shell.IsNavigationPanelInitiallyOpen = isOpen;
        }

        if (
            shell.UsePersistedNavigationWidth
            && TryGetDouble(
                configuration,
                $"{FlourishPreferenceKeys.Navigation}:OpenWidth",
                out var openWidth
            )
            && double.IsFinite(openWidth)
        )
        {
            shell.OpenPaneWidth = Math.Clamp(
                openWidth,
                shell.NavigationPaneMinWidth,
                shell.NavigationPaneMaxWidth
            );
            shell.ClosedPaneWidth = Math.Min(shell.ClosedPaneWidth, shell.OpenPaneWidth);
        }

        if (
            shell.UsePersistedLastNavigation
            && configuration[$"{FlourishPreferenceKeys.Navigation}:LastKey"] is { } lastKey
            && shell.InitialNavigationRoutes.FirstOrDefault(route =>
                string.Equals(route.NavigationKey, lastKey, StringComparison.Ordinal)
            )
                is { } route
        )
        {
            shell.InitialNavigationKey = route.NavigationKey;
            shell.InitialNavigationPageType = route.PageType;
        }
    }

    private static void ApplyMotion(IConfiguration configuration, FlourishShellOptions shell)
    {
        var motion = shell.Motion;
        if (
            shell.UsePersistedMotion
            && TryGetBoolean(
                configuration,
                $"{FlourishPreferenceKeys.Motion}:Enabled",
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
                $"{FlourishPreferenceKeys.Motion}:PageTransition:Transition",
                out FlourishPageTransition pageTransition
            )
            && TryGetDuration(
                configuration,
                $"{FlourishPreferenceKeys.Motion}:PageTransition:DurationMilliseconds",
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
                $"{FlourishPreferenceKeys.Motion}:NavigationPanelTransition:Transition",
                out FlourishNavigationPanelTransition navigationTransition
            )
            && TryGetDuration(
                configuration,
                $"{FlourishPreferenceKeys.Motion}:NavigationPanelTransition:DurationMilliseconds",
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
                $"{FlourishPreferenceKeys.Motion}:HoverReveal:Enabled",
                out var hoverEnabled
            )
            && TryGetDuration(
                configuration,
                $"{FlourishPreferenceKeys.Motion}:HoverReveal:DurationMilliseconds",
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
                $"{FlourishPreferenceKeys.Motion}:RespectSystemReducedMotion",
                out var reducedMotion
            )
        )
        {
            motion.RespectSystemReducedMotion = reducedMotion;
        }
    }

    private static void ApplyAppearance(IConfiguration configuration, FlourishShellOptions shell)
    {
        if (
            shell.UsePersistedMaterialEffect
            && TryGetBoolean(
                configuration,
                $"{FlourishPreferenceKeys.Material}:Enabled",
                out var materialEnabled
            )
            && TryGetEnum(
                configuration,
                $"{FlourishPreferenceKeys.Material}:Effect",
                out MaterialEffect material
            )
        )
        {
            shell.MaterialEffect = material;
            shell.IsMaterialEffectEnabled = materialEnabled && material != MaterialEffect.None;
        }

        if (
            shell.UsePersistedThemeColors
            && TryGetBoolean(
                configuration,
                $"{FlourishPreferenceKeys.ThemeColors}:Enabled",
                out var colorsEnabled
            )
        )
        {
            if (!colorsEnabled)
            {
                shell.ThemeColors = null;
            }
            else if (
                TryGetColor(
                    configuration,
                    $"{FlourishPreferenceKeys.ThemeColors}:Primary",
                    out var primary
                )
                && TryGetColor(
                    configuration,
                    $"{FlourishPreferenceKeys.ThemeColors}:Secondary",
                    out var secondary
                )
                && TryGetColor(
                    configuration,
                    $"{FlourishPreferenceKeys.ThemeColors}:Accent",
                    out var accent
                )
                && primary.A == byte.MaxValue
                && secondary.A == byte.MaxValue
                && accent.A == byte.MaxValue
            )
            {
                shell.ThemeColors = new FlourishThemeColors(primary, secondary, accent);
            }
        }

        if (
            shell.UsePersistedCornerRadius
            && TryGetBoolean(
                configuration,
                $"{FlourishPreferenceKeys.CornerRadius}:Enabled",
                out var radiusEnabled
            )
        )
        {
            if (!radiusEnabled)
            {
                shell.CornerRadius = null;
            }
            else if (
                TryGetDouble(
                    configuration,
                    $"{FlourishPreferenceKeys.CornerRadius}:Value",
                    out var radius
                )
                && double.IsFinite(radius)
                && radius >= 0
            )
            {
                shell.CornerRadius = radius;
            }
        }
    }

    private static void ApplyFontAndLayout(
        IConfiguration configuration,
        FlourishShellOptions shell
    )
    {
        if (shell.UsePersistedFont)
        {
            var prefix = FlourishPreferenceKeys.Font;
            var family = configuration[$"{prefix}:Family"];
            var iconFamily = configuration[$"{prefix}:IconFamily"];
            if (
                !string.IsNullOrWhiteSpace(family)
                && !string.IsNullOrWhiteSpace(iconFamily)
                && TryGetDouble(configuration, $"{prefix}:Small", out var small)
                && TryGetDouble(configuration, $"{prefix}:Standard", out var standard)
                && TryGetDouble(configuration, $"{prefix}:Icon", out var icon)
                && TryGetDouble(configuration, $"{prefix}:Large", out var large)
                && TryGetDouble(configuration, $"{prefix}:ExtraLarge", out var extraLarge)
                && TryGetDouble(configuration, $"{prefix}:Header", out var header)
                && new[] { small, standard, icon, large, extraLarge, header }
                    .All(IsPositiveFinite)
            )
            {
                shell.FontFamily = family.Trim();
                shell.IconFontFamily = iconFamily.Trim();
                shell.FontSizeSmall = small;
                shell.FontSizeStandard = standard;
                shell.FontSizeIcon = icon;
                shell.FontSizeLarge = large;
                shell.FontSizeExtraLarge = extraLarge;
                shell.FontSizeHeaderSize = header;
            }
        }

        if (
            shell.UsePersistedContentLayout
            && TryGetBoolean(
                configuration,
                $"{FlourishPreferenceKeys.ContentLayout}:Enabled",
                out var centered
            )
            && TryGetDouble(
                configuration,
                $"{FlourishPreferenceKeys.ContentLayout}:Width",
                out var contentWidth
            )
            && IsPositiveFinite(contentWidth)
        )
        {
            shell.IsCenterContentEnabled = centered;
            shell.CenterContentWidth = contentWidth;
        }
    }

    private static bool TryGetBoolean(
        IConfiguration configuration,
        string key,
        out bool value
    ) => bool.TryParse(configuration[key], out value);

    private static bool TryGetDouble(
        IConfiguration configuration,
        string key,
        out double value
    ) =>
        double.TryParse(
            configuration[key],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value
        );

    private static bool TryGetDuration(
        IConfiguration configuration,
        string key,
        out TimeSpan value
    )
    {
        value = default;
        if (!TryGetDouble(configuration, key, out var milliseconds) || !IsPositiveFinite(milliseconds))
        {
            return false;
        }

        value = TimeSpan.FromMilliseconds(milliseconds);
        return true;
    }

    private static bool TryGetEnum<TEnum>(
        IConfiguration configuration,
        string key,
        out TEnum value
    )
        where TEnum : struct, Enum =>
        Enum.TryParse(configuration[key], ignoreCase: true, out value) && Enum.IsDefined(value);

    private static bool TryGetColor(
        IConfiguration configuration,
        string key,
        out MediaColor color
    )
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
