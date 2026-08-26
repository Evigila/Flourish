using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls Flourish animation behavior at runtime.
/// </summary>
public interface IMotionService
{
    /// <summary>
    /// Gets the current animation settings.
    /// </summary>
    FlourishMotionSettings Current { get; }

    /// <summary>
    /// Raised after animation settings change.
    /// </summary>
    /// <remarks>
    /// When a shell window is attached, the event is raised on its dispatcher.
    /// </remarks>
    event EventHandler<FlourishStateTransitionEventArgs<FlourishMotionSettings>>? Changed;

    /// <summary>
    /// Enables or disables all Flourish animation.
    /// </summary>
    void SetEnabled(bool enabled);

    /// <summary>
    /// Changes the page transition and duration.
    /// </summary>
    void SetPageTransition(
        FlourishPageTransition transition,
        TimeSpan? duration = null
    );

    /// <summary>
    /// Changes the navigation panel transition and duration.
    /// </summary>
    void SetNavigationPanelTransition(
        FlourishNavigationPanelTransition transition,
        TimeSpan? duration = null
    );

    /// <summary>
    /// Enables or disables hover-reveal animation and changes its duration.
    /// </summary>
    void SetHoverReveal(bool enabled, TimeSpan? duration = null);

    /// <summary>
    /// Changes whether Windows reduced-motion preferences suppress animation.
    /// </summary>
    void SetRespectSystemReducedMotion(bool enabled);
}
