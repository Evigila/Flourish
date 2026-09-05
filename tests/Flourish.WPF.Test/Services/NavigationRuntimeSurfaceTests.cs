using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Navigation;

using System.Windows.Controls;

namespace ArkheideSystem.Flourish.WPF.Test.Services;

public sealed class NavigationRuntimeSurfaceTests
{
    [Fact]
    public void NavigationPanel_MutationsUpdateOptionsAndPublishVersionedSnapshots()
    {
        var options = new NavigationOptions
        {
            IsNavigationPanelEnabled = true,
            IsNavigationPanelInitiallyOpen = false,
        };
        var sut = new NavigationPanelService(options);
        var changes = new List<NavigationPanelChangedEventArgs>();
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
        var options = new NavigationOptions();
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
        var options = new NavigationOptions();
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

        sut.Set(editor =>
        {
            editor.RemoveItem("Home");
            editor.RemoveGroup("group:0");
            editor.AddGroup("runtime", "Runtime APIs");
            editor.AddItem(
                "runtime",
                NavigationMenuItem.Page("home-demo", "Home", "Home", "H")
            );
            editor.AddItem(
                "runtime",
                NavigationMenuItem.Command(
                    "runtime-command",
                    "Run",
                    "R",
                    "cmd_demo_run",
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
    public void NavigationMenu_EmptyAndEquivalentUpdatesDoNotPublish()
    {
        var options = CreateRouteOptions();
        var sut = new NavigationMenuService(options, new NavigationRouteRegistry(options));
        var changes = 0;
        sut.Changed += (_, _) => changes++;

        sut.Set(_ => { });
        sut.Set(editor =>
        {
            editor.AddGroup("temporary");
            editor.RemoveGroup("temporary");
        });

        Assert.Equal(0, changes);
        Assert.Equal(0, sut.Current.Version);
    }

    [Fact]
    public void NavigationMenu_AppendAndInsertHaveDistinctOrderingSemantics()
    {
        var options = new NavigationOptions();
        var sut = new NavigationMenuService(options, new NavigationRouteRegistry(options));

        sut.Set(editor =>
        {
            editor.AddGroup("last");
            editor.SetGroupIndex("first", index: 0);
            editor.AddItem("first", NavigationMenuItem.Command("last-item", "Last"));
            editor.SetItemIndex(
                "first",
                NavigationMenuItem.Command("first-item", "First"),
                index: 0
            );
            editor.AddFixedItem(NavigationMenuItem.Command("last-fixed", "Last fixed"));
            editor.SetFixedItemIndex(
                NavigationMenuItem.Command("first-fixed", "First fixed"),
                index: 0
            );
        });

        Assert.Equal(["first", "last"], sut.Current.Groups.Select(group => group.Id));
        Assert.Equal(
            ["first-item", "last-item"],
            sut.Current.Groups[0].Items.Select(item => item.Id)
        );
        Assert.Equal(["first-fixed", "last-fixed"], sut.Current.FixedItems.Select(item => item.Id));
    }

    [Fact]
    public void NavigationMenu_WhenBatchValidationFails_DoesNotCommitPartialChanges()
    {
        var options = CreateRouteOptions();
        var sut = new NavigationMenuService(options, new NavigationRouteRegistry(options));
        var before = sut.Current;

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("runtime");
                editor.AddItem(
                    "runtime",
                    NavigationMenuItem.Page("missing", "NotRegistered", "Missing")
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
            new NavigationItemDefinition(
                "Home",
                "Home",
                "H",
                0,
                NavigationItemKind.Page,
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
        var options = new NavigationOptions();
        options.InitialNavigationRoutes.Add(
            new NavigationRoute("Parent", typeof(ParentPage))
        );
        options.NavigationItems.Add(
            new NavigationItemDefinition(
                "Parent",
                "Parent",
                "P",
                0,
                NavigationItemKind.Page,
                typeof(ParentPage),
                parentId: 7,
                id: "parent-page"
            )
        );
        options.NavigationItems.Add(
            new NavigationItemDefinition(
                "child-command",
                "Child",
                "C",
                0,
                NavigationItemKind.Command,
                commandKey: "cmd_child_run",
                childId: 7,
                id: "child-command"
            )
        );
        options.NavigationItems.Add(
            new NavigationItemDefinition(
                "sibling-command",
                "Sibling",
                "S",
                0,
                NavigationItemKind.Command,
                commandKey: "cmd_sibling_run",
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
            new NavigationItemDefinition(
                "Home",
                "Home",
                "H",
                0,
                NavigationItemKind.Page,
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
            new NavigationItemDefinition(
                "Home",
                "Home",
                "H",
                0,
                NavigationItemKind.Page,
                typeof(HomePage),
                id: "home-menu"
            )
        );
        var routes = new NavigationRouteRegistry(options);
        IRegistration? replacement = null;
        var reentered = false;
        routes.Changed += (_, change) =>
        {
            if (
                !reentered
                && change.ChangeKind == CollectionChangeKind.Removed
                && change.PreviousRoute?.NavigationKey == "Home"
            )
            {
                reentered = true;
                replacement = routes.Append(
                    new NavigationRoute("Home", typeof(ReplacementHomePage))
                );
            }
        };
        var menu = new NavigationMenuService(options, routes);

        Assert.True(routes.Remove("Home"));

        Assert.NotNull(routes.Get("Home"));
        var item = Assert.Single(Assert.Single(menu.Current.Groups).Items);
        Assert.Equal("home-menu", item.Id);
        Assert.Equal(typeof(ReplacementHomePage), Assert.Single(options.NavigationItems).PageType);
        Assert.Equal(1, menu.Current.Version);

        replacement!.Dispose();
    }

    private static NavigationOptions CreateRouteOptions()
    {
        var options = new NavigationOptions();
        options.InitialNavigationRoutes.Add(new NavigationRoute("Home", typeof(HomePage)));
        return options;
    }

    private sealed class HomePage : Page { }

    private sealed class ParentPage : Page { }

    private sealed class ReplacementHomePage : Page { }
}
