using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishCompositionContractTests
{
    [Fact]
    public void Build_WithoutDataConfiguration_UsesBuiltInEnglishLocale()
    {
        using var flourish = FlourishBuilder.CreateDefaultBuilder([]).Build();

        var localization = flourish.GetRequiredService<FlourishLocalizationService>();

        Assert.Equal("EN", localization.CurrentLocale);
        Assert.Equal("Close", localization.Get(FlourishLocaleKeys.TitleBarClose));
    }

    [Fact]
    public void Build_WithOnlyLocaleConfiguration_DoesNotRequireApplicationIdentity()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetLocale("EN"));

        using var flourish = builder.Build();
        var localization = flourish.GetRequiredService<FlourishLocalizationService>();

        Assert.Equal("EN", localization.CurrentLocale);
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
    public void Build_EnablesBuiltInSystemStatusFlags()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureStatusBar(statusBar =>
                statusBar.ShowLANConnectionStatus().ShowPowerStatus()
            );

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();

        Assert.True(options.IsLANConnectionStatusEnabled);
        Assert.True(options.IsPowerStatusEnabled);
        Assert.Empty(options.StatusItems);
    }

    [Fact]
    public void Build_WithDuplicatePageTypeRegistrations_ThrowsInvalidOperationException()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices((_, services) =>
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
            .ConfigureServices((_, services) =>
            {
                services.AddNavigable<HomePage>("Home", "H");
                services.AddNavigable<SettingsPage>("Settings", "S");
            })
            .ConfigureNavigation(navigation =>
            {
                navigation.SetGroup(null, groupId: 0, group =>
                    group.AddNavigableViewItem<HomePage>(isInitial: true)
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
            .ConfigureServices((_, services) =>
            {
                services.AddNavigable<HomePage>("Home", "H");
                services.AddNavigable<SettingsPage>("Settings", "S");
            })
            .ConfigureNavigation(navigation =>
            {
                navigation.SetGroup("Second", groupId: 2, group =>
                    group.AddNavigableViewItem<SettingsPage>()
                );
                navigation.SetGroup("First", groupId: 1, group =>
                    group.AddNavigableViewItem<HomePage>()
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
            .ConfigureShell(shell => shell.UseNavigation());
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
}
