using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Navigation;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Navigation;

public sealed class PageHistoryServiceTests
{
    [Fact]
    public void NewHistory_HasNoAvailableNavigation()
    {
        var sut = new PageHistoryService();

        Assert.False(sut.CanGoBack);
        Assert.False(sut.CanGoForward);
        Assert.Empty(sut.BackStack);
        Assert.Empty(sut.ForwardStack);
    }

    [Fact]
    public void TryPopBack_ReturnsEntriesInLastInFirstOutOrder()
    {
        var sut = new PageHistoryService();
        var first = new NavigationStackEntry("home", 1);
        var second = new NavigationStackEntry("settings", 2);
        sut.Push(first);
        sut.Push(second);

        Assert.True(sut.TryPopBack(out var poppedSecond));
        Assert.Equal(second, poppedSecond);
        Assert.True(sut.TryPopBack(out var poppedFirst));
        Assert.Equal(first, poppedFirst);
        Assert.False(sut.CanGoBack);
    }

    [Fact]
    public void TryPopForward_ReturnsEntriesInLastInFirstOutOrder()
    {
        var sut = new PageHistoryService();
        var first = new NavigationStackEntry("home", null);
        var second = new NavigationStackEntry("gallery", "selection");
        sut.PushForward(first);
        sut.PushForward(second);

        Assert.True(sut.TryPopForward(out var poppedSecond));
        Assert.Equal(second, poppedSecond);
        Assert.True(sut.TryPopForward(out var poppedFirst));
        Assert.Equal(first, poppedFirst);
        Assert.False(sut.CanGoForward);
    }

    [Fact]
    public void TryPop_WhenStackIsEmpty_ReturnsFalseAndNullEntry()
    {
        var sut = new PageHistoryService();

        Assert.False(sut.TryPopBack(out var backEntry));
        Assert.Null(backEntry);
        Assert.False(sut.TryPopForward(out var forwardEntry));
        Assert.Null(forwardEntry);
    }

    [Fact]
    public void ClearForward_LeavesBackStackUntouched()
    {
        var sut = new PageHistoryService();
        var backEntry = new NavigationStackEntry("home", null);
        sut.Push(backEntry);
        sut.PushForward(new NavigationStackEntry("settings", null));

        sut.ClearForward();

        Assert.True(sut.CanGoBack);
        Assert.False(sut.CanGoForward);
        Assert.True(sut.TryPopBack(out var remainingEntry));
        Assert.Equal(backEntry, remainingEntry);
    }

    [Fact]
    public void Clear_RemovesBackAndForwardEntries()
    {
        var sut = new PageHistoryService();
        sut.Push(new NavigationStackEntry("home", null));
        sut.PushForward(new NavigationStackEntry("settings", null));

        sut.Clear();

        Assert.False(sut.CanGoBack);
        Assert.False(sut.CanGoForward);
        Assert.Empty(sut.BackStack);
        Assert.Empty(sut.ForwardStack);
    }

    [Fact]
    public void Push_BeyondCapacity_EvictsTheOldestBackEntry()
    {
        var sut = new PageHistoryService(maximumEntries: 2);
        var first = new NavigationStackEntry("first", 1);
        var second = new NavigationStackEntry("second", 2);
        var third = new NavigationStackEntry("third", 3);

        sut.Push(first);
        sut.Push(second);
        sut.Push(third);

        Assert.Equal(2, sut.BackStack.Count);
        Assert.True(sut.TryPopBack(out var newest));
        Assert.Equal(third, newest);
        Assert.True(sut.TryPopBack(out var next));
        Assert.Equal(second, next);
        Assert.False(sut.TryPopBack(out _));
    }

    [Fact]
    public void PushForward_BeyondCapacity_EvictsTheOldestForwardEntry()
    {
        var sut = new PageHistoryService(maximumEntries: 2);
        var first = new NavigationStackEntry("first", 1);
        var second = new NavigationStackEntry("second", 2);
        var third = new NavigationStackEntry("third", 3);

        sut.PushForward(first);
        sut.PushForward(second);
        sut.PushForward(third);

        Assert.Equal(2, sut.ForwardStack.Count);
        Assert.True(sut.TryPopForward(out var newest));
        Assert.Equal(third, newest);
        Assert.True(sut.TryPopForward(out var next));
        Assert.Equal(second, next);
        Assert.False(sut.TryPopForward(out _));
    }

    [Fact]
    public void DefaultCapacity_BoundsProductionHistory()
    {
        var sut = new PageHistoryService();

        for (var index = 0; index <= PageHistoryService.DefaultMaximumEntries; index++)
        {
            sut.Push(new NavigationStackEntry($"page-{index}", index));
        }

        Assert.Equal(PageHistoryService.DefaultMaximumEntries, sut.BackStack.Count);
        Assert.DoesNotContain(sut.BackStack, entry => entry.NavigationKey == "page-0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveCapacity_Throws(int maximumEntries)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PageHistoryService(maximumEntries)
        );
    }

    [Fact]
    public void Remove_UsesOrdinalKeyComparisonAndPreservesRemainingOrder()
    {
        var sut = new PageHistoryService();
        var oldest = new NavigationStackEntry("target", null);
        var caseVariant = new NavigationStackEntry("Target", null);
        var newest = new NavigationStackEntry("newest", null);
        sut.Push(oldest);
        sut.Push(caseVariant);
        sut.Push(newest);

        sut.Remove("target");

        Assert.Equal(
            ["newest", "Target"],
            sut.BackStack.Select(entry => entry.NavigationKey)
        );
    }

    [Fact]
    public void RemoveWhere_VisitsEachEntryOnceAndPreservesBothStackOrders()
    {
        var sut = new PageHistoryService();
        sut.Push(new NavigationStackEntry("back-oldest", null));
        sut.Push(new NavigationStackEntry("stale-back", null));
        sut.Push(new NavigationStackEntry("back-newest", null));
        sut.PushForward(new NavigationStackEntry("forward-oldest", null));
        sut.PushForward(new NavigationStackEntry("stale-forward", null));
        var visits = 0;

        var removed = sut.RemoveWhere(entry =>
        {
            visits++;
            return entry.NavigationKey.StartsWith("stale-", StringComparison.Ordinal);
        });

        Assert.True(removed);
        Assert.Equal(5, visits);
        Assert.Equal(
            ["back-newest", "back-oldest"],
            sut.BackStack.Select(entry => entry.NavigationKey)
        );
        Assert.Equal(
            ["forward-oldest"],
            sut.ForwardStack.Select(entry => entry.NavigationKey)
        );
    }

    [Fact]
    public void InvalidMutationArguments_ThrowBeforeChangingHistory()
    {
        var sut = new PageHistoryService();
        sut.Push(new NavigationStackEntry("home", null));

        Assert.Throws<ArgumentNullException>(() => sut.Push(null!));
        Assert.Throws<ArgumentNullException>(() => sut.PushForward(null!));
        Assert.Throws<ArgumentException>(() => sut.Remove(" "));
        Assert.Throws<ArgumentNullException>(() => sut.RemoveWhere(null!));
        Assert.Single(sut.BackStack);
        Assert.Empty(sut.ForwardStack);
    }

    [Fact]
    public void StackSnapshots_AreReadOnlyAndRemainStableAfterMutation()
    {
        var sut = new PageHistoryService();
        sut.Push(new NavigationStackEntry("home", null));
        var snapshot = sut.BackStack;
        var mutableView = Assert.IsAssignableFrom<IList<NavigationStackEntry>>(snapshot);

        sut.ClearBack();

        Assert.Single(snapshot);
        Assert.Throws<NotSupportedException>(() => mutableView.Clear());
        Assert.Empty(sut.BackStack);
    }

    [Fact]
    public void OpaqueParameter_IsRetainedByReferenceWithoutInspection()
    {
        var sut = new PageHistoryService();
        var parameter = new ThrowingOpaqueParameter();
        sut.Push(new NavigationStackEntry("opaque", parameter));

        var snapshot = Assert.Single(sut.BackStack);
        Assert.Same(parameter, snapshot.Parameter);
        Assert.True(sut.TryPopBack(out var popped));
        Assert.Same(parameter, popped.Parameter);
    }

    [Fact]
    public void ConcurrentPushes_AreSerializedWithoutLosingEntries()
    {
        const int entryCount = 512;
        var sut = new PageHistoryService(maximumEntries: entryCount);

        Parallel.For(
            0,
            entryCount,
            index => sut.Push(new NavigationStackEntry($"page-{index}", index))
        );

        Assert.Equal(entryCount, sut.BackStack.Count);
        Assert.Equal(
            entryCount,
            sut.BackStack.Select(entry => entry.NavigationKey).Distinct().Count()
        );
    }

    [Fact]
    public void ConcurrentPops_ReturnEveryEntryExactlyOnce()
    {
        const int entryCount = 512;
        var sut = new PageHistoryService(maximumEntries: entryCount);
        for (var index = 0; index < entryCount; index++)
        {
            sut.Push(new NavigationStackEntry($"page-{index}", null));
        }

        var popped = new System.Collections.Concurrent.ConcurrentBag<string>();
        Parallel.For(
            0,
            entryCount,
            _ =>
            {
                if (sut.TryPopBack(out var entry))
                {
                    popped.Add(entry.NavigationKey);
                }
            }
        );

        Assert.Equal(entryCount, popped.Count);
        Assert.Equal(entryCount, popped.Distinct().Count());
        Assert.False(sut.CanGoBack);
    }

    private sealed class ThrowingOpaqueParameter
    {
        public override bool Equals(object? obj) =>
            throw new InvalidOperationException("Navigation parameters must not be compared.");

        public override int GetHashCode() =>
            throw new InvalidOperationException("Navigation parameters must not be hashed.");

        public override string ToString() =>
            throw new InvalidOperationException("Navigation parameters must not be formatted.");
    }
}
