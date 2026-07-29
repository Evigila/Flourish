using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishDynamicToolbarBuilderTests
{
    [Fact]
    public void PublicContract_ExposesOneCoreMethodAndOneDefaultExtension()
    {
        var core = Assert.Single(
            typeof(IFlourishDynamicToolbarBuilder).GetMethods(),
            method => method.Name == "InitToolbarItems"
        );
        Assert.Equal("iconOnly", core.GetParameters()[0].Name);
        Assert.Equal(typeof(bool), core.GetParameters()[0].ParameterType);

        var convenience = Assert.Single(
            typeof(FlourishDynamicToolbarBuilderExtensions).GetMethods(),
            method => method.Name == "InitToolbarItems"
        );
        Assert.Equal(
            typeof(IFlourishDynamicToolbarBuilder),
            convenience.GetParameters()[0].ParameterType
        );
    }

    [Fact]
    public void CreateToolbarItems_WithGenericPage_UsesIconModeByDefault()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishDynamicToolbarBuilder(options);
        var items = new[] { new FlourishToolbarItem("Open", "O", "open") };

        var result = sut.InitToolbarItems<FirstPage>(items);

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

        var result = sut.InitToolbarItems<FirstPage>(false, items);

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

        sut.InitToolbarItems<SecondPage>(items);

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
        sut.InitToolbarItems<FirstPage>(false, firstItems);

        sut.InitToolbarItems<FirstPage>(true, replacementItems);

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
            sut.InitToolbarItems<FirstPage>(null!)
        );

        Assert.Equal("items", exception.ParamName);
    }

    [Fact]
    public void CreateToolbarItems_WithNullElement_ThrowsArgumentException()
    {
        var sut = new FlourishDynamicToolbarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.InitToolbarItems<FirstPage>(
                new FlourishToolbarItem("Valid", "V"),
                null!
            )
        );

        Assert.Equal("items", exception.ParamName);
    }

    private sealed class FirstPage : Page { }

    private sealed class SecondPage : Page { }
}
