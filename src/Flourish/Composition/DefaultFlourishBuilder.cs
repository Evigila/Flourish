using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AckSS.Flourish.Composition;

internal sealed class DefaultFlourishBuilder(string[] args) : IFlourishBuilder
{
    private readonly IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
    private readonly FlourishShellOptions shellOptions = new();
    private readonly FlourishDataOptions dataOptions = new();
    private readonly List<Action<IFlourishDataBuilder>> dataConfigurations = [];
    private readonly List<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations =
    [];
    private readonly List<Action<IFlourishShellBuilder>> shellConfigurations = [];
    private readonly List<Action<IFlourishTitlebarBuilder>> titleBarConfigurations = [];
    private readonly List<Action<IFlourishNavigationBuilder>> navigationConfigurations = [];
    private readonly List<Action<IFlourishCustomHandlerBuilder>> customHandlerConfigurations = [];
    private readonly List<Action<IFlourishDynamicToolbarBuilder>> toolbarConfigurations = [];
    private readonly List<Action<IFlourishTipsBuilder>> tipsConfigurations = [];
    private readonly List<Action<IFlourishMotionBuilder>> motionConfigurations = [];
    private readonly List<Action<IFlourishWindowPropertyBuilder>> windowConfigurations = [];
    private readonly List<Action<IFlourishFooterBuilder>> footerConfigurations = [];

    public IFlourishBuilder ConfigureData(Action<IFlourishDataBuilder> configureData)
    {
        dataConfigurations.Add(configureData);
        return this;
    }

    public IFlourishBuilder ConfigureServices(
        Action<HostBuilderContext, IServiceCollection> configureServices
    )
    {
        serviceConfigurations.Add(configureServices);
        return this;
    }

    public IFlourishBuilder ConfigureShell(Action<IFlourishShellBuilder> configureShell)
    {
        shellConfigurations.Add(configureShell);
        return this;
    }

    public IFlourishBuilder ConfigureTitleBar(
        Action<IFlourishTitlebarBuilder> configureTitleBar
    )
    {
        titleBarConfigurations.Add(configureTitleBar);
        return this;
    }

    public IFlourishBuilder ConfigureNavigation(
        Action<IFlourishNavigationBuilder> configureNavigation
    )
    {
        navigationConfigurations.Add(configureNavigation);
        return this;
    }

    public IFlourishBuilder ConfigureCustomHandler(
        Action<IFlourishCustomHandlerBuilder> configureCustomHandler
    )
    {
        customHandlerConfigurations.Add(configureCustomHandler);
        return this;
    }

    public IFlourishBuilder ConfigureDynamicToolbar(
        Action<IFlourishDynamicToolbarBuilder> configureToolbar
    )
    {
        toolbarConfigurations.Add(configureToolbar);
        return this;
    }

    public IFlourishBuilder ConfigureTips(Action<IFlourishTipsBuilder> configureTips)
    {
        tipsConfigurations.Add(configureTips);
        return this;
    }

    public IFlourishBuilder ConfigureMotion(Action<IFlourishMotionBuilder> configureMotion)
    {
        motionConfigurations.Add(configureMotion);
        return this;
    }

    public IFlourishBuilder ConfigureWindow(
        Action<IFlourishWindowPropertyBuilder> configureWindow
    )
    {
        windowConfigurations.Add(configureWindow);
        return this;
    }

    public IFlourishBuilder ConfigureFont(string fontFamily, double fontSize = 14)
    {
        shellOptions.FontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        shellOptions.FontSize = fontSize;
        return this;
    }

    public IFlourishBuilder ConfigureMaterialEffect(
        MaterialEffect effect = MaterialEffect.Mica
    )
    {
        ValidateEnum(effect, nameof(effect));
        shellOptions.MaterialEffect = effect;
        return this;
    }

    public IFlourishBuilder ConfigureThemes(
        FlourishTheme defaultTheme = FlourishTheme.System
    )
    {
        ValidateEnum(defaultTheme, nameof(defaultTheme));
        shellOptions.DefaultTheme = defaultTheme;
        return this;
    }

    public IFlourishBuilder ConfigureFooter(Action<IFlourishFooterBuilder> configureFooter)
    {
        footerConfigurations.Add(configureFooter);
        return this;
    }

    public IFlourish Build()
    {
        var compositionRoot = new FlourishCompositionRoot(
            shellOptions,
            dataOptions,
            dataConfigurations,
            serviceConfigurations,
            shellConfigurations,
            titleBarConfigurations,
            navigationConfigurations,
            customHandlerConfigurations,
            toolbarConfigurations,
            tipsConfigurations,
            motionConfigurations,
            windowConfigurations,
            footerConfigurations
        );

        hostBuilder.ConfigureServices(compositionRoot.ConfigureServices);
        return new FlourishRuntime(hostBuilder.Build());
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

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
    }
}
