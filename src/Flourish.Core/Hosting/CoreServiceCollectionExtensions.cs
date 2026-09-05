using System;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.BackgroundTasks;
using ArkheideSystem.Flourish.Commands;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Messaging;
using ArkheideSystem.Flourish.Navigation;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.StatusBar;
using ArkheideSystem.Flourish.Shell.TitleBar;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Hosting;

/// <summary>Registers the platform-independent application services.</summary>
internal static class CoreServiceCollectionExtensions
{
    internal static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        CoreServiceOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options.Localization);
        services.AddSingleton<ILocalizationService>(provider =>
            provider.GetRequiredService<LocalizationService>()
        );
        services.AddSingleton(options.Layout);
        services.AddSingleton(options.Projects);
        services.AddSingleton(options.TitleBar);
        services.AddSingleton(options.StatusBar);
        services.AddSingleton(options.Motion);
        services.AddSingleton(options.Profile);
        services.AddSingleton(options.Data);
        services.AddSingletonAdapter<StatusBarService, IStatusBarService>();
        services.AddSingletonAdapter<BackgroundTaskService, IBackgroundTaskService>();
        services.AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<BackgroundTaskService>()
        );
        services.AddSingletonAdapter<NotificationService, INotificationService>();
        services.AddSingletonAdapter<CommandDispatcher, ICommandRegistry>();
        services.AddSingletonAlias<CommandDispatcher, ICommandDispatcher>();
        services.AddSingletonAdapter<NavigationMenuStateService, INavigationMenuStateService>();
        services.AddSingletonAdapter<ContentLayoutService, IContentLayoutService>();
        services.AddSingletonAdapter<AppPreferenceService, ISettingsStore>();
        services.TryAddSingleton<IProfileAuthService, SimpleProfileAuthService>();
        services.TryAddSingleton<IProfileService, ProfileService>();
        services.AddSingleton<TitleBarService>();
        services.AddSingleton<TitleBarSearchService>();
        services.AddSingleton(provider =>
            new ProjectCatalogStore(
                provider.GetRequiredService<ApplicationDataOptions>().ProjectCatalogFilePath
            )
        );
        services.AddSingletonAlias<ProjectCatalogStore, IProjectCatalogStore>();
        services.AddSingletonAdapter<ProjectService, IProjectService>();

        return services;
    }
}

/// <summary>Registers one concrete singleton behind one or more service contracts.</summary>
internal static class SingletonServiceCollectionExtensions
{
    internal static IServiceCollection AddSingletonAdapter<TConcrete, TContract>(
        this IServiceCollection services
    )
        where TConcrete : class, TContract
        where TContract : class
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<TConcrete>();
        return services.AddSingletonAlias<TConcrete, TContract>();
    }

    internal static IServiceCollection AddSingletonAlias<TConcrete, TContract>(
        this IServiceCollection services
    )
        where TConcrete : class, TContract
        where TContract : class
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton<TContract>(provider =>
            provider.GetRequiredService<TConcrete>()
        );
    }
}
