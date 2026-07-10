using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class DefaultFlourishBuilder(string[] args) : IFlourishBuilder
{
    private readonly IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
    private readonly FlourishShellOptions shellOptions = new();
    private readonly FlourishDataOptions dataOptions = new();
    private readonly List<Action<IFlourishDataBuilder>> dataConfigurations = [];
    private readonly List<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations =
    [];
    private readonly List<Action<IFlourishShellBuilder>> shellConfigurations = [];
    private readonly List<Action<IFlourishProfileBuilder>> profileConfigurations = [];
    private readonly List<Action<IFlourishTitlebarBuilder>> titleBarConfigurations = [];
    private readonly List<Action<IFlourishNavigationBuilder>> navigationConfigurations = [];
    private readonly List<Action<IFlourishCustomHandlerBuilder>> customHandlerConfigurations = [];
    private readonly List<Action<IFlourishDynamicToolbarBuilder>> toolbarConfigurations = [];
    private readonly List<Action<IFlourishMotionBuilder>> motionConfigurations = [];
    private readonly List<Action<IFlourishWindowPropertyBuilder>> windowConfigurations = [];
    private readonly List<Action<IFlourishStatusBarBuilder>> statusBarConfigurations = [];

    public IFlourishBuilder ConfigureData(Action<IFlourishDataBuilder> configureData)
    {
        ArgumentNullException.ThrowIfNull(configureData);
        dataConfigurations.Add(configureData);
        return this;
    }

    public IFlourishBuilder ConfigureServices(
        Action<HostBuilderContext, IServiceCollection> configureServices
    )
    {
        ArgumentNullException.ThrowIfNull(configureServices);
        serviceConfigurations.Add(configureServices);
        return this;
    }

    public IFlourishBuilder ConfigureShell(Action<IFlourishShellBuilder> configureShell)
    {
        ArgumentNullException.ThrowIfNull(configureShell);
        shellConfigurations.Add(configureShell);
        return this;
    }

    public IFlourishBuilder ConfigureProfile(
        Action<IFlourishProfileBuilder> configureProfile
    )
    {
        ArgumentNullException.ThrowIfNull(configureProfile);
        profileConfigurations.Add(configureProfile);
        return this;
    }

    public IFlourishBuilder ConfigureTitleBar(
        Action<IFlourishTitlebarBuilder> configureTitleBar
    )
    {
        ArgumentNullException.ThrowIfNull(configureTitleBar);
        titleBarConfigurations.Add(configureTitleBar);
        return this;
    }

    public IFlourishBuilder ConfigureNavigation(
        Action<IFlourishNavigationBuilder> configureNavigation
    )
    {
        ArgumentNullException.ThrowIfNull(configureNavigation);
        navigationConfigurations.Add(configureNavigation);
        return this;
    }

    public IFlourishBuilder ConfigureCustomHandler(
        Action<IFlourishCustomHandlerBuilder> configureCustomHandler
    )
    {
        ArgumentNullException.ThrowIfNull(configureCustomHandler);
        customHandlerConfigurations.Add(configureCustomHandler);
        return this;
    }

    public IFlourishBuilder ConfigureDynamicToolbar(
        Action<IFlourishDynamicToolbarBuilder> configureToolbar
    )
    {
        ArgumentNullException.ThrowIfNull(configureToolbar);
        toolbarConfigurations.Add(configureToolbar);
        return this;
    }

    public IFlourishBuilder ConfigureMotion(Action<IFlourishMotionBuilder> configureMotion)
    {
        ArgumentNullException.ThrowIfNull(configureMotion);
        motionConfigurations.Add(configureMotion);
        return this;
    }

    public IFlourishBuilder ConfigureWindow(
        Action<IFlourishWindowPropertyBuilder> configureWindow
    )
    {
        ArgumentNullException.ThrowIfNull(configureWindow);
        windowConfigurations.Add(configureWindow);
        return this;
    }

    public IFlourishBuilder ConfigureStatusBar(
        Action<IFlourishStatusBarBuilder> configureStatusBar
    )
    {
        ArgumentNullException.ThrowIfNull(configureStatusBar);
        statusBarConfigurations.Add(configureStatusBar);
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
            profileConfigurations,
            titleBarConfigurations,
            navigationConfigurations,
            customHandlerConfigurations,
            toolbarConfigurations,
            motionConfigurations,
            windowConfigurations,
            statusBarConfigurations
        );

        hostBuilder.ConfigureServices(compositionRoot.ConfigureServices);
        return new FlourishRuntime(hostBuilder.Build());
    }

}
