using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Shell.Toolbar;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Shell.Toolbar;

public sealed class ToolbarStateTransactionTests
{
    [Fact]
    public void SetView_DuplicateIdsLeaveSnapshotVersionAndEventsUnchanged()
    {
        var options = new ToolbarStateOptions();
        options.ViewItems["editor"] = [Item("existing", "Existing")];
        var sut = new ToolbarStateService(options);
        ToolbarStateSnapshot before = sut.Current;
        int changes = 0;
        sut.Changed += (_, _) => changes++;

        Assert.Throws<InvalidOperationException>(() =>
            sut.SetView("editor", [Item("same", "First"), Item("same", "Second")])
        );

        Assert.Same(before, sut.Current);
        Assert.Equal("existing", Assert.Single(sut.Current.Views["editor"].Items).Id);
        Assert.Equal(0, changes);
    }

    [Fact]
    public void SetDefault_EnumerationFailureLeavesCommittedStateUntouched()
    {
        var options = new ToolbarStateOptions();
        options.DefaultItems.Add(Item("existing", "Existing"));
        var sut = new ToolbarStateService(options);
        ToolbarStateSnapshot before = sut.Current;

        Assert.Throws<InvalidOperationException>(() =>
            sut.SetDefault(new ThrowingItems(Item("candidate", "Candidate")))
        );

        Assert.Same(before, sut.Current);
        Assert.Equal("existing", Assert.Single(sut.Current.DefaultItems).Id);
    }

    [Fact]
    public void SetItem_InvalidMoveIndexDoesNotRemoveExistingItem()
    {
        var options = new ToolbarStateOptions();
        options.ViewItems["editor"] = [Item("first", "First"), Item("second", "Second")];
        var sut = new ToolbarStateService(options);
        ToolbarStateSnapshot before = sut.Current;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetItem(Item("first", "Renamed"), "editor", index: 3)
        );

        Assert.Same(before, sut.Current);
        Assert.Equal(
            ["first", "second"],
            sut.Current.Views["editor"].Items.Select(item => item.Id)
        );
        Assert.Equal("First", sut.Current.Views["editor"].Items[0].DisplayName);
    }

    [Fact]
    public void AddDuplicateFailureDoesNotPublishOrAdvanceVersion()
    {
        var options = new ToolbarStateOptions();
        options.DefaultItems.Add(Item("save", "Save"));
        var sut = new ToolbarStateService(options);
        ToolbarStateSnapshot before = sut.Current;
        int changes = 0;
        sut.Changed += (_, _) => changes++;

        Assert.Throws<InvalidOperationException>(() =>
            sut.AddItem(Item("save", "Duplicate"))
        );

        Assert.Same(before, sut.Current);
        Assert.Equal(0, changes);
    }

    [Fact]
    public void ChangedObserverFailureOccursAfterStateHasCommitted()
    {
        var sut = new ToolbarStateService(new ToolbarStateOptions());
        sut.Changed += (_, _) => throw new InvalidOperationException("observer failed");

        Assert.Throws<InvalidOperationException>(() =>
            sut.AddItem(Item("save", "Save"), "editor")
        );

        Assert.Equal(1, sut.Current.Version);
        Assert.Equal("save", Assert.Single(sut.Current.Views["editor"].Items).Id);
    }

    [Fact]
    public void Constructor_InvalidSeedFailsBeforePublishingAService()
    {
        var options = new ToolbarStateOptions();
        options.DefaultItems.Add(Item("same", "First"));
        options.DefaultItems.Add(Item("same", "Second"));

        Assert.Throws<InvalidOperationException>(() => new ToolbarStateService(options));
    }

    private static ToolbarItem Item(string id, string name)
    {
        return new ToolbarItem(name, name[..1]) { Id = id };
    }

    private sealed class ThrowingItems(ToolbarItem first) : IEnumerable<ToolbarItem>
    {
        public IEnumerator<ToolbarItem> GetEnumerator()
        {
            yield return first;
            throw new InvalidOperationException("enumeration failed");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
