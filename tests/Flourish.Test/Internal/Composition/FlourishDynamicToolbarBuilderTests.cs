using System;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Shell.Toolbar;

using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class ToolbarBuilderTests
{
    [Fact]
    public void PublicContract_ExposesCoreAndDefaultInterfaceMethods()
    {
        var core = Assert.Single(
            typeof(IToolbarBuilder).GetMethods(),
            method => method.Name == "Set" && method.IsAbstract
        );
        Assert.Equal("iconOnly", core.GetParameters()[0].Name);
        Assert.Equal(typeof(bool), core.GetParameters()[0].ParameterType);

        var convenience = Assert.Single(
            typeof(IToolbarBuilder).GetMethods(),
            method => method.Name == "Set" && !method.IsAbstract
        );
        Assert.Equal(typeof(ToolbarItem[]), convenience.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void CreateToolbarItems_WithGenericPage_UsesIconModeByDefault()
    {
        var options = new ToolbarOptions();
        IToolbarBuilder sut = new ToolbarBuilder(options);
        var items = new[] { new ToolbarItem("Open", "O", "open") };

        var result = sut.Set<FirstPage>(items);

        Assert.Same(sut, result);
        Assert.Equal(items, options.DynamicToolbarItems[typeof(FirstPage)]);
        Assert.True(options.DynamicToolbarIconModes[typeof(FirstPage)]);
    }

    [Fact]
    public void CreateToolbarItems_WithGenericPageAndExplicitIconMode_UpdatesOptions()
    {
        var options = new ToolbarOptions();
        IToolbarBuilder sut = new ToolbarBuilder(options);
        var items = new[] { new ToolbarItem("Save", "S", "save") };

        var result = sut.Set<FirstPage>(false, items);

        Assert.Same(sut, result);
        Assert.Equal(items, options.DynamicToolbarItems[typeof(FirstPage)]);
        Assert.False(options.DynamicToolbarIconModes[typeof(FirstPage)]);
    }

    [Fact]
    public void CreateToolbarItems_WithAnotherGenericPage_UsesIconModeByDefault()
    {
        var options = new ToolbarOptions();
        IToolbarBuilder sut = new ToolbarBuilder(options);
        var items = new[] { new ToolbarItem("Refresh", "R") };

        sut.Set<SecondPage>(items);

        Assert.Equal(items, options.DynamicToolbarItems[typeof(SecondPage)]);
        Assert.True(options.DynamicToolbarIconModes[typeof(SecondPage)]);
    }

    [Fact]
    public void CreateToolbarItems_WhenPageAlreadyConfigured_ReplacesItemsAndIconMode()
    {
        var options = new ToolbarOptions();
        IToolbarBuilder sut = new ToolbarBuilder(options);
        var firstItems = new[] { new ToolbarItem("First", "1") };
        var replacementItems = new[] { new ToolbarItem("Second", "2") };
        sut.Set<FirstPage>(false, firstItems);

        sut.Set<FirstPage>(true, replacementItems);

        Assert.Equal(replacementItems, options.DynamicToolbarItems[typeof(FirstPage)]);
        Assert.True(options.DynamicToolbarIconModes[typeof(FirstPage)]);
        Assert.Single(options.DynamicToolbarItems);
        Assert.Single(options.DynamicToolbarIconModes);
    }

    [Fact]
    public void CreateToolbarItems_WithNullItems_ThrowsArgumentNullException()
    {
        IToolbarBuilder sut = new ToolbarBuilder(new ToolbarOptions());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            sut.Set<FirstPage>(null!)
        );

        Assert.Equal("items", exception.ParamName);
    }

    [Fact]
    public void CreateToolbarItems_WithNullElement_ThrowsArgumentException()
    {
        IToolbarBuilder sut = new ToolbarBuilder(new ToolbarOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.Set<FirstPage>(new ToolbarItem("Valid", "V"), null!)
        );

        Assert.Equal("items", exception.ParamName);
    }

    private sealed class FirstPage : Page { }

    private sealed class SecondPage : Page { }
}
