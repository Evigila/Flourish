using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishMotionBuilder(FlourishMotionOptions options) : IFlourishMotionBuilder
{
    public IFlourishMotionBuilder EnablePageTransition(
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom,
        TimeSpan? duration = null
    )
    {
        ValidateEnum(transition, nameof(transition));
        options.PageTransition = transition;
        if (duration is { } value)
        {
            options.PageTransitionDuration = ValidateDuration(value, nameof(duration));
        }

        return this;
    }

    public IFlourishMotionBuilder EnableNavigationPanelTransition(
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize,
        TimeSpan? duration = null
    )
    {
        ValidateEnum(transition, nameof(transition));
        options.NavigationPanelTransition = transition;
        if (duration is { } value)
        {
            options.NavigationPanelTransitionDuration = ValidateDuration(value, nameof(duration));
        }

        return this;
    }

    public IFlourishMotionBuilder EnableHoverRevealAnimation(TimeSpan? duration = null)
    {
        options.IsHoverRevealEnabled = true;
        if (duration is { } value)
        {
            options.HoverRevealAnimationDuration = ValidateDuration(value, nameof(duration));
        }

        return this;
    }

    public IFlourishMotionBuilder RespectSystemReducedMotion(bool enabled = true)
    {
        options.RespectSystemReducedMotion = enabled;
        return this;
    }

    private static TimeSpan ValidateDuration(TimeSpan duration, string parameterName)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                duration,
                "Duration must be greater than zero."
            );
        }

        return duration;
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
    }
}
