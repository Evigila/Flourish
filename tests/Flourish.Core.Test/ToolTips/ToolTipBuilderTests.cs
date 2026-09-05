using System;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.ToolTips;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.ToolTips;

public sealed class ToolTipBuilderTests
{
    [Fact]
    public void Options_ExposeStableDefaults()
    {
        var options = new ToolTipOptions();

        Assert.False(options.IsTipsEnabled);
        Assert.Equal(200, options.InitialShowDelayMilliseconds);
        Assert.Equal(5, options.SpawnableMargin);
    }

    [Fact]
    public void Configuration_UpdatesOptionsAndReturnsSameBuilder()
    {
        var options = new ToolTipOptions();
        var builder = new ToolTipBuilder(options);

        var enabledResult = builder.SetEnabled();
        var settingsResult = builder.SetSettings(350, 8);

        Assert.Same(builder, enabledResult);
        Assert.Same(builder, settingsResult);
        Assert.True(options.IsTipsEnabled);
        Assert.Equal(350, options.InitialShowDelayMilliseconds);
        Assert.Equal(8, options.SpawnableMargin);
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(200, -1)]
    [InlineData(200, double.NaN)]
    [InlineData(200, double.PositiveInfinity)]
    [InlineData(200, double.NegativeInfinity)]
    public void SetSettings_RejectsInvalidValues(int delay, double margin)
    {
        IToolTipBuilder builder = new ToolTipBuilder(new ToolTipOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetSettings(delay, margin));
    }

    [Fact]
    public void Freeze_PreventsFurtherMutation()
    {
        var options = new ToolTipOptions();
        var builder = new ToolTipBuilder(options);
        builder.SetEnabled();
        builder.Freeze();

        Assert.Throws<InvalidOperationException>(() => builder.SetEnabled(false));
        Assert.Throws<InvalidOperationException>(() => builder.SetSettings());
        Assert.True(options.IsTipsEnabled);
    }
}
