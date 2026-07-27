using System.Windows;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeScrollServiceTests
{
    [Fact]
    public void GetCurrent_UsesStartupValue()
    {
        var sut = new ScrollService(
            new FlourishShellOptions { IsSmoothScrollingEnabled = false }
        );

        var current = sut.GetCurrent();

        Assert.False(current.IsSmoothScrollingEnabled);
        Assert.Equal(0, current.Version);
    }

    [Fact]
    public void SetSmoothScrollingEnabled_RaisesChangedOnlyForRealChanges()
    {
        var sut = new ScrollService(new FlourishShellOptions());
        FlourishScrollChangedEventArgs? change = null;
        var changeCount = 0;
        sut.Changed += (_, e) =>
        {
            change = e;
            changeCount++;
        };

        sut.SetSmoothScrollingEnabled(true);
        sut.SetSmoothScrollingEnabled(false);

        Assert.Equal(1, changeCount);
        Assert.NotNull(change);
        Assert.True(change.Previous.IsSmoothScrollingEnabled);
        Assert.False(change.Current.IsSmoothScrollingEnabled);
        Assert.Equal(0, change.Previous.Version);
        Assert.Equal(1, change.Current.Version);
    }

    [Fact]
    public void AttachedResources_UpdateWithRuntimeState()
    {
        var sut = new ScrollService(new FlourishShellOptions());
        var resources = new ResourceDictionary();

        sut.Attach(Dispatcher.CurrentDispatcher, resources);
        sut.SetSmoothScrollingEnabled(false);

        Assert.False((bool)resources[ScrollService.SmoothScrollingResourceKey]);
        Assert.False(sut.GetCurrent().IsSmoothScrollingEnabled);
    }
}
