using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishToolbarServiceTests
{
    [Fact]
    public void GetToolbarItems_WhenDynamicToolbarIsEnabledAndPageMatches_ReturnsDynamicItems()
    {
        var options = new FlourishShellOptions { IsDynamicToolbarEnabled = true };
        var dynamicItems = new[] { new FlourishToolbarItem("Dynamic", "D") };
        options.ToolbarItems.Add(new FlourishToolbarItem("Static", "S"));
        options.DynamicToolbarItems[typeof(TestPage)] = dynamicItems;
        var sut = new FlourishToolbarService(options);

        var result = sut.GetToolbarItems(typeof(TestPage));

        Assert.NotSame(dynamicItems, result);
        Assert.Equal(dynamicItems, result);
    }

    [Fact]
    public void GetToolbarItems_WhenDynamicToolbarIsDisabled_ReturnsStaticItems()
    {
        var options = new FlourishShellOptions { IsDynamicToolbarEnabled = false };
        options.ToolbarItems.Add(new FlourishToolbarItem("Static", "S"));
        options.DynamicToolbarItems[typeof(TestPage)] =
        [
            new FlourishToolbarItem("Dynamic", "D"),
        ];
        var sut = new FlourishToolbarService(options);

        var result = sut.GetToolbarItems(typeof(TestPage));

        Assert.NotSame(options.ToolbarItems, result);
        Assert.Equal(options.ToolbarItems, result);
    }

    [Fact]
    public void GetToolbarItems_WithNullOrUnknownPage_ReturnsStaticItems()
    {
        var options = new FlourishShellOptions { IsDynamicToolbarEnabled = true };
        options.ToolbarItems.Add(new FlourishToolbarItem("Static", "S"));
        options.DynamicToolbarItems[typeof(TestPage)] =
        [
            new FlourishToolbarItem("Dynamic", "D"),
        ];
        var sut = new FlourishToolbarService(options);

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
