using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishStatusBarBuilderTests
{
    [Fact]
    public void AddStatusItem_UpdatesOptionsAndReturnsBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishStatusBarBuilder(options);

        Assert.Same(sut, sut.AddStatusItem("Online", "N"));

        var item = Assert.Single(options.StatusItems);
        Assert.Equal("Online", item.Text);
        Assert.Equal("N", item.IconGlyph);
    }

    [Fact]
    public void ShowSystemStatuses_EnableFlagsAndReturnBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishStatusBarBuilder(options);

        var lanResult = sut.ShowLANConnectionStatus();
        var powerResult = sut.ShowPowerStatus();

        Assert.Same(sut, lanResult);
        Assert.Same(sut, powerResult);
        Assert.True(options.IsLANConnectionStatusEnabled);
        Assert.True(options.IsPowerStatusEnabled);
        Assert.Empty(options.StatusItems);
    }
}
