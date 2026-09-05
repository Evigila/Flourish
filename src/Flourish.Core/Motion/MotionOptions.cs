using System;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Motion;

internal sealed class MotionOptions
{
    public bool UsePersistedMotion { get; set; } = true;

    public bool IsEnabled { get; set; }

    public PageTransition PageTransition { get; set; } =
        PageTransition.EntranceFromBottom;

    public TimeSpan PageTransitionDuration { get; set; } = TimeSpan.FromMilliseconds(180);

    public NavigationPanelTransition NavigationPanelTransition { get; set; } =
        NavigationPanelTransition.Resize;

    public TimeSpan NavigationPanelTransitionDuration { get; set; } =
        TimeSpan.FromMilliseconds(180);

    public bool IsHoverRevealEnabled { get; set; }

    public TimeSpan HoverRevealAnimationDuration { get; set; } =
        TimeSpan.FromMilliseconds(140);

    public bool RespectSystemReducedMotion { get; set; } = true;

    public bool UsePersistedPageTransition { get; set; } = true;

    public bool UsePersistedNavigationPanelTransition { get; set; } = true;

    public bool UsePersistedHoverReveal { get; set; } = true;

    public bool UsePersistedReducedMotion { get; set; } = true;
}
