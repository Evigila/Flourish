using AckSS.Flourish.Abstract;

namespace AckSS.Flourish.Configuration;

internal sealed class FlourishMotionOptions
{
    public bool IsEnabled { get; set; }

    public FlourishPageTransition PageTransition { get; set; } =
        FlourishPageTransition.EntranceFromBottom;

    public TimeSpan PageTransitionDuration { get; set; } = TimeSpan.FromMilliseconds(180);

    public FlourishNavigationPanelTransition NavigationPanelTransition { get; set; } =
        FlourishNavigationPanelTransition.Resize;

    public TimeSpan NavigationPanelTransitionDuration { get; set; } =
        TimeSpan.FromMilliseconds(180);

    public bool IsHoverRevealEnabled { get; set; }

    public TimeSpan HoverRevealAnimationDuration { get; set; } = TimeSpan.FromMilliseconds(140);

    public bool RespectSystemReducedMotion { get; set; } = true;
}
