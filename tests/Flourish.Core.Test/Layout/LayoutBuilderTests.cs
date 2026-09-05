using System;

using ArkheideSystem.Flourish.Layout;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Layout;

public sealed class LayoutBuilderTests
{
    [Fact]
    public void SetCenterContent_UpdatesEveryRelatedOptionAndReturnsBuilder()
    {
        var options = new LayoutOptions();
        var sut = new LayoutBuilder(options);

        var result = sut.SetCenterContent(
            enabled: true,
            contentWidth: 960,
            usePersistedPreference: false
        );

        Assert.Same(sut, result);
        Assert.True(options.IsCenterContentEnabled);
        Assert.Equal(960, options.CenterContentWidth);
        Assert.False(options.UsePersistedContentLayout);
    }

    [Fact]
    public void SetCenterContent_UsesPublicDefaults()
    {
        var options = new LayoutOptions();
        var sut = new LayoutBuilder(options);

        sut.SetCenterContent();

        Assert.True(options.IsCenterContentEnabled);
        Assert.Equal(1200, options.CenterContentWidth);
        Assert.True(options.UsePersistedContentLayout);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void SetCenterContent_RejectsNonPositiveOrNonFiniteWidth(double width)
    {
        var options = new LayoutOptions
        {
            IsCenterContentEnabled = true,
            CenterContentWidth = 900,
            UsePersistedContentLayout = false,
        };
        var sut = new LayoutBuilder(options);

        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetCenterContent(false, width, usePersistedPreference: true)
        );

        Assert.Equal("contentWidth", error.ParamName);
        Assert.True(options.IsCenterContentEnabled);
        Assert.Equal(900, options.CenterContentWidth);
        Assert.False(options.UsePersistedContentLayout);
    }

    [Fact]
    public void SetSmoothScrollingEnabled_UpdatesPlatformNeutralPreferenceOptions()
    {
        var options = new LayoutOptions();
        var sut = new LayoutBuilder(options);

        var result = sut.SetSmoothScrollingEnabled(
            enabled: false,
            usePersistedPreference: false
        );

        Assert.Same(sut, result);
        Assert.False(options.IsSmoothScrollingEnabled);
        Assert.False(options.UsePersistedSmoothScroll);
    }

    [Fact]
    public void Freeze_RejectsContentAndScrollingConfiguration()
    {
        var sut = new LayoutBuilder(new LayoutOptions());
        sut.Freeze();

        Assert.Throws<InvalidOperationException>(() => sut.SetCenterContent());
        Assert.Throws<InvalidOperationException>(() => sut.SetSmoothScrollingEnabled());
    }
}
