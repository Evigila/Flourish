using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Shell.StatusBar;


namespace ArkheideSystem.Flourish.Test.Services;

public sealed class StatusBarServiceTests
{
    [Fact]
    public void Current_ReusesTheCommittedImmutableStatusSnapshot()
    {
        var options = new StatusBarOptions
        {
            IsLANConnectionStatusEnabled = true,
            IsPowerStatusEnabled = false,
        };
        options.StatusItems.Add(new StatusBarItem("Offline", "O"));
        var sut = new StatusBarService(options);

        var initial = sut.Current;
        Assert.True(initial.IsLanStatusEnabled);
        Assert.False(initial.IsPowerStatusEnabled);
        Assert.Equal("Offline", Assert.Single(initial.Items).Text);

        options.IsPowerStatusEnabled = true;
        options.StatusItems.Add(new StatusBarItem("Online", "N"));

        var updated = sut.Current;
        Assert.Same(initial, updated);
        Assert.False(updated.IsPowerStatusEnabled);
        Assert.Single(initial.Items);
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

        var error = Record.Exception(() =>
        {
            using var handle = sut.Show("transient", "Working", "W", TimeSpan.FromSeconds(1));
        });

        Assert.Null(error);
        Assert.Empty(sut.Current.Items);
    }

    [Fact]
    public async Task Dispose_CancelsPendingExpirationsAndRejectsFurtherMutations()
    {
        var options = new StatusBarOptions();
        var sut = new StatusBarService(options);
        var changeCount = 0;
        sut.Changed += (_, _) => Interlocked.Increment(ref changeCount);
        var handle = sut.Show(
            "transient",
            "Working",
            "W",
            TimeSpan.FromMilliseconds(100)
        );

        sut.Dispose();
        sut.Dispose();
        var handleError = Record.Exception(handle.Dispose);
        await Task.Delay(250);

        Assert.Null(handleError);
        Assert.Equal(1, Volatile.Read(ref changeCount));
        Assert.Single(options.StatusItems);
        Assert.Throws<ObjectDisposedException>(() => sut.SetEnabled(true));
        Assert.Throws<ObjectDisposedException>(() => _ = sut.Current);
    }
}
