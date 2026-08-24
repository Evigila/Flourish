using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

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
        var first = new FlourishPageStackEntry("home", 1);
        var second = new FlourishPageStackEntry("settings", 2);
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
        var first = new FlourishPageStackEntry("home", null);
        var second = new FlourishPageStackEntry("gallery", "selection");
        sut.PushForward(first);
        sut.PushForward(second);

        Assert.True(sut.TryPopForward(out var poppedSecond));
        Assert.Equal(second, poppedSecond);
        Assert.True(sut.TryPopForward(out var poppedFirst));
        Assert.Equal(first, poppedFirst);
        Assert.False(sut.CanGoForward);
    }

    [Fact]
    public void TryPop_WhenStackIsEmpty_ReturnsFalse()
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
        var backEntry = new FlourishPageStackEntry("home", null);
        sut.Push(backEntry);
        sut.PushForward(new FlourishPageStackEntry("settings", null));

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
        sut.Push(new FlourishPageStackEntry("home", null));
        sut.PushForward(new FlourishPageStackEntry("settings", null));

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
        var first = new FlourishPageStackEntry("first", 1);
        var second = new FlourishPageStackEntry("second", 2);
        var third = new FlourishPageStackEntry("third", 3);

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
        var first = new FlourishPageStackEntry("first", 1);
        var second = new FlourishPageStackEntry("second", 2);
        var third = new FlourishPageStackEntry("third", 3);

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
            sut.Push(new FlourishPageStackEntry($"page-{index}", index));
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
    public void Remove_PreservesTheOrderOfRemainingEntries()
    {
        var sut = new PageHistoryService();
        var oldest = new FlourishPageStackEntry("oldest", null);
        var removed = new FlourishPageStackEntry("removed", null);
        var newest = new FlourishPageStackEntry("newest", null);
        sut.Push(oldest);
        sut.Push(removed);
        sut.Push(newest);

        sut.Remove(removed.NavigationKey);

        Assert.True(sut.TryPopBack(out var first));
        Assert.Equal(newest, first);
        Assert.True(sut.TryPopBack(out var second));
        Assert.Equal(oldest, second);
    }

    [Fact]
    public void RemoveWhere_VisitsEachEntryOnceAndPreservesBothStackOrders()
    {
        var sut = new PageHistoryService();
        sut.Push(new FlourishPageStackEntry("back-oldest", null));
        sut.Push(new FlourishPageStackEntry("stale-back", null));
        sut.Push(new FlourishPageStackEntry("back-newest", null));
        sut.PushForward(new FlourishPageStackEntry("forward-oldest", null));
        sut.PushForward(new FlourishPageStackEntry("stale-forward", null));
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
    public void ConcurrentPushes_AreSerializedWithoutLosingEntries()
    {
        const int entryCount = 512;
        var sut = new PageHistoryService(maximumEntries: entryCount);

        Parallel.For(
            0,
            entryCount,
            index => sut.Push(new FlourishPageStackEntry($"page-{index}", index))
        );

        Assert.Equal(entryCount, sut.BackStack.Count);
        Assert.Equal(
            entryCount,
            sut.BackStack.Select(entry => entry.NavigationKey).Distinct().Count()
        );
    }
}
