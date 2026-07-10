using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishDynamicToolbarBuilderTests
{
    [Fact]
    public void CreateToolbarItems_WithGenericPage_UsesIconModeByDefault()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishDynamicToolbarBuilder(options);
        var items = new[] { new FlourishToolbarItem("Open", "O", "open") };

        var result = sut.CreateToolbarItems<FirstPage>(items);

        Assert.Same(sut, result);
        Assert.Equal(items, options.DynamicToolbarItems[typeof(FirstPage)]);
        Assert.True(options.DynamicToolbarIconModes[typeof(FirstPage)]);
    }

    [Fact]
    public void CreateToolbarItems_WithGenericPageAndExplicitIconMode_UpdatesOptions()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishDynamicToolbarBuilder(options);
        var items = new[] { new FlourishToolbarItem("Save", "S", "save") };

        var result = sut.CreateToolbarItems<FirstPage>(false, items);

        Assert.Same(sut, result);
        Assert.Equal(items, options.DynamicToolbarItems[typeof(FirstPage)]);
        Assert.False(options.DynamicToolbarIconModes[typeof(FirstPage)]);
    }

    [Fact]
    public void CreateToolbarItems_WithAnotherGenericPage_UsesIconModeByDefault()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishDynamicToolbarBuilder(options);
        var items = new[] { new FlourishToolbarItem("Refresh", "R") };

        sut.CreateToolbarItems<SecondPage>(items);

        Assert.Equal(items, options.DynamicToolbarItems[typeof(SecondPage)]);
        Assert.True(options.DynamicToolbarIconModes[typeof(SecondPage)]);
    }

    [Fact]
    public void CreateToolbarItems_WhenPageAlreadyConfigured_ReplacesItemsAndIconMode()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishDynamicToolbarBuilder(options);
        var firstItems = new[] { new FlourishToolbarItem("First", "1") };
        var replacementItems = new[] { new FlourishToolbarItem("Second", "2") };
        sut.CreateToolbarItems<FirstPage>(false, firstItems);

        sut.CreateToolbarItems<FirstPage>(true, replacementItems);

        Assert.Equal(replacementItems, options.DynamicToolbarItems[typeof(FirstPage)]);
        Assert.True(options.DynamicToolbarIconModes[typeof(FirstPage)]);
        Assert.Single(options.DynamicToolbarItems);
        Assert.Single(options.DynamicToolbarIconModes);
    }

    [Fact]
    public void CreateToolbarItems_WithNullItems_ThrowsArgumentNullException()
    {
        var sut = new FlourishDynamicToolbarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            sut.CreateToolbarItems<FirstPage>(null!)
        );

        Assert.Equal("items", exception.ParamName);
    }

    [Fact]
    public void CreateToolbarItems_WithNullElement_ThrowsArgumentException()
    {
        var sut = new FlourishDynamicToolbarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.CreateToolbarItems<FirstPage>(
                new FlourishToolbarItem("Valid", "V"),
                null!
            )
        );

        Assert.Equal("items", exception.ParamName);
    }

    private sealed class FirstPage : Page { }

    private sealed class SecondPage : Page { }
}
