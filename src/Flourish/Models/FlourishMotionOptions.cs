using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Models;

internal sealed class FlourishMotionOptions
{
    public bool IsEnabled { get; set; }

    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(180);

    public FlourishPageTransition PageTransition { get; set; } =
        FlourishPageTransition.EntranceFromBottom;

    public FlourishNavigationPanelTransition NavigationPanelTransition { get; set; } =
        FlourishNavigationPanelTransition.Resize;

    public bool RespectSystemReducedMotion { get; set; } = true;
}
