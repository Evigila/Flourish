using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents runtime Flourish animation settings.
/// </summary>
public sealed record FlourishMotionSettings(
    bool IsEnabled,
    FlourishPageTransition PageTransition,
    TimeSpan PageTransitionDuration,
    FlourishNavigationPanelTransition NavigationPanelTransition,
    TimeSpan NavigationPanelTransitionDuration,
    bool IsHoverRevealEnabled,
    TimeSpan HoverRevealAnimationDuration,
    bool RespectSystemReducedMotion,
    bool CanAnimate
);
