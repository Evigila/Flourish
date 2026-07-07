using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishMotionBuilder(FlourishMotionOptions options) : IFlourishMotionBuilder
{
    public IFlourishMotionBuilder SetDuration()
    {
        return SetDuration(TimeSpan.FromMilliseconds(180));
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

    public IFlourishMotionBuilder SetHoverReveal(bool enabled = true)
    {
        options.IsHoverRevealEnabled = enabled;
        return this;
    }

    public IFlourishMotionBuilder RespectSystemReducedMotion(bool enabled = true)
    {
        options.RespectSystemReducedMotion = enabled;
        return this;
    }
}
