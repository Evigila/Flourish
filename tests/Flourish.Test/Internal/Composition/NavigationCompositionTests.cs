using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class NavigationCompositionTests
{
    [Fact]
    public void Build_WithRegisteredKeyTree_CreatesFinalNavigationModel()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddNavigable<HomePage>("Home", "H");
                services.AddNavigable<SettingsPage>(
                    "Settings",
                    "S",
                    FlourishPageCacheMode.Disabled
                );
            })
            .ConfigureNavigation(navigation =>
            {
                navigation.SetGroup(null, groupId: 0, group =>
                {
                    group.AddNavigableViewItem<HomePage>(isInitial: true, parentId: 10);
                    group.AddNavigableViewItem<SettingsPage>(childId: 10);
                });
            });

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();

        Assert.Equal(typeof(HomePage), options.PageTypesByNavigationKey["Home"]);
        Assert.Equal("Settings", options.NavigationKeysByPageType[typeof(SettingsPage)]);
        Assert.Equal(
            FlourishPageCacheMode.Disabled,
            options.PageCacheModesByPageType[typeof(SettingsPage)]
        );
        Assert.Equal("Home", options.InitialNavigationKey);
        Assert.Equal(typeof(HomePage), options.InitialNavigationPageType);

        Assert.Collection(
            options.NavigationItems,
            home =>
            {
                Assert.Equal("Home", home.Key);
                Assert.Equal("Home", home.Label);
                Assert.Equal("H", home.IconGlyph);
                Assert.True(home.HasChildren);
                Assert.True(home.IsVisible);
            },
            settings =>
            {
                Assert.Equal("Settings", settings.Key);
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
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices((_, services) =>
            {
                services.AddNavigable<FirstFeature.SettingsPage>("First settings", "1");
                services.AddNavigable<SecondFeature.SettingsPage>("Second settings", "2");
            });

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Navigation keys must be unique", exception.Message);
        Assert.Contains("'Settings'", exception.Message);
        Assert.Contains(typeof(FirstFeature.SettingsPage).FullName!, exception.Message);
        Assert.Contains(typeof(SecondFeature.SettingsPage).FullName!, exception.Message);
    }

    [Fact]
    public void Build_WithUnregisteredPageType_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder().ConfigureNavigation(navigation =>
            navigation.SetGroup(null, groupId: 0, group =>
                group.AddNavigableViewItem<UnregisteredPage>()
            )
        );

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains(typeof(UnregisteredPage).FullName!, exception.Message);
        Assert.Contains("must be registered with AddNavigable", exception.Message);
    }

    [Fact]
    public void Build_WithPageInGroupAndFixedArea_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureServices((_, services) =>
                services.AddNavigable<HomePage>("Home", "H")
            )
            .ConfigureNavigation(navigation =>
            {
                navigation.SetGroup(null, groupId: 0, group =>
                    group.AddNavigableViewItem<HomePage>()
                );
                navigation.AddFixedNavigableViewItem<HomePage>();
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
                group.AddNavigableItem("Orphan", null, "orphan.command", childId: 42)
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
            services.AddNavigable<HomePage>("Home", "H");
            services.AddNavigable<SettingsPage>("Settings", "S");
        });

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();

        Assert.Collection(
            options.NavigationItems,
            home => Assert.Equal("Home", home.Key),
            settings => Assert.Equal("Settings", settings.Key)
        );
    }

    [Fact]
    public void Build_WithNavigationDisabled_DoesNotCreateVisibleItems()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureNavigation(navigation =>
                navigation.SetGroup(null, groupId: 0, group =>
                    group.AddNavigableViewItem<UnregisteredPage>()
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

    private sealed class UnregisteredPage : Page { }

    private static class FirstFeature
    {
        internal sealed class SettingsPage : Page { }
    }

    private static class SecondFeature
    {
        internal sealed class SettingsPage : Page { }
    }
}
