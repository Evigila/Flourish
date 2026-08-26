using System;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Motion;

internal sealed class MotionBuilder(FlourishMotionOptions options)
    : FlourishBuilderMutationGuard,
        IMotionBuilder
{
    public IMotionBuilder SetEnabled(
        bool enabled = true,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.IsEnabled = enabled;
        options.UsePersistedMotion = usePersistedPreference;

        return this;
    }

    public IMotionBuilder SetPageTransition(
        bool enabled = true,
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(transition, nameof(transition));
        options.PageTransition = enabled ? transition : FlourishPageTransition.None;
        if (duration is { } value)
        {
            options.PageTransitionDuration = ValidateDuration(value, nameof(duration));
        }

        options.UsePersistedPageTransition = usePersistedPreference;
        return this;
    }

    public IMotionBuilder SetNavigationPanelTransition(
        bool enabled = true,
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
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

        options.UsePersistedNavigationPanelTransition = usePersistedPreference;
        return this;
    }

    public IMotionBuilder SetHoverReveal(
        bool enabled = true,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.IsHoverRevealEnabled = enabled;
        if (duration is { } value)
        {
            options.HoverRevealAnimationDuration = ValidateDuration(value, nameof(duration));
        }

        options.UsePersistedHoverReveal = usePersistedPreference;
        return this;
    }

    public IMotionBuilder SetRespectSystemReducedMotion(
        bool enabled = true,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.RespectSystemReducedMotion = enabled;
        options.UsePersistedReducedMotion = usePersistedPreference;
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
