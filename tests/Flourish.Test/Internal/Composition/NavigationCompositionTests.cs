using System;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Navigation;

using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class NavigationCompositionTests
{
    [Fact]
    public void Build_WithRegisteredKeyTree_CreatesFinalNavigationModel()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddNavigable<HomePage>("Home", "H");
                    services.AddNavigable<SettingsPage>(
                        "Settings",
                        "S",
                        PageCacheMode.Disabled
                    );
                }
            )
            .ConfigureNavigation(navigation =>
            {
                navigation.AddGroup(
                    null,
                    groupId: 0,
                    group =>
                    {
                        group.AddNavigableViewItem<HomePage>(isInitial: true, parentId: 10);
                        group.AddNavigableViewItem<SettingsPage>(childId: 10);
                    }
                );
            });

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<NavigationOptions>();

        Assert.Collection(
            options.InitialNavigationRoutes,
            home =>
            {
                Assert.Equal("Home", home.NavigationKey);
                Assert.Equal(typeof(HomePage), home.PageType);
            },
            settings =>
            {
                Assert.Equal("Settings", settings.NavigationKey);
                Assert.Equal(typeof(SettingsPage), settings.PageType);
                Assert.Equal(PageCacheMode.Disabled, settings.CacheMode);
            }
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
        var builder = ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddNavigable<FirstFeature.SettingsPage>("First settings", "1");
                    services.AddNavigable<SecondFeature.SettingsPage>("Second settings", "2");
                }
            );

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Navigation keys must be unique", exception.Message);
        Assert.Contains("'Settings'", exception.Message);
        Assert.Contains(typeof(FirstFeature.SettingsPage).FullName!, exception.Message);
        Assert.Contains(typeof(SecondFeature.SettingsPage).FullName!, exception.Message);
    }

    [Fact]
    public void Build_WithUnregisteredPageType_ThrowsInvalidOperationException()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureNavigation(navigation =>
                navigation.AddGroup(
                    null,
                    groupId: 0,
                    group => group.AddNavigableViewItem<UnregisteredPage>()
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
            .ConfigureServices((_, services) => services.AddNavigable<HomePage>("Home", "H"))
            .ConfigureNavigation(navigation =>
            {
                navigation.AddGroup(
                    null,
                    groupId: 0,
                    group => group.AddNavigableViewItem<HomePage>()
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
        var builder = CreateNavigationBuilder()
            .ConfigureNavigation(navigation =>
                navigation.AddGroup(
                    null,
                    groupId: 0,
                    group =>
                        group.AddNavigableItem("Orphan", null, "cmd_orphan_command", childId: 42)
                )
            );

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("childId 42", exception.Message);
        Assert.Contains("does not match a parentId", exception.Message);
    }

    [Fact]
    public void Build_WithNavigationEnabledAndNoVisibleConfiguration_KeepsMenuEmpty()
    {
        var builder = CreateNavigationBuilder()
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddNavigable<HomePage>("Home", "H");
                    services.AddNavigable<SettingsPage>("Settings", "S");
                }
            );

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<NavigationOptions>();

        Assert.Empty(options.NavigationItems);
        Assert.Equal(2, options.InitialNavigationRoutes.Count);
    }

    [Fact]
    public void Build_WithNavigationDisabled_DoesNotCreateVisibleItems()
    {
        var builder = ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureNavigation(navigation =>
                navigation.AddGroup(
                    null,
                    groupId: 0,
                    group => group.AddNavigableViewItem<UnregisteredPage>()
                )
            );

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<NavigationOptions>();

        Assert.False(options.IsNavigationPanelEnabled);
        Assert.Empty(options.NavigationItems);
        Assert.Empty(options.FixedNavigationItems);
    }

    private static IApplicationBuilder CreateNavigationBuilder()
    {
        return ApplicationBuilder
            .CreateDefaultBuilder([])
            .ConfigureNavigation(navigation => navigation.SetEnabled());
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
