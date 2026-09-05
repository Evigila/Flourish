using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Specifies the transition used when content enters a navigation frame.</summary>
public enum PageTransition
{
    /// <summary>Disables the content transition.</summary>
    None,

    /// <summary>Fades the content into view.</summary>
    Fade,

    /// <summary>Moves and fades the content into view from the bottom edge.</summary>
    EntranceFromBottom,
}

/// <summary>Specifies the transition used when a navigation panel opens or closes.</summary>
public enum NavigationPanelTransition
{
    /// <summary>Disables the navigation-panel transition.</summary>
    None,

    /// <summary>Animates a visual resize and commits the final layout width when complete.</summary>
    Resize,
}

/// <summary>Represents runtime motion settings.</summary>
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

/// <summary>Configures motion and transition behavior.</summary>
public interface IMotionBuilder
{
    /// <summary>Enables or disables motion globally.</summary>
    IMotionBuilder SetEnabled(bool enabled = true, bool usePersistedPreference = true);

    /// <summary>Configures the transition used when content enters a navigation frame.</summary>
    /// <param name="enabled">Whether this transition is initially enabled.</param>
    /// <param name="transition">The transition to use.</param>
    /// <param name="duration">An optional positive transition duration.</param>
    /// <param name="usePersistedPreference">Whether the persisted preference is restored and updated.</param>
    IMotionBuilder SetPageTransition(
        bool enabled = true,
        PageTransition transition = PageTransition.EntranceFromBottom,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    );

    /// <summary>Configures the transition used when the navigation panel opens or closes.</summary>
    /// <param name="enabled">Whether this transition is initially enabled.</param>
    /// <param name="transition">The transition to use.</param>
    /// <param name="duration">An optional positive transition duration.</param>
    /// <param name="usePersistedPreference">Whether the persisted preference is restored and updated.</param>
    IMotionBuilder SetNavigationPanelTransition(
        bool enabled = true,
        NavigationPanelTransition transition = NavigationPanelTransition.Resize,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    );

    /// <summary>Configures hover-reveal motion.</summary>
    /// <param name="enabled">Whether hover-reveal motion is initially enabled.</param>
    /// <param name="duration">An optional positive animation duration.</param>
    /// <param name="usePersistedPreference">Whether the persisted preference is restored and updated.</param>
    IMotionBuilder SetHoverReveal(
        bool enabled = true,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    );

    /// <summary>Controls whether the operating-system reduced-motion preference is respected.</summary>
    /// <param name="enabled">Whether reduced-motion preferences should be respected.</param>
    /// <param name="usePersistedPreference">Whether the persisted preference is restored and updated.</param>
    IMotionBuilder SetRespectSystemReducedMotion(
        bool enabled = true,
        bool usePersistedPreference = true
    );
}

/// <summary>Controls motion behavior at runtime.</summary>
public interface IMotionService
{
    /// <summary>Gets the current motion settings.</summary>
    MotionSettings Current { get; }

    /// <summary>Occurs after the motion settings change.</summary>
    event EventHandler<StateTransitionEventArgs<MotionSettings>>? Changed;

    /// <summary>Enables or disables all motion.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Changes the content transition and optional duration.</summary>
    void SetPageTransition(PageTransition transition, TimeSpan? duration = null);

    /// <summary>Changes the navigation-panel transition and optional duration.</summary>
    void SetNavigationPanelTransition(
        NavigationPanelTransition transition,
        TimeSpan? duration = null
    );

    /// <summary>Enables or disables hover-reveal motion and optionally changes its duration.</summary>
    void SetHoverReveal(bool enabled, TimeSpan? duration = null);

    /// <summary>Changes whether operating-system reduced-motion preferences suppress motion.</summary>
    void SetRespectSystemReducedMotion(bool enabled);
}
