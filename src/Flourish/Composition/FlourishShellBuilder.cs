using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;
using Microsoft.Extensions.Hosting;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishShellBuilder(FlourishShellOptions options, HostBuilderContext context)
    : IFlourishShellBuilder
{
    public IFlourishShellBuilder UseTitlebar(
        Action<HostBuilderContext, IFlourishTitlebarBuilder> configureTitlebar
    )
    {
        options.IsTitlebarEnabled = true;
        configureTitlebar(context, new FlourishTitlebarBuilder(options));
        return this;
    }

    public IFlourishShellBuilder UseNavigationPanel(
        Action<HostBuilderContext, IFlourishNavigationPanelBuilder> configureNavigationPanel
    )
    {
        options.IsNavigationPanelEnabled = true;
        configureNavigationPanel(context, new FlourishNavigationPanelBuilder(options));
        return this;
    }

    public IFlourishShellBuilder UseRegions(
        Action<HostBuilderContext, IFlourishRegionBuilder> configureRegions
    )
    {
        configureRegions(context, new FlourishRegionBuilder(options));
        return this;
    }

    public IFlourishShellBuilder SetWindowProperty(
        Action<HostBuilderContext, IFlourishWindowPropertyBuilder> configureWindow
    )
    {
        configureWindow(context, new FlourishWindowPropertyBuilder(options));
        return this;
    }

    public IFlourishShellBuilder SetGlobalFont(string fontFamily, double fontSize = 14)
    {
        options.FontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        options.FontSize = fontSize;
        return this;
    }

    public IFlourishShellBuilder UseMaterialEffect(MaterialEffect effect = MaterialEffect.Mica)
    {
        options.MaterialEffect = effect;
        return this;
    }

    public IFlourishShellBuilder UseThemes(FlourishTheme defaultTheme = FlourishTheme.System)
    {
        ValidateTheme(defaultTheme, nameof(defaultTheme));
        options.IsThemeEnabled = true;
        options.DefaultTheme = defaultTheme;
        return this;
    }

    public IFlourishShellBuilder UseDynamicToolbar(bool enabled = true)
    {
        options.IsDynamicToolbarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseMotion(bool enabled = true)
    {
        options.Motion.IsEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseMotion(
        Action<HostBuilderContext, IFlourishMotionBuilder> configureMotion
    )
    {
        options.Motion.IsEnabled = true;
        configureMotion(context, new FlourishMotionBuilder(options.Motion));
        return this;
    }

    public IFlourishShellBuilder UseTips(
        Action<HostBuilderContext, IFlourishTipsBuilder> configureTips
    )
    {
        configureTips(context, new FlourishTipsBuilder(options.Tips));
        return this;
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than 0."
            );
        }
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }

    private static void ValidateTheme(FlourishTheme theme, string parameterName)
    {
        if (!Enum.IsDefined(theme))
        {
            throw new ArgumentOutOfRangeException(parameterName, theme, "Unknown theme.");
        }
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
        }
    }
}
