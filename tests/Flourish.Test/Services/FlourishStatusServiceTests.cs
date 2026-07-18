using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishStatusServiceTests
{
    [Fact]
    public void Current_ReturnsAnIndependentStatusSnapshot()
    {
        var options = new FlourishShellOptions
        {
            IsLANConnectionStatusEnabled = true,
            IsPowerStatusEnabled = false,
        };
        options.StatusItems.Add(new FlourishStatusItem("Offline", "O"));
        var sut = new FlourishStatusService(options);

        var initial = sut.Current;
        Assert.True(initial.IsLanStatusEnabled);
        Assert.False(initial.IsPowerStatusEnabled);
        Assert.Equal("Offline", Assert.Single(initial.Items).Text);

        options.IsPowerStatusEnabled = true;
        options.StatusItems.Add(new FlourishStatusItem("Online", "N"));

        var updated = sut.Current;
        Assert.True(updated.IsPowerStatusEnabled);
        Assert.Equal(2, updated.Items.Count);
        Assert.Equal("Online", updated.Items[1].Text);
        Assert.Single(initial.Items);
    }

    [Fact]
    public void ShowWithDuration_AllowsReentrantRemovalDuringChangedEvent()
    {
        var sut = new FlourishStatusService(new FlourishShellOptions());
        sut.Changed += (_, change) =>
        {
            if (
                change.ChangeKind == FlourishRuntimeChangeKind.Updated
                && change.ItemId == "transient"
            )
            {
                sut.Remove("transient");
            }
        };

        var error = Record.Exception(() =>
        {
            using var handle = sut.Show(
                "transient",
                "Working",
                "W",
                TimeSpan.FromSeconds(1)
            );
        });

        Assert.Null(error);
        Assert.Empty(sut.Current.Items);
    }
}
