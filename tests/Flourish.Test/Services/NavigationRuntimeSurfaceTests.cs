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
        sut.SetPanelWidth(280, 64, 500, 180);

        Assert.True(sut.Current.IsOpen);
        Assert.Equal(NavigationPanelDirection.Right, options.NavigationPanelDirection);
        Assert.Equal(280, options.OpenPaneWidth);
        Assert.Equal(3, sut.Current.Version);
        Assert.Equal(3, changes.Count);
        Assert.True(changes[0].Animate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(64)]
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
    [InlineData(63)]
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
        Assert.Contains("0 (fully hidden) or at least 64", exception.Message);
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
        Assert.Empty(options.NavigationItems);
    }

    [Fact]
    public void NavigationMenu_DoesNotCreateVisibleItemsFromRoutes()
    {
        var options = CreateRouteOptions();
        options.IsNavigationPanelEnabled = false;

        var sut = new NavigationMenuService(options, new NavigationRouteRegistry(options));

        Assert.Empty(sut.Current.Groups);
        Assert.Empty(sut.Current.FixedItems);
        Assert.Empty(options.NavigationItems);
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

    [Fact]
    public void RemovingParentPageRoute_CascadesItsChildrenAndKeepsMenuConsistent()
    {
        var options = new FlourishShellOptions();
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute("Parent", typeof(ParentPage))
        );
        options.NavigationItems.Add(
            new FlourishNavigationItem(
                "Parent",
                "Parent",
                "P",
                0,
                FlourishNavigationItemKind.Page,
                typeof(ParentPage),
                parentId: 7,
                id: "parent-page"
            )
        );
        options.NavigationItems.Add(
            new FlourishNavigationItem(
                "child-command",
                "Child",
                "C",
                0,
                FlourishNavigationItemKind.Command,
                commandKey: "child.run",
                childId: 7,
                id: "child-command"
            )
        );
        options.NavigationItems.Add(
            new FlourishNavigationItem(
                "sibling-command",
                "Sibling",
                "S",
                0,
                FlourishNavigationItemKind.Command,
                commandKey: "sibling.run",
                id: "sibling-command"
            )
        );
        var routes = new NavigationRouteRegistry(options);
        var menu = new NavigationMenuService(options, routes);
        var eventCount = 0;
        menu.Changed += (_, _) => eventCount++;

        Assert.True(routes.Remove("Parent"));

        var remaining = Assert.Single(menu.Current.Groups).Items;
        Assert.Equal("sibling-command", Assert.Single(remaining).Id);
        Assert.Equal("sibling-command", Assert.Single(options.NavigationItems).Id);
        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void NavigationMenu_ReconcilesRouteRemovalCompletedBeforeConstruction()
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
        Assert.True(routes.Remove("Home"));

        var menu = new NavigationMenuService(options, routes);

        Assert.Empty(menu.Current.Groups.SelectMany(group => group.Items));
        Assert.Empty(options.NavigationItems);
        Assert.Equal(0, menu.Current.Version);
    }

    [Fact]
    public void NavigationMenu_IgnoresOlderRouteEventDeliveredAfterNewerSnapshot()
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
        INavigationRouteRegistration? replacement = null;
        var reentered = false;
        routes.Changed += (_, change) =>
        {
            if (
                !reentered
                && change.ChangeKind == FlourishRuntimeChangeKind.Removed
                && change.PreviousRoute?.NavigationKey == "Home"
            )
            {
                reentered = true;
                replacement = routes.Register(
                    new FlourishNavigationRoute("Home", typeof(ReplacementHomePage))
                );
            }
        };
        var menu = new NavigationMenuService(options, routes);

        Assert.True(routes.Remove("Home"));

        Assert.True(routes.Contains("Home"));
        var item = Assert.Single(Assert.Single(menu.Current.Groups).Items);
        Assert.Equal("home-menu", item.Id);
        Assert.Equal(
            typeof(ReplacementHomePage),
            Assert.Single(options.NavigationItems).PageType
        );
        Assert.Equal(1, menu.Current.Version);

        replacement!.Dispose();
    }

    private static FlourishShellOptions CreateRouteOptions()
    {
        var options = new FlourishShellOptions();
        options.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute("Home", typeof(HomePage))
        );
        return options;
    }

    private sealed class HomePage : Page { }

    private sealed class ParentPage : Page { }

    private sealed class ReplacementHomePage : Page { }
}
