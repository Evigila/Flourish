using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.BackgroundTasks;
using ArkheideSystem.Flourish.Commands;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Messaging;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Navigation;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.Regions;
using ArkheideSystem.Flourish.Shell.StatusBar;
using ArkheideSystem.Flourish.Shell.TitleBar;
using ArkheideSystem.Flourish.Shell.Toolbar;
using ArkheideSystem.Flourish.ToolTips;
using ArkheideSystem.Flourish.Windowing;

using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ToolTipService = ArkheideSystem.Flourish.ToolTips.ToolTipService;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishCompositionContractTests
{
    [Fact]
    public void Build_WithoutDataConfiguration_UsesBuiltInEnglishLocale()
    {
        using var flourish = ApplicationBuilder.CreateDefaultBuilder([]).Build();

        var localization = flourish.GetRequiredService<LocalizationService>();

        Assert.Equal("en-US", localization.Current.Locale);
        Assert.Equal("Close", localization.Get(LocaleKeys.TitleBarClose));
    }

    [Fact]
    public void Build_WithoutOptionalConfigurations_AppliesLatentDefaultsWithoutEnablingShell()
    {
        using var flourish = ApplicationBuilder.CreateDefaultBuilder([]).Build();
        var titleBar = flourish.GetRequiredService<TitleBarOptions>();
        var navigation = flourish.GetRequiredService<NavigationOptions>();
        var motion = flourish.GetRequiredService<MotionOptions>();
        var window = flourish.GetRequiredService<WindowOptions>();
        var statusBar = flourish.GetRequiredService<StatusBarOptions>();
        var toolbar = flourish.GetRequiredService<ToolbarOptions>();

        Assert.False(titleBar.IsTitlebarEnabled);
        Assert.False(navigation.IsNavigationPanelEnabled);
        Assert.False(motion.IsEnabled);
        Assert.False(statusBar.IsStatusBarEnabled);
        Assert.False(toolbar.IsDynamicToolbarEnabled);

        Assert.True(titleBar.IsBreadcrumbEnabled);
        Assert.True(titleBar.IsTitlebarNavigationToggleEnabled);
        Assert.True(titleBar.IsTitlebarLogoEnabled);
        Assert.True(titleBar.IsTitlebarTitleEnabled);
        Assert.True(titleBar.IsTitlebarProfileEnabled);
        Assert.True(titleBar.IsTitlebarThemeToggleEnabled);
        Assert.False(titleBar.IsTitlebarSearchEnabled);
        Assert.Equal("MyApp", titleBar.ApplicationTitle);
        Assert.Equal("MyApp", titleBar.ApplicationSubtitle);

        Assert.Equal(NavigationPanelDirection.Left, navigation.NavigationPanelDirection);
        Assert.True(navigation.IsNavigationPanelInitiallyOpen);
        Assert.Equal(250, navigation.OpenPaneWidth);
        Assert.Equal(64, navigation.ClosedPaneWidth);
        Assert.Equal(180, navigation.NavigationPaneMinWidth);
        Assert.Equal(520, navigation.NavigationPaneMaxWidth);
        Assert.Empty(navigation.NavigationGroups);
        Assert.Empty(navigation.FixedNavigationItemDefinitions);

        Assert.True(motion.IsHoverRevealEnabled);
        Assert.True(motion.RespectSystemReducedMotion);
        Assert.Equal(1536, window.WindowWidth);
        Assert.Equal(864, window.WindowHeight);
        Assert.Equal(1280, window.WindowMinWidth);
        Assert.Equal(720, window.WindowMinHeight);
        Assert.False(window.WindowTopmost);
        Assert.False(window.IsTrayExitEnabled);

        var statusItem = Assert.Single(statusBar.StatusItems);
        Assert.Equal("OK", statusItem.Text);
        Assert.Equal("\uE930", statusItem.IconGlyph);
        Assert.True(statusBar.IsLANConnectionStatusEnabled);
        Assert.True(statusBar.IsPowerStatusEnabled);
        Assert.Empty(toolbar.ToolbarItems);
        Assert.Empty(toolbar.DynamicToolbarItems);
    }

    [Fact]
    public void Build_ExplicitOptionalConfigurations_OverrideImplicitDefaultsInOrder()
    {
        using var flourish = ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetLocale("zh-CN", usePersistedPreference: false))
            .ConfigureTitleBar(titleBar =>
                titleBar.SetApplicationTitle("Configured").SetThemeToggle(mode: ApplicationTheme.Dark)
            )
            .ConfigureNavigation(navigation =>
                navigation
                    .SetDirection(NavigationPanelDirection.Right, usePersistedPreference: false)
                    .SetInitiallyOpen(false, usePersistedPreference: false)
                    .SetPanelWidth(300, 64, 600, 200, usePersistedPreference: false)
            )
            .ConfigureMotion(motion =>
                motion
                    .SetHoverReveal(
                        duration: TimeSpan.FromMilliseconds(250),
                        usePersistedPreference: false
                    )
                    .SetRespectSystemReducedMotion(false, usePersistedPreference: false)
            )
            .ConfigureWindow(window =>
                window
                    .SetSize(1400, 800, usePersistedPreference: false)
                    .SetMinimumSize(900, 600)
                    .SetTopmost(false, usePersistedPreference: false)
                    .SetTrayExit(false, usePersistedPreference: false)
            )
            .Build();
        var titleBar = flourish.GetRequiredService<TitleBarOptions>();
        var appearance = flourish.GetRequiredService<AppearanceOptions>();
        var navigation = flourish.GetRequiredService<NavigationOptions>();
        var motion = flourish.GetRequiredService<MotionOptions>();
        var window = flourish.GetRequiredService<WindowOptions>();
        var data = flourish.GetRequiredService<ApplicationDataOptions>();

        Assert.Equal("zh-CN", data.Locale);
        Assert.Equal("Configured", titleBar.ApplicationTitle);
        Assert.Equal(ApplicationTheme.Dark, appearance.DefaultTheme);
        Assert.Equal(NavigationPanelDirection.Right, navigation.NavigationPanelDirection);
        Assert.False(navigation.IsNavigationPanelInitiallyOpen);
        Assert.Equal(300, navigation.OpenPaneWidth);
        Assert.Equal(200, navigation.NavigationPaneMinWidth);
        Assert.Equal(600, navigation.NavigationPaneMaxWidth);
        Assert.Equal(TimeSpan.FromMilliseconds(250), motion.HoverRevealAnimationDuration);
        Assert.False(motion.RespectSystemReducedMotion);
        Assert.Equal(1400, window.WindowWidth);
        Assert.Equal(800, window.WindowHeight);
        Assert.Equal(900, window.WindowMinWidth);
        Assert.Equal(600, window.WindowMinHeight);
        Assert.False(window.WindowTopmost);
        Assert.False(window.IsTrayExitEnabled);
    }

    [Fact]
    public void Build_WithOnlyLocaleConfiguration_DoesNotRequireApplicationIdentity()
    {
        var builder = ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetLocale("en-US"));

        using var flourish = builder.Build();
        var localization = flourish.GetRequiredService<LocalizationService>();

        Assert.Equal("en-US", localization.Current.Locale);
        Assert.Equal("Close", localization.Get(LocaleKeys.TitleBarClose));
    }

    [Fact]
    public void Build_RegistersRuntimeConfigurationAndAppearanceContractsAsSingletonAdapters()
    {
        using var flourish = ApplicationBuilder.CreateDefaultBuilder([]).Build();

        Assert.Same(
            flourish.GetRequiredService<LocalizationService>(),
            flourish.GetRequiredService<ILocalizationService>()
        );
        Assert.Same(
            flourish.GetRequiredService<AppPreferenceService>(),
            flourish.GetRequiredService<ISettingsStore>()
        );
        Assert.Same(
            flourish.GetRequiredService<ThemeService>(),
            flourish.GetRequiredService<IThemeService>()
        );
        Assert.Same(
            flourish.GetRequiredService<FontService>(),
            flourish.GetRequiredService<IFontService>()
        );
        Assert.Same(
            flourish.GetRequiredService<ToolTipService>(),
            flourish.GetRequiredService<IToolTipService>()
        );
        Assert.Same(
            flourish.GetRequiredService<ScrollService>(),
            flourish.GetRequiredService<IScrollService>()
        );
        Assert.Same(
            flourish.GetRequiredService<AppearanceService>(),
            flourish.GetRequiredService<IAppearanceService>()
        );
        Assert.Same(
            flourish.GetRequiredService<ContentLayoutService>(),
            flourish.GetRequiredService<IContentLayoutService>()
        );
        Assert.Same(
            flourish.GetRequiredService<MotionService>(),
            flourish.GetRequiredService<IMotionService>()
        );
        Assert.Same(
            flourish.GetRequiredService<MaterialEffectService>(),
            flourish.GetRequiredService<IMaterialEffectService>()
        );
    }

    [Fact]
    public void Build_RegistersRemainingRuntimeContractsAsConcreteSingletonAdapters()
    {
        using var flourish = ApplicationBuilder.CreateDefaultBuilder([]).Build();

        AssertSingleton<NavigationPanelService>(flourish);
        AssertSingleton<NavigationMenuService>(flourish);
        AssertSingletonAdapter<ToolbarService, IToolbarService>(flourish);
        AssertSingletonAdapter<StatusBarService, IStatusBarService>(flourish);
        AssertSingletonAdapter<ShellRegionService, IShellRegionService>(flourish);
        AssertSingletonAdapter<BackgroundTaskService, IBackgroundTaskService>(flourish);
        AssertSingletonAdapter<NotificationService, INotificationService>(flourish);
        AssertSingletonAdapter<TrayIconService, ITrayService>(flourish);
        AssertSingletonAdapter<CommandDispatcher, ICommandRegistry>(flourish);
        AssertSingletonAdapter<CommandDispatcher, ICommandDispatcher>(flourish);
        AssertSingletonAdapter<ShortcutService, IShortcutService>(flourish);
        AssertSingleton<TitleBarService>(flourish);
        AssertSingletonAdapter<TitleBarRuntimeFacade, ITitleBarService>(flourish);
        AssertSingletonAdapter<ProjectCatalogStore, IProjectCatalogStore>(flourish);
        AssertSingletonAdapter<ProjectService, IProjectService>(flourish);
        Assert.IsType<DefaultProjectBehavior>(flourish.GetRequiredService<IProjectBehavior>());
        AssertSingleton<TitleBarSearchService>(flourish);
        AssertSingletonAdapter<WindowService, IWindowService>(flourish);
        AssertSingletonAdapter<WindowCloseService, IWindowCloseService>(flourish);
        AssertSingletonAdapter<ProfileFlyoutService, IProfileFlyoutService>(flourish);
        AssertSingleton<NavigationRouteRegistry>(flourish);
        AssertSingleton<PageCacheService>(flourish);
        AssertSingleton<NavigationService>(flourish);
        AssertSingletonAdapter<NavigationRuntimeFacade, INavigationService>(flourish);
    }

    [Fact]
    public void Build_CustomProjectBehaviorRegistration_ReplacesDefaultBehavior()
    {
        var customBehavior = new TestProjectBehavior();
        var builder = ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices(
                (_, services) => services.AddSingleton<IProjectBehavior>(customBehavior)
            );

        using var flourish = builder.Build();

        Assert.Same(customBehavior, flourish.GetRequiredService<IProjectBehavior>());
    }

    [Fact]
    public void Build_EnablesBuiltInSystemStatusFlags()
    {
        var builder = ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureStatusBar(statusBar => statusBar.SetLanStatusEnabled().SetPowerStatusEnabled());

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<StatusBarOptions>();

        Assert.True(options.IsLANConnectionStatusEnabled);
        Assert.True(options.IsPowerStatusEnabled);
        var statusItem = Assert.Single(options.StatusItems);
        Assert.Equal("OK", statusItem.Text);
        Assert.Equal("\uE930", statusItem.IconGlyph);
    }

    [Fact]
    public void Build_WithDuplicatePageTypeRegistrations_ThrowsInvalidOperationException()
    {
        var builder = ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddNavigable<HomePage>("Home", "H");
                    services.AddNavigable<HomePage>("Start", "S");
                }
            );

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Navigable page types must be unique", exception.Message);
        Assert.Contains(typeof(HomePage).FullName!, exception.Message);
    }

    [Fact]
    public void Build_WithMultipleInitialPages_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddNavigable<HomePage>("Home", "H");
                    services.AddNavigable<SettingsPage>("Settings", "S");
                }
            )
            .ConfigureNavigation(navigation =>
            {
                navigation.AddGroup(
                    null,
                    groupId: 0,
                    group => group.AddNavigableViewItem<HomePage>(isInitial: true)
                );
                navigation.AddFixedNavigableViewItem<SettingsPage>(isInitial: true);
            });

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Only one navigable page", exception.Message);
    }

    [Fact]
    public void Build_OrdersGroupsAndCreatesHeaders()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddNavigable<HomePage>("Home", "H");
                    services.AddNavigable<SettingsPage>("Settings", "S");
                }
            )
            .ConfigureNavigation(navigation =>
            {
                navigation.AddGroup(
                    "Second",
                    groupId: 2,
                    group => group.AddNavigableViewItem<SettingsPage>()
                );
                navigation.AddGroup(
                    "First",
                    groupId: 1,
                    group => group.AddNavigableViewItem<HomePage>()
                );
            });

        using var flourish = builder.Build();
        var items = flourish.GetRequiredService<NavigationOptions>().NavigationItems;

        Assert.Collection(
            items,
            header =>
            {
                Assert.True(header.IsGroupHeader);
                Assert.Equal("First", header.Label);
            },
            home => Assert.Equal("Home", home.Key),
            header =>
            {
                Assert.True(header.IsGroupHeader);
                Assert.Equal("Second", header.Label);
            },
            settings => Assert.Equal("Settings", settings.Key)
        );
    }

    private static IApplicationBuilder CreateNavigationBuilder()
    {
        return ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureNavigation(navigation => navigation.SetEnabled());
    }

    private static void AssertSingletonAdapter<TConcrete, TContract>(IApplicationRuntime flourish)
        where TConcrete : class
        where TContract : class
    {
        Assert.Same(
            flourish.GetRequiredService<TConcrete>(),
            flourish.GetRequiredService<TContract>()
        );
    }

    private static void AssertSingleton<TService>(IApplicationRuntime flourish)
        where TService : class
    {
        Assert.Same(
            flourish.GetRequiredService<TService>(),
            flourish.GetRequiredService<TService>()
        );
    }

    private sealed class HomePage : Page { }

    private sealed class SettingsPage : Page { }

    private sealed class TestProjectBehavior : IProjectBehavior
    {
        public ValueTask<bool> CreateProjectAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);

        public ValueTask<bool> SaveActiveProjectAsync(
            CancellationToken cancellationToken = default
        ) => ValueTask.FromResult(true);

        public ValueTask<bool> ActivateProjectAsync(
            string projectId,
            CancellationToken cancellationToken = default
        ) => ValueTask.FromResult(true);

        public ValueTask<bool> DeleteProjectAsync(
            string projectId,
            CancellationToken cancellationToken = default
        ) => ValueTask.FromResult(true);

        public ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);
    }
}
