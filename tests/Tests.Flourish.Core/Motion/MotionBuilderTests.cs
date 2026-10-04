using System;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Motion;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Motion;

public sealed class MotionBuilderTests
{
    [Fact]
    public void Options_HaveDocumentedDefaults()
    {
        var options = new MotionOptions();

        Assert.True(options.UsePersistedMotion);
        Assert.False(options.IsEnabled);
        Assert.Equal(PageTransition.EntranceFromBottom, options.PageTransition);
        Assert.Equal(TimeSpan.FromMilliseconds(180), options.PageTransitionDuration);
        Assert.Equal(
            NavigationPanelTransition.Resize,
            options.NavigationPanelTransition
        );
        Assert.Equal(
            TimeSpan.FromMilliseconds(180),
            options.NavigationPanelTransitionDuration
        );
        Assert.False(options.IsHoverRevealEnabled);
        Assert.Equal(
            TimeSpan.FromMilliseconds(140),
            options.HoverRevealAnimationDuration
        );
        Assert.True(options.RespectSystemReducedMotion);
        Assert.True(options.UsePersistedPageTransition);
        Assert.True(options.UsePersistedNavigationPanelTransition);
        Assert.True(options.UsePersistedHoverReveal);
        Assert.True(options.UsePersistedReducedMotion);
    }

    [Fact]
    public void ConfigurationMethods_WithExplicitValuesUpdateOptionsAndReturnBuilder()
    {
        var options = new MotionOptions();
        var sut = new MotionBuilder(options);
        var pageDuration = TimeSpan.FromMilliseconds(250);
        var navigationDuration = TimeSpan.FromMilliseconds(300);
        var hoverDuration = TimeSpan.FromMilliseconds(90);

        Assert.Same(sut, sut.SetEnabled());
        Assert.Same(
            sut,
            sut.SetPageTransition(
                transition: PageTransition.Fade,
                duration: pageDuration
            )
        );
        Assert.Same(
            sut,
            sut.SetNavigationPanelTransition(
                transition: NavigationPanelTransition.None,
                duration: navigationDuration
            )
        );
        Assert.Same(sut, sut.SetHoverReveal(duration: hoverDuration));
        Assert.Same(sut, sut.SetRespectSystemReducedMotion(false));

        Assert.True(options.IsEnabled);
        Assert.Equal(PageTransition.Fade, options.PageTransition);
        Assert.Equal(pageDuration, options.PageTransitionDuration);
        Assert.Equal(NavigationPanelTransition.None, options.NavigationPanelTransition);
        Assert.Equal(navigationDuration, options.NavigationPanelTransitionDuration);
        Assert.True(options.IsHoverRevealEnabled);
        Assert.Equal(hoverDuration, options.HoverRevealAnimationDuration);
        Assert.False(options.RespectSystemReducedMotion);
    }

    [Fact]
    public void ConfigurationMethods_WithoutDurationPreserveExistingDurations()
    {
        var options = new MotionOptions
        {
            PageTransitionDuration = TimeSpan.FromMilliseconds(11),
            NavigationPanelTransitionDuration = TimeSpan.FromMilliseconds(12),
            HoverRevealAnimationDuration = TimeSpan.FromMilliseconds(13),
        };
        var sut = new MotionBuilder(options);

        sut.SetPageTransition(transition: PageTransition.None);
        sut.SetNavigationPanelTransition(transition: NavigationPanelTransition.Resize);
        sut.SetHoverReveal();

        Assert.Equal(TimeSpan.FromMilliseconds(11), options.PageTransitionDuration);
        Assert.Equal(
            TimeSpan.FromMilliseconds(12),
            options.NavigationPanelTransitionDuration
        );
        Assert.Equal(
            TimeSpan.FromMilliseconds(13),
            options.HoverRevealAnimationDuration
        );
    }

    [Fact]
    public void DisabledTransitionsUseNoneAndRetainConfiguredDurations()
    {
        var options = new MotionOptions();
        var sut = new MotionBuilder(options);

        sut.SetPageTransition(
            enabled: false,
            transition: PageTransition.Fade,
            duration: TimeSpan.FromMilliseconds(25)
        );
        sut.SetNavigationPanelTransition(
            enabled: false,
            transition: NavigationPanelTransition.Resize,
            duration: TimeSpan.FromMilliseconds(35)
        );

        Assert.Equal(PageTransition.None, options.PageTransition);
        Assert.Equal(TimeSpan.FromMilliseconds(25), options.PageTransitionDuration);
        Assert.Equal(
            NavigationPanelTransition.None,
            options.NavigationPanelTransition
        );
        Assert.Equal(
            TimeSpan.FromMilliseconds(35),
            options.NavigationPanelTransitionDuration
        );
    }

    [Fact]
    public void PreferenceFlagsCanBeConfiguredIndependently()
    {
        var options = new MotionOptions();
        var sut = new MotionBuilder(options);

        sut.SetEnabled(usePersistedPreference: false)
            .SetPageTransition(usePersistedPreference: false)
            .SetNavigationPanelTransition(usePersistedPreference: false)
            .SetHoverReveal(usePersistedPreference: false)
            .SetRespectSystemReducedMotion(usePersistedPreference: false);

        Assert.False(options.UsePersistedMotion);
        Assert.False(options.UsePersistedPageTransition);
        Assert.False(options.UsePersistedNavigationPanelTransition);
        Assert.False(options.UsePersistedHoverReveal);
        Assert.False(options.UsePersistedReducedMotion);
    }

    [Theory]
    [InlineData("page", 0)]
    [InlineData("page", -1)]
    [InlineData("navigation", 0)]
    [InlineData("navigation", -1)]
    [InlineData("hover", 0)]
    [InlineData("hover", -1)]
    public void AnimationMethods_WithNonPositiveDurationThrow(
        string animation,
        long ticks
    )
    {
        var sut = new MotionBuilder(new MotionOptions());
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
    public void InvalidDuration_DoesNotPartiallyMutateOptions()
    {
        var options = new MotionOptions
        {
            PageTransition = PageTransition.Fade,
            IsHoverRevealEnabled = false,
        };
        var sut = new MotionBuilder(options);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPageTransition(
                transition: PageTransition.EntranceFromBottom,
                duration: TimeSpan.Zero,
                usePersistedPreference: false
            )
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetHoverReveal(
                enabled: true,
                duration: TimeSpan.Zero,
                usePersistedPreference: false
            )
        );

        Assert.Equal(PageTransition.Fade, options.PageTransition);
        Assert.True(options.UsePersistedPageTransition);
        Assert.False(options.IsHoverRevealEnabled);
        Assert.True(options.UsePersistedHoverReveal);
    }

    [Fact]
    public void UndefinedTransitionsThrowBeforeMutatingOptions()
    {
        var options = new MotionOptions();
        var sut = new MotionBuilder(options);

        var pageError = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPageTransition(transition: (PageTransition)int.MaxValue)
        );
        var navigationError = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetNavigationPanelTransition(
                transition: (NavigationPanelTransition)int.MaxValue
            )
        );

        Assert.Equal("transition", pageError.ParamName);
        Assert.Equal("transition", navigationError.ParamName);
        Assert.Equal(PageTransition.EntranceFromBottom, options.PageTransition);
        Assert.Equal(
            NavigationPanelTransition.Resize,
            options.NavigationPanelTransition
        );
    }

    [Fact]
    public void FrozenBuilderRejectsEveryMutation()
    {
        var sut = new MotionBuilder(new MotionOptions());
        sut.Freeze();

        Assert.Throws<InvalidOperationException>(() => sut.SetEnabled());
        Assert.Throws<InvalidOperationException>(() => sut.SetPageTransition());
        Assert.Throws<InvalidOperationException>(() =>
            sut.SetNavigationPanelTransition()
        );
        Assert.Throws<InvalidOperationException>(() => sut.SetHoverReveal());
        Assert.Throws<InvalidOperationException>(() =>
            sut.SetRespectSystemReducedMotion()
        );
    }

    [Fact]
    public void ConstructorRejectsNullOptions()
    {
        Assert.Throws<ArgumentNullException>(() => new MotionBuilder(null!));
    }

    [Fact]
    public void MotionSettingsPreserveRequestedAndEffectiveState()
    {
        var settings = new MotionSettings(
            IsEnabled: true,
            PageTransition.Fade,
            TimeSpan.FromMilliseconds(100),
            NavigationPanelTransition.Resize,
            TimeSpan.FromMilliseconds(120),
            IsHoverRevealEnabled: true,
            TimeSpan.FromMilliseconds(80),
            RespectSystemReducedMotion: true,
            CanAnimate: false
        );

        Assert.True(settings.IsEnabled);
        Assert.Equal(PageTransition.Fade, settings.PageTransition);
        Assert.True(settings.IsHoverRevealEnabled);
        Assert.True(settings.RespectSystemReducedMotion);
        Assert.False(settings.CanAnimate);
    }
}
