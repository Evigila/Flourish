using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishCompositionContractTests
{
    [Fact]
    public void Build_WithoutDataConfiguration_UsesBuiltInEnglishLocale()
    {
        using var flourish = FlourishBuilder.CreateDefaultBuilder([]).Build();

        var localization = flourish.GetRequiredService<FlourishLocalizationService>();

        Assert.Equal("en-US", localization.CurrentLocale);
        Assert.Equal("Close", localization.Get(FlourishLocaleKeys.TitleBarClose));
    }

    [Fact]
    public void Build_WithoutOptionalConfigurations_AppliesLatentDefaultsWithoutEnablingShell()
    {
        using var flourish = FlourishBuilder.CreateDefaultBuilder([]).Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();

        Assert.False(options.IsTitlebarEnabled);
        Assert.False(options.IsNavigationPanelEnabled);
        Assert.False(options.Motion.IsEnabled);
        Assert.False(options.IsStatusBarEnabled);
        Assert.False(options.IsDynamicToolbarEnabled);

        Assert.True(options.IsBreadcrumbEnabled);
        Assert.True(options.IsTitlebarNavigationToggleEnabled);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.True(options.IsTitlebarTitleEnabled);
        Assert.True(options.IsTitlebarProfileEnabled);
        Assert.True(options.IsTitlebarThemeToggleEnabled);
        Assert.False(options.IsTitlebarSearchEnabled);
        Assert.Equal("MyApp", options.ApplicationTitle);
        Assert.Equal("MyApp", options.ApplicationSubtitle);

        Assert.Equal(NavigationPanelDirection.Left, options.NavigationPanelDirection);
        Assert.True(options.IsNavigationPanelInitiallyOpen);
        Assert.Equal(250, options.OpenPaneWidth);
        Assert.Equal(64, options.ClosedPaneWidth);
        Assert.Equal(180, options.NavigationPaneMinWidth);
        Assert.Equal(520, options.NavigationPaneMaxWidth);
        Assert.Empty(options.NavigationGroups);
        Assert.Empty(options.FixedNavigationItemDefinitions);

        Assert.True(options.Motion.IsHoverRevealEnabled);
        Assert.True(options.Motion.RespectSystemReducedMotion);
        Assert.Equal(1536, options.WindowWidth);
        Assert.Equal(864, options.WindowHeight);
        Assert.Equal(1280, options.WindowMinWidth);
        Assert.Equal(720, options.WindowMinHeight);
        Assert.False(options.WindowTopmost);
        Assert.False(options.IsTrayExitEnabled);

        var statusItem = Assert.Single(options.StatusItems);
        Assert.Equal("OK", statusItem.Text);
        Assert.Equal("\uE930", statusItem.IconGlyph);
        Assert.True(options.IsLANConnectionStatusEnabled);
        Assert.True(options.IsPowerStatusEnabled);
        Assert.Empty(options.ToolbarItems);
        Assert.Empty(options.DynamicToolbarItems);
    }

    [Fact]
    public void Build_ExplicitOptionalConfigurations_OverrideImplicitDefaultsInOrder()
    {
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigData(data => data.InitLocale("zh-CN"))
            .ConfigTitleBar(titleBar =>
                titleBar
                    .InitApplicationTitle("Configured")
                    .UseThemeToggle(mode: FlourishTheme.Dark)
            )
            .ConfigNavigation(navigation =>
                navigation
                    .InitDirection(NavigationPanelDirection.Right)
                    .InitInitiallyOpen(false)
                    .InitPanelWidth(300, 64, 600, 200)
            )
            .ConfigMotion(motion =>
                motion
                    .UseHoverRevealAnimation(
                        duration: TimeSpan.FromMilliseconds(250)
                    )
                    .UseSystemReducedMotion(false)
            )
            .ConfigWindow(window =>
                window
                    .InitWindowSize(1400, 800)
                    .InitWindowMinSize(900, 600)
                    .UseTopmost(false)
                    .UseTrayExit(false)
            )
            .Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();
        var data = flourish.GetRequiredService<FlourishDataOptions>();

        Assert.Equal("zh-CN", data.Locale);
        Assert.Equal("Configured", options.ApplicationTitle);
        Assert.Equal(FlourishTheme.Dark, options.DefaultTheme);
        Assert.Equal(NavigationPanelDirection.Right, options.NavigationPanelDirection);
        Assert.False(options.IsNavigationPanelInitiallyOpen);
        Assert.Equal(300, options.OpenPaneWidth);
        Assert.Equal(200, options.NavigationPaneMinWidth);
        Assert.Equal(600, options.NavigationPaneMaxWidth);
        Assert.Equal(TimeSpan.FromMilliseconds(250), options.Motion.HoverRevealAnimationDuration);
        Assert.False(options.Motion.RespectSystemReducedMotion);
        Assert.Equal(1400, options.WindowWidth);
        Assert.Equal(800, options.WindowHeight);
        Assert.Equal(900, options.WindowMinWidth);
        Assert.Equal(600, options.WindowMinHeight);
        Assert.False(options.WindowTopmost);
        Assert.False(options.IsTrayExitEnabled);
    }

    [Fact]
    public void Build_WithOnlyLocaleConfiguration_DoesNotRequireApplicationIdentity()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigData(data => data.InitLocale("en-US"));

        using var flourish = builder.Build();
        var localization = flourish.GetRequiredService<FlourishLocalizationService>();

        Assert.Equal("en-US", localization.CurrentLocale);
        Assert.Equal("Close", localization.Get(FlourishLocaleKeys.TitleBarClose));
    }

    [Fact]
    public void Build_RegistersRuntimeConfigurationAndAppearanceContractsAsSingletonAdapters()
    {
        using var flourish = FlourishBuilder.CreateDefaultBuilder([]).Build();

        Assert.Same(
            flourish.GetRequiredService<FlourishLocalizationService>(),
            flourish.GetRequiredService<IFlourishLocalization>()
        );
        Assert.Same(
            flourish.GetRequiredService<FlourishConfigurationService>(),
            flourish.GetRequiredService<IFlourishConfiguration>()
        );
        Assert.Same(
            flourish.GetRequiredService<AppPreferenceService>(),
            flourish.GetRequiredService<IAppSettingsStore>()
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
            flourish.GetRequiredService<FlourishToolTipService>(),
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
            flourish.GetRequiredService<FlourishMotionService>(),
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
        using var flourish = FlourishBuilder.CreateDefaultBuilder([]).Build();

        AssertSingletonAdapter<NavigationPanelService, INavigationPanelService>(flourish);
        AssertSingletonAdapter<NavigationMenuService, INavigationMenuService>(flourish);
        AssertSingletonAdapter<FlourishToolbarService, IToolbarService>(flourish);
        AssertSingletonAdapter<FlourishStatusService, IStatusBarService>(flourish);
        AssertSingletonAdapter<ShellRegionService, IShellRegionService>(flourish);
        AssertSingletonAdapter<FlourishBackgroundTaskService, IBackgroundTaskService>(flourish);
        AssertSingletonAdapter<NotificationService, INotificationService>(flourish);
        AssertSingletonAdapter<TrayIconService, ITrayService>(flourish);
        AssertSingletonAdapter<CommandDispatcher, ICommandRegistry>(flourish);
        AssertSingletonAdapter<CommandDispatcher, ICommandDispatcher>(flourish);
        AssertSingletonAdapter<ShortcutService, IShortcutService>(flourish);
        AssertSingletonAdapter<TitleBarService, ITitleBarService>(flourish);
        AssertSingletonAdapter<ProjectService, IProjectService>(flourish);
        Assert.IsType<DefaultProjectBehavior>(
            flourish.GetRequiredService<IProjectBehavior>()
        );
        AssertSingletonAdapter<TitleBarSearchService, ITitleBarSearchService>(flourish);
        AssertSingletonAdapter<WindowService, IWindowService>(flourish);
        AssertSingletonAdapter<WindowCloseService, IWindowCloseService>(flourish);
        AssertSingletonAdapter<ProfileFlyoutService, IProfileFlyoutService>(flourish);
        AssertSingletonAdapter<ShellFeatureService, IShellFeatureService>(flourish);
        AssertSingletonAdapter<NavigationRouteRegistry, INavigationRouteRegistry>(flourish);
        AssertSingletonAdapter<PageCacheService, IPageCacheService>(flourish);
        AssertSingletonAdapter<NavigationService, INavigationService>(flourish);
        AssertSingletonAdapter<NavigationService, IFrameNavigationService>(flourish);
    }

    [Fact]
    public void Build_CustomProjectBehaviorRegistration_ReplacesDefaultBehavior()
    {
        var customBehavior = new TestProjectBehavior();
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigServices((_, services) =>
                services.AddSingleton<IProjectBehavior>(customBehavior)
            );

        using var flourish = builder.Build();

        Assert.Same(customBehavior, flourish.GetRequiredService<IProjectBehavior>());
    }

    [Fact]
    public void Build_EnablesBuiltInSystemStatusFlags()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigStatusBar(statusBar =>
                statusBar.UseLanConnectionStatus().UsePowerStatus()
            );

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();

        Assert.True(options.IsLANConnectionStatusEnabled);
        Assert.True(options.IsPowerStatusEnabled);
        var statusItem = Assert.Single(options.StatusItems);
        Assert.Equal("OK", statusItem.Text);
        Assert.Equal("\uE930", statusItem.IconGlyph);
    }

    [Fact]
    public void Build_WithDuplicatePageTypeRegistrations_ThrowsInvalidOperationException()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigServices((_, services) =>
            {
                services.AddNavigable<HomePage>("Home", "H");
                services.AddNavigable<HomePage>("Start", "S");
            });

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Navigable page types must be unique", exception.Message);
        Assert.Contains(typeof(HomePage).FullName!, exception.Message);
    }

    [Fact]
    public void Build_WithMultipleInitialPages_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder()
            .ConfigServices((_, services) =>
            {
                services.AddNavigable<HomePage>("Home", "H");
                services.AddNavigable<SettingsPage>("Settings", "S");
            })
            .ConfigNavigation(navigation =>
            {
                navigation.InitGroup(null, groupId: 0, group =>
                    group.InitNavigableViewItem<HomePage>(isInitial: true)
                );
                navigation.InitFixedNavigableViewItem<SettingsPage>(isInitial: true);
            });

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Only one navigable page", exception.Message);
    }

    [Fact]
    public void Build_OrdersGroupsAndCreatesHeaders()
    {
        var builder = CreateNavigationBuilder()
            .ConfigServices((_, services) =>
            {
                services.AddNavigable<HomePage>("Home", "H");
                services.AddNavigable<SettingsPage>("Settings", "S");
            })
            .ConfigNavigation(navigation =>
            {
                navigation.InitGroup("Second", groupId: 2, group =>
                    group.InitNavigableViewItem<SettingsPage>()
                );
                navigation.InitGroup("First", groupId: 1, group =>
                    group.InitNavigableViewItem<HomePage>()
                );
            });

        using var flourish = builder.Build();
        var items = flourish.GetRequiredService<FlourishShellOptions>().NavigationItems;

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

    private static IFlourishBuilder CreateNavigationBuilder()
    {
        return FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigShell(shell => shell.UseNavigation());
    }

    private static void AssertSingletonAdapter<TConcrete, TContract>(IFlourish flourish)
        where TConcrete : class
        where TContract : class
    {
        Assert.Same(
            flourish.GetRequiredService<TConcrete>(),
            flourish.GetRequiredService<TContract>()
        );
    }

    private sealed class HomePage : Page { }

    private sealed class SettingsPage : Page { }

    private sealed class TestProjectBehavior : IProjectBehavior
    {
        public ValueTask<bool> CreateProjectAsync(
            CancellationToken cancellationToken = default
        ) => ValueTask.FromResult(true);

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

        public ValueTask<bool> CanCloseAsync(
            CancellationToken cancellationToken = default
        ) => ValueTask.FromResult(true);
    }
}
