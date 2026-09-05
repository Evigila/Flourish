using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Layout;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Layout;

public sealed class ContentLayoutServiceTests
{
    [Fact]
    public void Current_UsesStartupStateAndSuppressesEquivalentUpdates()
    {
        var sut = new ContentLayoutService(
            new LayoutOptions { IsCenterContentEnabled = true, CenterContentWidth = 960 }
        );
        var changes = new List<StateTransitionEventArgs<ContentLayoutSettings>>();
        sut.Changed += (_, change) => changes.Add(change);
        ContentLayoutSettings initial = sut.Current;

        sut.SetCenterContent(true, 960);
        sut.SetCenterContent(false, 1080);

        Assert.Equal(new ContentLayoutSettings(true, 960, 0), initial);
        Assert.Equal(new ContentLayoutSettings(false, 1080, 1), sut.Current);
        StateTransitionEventArgs<ContentLayoutSettings> change = Assert.Single(changes);
        Assert.Same(initial, change.Previous);
        Assert.Equal(sut.Current, change.Current);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_InvalidConfiguredWidthFallsBackToDefault(double width)
    {
        var sut = new ContentLayoutService(
            new LayoutOptions { IsCenterContentEnabled = true, CenterContentWidth = width }
        );

        Assert.Equal(new ContentLayoutSettings(true, 1200, 0), sut.Current);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void SetCenterContent_RejectsInvalidWidthsWithoutPublishing(double width)
    {
        var sut = new ContentLayoutService(new LayoutOptions());
        int changes = 0;
        sut.Changed += (_, _) => changes++;
        ContentLayoutSettings before = sut.Current;

        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetCenterContent(true, width)
        );

        Assert.Equal("contentWidth", error.ParamName);
        Assert.Same(before, sut.Current);
        Assert.Equal(0, changes);
    }

    [Fact]
    public void WidthChangeWhileCenteringIsDisabled_IsStillMaterialState()
    {
        var sut = new ContentLayoutService(
            new LayoutOptions { IsCenterContentEnabled = false, CenterContentWidth = 900 }
        );
        StateTransitionEventArgs<ContentLayoutSettings>? change = null;
        sut.Changed += (_, args) => change = args;

        sut.SetCenterContent(false, 1000);

        Assert.NotNull(change);
        Assert.False(change.Previous.IsCenterContentEnabled);
        Assert.Equal(900, change.Previous.ContentWidth);
        Assert.False(change.Current.IsCenterContentEnabled);
        Assert.Equal(1000, change.Current.ContentWidth);
        Assert.Equal(1, change.Current.Version);
    }

    [Fact]
    public void ChangedHandler_CanPerformAReentrantUpdateAgainstCommittedState()
    {
        var sut = new ContentLayoutService(new LayoutOptions());
        var versions = new List<long>();
        sut.Changed += (_, change) =>
        {
            versions.Add(change.Current.Version);
            if (change.Current.Version == 1)
            {
                Assert.Same(change.Current, sut.Current);
                sut.SetCenterContent(false, 720);
            }
        };

        sut.SetCenterContent(true, 960);

        Assert.Equal([1L, 2L], versions);
        Assert.Equal(new ContentLayoutSettings(false, 720, 2), sut.Current);
    }

    [Fact]
    public void Constructor_RejectsNullOptions()
    {
        Assert.Equal(
            "options",
            Assert.Throws<ArgumentNullException>(() => new ContentLayoutService(null!)).ParamName
        );
    }
}
