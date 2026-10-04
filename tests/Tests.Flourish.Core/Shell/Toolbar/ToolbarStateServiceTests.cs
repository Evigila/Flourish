using System;
using System.Collections.Generic;
using System.Linq;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Shell.Toolbar;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Shell.Toolbar;

public sealed class ToolbarStateServiceTests
{
    [Fact]
    public void Current_CapturesDeeplyImmutableSeedStateWithCaseSensitiveViewKeys()
    {
        var options = new ToolbarStateOptions { IsEnabled = true };
        var defaultItem = Item("default", "Default");
        var editorItem = Item("save", "Save");
        options.DefaultItems.Add(defaultItem);
        options.ViewItems["editor"] = [editorItem];
        options.ViewItems["EDITOR"] = [Item("build", "Build")];
        options.ViewIconModes["editor"] = false;
        var sut = new ToolbarStateService(options);

        ToolbarStateSnapshot initial = sut.Current;
        options.DefaultItems.Add(Item("late", "Late"));
        options.ViewItems["editor"] = [Item("late-view", "Late view")];

        Assert.Same(initial, sut.Current);
        Assert.True(initial.IsEnabled);
        Assert.Equal(defaultItem, Assert.Single(initial.DefaultItems));
        Assert.Equal(["editor", "EDITOR"], initial.Views.Keys.OrderBy(key => key));
        Assert.False(initial.Views["editor"].IconOnly);
        Assert.True(initial.Views["EDITOR"].IconOnly);
        Assert.Equal(editorItem, Assert.Single(initial.Views["editor"].Items));
        IDictionary<string, ToolbarViewState> views = Assert.IsAssignableFrom<
            IDictionary<string, ToolbarViewState>
        >(initial.Views);
        Assert.Throws<NotSupportedException>(() =>
            views["other"] = new ToolbarViewState("other", true, [])
        );
        IList<ToolbarItem> items = Assert.IsAssignableFrom<IList<ToolbarItem>>(
            initial.Views["editor"].Items
        );
        Assert.Throws<NotSupportedException>(() => items[0] = Item("other", "Other"));
    }

    [Fact]
    public void GetItems_UsesEnabledViewDefinitionOtherwiseFallsBackToDefault()
    {
        var options = new ToolbarStateOptions { IsEnabled = true };
        options.DefaultItems.Add(Item("default", "Default"));
        options.ViewItems["editor"] = [Item("save", "Save")];
        var sut = new ToolbarStateService(options);

        Assert.Equal("save", Assert.Single(sut.GetItems("editor")).Id);
        Assert.Equal("default", Assert.Single(sut.GetItems("unknown")).Id);
        Assert.Equal("default", Assert.Single(sut.GetItems()).Id);

        sut.SetEnabled(false);

        Assert.Equal("default", Assert.Single(sut.GetItems("editor")).Id);
    }

    [Fact]
    public void SetEnabled_PublishesOnlyMaterialChanges()
    {
        var sut = new ToolbarStateService(new ToolbarStateOptions());
        var changes = new List<ToolbarStateChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.SetEnabled(false);
        sut.SetEnabled(true);
        sut.SetEnabled(true);
        sut.SetEnabled(false);

        Assert.False(sut.Current.IsEnabled);
        Assert.Equal(2, sut.Current.Version);
        Assert.Equal([1L, 2L], changes.Select(change => change.Current.Version));
        Assert.All(changes, change => Assert.Equal(CollectionChangeKind.Updated, change.ChangeKind));
        Assert.All(changes, change => Assert.Null(change.ViewKey));
        Assert.All(changes, change => Assert.Null(change.ItemId));
    }

    [Fact]
    public void SetDefault_UsesNullViewKeyAndSuppressesEquivalentReplacement()
    {
        var sut = new ToolbarStateService(new ToolbarStateOptions());
        var item = Item("save", "Save");
        var changes = new List<ToolbarStateChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.SetDefault([item]);
        sut.SetDefault([item]);

        Assert.Equal(item, Assert.Single(sut.Current.DefaultItems));
        Assert.Equal(1, sut.Current.Version);
        ToolbarStateChangedEventArgs change = Assert.Single(changes);
        Assert.Equal(CollectionChangeKind.Reset, change.ChangeKind);
        Assert.Null(change.ViewKey);
        Assert.Null(change.ItemId);
    }

    [Fact]
    public void SetView_PreservesCaseSensitiveKeyItemsAndIconModeAndSuppressesNoOp()
    {
        var sut = new ToolbarStateService(new ToolbarStateOptions());
        var save = Item("save", "Save");
        int changes = 0;
        sut.Changed += (_, _) => changes++;

        sut.SetView("editor", [save], iconOnly: false);
        sut.SetView("editor", [save], iconOnly: false);
        sut.SetView("EDITOR", [Item("build", "Build")]);

        Assert.Equal(2, sut.Current.Version);
        Assert.Equal(2, changes);
        Assert.False(sut.Current.Views["editor"].IconOnly);
        Assert.Equal(save, Assert.Single(sut.Current.Views["editor"].Items));
        Assert.True(sut.Current.Views["EDITOR"].IconOnly);
    }

    [Fact]
    public void AddItem_SupportsDefaultAndViewIndexesAndRejectsDuplicates()
    {
        var sut = new ToolbarStateService(new ToolbarStateOptions());
        var changes = new List<ToolbarStateChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.AddItem(Item("first", "First"));
        sut.AddItem(Item("third", "Third"), "editor");
        sut.AddItem(Item("first-view", "First view"), "editor", index: 0);

        Assert.Equal("first", Assert.Single(sut.Current.DefaultItems).Id);
        Assert.Equal(
            ["first-view", "third"],
            sut.Current.Views["editor"].Items.Select(item => item.Id)
        );
        Assert.Equal(3, sut.Current.Version);
        Assert.Equal([null, "editor", "editor"], changes.Select(change => change.ViewKey));
        Assert.Throws<InvalidOperationException>(() => sut.AddItem(Item("third", "Again"), "editor"));
        Assert.Equal(3, sut.Current.Version);
    }

    [Fact]
    public void SetItem_UpsertsMovesAndSuppressesEquivalentValueAtSameIndex()
    {
        var options = new ToolbarStateOptions();
        options.ViewItems["editor"] = [Item("first", "First"), Item("second", "Second")];
        var sut = new ToolbarStateService(options);
        int changes = 0;
        sut.Changed += (_, _) => changes++;

        ToolbarItem first = options.ViewItems["editor"][0];
        sut.SetItem(first, "editor");
        sut.SetItem(first, "editor", index: 0);
        sut.SetItem(first with { DisplayName = "Moved" }, "editor", index: 1);
        sut.SetItem(Item("third", "Third"), "editor", index: 0);

        Assert.Equal(
            ["third", "second", "first"],
            sut.Current.Views["editor"].Items.Select(item => item.Id)
        );
        Assert.Equal("Moved", sut.Current.Views["editor"].Items[2].DisplayName);
        Assert.Equal(2, sut.Current.Version);
        Assert.Equal(2, changes);
    }

    [Fact]
    public void EnabledAndVisibleUpdates_UseStableIdAndSuppressNoOps()
    {
        var options = new ToolbarStateOptions();
        options.ViewItems["editor"] = [Item("save", "Save")];
        var sut = new ToolbarStateService(options);
        var changes = new List<ToolbarStateChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.SetItemEnabled("save", true, "editor");
        sut.SetItemEnabled("save", false, "editor");
        sut.SetItemVisible("save", false, "editor");
        sut.SetItemVisible("save", false, "editor");

        ToolbarItem item = Assert.Single(sut.Current.Views["editor"].Items);
        Assert.False(item.IsEnabled);
        Assert.False(item.IsVisible);
        Assert.Equal(2, sut.Current.Version);
        Assert.Equal(["save", "save"], changes.Select(change => change.ItemId));
        Assert.All(changes, change => Assert.Equal("editor", change.ViewKey));
    }

    [Fact]
    public void SetOrderRemoveAndRemoveAll_PreserveDefinitionAndPublishMaterialChanges()
    {
        var options = new ToolbarStateOptions();
        options.ViewItems["editor"] =
        [
            Item("first", "First"),
            Item("second", "Second"),
            Item("third", "Third"),
        ];
        var sut = new ToolbarStateService(options);
        var changes = new List<ToolbarStateChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.SetOrder("third", 0, "editor");
        sut.SetOrder("third", 0, "editor");
        Assert.True(sut.Remove("second", "editor"));
        Assert.False(sut.Remove("missing", "editor"));
        sut.RemoveAll("editor");
        sut.RemoveAll("editor");

        Assert.True(sut.Current.Views.ContainsKey("editor"));
        Assert.Empty(sut.Current.Views["editor"].Items);
        Assert.Equal(3, sut.Current.Version);
        Assert.Equal(
            [CollectionChangeKind.Moved, CollectionChangeKind.Removed, CollectionChangeKind.Reset],
            changes.Select(change => change.ChangeKind)
        );
    }

    [Fact]
    public void SetIconOnly_TracksExplicitFalseModeAndUsesTrueAsUnknownDefault()
    {
        var sut = new ToolbarStateService(new ToolbarStateOptions());
        var changes = new List<ToolbarStateChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        Assert.True(sut.GetIconOnly("editor"));
        sut.SetIconOnly("editor", true);
        sut.SetIconOnly("editor", false);
        sut.SetIconOnly("editor", false);

        Assert.False(sut.GetIconOnly("editor"));
        Assert.False(sut.Current.Views["editor"].IconOnly);
        Assert.Empty(sut.Current.Views["editor"].Items);
        Assert.Equal(1, sut.Current.Version);
        ToolbarStateChangedEventArgs change = Assert.Single(changes);
        Assert.Equal("editor", change.ViewKey);
        Assert.Null(change.ItemId);
    }

    [Fact]
    public void ChangedHandler_CanReenterAgainstAlreadyCommittedSnapshot()
    {
        var sut = new ToolbarStateService(new ToolbarStateOptions());
        var versions = new List<long>();
        sut.Changed += (_, change) =>
        {
            versions.Add(change.Current.Version);
            Assert.Same(change.Current, sut.Current);
            if (change.Current.Version == 1)
            {
                sut.SetEnabled(true);
            }
        };

        sut.SetDefault([Item("save", "Save")]);

        Assert.Equal([1L, 2L], versions);
        Assert.True(sut.Current.IsEnabled);
        Assert.Equal("save", Assert.Single(sut.Current.DefaultItems).Id);
    }

    [Fact]
    public void PublicOperations_ValidateViewKeysIdsItemsAndIndexes()
    {
        var options = new ToolbarStateOptions();
        options.ViewItems["editor"] = [Item("save", "Save")];
        var sut = new ToolbarStateService(options);

        Assert.Throws<ArgumentException>(() => sut.SetView(" ", []));
        Assert.Throws<ArgumentException>(() => sut.GetItems(" "));
        Assert.Throws<ArgumentException>(() => sut.GetIconOnly(" "));
        Assert.Throws<ArgumentException>(() => sut.AddItem(Item("new", "New"), " "));
        Assert.Throws<ArgumentException>(() => sut.Remove(" "));
        Assert.Throws<ArgumentNullException>(() => sut.AddItem(null!));
        Assert.Throws<ArgumentException>(() =>
            sut.AddItem(new ToolbarItem(" ", "I") { Id = "empty-name" })
        );
        Assert.Throws<ArgumentNullException>(() =>
            sut.AddItem(new ToolbarItem("Null icon", null!) { Id = "null-icon" })
        );
        Assert.Throws<KeyNotFoundException>(() => sut.SetItemVisible("missing", false, "editor"));
        Assert.Throws<KeyNotFoundException>(() => sut.SetOrder("missing", 0, "editor"));
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetOrder("save", 1, "editor"));
    }

    private static ToolbarItem Item(string id, string name)
    {
        return new ToolbarItem(name, name[..1]) { Id = id };
    }
}
