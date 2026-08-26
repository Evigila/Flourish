using System;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Motion;


namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class MotionBuilderTests
{
    [Fact]
    public void ConfigurationMethods_WithExplicitValues_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishMotionOptions();
        var sut = new MotionBuilder(options);
        var pageDuration = TimeSpan.FromMilliseconds(250);
        var navigationDuration = TimeSpan.FromMilliseconds(300);
        var hoverDuration = TimeSpan.FromMilliseconds(90);

        Assert.Same(
            sut,
            sut.SetPageTransition(transition: FlourishPageTransition.Fade, duration: pageDuration)
        );
        Assert.Same(
            sut,
            sut.SetNavigationPanelTransition(
                transition: FlourishNavigationPanelTransition.None,
                duration: navigationDuration
            )
        );
        Assert.Same(sut, sut.SetHoverReveal(duration: hoverDuration));
        Assert.Same(sut, sut.SetRespectSystemReducedMotion(false));

        Assert.Equal(FlourishPageTransition.Fade, options.PageTransition);
        Assert.Equal(pageDuration, options.PageTransitionDuration);
        Assert.Equal(FlourishNavigationPanelTransition.None, options.NavigationPanelTransition);
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
        var sut = new MotionBuilder(options);

        sut.SetPageTransition(transition: FlourishPageTransition.None);
        sut.SetNavigationPanelTransition(transition: FlourishNavigationPanelTransition.Resize);
        sut.SetHoverReveal();

        Assert.Equal(TimeSpan.FromMilliseconds(11), options.PageTransitionDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(12), options.NavigationPanelTransitionDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(13), options.HoverRevealAnimationDuration);
    }

    [Fact]
    public void PreferenceAwareMethods_EnableTheirIndependentPolicies()
    {
        var options = new FlourishMotionOptions();
        var sut = new MotionBuilder(options);

        sut.SetPageTransition(true, FlourishPageTransition.Fade, null, true)
            .SetNavigationPanelTransition(
                true,
                FlourishNavigationPanelTransition.Resize,
                null,
                true
            )
            .SetHoverReveal(true, null, true)
            .SetRespectSystemReducedMotion(true, true);

        Assert.True(options.UsePersistedPageTransition);
        Assert.True(options.UsePersistedNavigationPanelTransition);
        Assert.True(options.UsePersistedHoverReveal);
        Assert.True(options.UsePersistedReducedMotion);
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
        var sut = new MotionBuilder(new FlourishMotionOptions());
        var duration = TimeSpan.FromTicks(ticks);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            switch (animation)
            {
                case "page":
                    sut.SetPageTransition(duration: duration);
                    break;
                case "navigation":
                    sut.SetNavigationPanelTransition(duration: duration);
                    break;
                case "hover":
                    sut.SetHoverReveal(duration: duration);
                    break;
            }
        });

        Assert.Equal("duration", exception.ParamName);
    }

    [Fact]
    public void EnablePageTransition_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new MotionBuilder(new FlourishMotionOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPageTransition(transition: (FlourishPageTransition)int.MaxValue)
        );

        Assert.Equal("transition", exception.ParamName);
    }

    [Fact]
    public void EnableNavigationPanelTransition_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new MotionBuilder(new FlourishMotionOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetNavigationPanelTransition(
                transition: (FlourishNavigationPanelTransition)int.MaxValue
            )
        );

        Assert.Equal("transition", exception.ParamName);
    }
}
