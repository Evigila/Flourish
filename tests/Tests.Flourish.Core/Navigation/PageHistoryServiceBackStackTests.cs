using ArkheideSystem.Flourish.Navigation;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Navigation;

public sealed class PageHistoryServiceBackStackTests
{
    [Fact]
    public void ClearBack_RemovesBackEntriesAndLeavesForwardEntries()
    {
        var sut = new PageHistoryService();
        var forwardEntry = new NavigationStackEntry("settings", "parameter");
        sut.Push(new NavigationStackEntry("home", null));
        sut.PushForward(forwardEntry);

        sut.ClearBack();

        Assert.False(sut.CanGoBack);
        Assert.Empty(sut.BackStack);
        Assert.True(sut.CanGoForward);
        Assert.True(sut.TryPopForward(out var remaining));
        Assert.Equal(forwardEntry, remaining);
    }
}
