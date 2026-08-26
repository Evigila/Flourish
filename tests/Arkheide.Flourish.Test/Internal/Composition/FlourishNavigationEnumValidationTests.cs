using System;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Navigation;
using Microsoft.Extensions.DependencyInjection;


namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishNavigationEnumValidationTests
{
    [Fact]
    public void SetDirection_WithDefinedValue_UpdatesOptionsAndReturnsBuilder()
    {
        var options = new FlourishNavigationOptions();
        var sut = new NavigationBuilder(options, new ServiceCollection());

        var result = sut.SetDirection(NavigationPanelDirection.Right);

        Assert.Same(sut, result);
        Assert.Equal(NavigationPanelDirection.Right, options.NavigationPanelDirection);
    }

    [Fact]
    public void SetDirection_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new NavigationBuilder(new FlourishNavigationOptions(), new ServiceCollection());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetDirection((NavigationPanelDirection)int.MaxValue)
        );

        Assert.Equal("direction", exception.ParamName);
    }
}
