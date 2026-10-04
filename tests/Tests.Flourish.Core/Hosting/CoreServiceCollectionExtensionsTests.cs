using System;
using System.Linq;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.BackgroundTasks;
using ArkheideSystem.Flourish.Commands;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Hosting;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Messaging;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Navigation;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.StatusBar;
using ArkheideSystem.Flourish.Shell.TitleBar;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Hosting;

public sealed class CoreServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCoreServices_RegistersTheStableCoreServiceSequence()
    {
        var services = new ServiceCollection();

        Assert.Same(services, services.AddCoreServices(CreateOptions()));

        Assert.Equal(
            [
                typeof(LocalizationService),
                typeof(ILocalizationService),
                typeof(LayoutOptions),
                typeof(ProjectOptions),
                typeof(TitleBarOptions),
                typeof(StatusBarOptions),
                typeof(MotionOptions),
                typeof(ProfileOptions),
                typeof(ApplicationDataOptions),
                typeof(StatusBarService),
                typeof(IStatusBarService),
                typeof(BackgroundTaskService),
                typeof(IBackgroundTaskService),
                typeof(IHostedService),
                typeof(NotificationService),
                typeof(INotificationService),
                typeof(CommandDispatcher),
                typeof(ICommandRegistry),
                typeof(ICommandDispatcher),
                typeof(NavigationMenuStateService),
                typeof(INavigationMenuStateService),
                typeof(ContentLayoutService),
                typeof(IContentLayoutService),
                typeof(AppPreferenceService),
                typeof(ISettingsStore),
                typeof(IProfileAuthService),
                typeof(IProfileService),
                typeof(TitleBarService),
                typeof(TitleBarSearchService),
                typeof(ProjectCatalogStore),
                typeof(IProjectCatalogStore),
                typeof(ProjectService),
                typeof(IProjectService),
            ],
            services.Select(descriptor => descriptor.ServiceType)
        );
    }

    [Fact]
    public void AddCoreServices_UsesAliasesWithoutRegisteringAPlatformCredentialStore()
    {
        var services = new ServiceCollection();
        services.AddCoreServices(CreateOptions());
        var dispatcher = new CommandDispatcher();
        var provider = new SingleServiceProvider(dispatcher);

        var registry = services.Single(descriptor =>
            descriptor.ServiceType == typeof(ICommandRegistry)
        );
        var commandDispatcher = services.Single(descriptor =>
            descriptor.ServiceType == typeof(ICommandDispatcher)
        );
        var navigationMenu = new NavigationMenuStateService();
        var navigationProvider = new SingleServiceProvider(navigationMenu);
        var navigationState = services.Single(descriptor =>
            descriptor.ServiceType == typeof(INavigationMenuStateService)
        );

        Assert.Same(dispatcher, registry.ImplementationFactory!(provider));
        Assert.Same(dispatcher, commandDispatcher.ImplementationFactory!(provider));
        Assert.Same(
            navigationMenu,
            navigationState.ImplementationFactory!(navigationProvider)
        );
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(IProfileCredentialStore)
        );
    }

    [Fact]
    public void AddCoreServices_PreservesAConsumerProfileAuthenticationService()
    {
        var customAuthentication = Mock.Of<IProfileAuthService>();
        var services = new ServiceCollection();
        services.AddSingleton(customAuthentication);

        services.AddCoreServices(CreateOptions());

        var registration = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(IProfileAuthService)
        );
        Assert.Same(customAuthentication, registration.ImplementationInstance);
    }

    [Fact]
    public void AddCoreServices_WithNullArgumentsThrows()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            CoreServiceCollectionExtensions.AddCoreServices(null!, CreateOptions())
        );
        Assert.Throws<ArgumentNullException>(() => services.AddCoreServices(null!));
    }

    private static CoreServiceOptions CreateOptions()
    {
        var data = new ApplicationDataOptions();
        return new CoreServiceOptions(
            data,
            new LocalizationService(data),
            new LayoutOptions(),
            new ProjectOptions(),
            new TitleBarOptions(),
            new StatusBarOptions(),
            new MotionOptions(),
            new ProfileOptions()
        );
    }

    private sealed class SingleServiceProvider(object service) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return serviceType.IsInstanceOfType(service) ? service : null;
        }
    }
}
