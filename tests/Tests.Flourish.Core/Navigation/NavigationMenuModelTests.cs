using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Navigation;

public sealed class NavigationMenuModelTests
{
    [Fact]
    public void PageFactory_CreatesPageItemAndUsesPresentationDefaults()
    {
        var item = NavigationMenuItem.Page("home-item", "Home", "Home");

        Assert.Equal("home-item", item.Id);
        Assert.Equal("Home", item.Label);
        Assert.Equal(NavigationMenuItemKind.Page, item.Kind);
        Assert.Equal("Home", item.NavigationKey);
        Assert.Null(item.CommandKey);
        Assert.Null(item.ParentId);
        Assert.Equal(string.Empty, item.IconGlyph);
        Assert.True(item.IsVisible);
        Assert.True(item.IsEnabled);
        Assert.False(item.IsExpanded);
    }

    [Fact]
    public void CommandFactory_CreatesCommandItemAndPreservesOptionalValues()
    {
        var item = NavigationMenuItem.Command(
            "run-item",
            "Run",
            "R",
            "cmd_run",
            parentId: "tools"
        );

        Assert.Equal("run-item", item.Id);
        Assert.Equal("Run", item.Label);
        Assert.Equal(NavigationMenuItemKind.Command, item.Kind);
        Assert.Null(item.NavigationKey);
        Assert.Equal("cmd_run", item.CommandKey);
        Assert.Equal("tools", item.ParentId);
        Assert.Equal("R", item.IconGlyph);
        Assert.True(item.IsVisible);
        Assert.True(item.IsEnabled);
        Assert.False(item.IsExpanded);
    }

    [Fact]
    public void IdentifiersAndKeys_PreserveOrdinalCasing()
    {
        var upper = NavigationMenuItem.Page("Home", "Home", "Home");
        var lower = NavigationMenuItem.Page("home", "home", "Home");

        Assert.Equal("Home", upper.Id);
        Assert.Equal("Home", upper.NavigationKey);
        Assert.Equal("home", lower.Id);
        Assert.Equal("home", lower.NavigationKey);
        Assert.NotEqual(upper, lower);
    }

    [Fact]
    public void Snapshot_DefensivelyCopiesSourceCollections()
    {
        var groupedItem = NavigationMenuItem.Page("home", "Home", "Home");
        var laterItem = NavigationMenuItem.Command("later", "Later");
        var groupItems = new List<NavigationMenuItem> { groupedItem };
        var group = new NavigationMenuGroup("main", "Main", groupItems);
        var groups = new List<NavigationMenuGroup> { group };
        var fixedItems = new List<NavigationMenuItem>
        {
            NavigationMenuItem.Command("settings", "Settings"),
        };
        var snapshot = new NavigationMenuSnapshot(groups, fixedItems, Version: 7);

        groupItems.Add(laterItem);
        groups.Clear();
        fixedItems.Clear();

        Assert.Equal(7, snapshot.Version);
        Assert.Same(group, Assert.Single(snapshot.Groups));
        Assert.Same(groupedItem, Assert.Single(snapshot.Groups[0].Items));
        Assert.Equal("settings", Assert.Single(snapshot.FixedItems).Id);
    }

    [Fact]
    public void Snapshot_CollectionsRejectMutation()
    {
        var item = NavigationMenuItem.Command("settings", "Settings");
        var group = new NavigationMenuGroup("main", "Main", [item]);
        var snapshot = new NavigationMenuSnapshot([group], [item], Version: 1);

        var groupItems = Assert.IsAssignableFrom<ICollection<NavigationMenuItem>>(group.Items);
        var groups = Assert.IsAssignableFrom<ICollection<NavigationMenuGroup>>(snapshot.Groups);
        var fixedItems = Assert.IsAssignableFrom<ICollection<NavigationMenuItem>>(
            snapshot.FixedItems
        );

        Assert.True(groupItems.IsReadOnly);
        Assert.True(groups.IsReadOnly);
        Assert.True(fixedItems.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => groupItems.Add(item));
        Assert.Throws<NotSupportedException>(() => groups.Add(group));
        Assert.Throws<NotSupportedException>(() => fixedItems.Add(item));
    }
}
