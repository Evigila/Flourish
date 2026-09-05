using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Shell.Toolbar;

using System.Windows.Controls;

namespace ArkheideSystem.Flourish.WPF.Test.Services;

public sealed class ToolbarServiceTests
{
    [Fact]
    public void GetToolbarItems_WhenDynamicToolbarIsEnabledAndPageMatches_ReturnsDynamicItems()
    {
        var options = new ToolbarOptions { IsDynamicToolbarEnabled = true };
        var dynamicItems = new[] { new ToolbarItem("Dynamic", "D") };
        options.ToolbarItems.Add(new ToolbarItem("Static", "S"));
        options.DynamicToolbarItems[typeof(TestPage)] = dynamicItems;
        var sut = new ToolbarService(options);

        var result = sut.GetToolbarItems(typeof(TestPage));

        Assert.NotSame(dynamicItems, result);
        Assert.Equal(dynamicItems, result);
    }

    [Fact]
    public void GetToolbarItems_WhenDynamicToolbarIsDisabled_ReturnsStaticItems()
    {
        var options = new ToolbarOptions { IsDynamicToolbarEnabled = false };
        options.ToolbarItems.Add(new ToolbarItem("Static", "S"));
        options.DynamicToolbarItems[typeof(TestPage)] = [new ToolbarItem("Dynamic", "D")];
        var sut = new ToolbarService(options);

        var result = sut.GetToolbarItems(typeof(TestPage));

        Assert.NotSame(options.ToolbarItems, result);
        Assert.Equal(options.ToolbarItems, result);
    }

    [Fact]
    public void GetToolbarItems_WithNullOrUnknownPage_ReturnsStaticItems()
    {
        var options = new ToolbarOptions { IsDynamicToolbarEnabled = true };
        options.ToolbarItems.Add(new ToolbarItem("Static", "S"));
        options.DynamicToolbarItems[typeof(TestPage)] = [new ToolbarItem("Dynamic", "D")];
        var sut = new ToolbarService(options);

        var nullPageResult = sut.GetToolbarItems();
        var unknownPageResult = sut.GetToolbarItems(typeof(OtherPage));

        Assert.NotSame(options.ToolbarItems, nullPageResult);
        Assert.NotSame(options.ToolbarItems, unknownPageResult);
        Assert.Equal(options.ToolbarItems, nullPageResult);
        Assert.Equal(options.ToolbarItems, unknownPageResult);
    }

    private sealed class TestPage : Page { }

    private sealed class OtherPage : Page { }
}
