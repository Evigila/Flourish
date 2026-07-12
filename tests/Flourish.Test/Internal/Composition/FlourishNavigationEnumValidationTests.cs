using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishNavigationEnumValidationTests
{
    [Fact]
    public void SetDirection_WithDefinedValue_UpdatesOptionsAndReturnsBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishNavigationBuilder(options);

        var result = sut.SetDirection(NavigationPanelDirection.Right);

        Assert.Same(sut, result);
        Assert.Equal(NavigationPanelDirection.Right, options.NavigationPanelDirection);
    }

    [Fact]
    public void SetDirection_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishNavigationBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetDirection((NavigationPanelDirection)int.MaxValue)
        );

        Assert.Equal("direction", exception.ParamName);
    }
}
