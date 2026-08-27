using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents runtime Flourish animation settings.
/// </summary>
public sealed record MotionSettings(
    bool IsEnabled,
    PageTransition PageTransition,
    TimeSpan PageTransitionDuration,
    NavigationPanelTransition NavigationPanelTransition,
    TimeSpan NavigationPanelTransitionDuration,
    bool IsHoverRevealEnabled,
    TimeSpan HoverRevealAnimationDuration,
    bool RespectSystemReducedMotion,
    bool CanAnimate
);
