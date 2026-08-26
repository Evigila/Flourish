using Xunit;
using ArkheideSystem.Flourish.Shell.StatusBar;


namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class StatusBarBuilderTests
{
    [Fact]
    public void AddStatusItem_UpdatesOptionsAndReturnsBuilder()
    {
        var options = new FlourishStatusBarOptions();
        var sut = new StatusBarBuilder(options);

        Assert.Same(sut, sut.AddStatusItem("Online", "N"));

        var item = Assert.Single(options.StatusItems);
        Assert.Equal("Online", item.Text);
        Assert.Equal("N", item.IconGlyph);
    }

    [Fact]
    public void ShowSystemStatuses_EnableFlagsAndReturnBuilder()
    {
        var options = new FlourishStatusBarOptions();
        var sut = new StatusBarBuilder(options);

        var lanResult = sut.SetLanStatusEnabled();
        var powerResult = sut.SetPowerStatusEnabled();

        Assert.Same(sut, lanResult);
        Assert.Same(sut, powerResult);
        Assert.True(options.IsLANConnectionStatusEnabled);
        Assert.True(options.IsPowerStatusEnabled);
        Assert.Empty(options.StatusItems);
    }
}
