using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Internal;
using AcksheedSys.Flourish.Models;
using AcksheedSys.Flourish.Services;
using AcksheedSys.Flourish.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishCompositionRoot(
    FlourishShellOptions shellOptions,
    IReadOnlyList<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations,
    IReadOnlyList<Action<HostBuilderContext, IFlourishShellBuilder>> shellConfigurations,
    IReadOnlyList<Action<HostBuilderContext, IFlourishDynamicToolbarBuilder>> toolbarConfigurations,
    IReadOnlyList<Action<HostBuilderContext, IFlourishStatusBuilder>> statusConfigurations
)
{
    private readonly FlourishShellOptions shellOptions = shellOptions;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IServiceCollection>
    > serviceConfigurations = serviceConfigurations;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IFlourishShellBuilder>
    > shellConfigurations = shellConfigurations;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IFlourishDynamicToolbarBuilder>
    > toolbarConfigurations = toolbarConfigurations;
    private readonly IReadOnlyList<
        Action<HostBuilderContext, IFlourishStatusBuilder>
    > statusConfigurations = statusConfigurations;

    public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        foreach (var configureServices in serviceConfigurations)
        {
            configureServices(context, services);
        }

        ApplyFlourishConfigurations(context);
        ApplyServiceCollectionRegistrations(services);
        RegisterCoreServices(services);
    }

    private void ApplyFlourishConfigurations(HostBuilderContext context)
    {
        var shellBuilder = new FlourishShellBuilder(shellOptions);
        foreach (var configureShell in shellConfigurations)
        {
            configureShell(context, shellBuilder);
        }

        var toolbarBuilder = new FlourishDynamicToolbarBuilder(shellOptions);
        foreach (var configureToolbar in toolbarConfigurations)
        {
            configureToolbar(context, toolbarBuilder);
        }

        var statusBuilder = new FlourishStatusBuilder(shellOptions);
        foreach (var configureStatus in statusConfigurations)
        {
            configureStatus(context, statusBuilder);
        }
    }

    private void ApplyServiceCollectionRegistrations(IServiceCollection services)
    {
        if (
            services
                .FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(FlourishServiceCollectionState)
                    && descriptor.ImplementationInstance is FlourishServiceCollectionState
                )
                ?.ImplementationInstance
            is not FlourishServiceCollectionState state
        )
        {
            return;
        }

        foreach (var page in state.NavigablePages)
        {
            var key = page.PageType.FullName ?? page.DisplayName;
            if (shellOptions.NavigationItems.Any(item => item.Key == key))
            {
                continue;
            }

            shellOptions.NavigationItems.Add(
                new FlourishNavigationItem(
                    key,
                    page.DisplayName,
                    page.IconGlyph,
                    page.PageType,
                    page.CacheMode
                )
            );

            if (page.IsInitial)
            {
                shellOptions.InitialNavigationPageType = page.PageType;
            }
        }
    }

    private void RegisterCoreServices(IServiceCollection services)
    {
        services.AddSingleton(shellOptions);
        services.AddSingleton<FlourishShellWindow>();
        services.AddSingleton<FlourishToolbarService>();
        services.AddSingleton<IFlourishToolbarService>(provider =>
            provider.GetRequiredService<FlourishToolbarService>()
        );
        services.AddSingleton<FlourishStatusService>();
        services.AddSingleton<IFlourishStatusService>(provider =>
            provider.GetRequiredService<FlourishStatusService>()
        );
        services.AddSingleton<TrayIconService>();
        services.AddSingleton<ITrayIconService>(provider =>
            provider.GetRequiredService<TrayIconService>()
        );
        services.AddSingleton<FontService>();
        services.AddSingleton<IFlourishFontService>(provider =>
            provider.GetRequiredService<FontService>()
        );
        services.AddSingleton<CommandParser>();
        services.AddSingleton<MaterialEffectService>();
        services.AddSingleton<WindowFrameFixService>();
        services.AddSingleton<PageHistoryService>();
        services.AddSingleton<IFlourishPageHistoryService>(provider =>
            provider.GetRequiredService<PageHistoryService>()
        );
        services.AddSingleton<PageCacheService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<INavigationService>(provider =>
            provider.GetRequiredService<NavigationService>()
        );
        services.AddSingleton<IFrameNavigationService>(provider =>
            provider.GetRequiredService<NavigationService>()
        );
    }
}
