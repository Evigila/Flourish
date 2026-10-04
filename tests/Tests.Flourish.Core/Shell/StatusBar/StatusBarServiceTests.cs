using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Shell.StatusBar;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Shell.StatusBar;

public sealed class StatusBarServiceTests
{
    [Fact]
    public void Current_CapturesAnImmutableSeedSnapshotAndNormalizesDuplicateIds()
    {
        var options = new StatusBarOptions
        {
            IsStatusBarEnabled = true,
            IsLANConnectionStatusEnabled = true,
            IsPowerStatusEnabled = false,
        };
        options.StatusItems.Add(new StatusBarItem("Offline", "O"));
        options.StatusItems.Add(new StatusBarItem("Offline", "N"));
        var sut = new StatusBarService(options);

        StatusBarSnapshot initial = sut.Current;
        options.IsPowerStatusEnabled = true;
        options.StatusItems.Add(new StatusBarItem("Online", "P"));

        Assert.Same(initial, sut.Current);
        Assert.True(initial.IsEnabled);
        Assert.True(initial.IsLanStatusEnabled);
        Assert.False(initial.IsPowerStatusEnabled);
        Assert.Equal(["status:Offline", "status:Offline:2"], initial.Items.Select(item => item.Id));
        IList<StatusBarItem> items = Assert.IsAssignableFrom<IList<StatusBarItem>>(
            initial.Items
        );
        Assert.Throws<NotSupportedException>(() =>
            items[0] = new StatusBarItem("replacement", "Replacement", "R")
        );
    }

    [Fact]
    public void FlagMutations_PublishOnlyMaterialChanges()
    {
        var sut = new StatusBarService(new StatusBarOptions());
        var changes = new List<StatusBarChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.SetEnabled(false);
        sut.SetEnabled(true);
        sut.SetEnabled(true);
        sut.SetLanStatusEnabled(true);
        sut.SetLanStatusEnabled(true);
        sut.SetPowerStatusEnabled(true);
        sut.SetPowerStatusEnabled(false);

        Assert.True(sut.Current.IsEnabled);
        Assert.True(sut.Current.IsLanStatusEnabled);
        Assert.False(sut.Current.IsPowerStatusEnabled);
        Assert.Equal(4, sut.Current.Version);
        Assert.Equal([1L, 2L, 3L, 4L], changes.Select(change => change.Current.Version));
        Assert.All(changes, change => Assert.Equal(CollectionChangeKind.Updated, change.ChangeKind));
        Assert.All(changes, change => Assert.Null(change.ItemId));
    }

    [Fact]
    public void AddStatusItem_PreservesRequestedOrderAndRejectsDuplicateIds()
    {
        var sut = new StatusBarService(new StatusBarOptions());
        var changes = new List<StatusBarChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.AddStatusItem(new StatusBarItem("first", "First", "F"));
        sut.AddStatusItem(new StatusBarItem("third", "Third", "T"));
        sut.AddStatusItem(new StatusBarItem("second", "Second", "S"), index: 1);

        Assert.Equal(
            ["first", "second", "third"],
            sut.Current.Items.Select(item => item.Id)
        );
        Assert.Equal(3, sut.Current.Version);
        Assert.All(changes, change => Assert.Equal(CollectionChangeKind.Added, change.ChangeKind));
        Assert.Throws<InvalidOperationException>(() =>
            sut.AddStatusItem(new StatusBarItem("second", "Duplicate", "D"))
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.AddStatusItem(new StatusBarItem("fourth", "Fourth", "4"), index: 4)
        );
        Assert.Equal(3, sut.Current.Version);
    }

    [Fact]
    public void SetItem_ReplacesInPlaceOrMovesToRequestedIndex()
    {
        var options = new StatusBarOptions();
        options.StatusItems.Add(new StatusBarItem("first", "First", "F"));
        options.StatusItems.Add(new StatusBarItem("second", "Second", "S"));
        var sut = new StatusBarService(options);

        sut.SetItem(new StatusBarItem("first", "Renamed", "R"));
        sut.SetItem(new StatusBarItem("third", "Third", "T"), index: 1);
        sut.SetItem(new StatusBarItem("first", "Moved", "M"), index: 2);

        Assert.Equal(
            ["third", "second", "first"],
            sut.Current.Items.Select(item => item.Id)
        );
        Assert.Equal("Moved", sut.Current.Items[2].Text);
        Assert.Equal(3, sut.Current.Version);
    }

    [Fact]
    public void ItemUpdates_PublishMaterialChangesAndPreserveStableId()
    {
        var options = new StatusBarOptions();
        options.StatusItems.Add(new StatusBarItem("sync", "Syncing", "S"));
        var sut = new StatusBarService(options);
        var changes = new List<StatusBarChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        sut.SetItemText("sync", "Syncing");
        sut.SetItemText("sync", "Complete");
        sut.SetItemIcon("sync", "C");
        sut.SetItemIcon("sync", "C");
        sut.SetItemVisible("sync", false);
        sut.SetItemVisible("sync", false);

        StatusBarItem item = Assert.Single(sut.Current.Items);
        Assert.Equal("sync", item.Id);
        Assert.Equal("Complete", item.Text);
        Assert.Equal("C", item.IconGlyph);
        Assert.False(item.IsVisible);
        Assert.Equal(3, sut.Current.Version);
        Assert.All(changes, change => Assert.Equal("sync", change.ItemId));
        Assert.All(changes, change => Assert.Equal(CollectionChangeKind.Updated, change.ChangeKind));
    }

    [Fact]
    public void SetOrder_UsesZeroBasedIndexesAndSuppressesNoOps()
    {
        var options = new StatusBarOptions();
        options.StatusItems.Add(new StatusBarItem("first", "First", "F"));
        options.StatusItems.Add(new StatusBarItem("second", "Second", "S"));
        options.StatusItems.Add(new StatusBarItem("third", "Third", "T"));
        var sut = new StatusBarService(options);
        StatusBarChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;

        sut.SetOrder("third", 0);
        sut.SetOrder("third", 0);

        Assert.Equal(
            ["third", "first", "second"],
            sut.Current.Items.Select(item => item.Id)
        );
        Assert.Equal(1, sut.Current.Version);
        Assert.Equal(CollectionChangeKind.Moved, change?.ChangeKind);
        Assert.Equal("third", change?.ItemId);
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetOrder("third", 3));
        Assert.Throws<KeyNotFoundException>(() => sut.SetOrder("missing", 0));
    }

    [Fact]
    public void RemoveAndRemoveAll_PublishOnlyWhenCollectionChanges()
    {
        var options = new StatusBarOptions();
        options.StatusItems.Add(new StatusBarItem("first", "First", "F"));
        options.StatusItems.Add(new StatusBarItem("second", "Second", "S"));
        var sut = new StatusBarService(options);
        var changes = new List<StatusBarChangedEventArgs>();
        sut.Changed += (_, change) => changes.Add(change);

        Assert.True(sut.Remove("first"));
        Assert.False(sut.Remove("first"));
        sut.RemoveAll();
        sut.RemoveAll();

        Assert.Empty(sut.Current.Items);
        Assert.Equal(2, sut.Current.Version);
        Assert.Equal(CollectionChangeKind.Removed, changes[0].ChangeKind);
        Assert.Equal("first", changes[0].ItemId);
        Assert.Equal(CollectionChangeKind.Reset, changes[1].ChangeKind);
        Assert.Null(changes[1].ItemId);
    }

    [Fact]
    public void HandleOnlyMutatesAndRemovesTheRegistrationItOwns()
    {
        var sut = new StatusBarService(new StatusBarOptions());
        IStatusBarItemHandle oldHandle = sut.Show("sync", "Syncing", "S");
        IStatusBarItemHandle newHandle = sut.Show("sync", "Complete", "C");

        Assert.False(oldHandle.IsRegistered);
        Assert.True(newHandle.IsRegistered);
        oldHandle.SetText("Stale");
        oldHandle.SetIcon("X");
        oldHandle.Dispose();

        Assert.Equal("Complete", Assert.Single(sut.Current.Items).Text);
        Assert.Equal("C", Assert.Single(sut.Current.Items).IconGlyph);
        newHandle.SetText("Done");
        newHandle.SetIcon("D");
        Assert.Equal("Done", Assert.Single(sut.Current.Items).Text);
        Assert.Equal("D", Assert.Single(sut.Current.Items).IconGlyph);

        newHandle.Dispose();
        newHandle.Dispose();

        Assert.False(newHandle.IsRegistered);
        Assert.Empty(sut.Current.Items);
    }

    [Fact]
    public async Task TimedHandle_ExpiresAndReportsItIsNoLongerRegistered()
    {
        var sut = new StatusBarService(new StatusBarOptions());
        var expired = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        sut.Changed += (_, change) =>
        {
            if (
                change.ChangeKind == CollectionChangeKind.Removed
                && change.ItemId == "transient"
            )
            {
                expired.TrySetResult(true);
            }
        };

        IStatusBarItemHandle handle = sut.Show(
            "transient",
            "Working",
            "W",
            TimeSpan.FromMilliseconds(20)
        );

        await expired.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Empty(sut.Current.Items);
        Assert.False(handle.IsRegistered);
        handle.Dispose();
    }

    [Fact]
    public void ShowWithDuration_AllowsReentrantRemovalDuringChangedEvent()
    {
        var sut = new StatusBarService(new StatusBarOptions());
        sut.Changed += (_, change) =>
        {
            if (
                change.ChangeKind == CollectionChangeKind.Updated
                && change.ItemId == "transient"
            )
            {
                sut.Remove("transient");
            }
        };

        Exception? error = Record.Exception(() =>
        {
            using IStatusBarItemHandle handle = sut.Show(
                "transient",
                "Working",
                "W",
                TimeSpan.FromSeconds(1)
            );
        });

        Assert.Null(error);
        Assert.Empty(sut.Current.Items);
    }

    [Fact]
    public async Task Dispose_CancelsExpirationsInvalidatesHandlesAndRejectsMutations()
    {
        var options = new StatusBarOptions();
        var sut = new StatusBarService(options);
        int changeCount = 0;
        sut.Changed += (_, _) => Interlocked.Increment(ref changeCount);
        IStatusBarItemHandle handle = sut.Show(
            "transient",
            "Working",
            "W",
            TimeSpan.FromMilliseconds(50)
        );

        sut.Dispose();
        sut.Dispose();
        Exception? handleError = Record.Exception(handle.Dispose);
        await Task.Delay(150);

        Assert.Null(handleError);
        Assert.False(handle.IsRegistered);
        Assert.Equal(1, Volatile.Read(ref changeCount));
        Assert.Single(options.StatusItems);
        Assert.Throws<ObjectDisposedException>(() => sut.SetEnabled(true));
        Assert.Throws<ObjectDisposedException>(() => _ = sut.Current);
    }

    [Fact]
    public void PublicMutations_ValidateIdsItemsTextIconsIndexesAndDuration()
    {
        var sut = new StatusBarService(new StatusBarOptions());
        sut.AddStatusItem(new StatusBarItem("valid", "Valid", "V"));

        Assert.Equal(
            "item",
            Assert.Throws<ArgumentNullException>(() => sut.AddStatusItem(null!)).ParamName
        );
        Assert.Throws<ArgumentException>(() =>
            sut.AddStatusItem(new StatusBarItem(" ", "Text", "I"))
        );
        Assert.Throws<ArgumentException>(() =>
            sut.AddStatusItem(new StatusBarItem("empty", " ", "I"))
        );
        Assert.Throws<ArgumentNullException>(() =>
            sut.AddStatusItem(new StatusBarItem("icon", "Text", null!))
        );
        Assert.Throws<ArgumentException>(() => sut.SetItemText("valid", " "));
        Assert.Throws<ArgumentNullException>(() => sut.SetItemIcon("valid", null!));
        Assert.Throws<KeyNotFoundException>(() => sut.SetItemText("missing", "Text"));
        Assert.Throws<ArgumentException>(() => sut.Remove(" "));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.Show("timed", "Timed", "T", TimeSpan.Zero)
        );
    }
}
