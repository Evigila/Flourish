using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishMotionBuilder(FlourishMotionOptions options) : IFlourishMotionBuilder
{
    public IFlourishMotionBuilder SetEnabled(bool enabled = true)
    {
        options.IsEnabled = enabled;
        return this;
    }

    public IFlourishMotionBuilder SetDuration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Duration must be greater than zero."
            );
        }

        options.Duration = duration;
        return this;
    }

    public IFlourishMotionBuilder SetPageTransition(
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom
    )
    {
        options.PageTransition = transition;
        return this;
    }

    public IFlourishMotionBuilder SetNavigationPanelTransition(
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize
    )
    {
        options.NavigationPanelTransition = transition;
        return this;
    }

    public IFlourishMotionBuilder RespectSystemReducedMotion(bool enabled = true)
    {
        options.RespectSystemReducedMotion = enabled;
        return this;
    }
}
