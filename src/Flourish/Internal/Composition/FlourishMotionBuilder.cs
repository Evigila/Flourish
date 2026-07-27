using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishMotionBuilder(FlourishMotionOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishMotionBuilder
{
    public IFlourishMotionBuilder UsePageTransition(
        bool enabled = true,
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom,
        TimeSpan? duration = null
    )
    {
        ThrowIfFrozen();
        ValidateEnum(transition, nameof(transition));
        options.PageTransition = enabled ? transition : FlourishPageTransition.None;
        if (duration is { } value)
        {
            options.PageTransitionDuration = ValidateDuration(value, nameof(duration));
        }

        return this;
    }

    public IFlourishMotionBuilder UseNavigationPanelTransition(
        bool enabled = true,
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize,
        TimeSpan? duration = null
    )
    {
        ThrowIfFrozen();
        ValidateEnum(transition, nameof(transition));
        options.NavigationPanelTransition = enabled
            ? transition
            : FlourishNavigationPanelTransition.None;
        if (duration is { } value)
        {
            options.NavigationPanelTransitionDuration = ValidateDuration(value, nameof(duration));
        }

        return this;
    }

    public IFlourishMotionBuilder UseHoverRevealAnimation(
        bool enabled = true,
        TimeSpan? duration = null
    )
    {
        ThrowIfFrozen();
        options.IsHoverRevealEnabled = enabled;
        if (duration is { } value)
        {
            options.HoverRevealAnimationDuration = ValidateDuration(value, nameof(duration));
        }

        return this;
    }

    public IFlourishMotionBuilder UseSystemReducedMotion(bool enabled = true)
    {
        ThrowIfFrozen();
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
