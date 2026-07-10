using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishCompositionContractTests
{
    [Fact]
    public void Build_WithoutDataConfiguration_UsesBuiltInChineseLocale()
    {
        using var flourish = FlourishBuilder.CreateDefaultBuilder([]).Build();

        var localization = flourish.GetRequiredService<FlourishLocalizationService>();

        Assert.Equal("CN", localization.Locale);
        Assert.Equal("关闭", localization.Get(FlourishLocaleKeys.TitleBarClose));
    }

    [Fact]
    public void Build_WithOnlyLocaleConfiguration_DoesNotRequireApplicationIdentity()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetLocale("EN"));

        using var flourish = builder.Build();
        var localization = flourish.GetRequiredService<FlourishLocalizationService>();

        Assert.Equal("EN", localization.Locale);
        Assert.Equal("Close", localization.Get(FlourishLocaleKeys.TitleBarClose));
    }

    [Fact]
    public void Build_AppliesLocaleBeforeBuiltInStatusItems()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetLocale("EN"))
            .ConfigureStatusBar(statusBar => statusBar.ShowPowerStatus());

        using var flourish = builder.Build();
        var item = Assert.Single(
            flourish.GetRequiredService<FlourishShellOptions>().StatusItems
        );

        Assert.Equal("Power", item.Text);
    }

    [Fact]
    public void Build_WithOnlyExplicitPreferencePath_DoesNotRequireApplicationIdentity()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetAppPreferenceDataPath(@"C:\Portable\Flourish"));

        using var flourish = builder.Build();

        Assert.Equal(
            @"C:\Portable\Flourish",
            flourish.GetRequiredService<FlourishDataOptions>().AppPreferenceDataPath
        );
    }

    [Fact]
    public void Build_WithDuplicatePageTypeRegistrations_ThrowsInvalidOperationException()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices((_, services) =>
            {
                services.AddNavigable<HomePage>("Home", "H", navigationKey: "home");
                services.AddNavigable<HomePage>("Start", "S", navigationKey: "start");
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
                services.AddNavigable<HomePage>("Home", "H", navigationKey: "home");
                services.AddNavigable<SettingsPage>(
                    "Settings",
                    "S",
                    navigationKey: "settings"
                );
            })
            .ConfigureNavigation(navigation =>
            {
                navigation.SetGroup(null, groupId: 0, group =>
                    group.AddNavigableViewItem("home", isInitial: true)
                );
                navigation.AddFixedNavigableViewItem("settings", isInitial: true);
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
                services.AddNavigable<HomePage>("Home", "H", navigationKey: "home");
                services.AddNavigable<SettingsPage>(
                    "Settings",
                    "S",
                    navigationKey: "settings"
                );
            })
            .ConfigureNavigation(navigation =>
            {
                navigation.SetGroup("Second", groupId: 2, group =>
                    group.AddNavigableViewItem("settings")
                );
                navigation.SetGroup("First", groupId: 1, group =>
                    group.AddNavigableViewItem("home")
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
            home => Assert.Equal("home", home.Key),
            header =>
            {
                Assert.True(header.IsGroupHeader);
                Assert.Equal("Second", header.Label);
            },
            settings => Assert.Equal("settings", settings.Key)
        );
    }

    private static IFlourishBuilder CreateNavigationBuilder()
    {
        return FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureShell(shell => shell.UseNavigation());
    }

    private sealed class HomePage : Page { }

    private sealed class SettingsPage : Page { }
}
