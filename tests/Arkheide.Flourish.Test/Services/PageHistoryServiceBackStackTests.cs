using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class PageHistoryServiceBackStackTests
{
    [Fact]
    public void ClearBack_RemovesBackEntriesAndLeavesForwardEntries()
    {
        var sut = new PageHistoryService();
        var forwardEntry = new FlourishPageStackEntry("settings", "parameter");
        sut.Push(new FlourishPageStackEntry("home", null));
        sut.PushForward(forwardEntry);

        sut.ClearBack();

        Assert.False(sut.CanGoBack);
        Assert.Empty(sut.BackStack);
        Assert.True(sut.CanGoForward);
        Assert.True(sut.TryPopForward(out var remaining));
        Assert.Equal(forwardEntry, remaining);
    }
}
