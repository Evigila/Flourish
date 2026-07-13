using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class NavigationRuntimeSurfaceTests
{
    [Fact]
    public void NavigationPanel_MutationsUpdateOptionsAndPublishVersionedSnapshots()
    {
        var options = new FlourishShellOptions
        {
            IsNavigationPanelEnabled = true,
            IsNavigationPanelInitiallyOpen = false,
        };
        var sut = new NavigationPanelService(options);
        var changes = new List<FlourishNavigationPanelChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.Open();
        sut.SetDirection(NavigationPanelDirection.Right);
        sut.SetPanelWidth(280, 56, 500, 180);

        Assert.True(sut.Current.IsOpen);
        Assert.Equal(NavigationPanelDirection.Right, options.NavigationPanelDirection);
        Assert.Equal(280, options.OpenPaneWidth);
        Assert.Equal(3, sut.Current.Version);
        Assert.Equal(3, changes.Count);
        Assert.True(changes[0].Animate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(56)]
    public void NavigationPanel_SetPanelWidthAcceptsHiddenOrMinimumVisibleCollapsedWidth(
        double closedWidth
    )
    {
        var options = new FlourishShellOptions();
        var sut = new NavigationPanelService(options);

        sut.SetPanelWidth(280, closedWidth, 500, 180);

        Assert.Equal(closedWidth, options.ClosedPaneWidth);
        Assert.Equal(closedWidth, sut.Current.ClosedWidth);
        Assert.Equal(1, sut.Current.Version);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(55)]
    public void NavigationPanel_SetPanelWidthRejectsUndersizedVisibleCollapsedWidth(
        double closedWidth
    )
    {
        var options = new FlourishShellOptions();
        var sut = new NavigationPanelService(options);
        var before = sut.Current;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPanelWidth(280, closedWidth, 500, 180)
        );

        Assert.Equal("closedWidth", exception.ParamName);
        Assert.Equal(before, sut.Current);
        Assert.Contains("0 (fully hidden) or at least 56", exception.Message);
    }

    [Fact]
    public void NavigationMenu_UpdateCommitsBatchOnceAndRebuildsShellItems()
    {
        var options = CreateRouteOptions();
        var routes = new NavigationRouteRegistry(options);
        var sut = new NavigationMenuService(options, routes);
        var eventCount = 0;
        sut.Changed += (_, _) => eventCount++;

        sut.Update(editor =>
        {
            editor.RemoveItem("Home");
            editor.RemoveGroup("group:0");
            editor.AddGroup("runtime", "Runtime APIs");
            editor.AddItem(
                "runtime",
                FlourishNavigationMenuItem.Page("home-demo", "Home", "Home", "H")
            );
            editor.AddItem(
                "runtime",
                FlourishNavigationMenuItem.Command(
                    "runtime-command",
                    "Run",
                    "R",
                    "demo.run",
                    parentId: "home-demo"
                )
            );
            editor.SetItemExpanded("home-demo", true);
            editor.SetItemEnabled("runtime-command", false);
        });

        Assert.Equal(1, eventCount);
        Assert.Equal(1, sut.Current.Version);
        var group = Assert.Single(sut.Current.Groups);
        Assert.Equal("runtime", group.Id);
        Assert.Equal(2, group.Items.Count);
        Assert.True(group.Items[0].IsExpanded);
        Assert.False(group.Items[1].IsEnabled);
        Assert.Contains(options.NavigationItems, item => item.Id == "home-demo");
        var child = Assert.Single(options.NavigationItems, item => item.Id == "runtime-command");
        Assert.True(child.IsVisible);
        Assert.False(child.IsEnabled);
    }

    [Fact]
    public void NavigationMenu_WhenBatchValidationFails_DoesNotCommitPartialChanges()
    {
        var options = CreateRouteOptions();
        var sut = new NavigationMenuService(options, new NavigationRouteRegistry(options));
        var before = sut.Current;

        Assert.Throws<InvalidOperationException>(() =>
            sut.Update(editor =>
            {
                editor.AddGroup("runtime");
                editor.AddItem(
                    "runtime",
                    FlourishNavigationMenuItem.Page(
                        "missing",
                        "NotRegistered",
                        "Missing"
                    )
                );
            })
        );

        Assert.Equal(before.Groups.Count, sut.Current.Groups.Count);
        Assert.Equal(
            before.Groups.SelectMany(group => group.Items).Select(item => item.Id),
            sut.Current.Groups.SelectMany(group => group.Items).Select(item => item.Id)
        );
        Assert.Equal(0, sut.Current.Version);
        Assert.Single(options.NavigationItems);
    }

    [Fact]
    public void NavigationMenu_SeedsRegisteredRoutesEvenWhenPanelStartedDisabled()
    {
        var options = CreateRouteOptions();
        options.IsNavigationPanelEnabled = false;

        var sut = new NavigationMenuService(options, new NavigationRouteRegistry(options));

        var item = Assert.Single(Assert.Single(sut.Current.Groups).Items);
        Assert.Equal("Home", item.NavigationKey);
        Assert.Contains(options.NavigationItems, candidate => candidate.Key == "Home");
    }

    [Fact]
    public void RemovingRoute_RemovesItsMenuItemAndHistoryEntries()
    {
        var options = CreateRouteOptions();
        options.NavigationItems.Add(
            new FlourishNavigationItem(
                "Home",
                "Home",
                "H",
                0,
                FlourishNavigationItemKind.Page,
                typeof(HomePage),
                id: "home-menu"
            )
        );
        var routes = new NavigationRouteRegistry(options);
        var menu = new NavigationMenuService(options, routes);

        Assert.True(routes.Remove("Home"));

        Assert.Empty(menu.Current.Groups.SelectMany(group => group.Items));
        Assert.Empty(options.NavigationItems);
    }

    private static FlourishShellOptions CreateRouteOptions()
    {
        var options = new FlourishShellOptions();
        options.PageTypesByNavigationKey["Home"] = typeof(HomePage);
        options.NavigationKeysByPageType[typeof(HomePage)] = "Home";
        options.PageCacheModesByPageType[typeof(HomePage)] = FlourishPageCacheMode.Disabled;
        return options;
    }

    private sealed class HomePage : Page { }
}
