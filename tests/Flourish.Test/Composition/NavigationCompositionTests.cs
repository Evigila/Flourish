using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class NavigationCompositionTests
{
    [Fact]
    public void Build_WithRegisteredKeyTree_CreatesFinalNavigationModel()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddNavigable<HomePage>("Home", "H", navigationKey: "home");
                services.AddNavigable<SettingsPage>(
                    "Settings",
                    "S",
                    FlourishPageCacheMode.Disabled,
                    "settings"
                );
            })
            .ConfigureNavigation(navigation =>
            {
                navigation.SetGroup(null, groupId: 0, group =>
                {
                    group.AddNavigableViewItem("home", isInitial: true, parentId: 10);
                    group.AddNavigableViewItem("settings", childId: 10);
                });
            });

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();

        Assert.Equal(typeof(HomePage), options.PageTypesByNavigationKey["home"]);
        Assert.Equal("settings", options.NavigationKeysByPageType[typeof(SettingsPage)]);
        Assert.Equal(
            FlourishPageCacheMode.Disabled,
            options.PageCacheModesByPageType[typeof(SettingsPage)]
        );
        Assert.Equal("home", options.InitialNavigationKey);
        Assert.Equal(typeof(HomePage), options.InitialNavigationPageType);

        Assert.Collection(
            options.NavigationItems,
            home =>
            {
                Assert.Equal("home", home.Key);
                Assert.Equal("Home", home.Label);
                Assert.Equal("H", home.IconGlyph);
                Assert.True(home.HasChildren);
                Assert.True(home.IsVisible);
            },
            settings =>
            {
                Assert.Equal("settings", settings.Key);
                Assert.Equal("Settings", settings.Label);
                Assert.Equal("S", settings.IconGlyph);
                Assert.True(settings.IsChild);
                Assert.False(settings.IsVisible);
            }
        );
    }

    [Fact]
    public void Build_WithDuplicateNavigationKey_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder().ConfigureServices((_, services) =>
        {
            services.AddNavigable<HomePage>("Home", "H", navigationKey: "duplicate");
            services.AddNavigable<SettingsPage>(
                "Settings",
                "S",
                navigationKey: "duplicate"
            );
        });

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Navigation keys must be unique", exception.Message);
        Assert.Contains("duplicate", exception.Message);
    }

    [Fact]
    public void Build_WithUnregisteredNavigationKey_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder().ConfigureNavigation(navigation =>
            navigation.SetGroup(null, groupId: 0, group =>
                group.AddNavigableViewItem("missing")
            )
        );

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Navigation key 'missing'", exception.Message);
        Assert.Contains("must be registered with AddNavigable", exception.Message);
    }

    [Fact]
    public void Build_WithPageInGroupAndFixedArea_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureServices((_, services) =>
                services.AddNavigable<HomePage>("Home", "H", navigationKey: "home")
            )
            .ConfigureNavigation(navigation =>
            {
                navigation.SetGroup(null, groupId: 0, group =>
                    group.AddNavigableViewItem("home")
                );
                navigation.AddFixedNavigableViewItem("home");
            });

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("A page can only be added to one navigation location", exception.Message);
        Assert.Contains(typeof(HomePage).FullName!, exception.Message);
        Assert.Contains("group 0", exception.Message);
        Assert.Contains("fixed navigation items", exception.Message);
    }

    [Fact]
    public void Build_WithOrphanedChild_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder().ConfigureNavigation(navigation =>
            navigation.SetGroup(null, groupId: 0, group =>
                group.AddNavigableItem("Orphan", "orphan.command", childId: 42)
            )
        );

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("childId 42", exception.Message);
        Assert.Contains("does not match a parentId", exception.Message);
    }

    [Fact]
    public void Build_WithNavigationEnabledAndNoVisibleConfiguration_CreatesLegacyList()
    {
        var builder = CreateNavigationBuilder().ConfigureServices((_, services) =>
        {
            services.AddNavigable<HomePage>("Home", "H", navigationKey: "home");
            services.AddNavigable<SettingsPage>(
                "Settings",
                "S",
                navigationKey: "settings"
            );
        });

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();

        Assert.Collection(
            options.NavigationItems,
            home => Assert.Equal("home", home.Key),
            settings => Assert.Equal("settings", settings.Key)
        );
    }

    [Fact]
    public void Build_WithNavigationDisabled_DoesNotCreateVisibleItems()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureNavigation(navigation =>
                navigation.SetGroup(null, groupId: 0, group =>
                    group.AddNavigableViewItem("not-registered")
                )
            );

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();

        Assert.False(options.IsNavigationPanelEnabled);
        Assert.Empty(options.NavigationItems);
        Assert.Empty(options.FixedNavigationItems);
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
