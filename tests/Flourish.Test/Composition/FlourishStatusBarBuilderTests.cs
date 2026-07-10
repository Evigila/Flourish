using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishStatusBarBuilderTests
{
    [Fact]
    public void SetStatusTextAndAddStatusItem_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishStatusBarBuilder(options);

        Assert.Same(sut, sut.SetStatusText("Ready"));
        Assert.Same(sut, sut.AddStatusItem("Online", "N"));

        Assert.Equal("Ready", options.StatusText);
        var item = Assert.Single(options.StatusItems);
        Assert.Equal("Online", item.Text);
        Assert.Equal("N", item.IconGlyph);
    }

    [Fact]
    public void ShowPowerStatus_AddsBuiltInItemAndReturnsBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishStatusBarBuilder(options);

        var result = sut.ShowPowerStatus();

        Assert.Same(sut, result);
        var item = Assert.Single(options.StatusItems);
        Assert.False(string.IsNullOrWhiteSpace(item.Text));
        Assert.Equal("\uE850", item.IconGlyph);
    }
}
