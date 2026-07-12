using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishMotionBuilderTests
{
    [Fact]
    public void ConfigurationMethods_WithExplicitValues_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishMotionOptions();
        var sut = new FlourishMotionBuilder(options);
        var pageDuration = TimeSpan.FromMilliseconds(250);
        var navigationDuration = TimeSpan.FromMilliseconds(300);
        var hoverDuration = TimeSpan.FromMilliseconds(90);

        Assert.Same(
            sut,
            sut.EnablePageTransition(FlourishPageTransition.Fade, pageDuration)
        );
        Assert.Same(
            sut,
            sut.EnableNavigationPanelTransition(
                FlourishNavigationPanelTransition.None,
                navigationDuration
            )
        );
        Assert.Same(sut, sut.EnableHoverRevealAnimation(hoverDuration));
        Assert.Same(sut, sut.RespectSystemReducedMotion(false));

        Assert.Equal(FlourishPageTransition.Fade, options.PageTransition);
        Assert.Equal(pageDuration, options.PageTransitionDuration);
        Assert.Equal(
            FlourishNavigationPanelTransition.None,
            options.NavigationPanelTransition
        );
        Assert.Equal(navigationDuration, options.NavigationPanelTransitionDuration);
        Assert.True(options.IsHoverRevealEnabled);
        Assert.Equal(hoverDuration, options.HoverRevealAnimationDuration);
        Assert.False(options.RespectSystemReducedMotion);
    }

    [Fact]
    public void ConfigurationMethods_WithoutDuration_PreserveExistingDurations()
    {
        var options = new FlourishMotionOptions
        {
            PageTransitionDuration = TimeSpan.FromMilliseconds(11),
            NavigationPanelTransitionDuration = TimeSpan.FromMilliseconds(12),
            HoverRevealAnimationDuration = TimeSpan.FromMilliseconds(13),
        };
        var sut = new FlourishMotionBuilder(options);

        sut.EnablePageTransition(FlourishPageTransition.None);
        sut.EnableNavigationPanelTransition(FlourishNavigationPanelTransition.Resize);
        sut.EnableHoverRevealAnimation();

        Assert.Equal(TimeSpan.FromMilliseconds(11), options.PageTransitionDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(12), options.NavigationPanelTransitionDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(13), options.HoverRevealAnimationDuration);
    }

    [Theory]
    [InlineData("page", 0)]
    [InlineData("page", -1)]
    [InlineData("navigation", 0)]
    [InlineData("navigation", -1)]
    [InlineData("hover", 0)]
    [InlineData("hover", -1)]
    public void AnimationMethods_WithNonPositiveDuration_ThrowArgumentOutOfRangeException(
        string animation,
        long ticks
    )
    {
        var sut = new FlourishMotionBuilder(new FlourishMotionOptions());
        var duration = TimeSpan.FromTicks(ticks);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            switch (animation)
            {
                case "page":
                    sut.EnablePageTransition(duration: duration);
                    break;
                case "navigation":
                    sut.EnableNavigationPanelTransition(duration: duration);
                    break;
                case "hover":
                    sut.EnableHoverRevealAnimation(duration);
                    break;
            }
        });

        Assert.Equal("duration", exception.ParamName);
    }

    [Fact]
    public void EnablePageTransition_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishMotionBuilder(new FlourishMotionOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.EnablePageTransition((FlourishPageTransition)int.MaxValue)
        );

        Assert.Equal("transition", exception.ParamName);
    }

    [Fact]
    public void EnableNavigationPanelTransition_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishMotionBuilder(new FlourishMotionOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.EnableNavigationPanelTransition(
                (FlourishNavigationPanelTransition)int.MaxValue
            )
        );

        Assert.Equal("transition", exception.ParamName);
    }
}
