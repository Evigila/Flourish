using System;

using ArkheideSystem.Flourish.Shell.StatusBar;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Shell.StatusBar;

public sealed class StatusBarBuilderTests
{
    [Fact]
    public void ConfigurationMethods_UpdateOptionsAndReturnBuilder()
    {
        var options = new StatusBarOptions();
        var sut = new StatusBarBuilder(options);

        Assert.Same(sut, sut.SetEnabled());
        Assert.Same(sut, sut.AddStatusItem("Online", "N"));
        Assert.Same(sut, sut.SetLanStatusEnabled());
        Assert.Same(sut, sut.SetPowerStatusEnabled());

        Assert.True(options.IsStatusBarEnabled);
        Assert.True(options.IsLANConnectionStatusEnabled);
        Assert.True(options.IsPowerStatusEnabled);
        var item = Assert.Single(options.StatusItems);
        Assert.Equal("status:Online", item.Id);
        Assert.Equal("Online", item.Text);
        Assert.Equal("N", item.IconGlyph);
    }

    [Fact]
    public void AddStatusItem_UsesPublicDefaults()
    {
        var options = new StatusBarOptions();
        var sut = new StatusBarBuilder(options);

        sut.AddStatusItem();

        var item = Assert.Single(options.StatusItems);
        Assert.Equal("status:OK", item.Id);
        Assert.Equal("OK", item.Text);
        Assert.Equal("\uE930", item.IconGlyph);
    }

    [Fact]
    public void Freeze_RejectsEveryLaterMutation()
    {
        var sut = new StatusBarBuilder(new StatusBarOptions());
        sut.Freeze();

        Assert.Throws<InvalidOperationException>(() => sut.SetEnabled());
        Assert.Throws<InvalidOperationException>(() => sut.AddStatusItem());
        Assert.Throws<InvalidOperationException>(() => sut.SetLanStatusEnabled());
        Assert.Throws<InvalidOperationException>(() => sut.SetPowerStatusEnabled());
    }
}
