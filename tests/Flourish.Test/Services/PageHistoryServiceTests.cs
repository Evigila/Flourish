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
}
