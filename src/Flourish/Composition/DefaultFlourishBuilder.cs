using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Composition;

internal sealed class DefaultFlourishBuilder(string[] args) : IFlourishBuilder
{
    private readonly IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
    private readonly FlourishShellOptions shellOptions = new();
    private readonly List<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations =
    [];
    private readonly List<Action<HostBuilderContext, IFlourishShellBuilder>> shellConfigurations =
    [];
    private readonly List<
        Action<HostBuilderContext, IFlourishDynamicToolbarBuilder>
    > toolbarConfigurations = [];
    private readonly List<
        Action<HostBuilderContext, IFlourishStatusBuilder>
    > statusConfigurations = [];

    public IFlourishBuilder ConfigureServices(
        Action<HostBuilderContext, IServiceCollection> configureServices
    )
    {
        serviceConfigurations.Add(configureServices);
        return this;
    }

    public IFlourishBuilder ConfigureShell(
        Action<HostBuilderContext, IFlourishShellBuilder> configureShell
    )
    {
        shellConfigurations.Add(configureShell);
        return this;
    }

    public IFlourishBuilder ConfigureDynamicToolbar(
        Action<HostBuilderContext, IFlourishDynamicToolbarBuilder> configureToolbar
    )
    {
        toolbarConfigurations.Add(configureToolbar);
        return this;
    }

    public IFlourishBuilder ConfigureStatus(
        Action<HostBuilderContext, IFlourishStatusBuilder> configureStatus
    )
    {
        statusConfigurations.Add(configureStatus);
        return this;
    }

    public IFlourish Build()
    {
        var compositionRoot = new FlourishCompositionRoot(
            shellOptions,
            serviceConfigurations,
            shellConfigurations,
            toolbarConfigurations,
            statusConfigurations
        );

        hostBuilder.ConfigureServices(compositionRoot.ConfigureServices);
        return new FlourishRuntime(hostBuilder.Build());
    }
}
