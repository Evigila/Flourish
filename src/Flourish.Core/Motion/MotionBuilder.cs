using System;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Motion;

internal sealed class MotionBuilder : BuilderMutationGuard, IMotionBuilder
{
    private readonly MotionOptions options;

    internal MotionBuilder(MotionOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

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
        PageTransition transition = PageTransition.EntranceFromBottom,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(transition, nameof(transition));
        var validatedDuration = ValidateOptionalDuration(duration, nameof(duration));

        options.PageTransition = enabled ? transition : PageTransition.None;
        if (validatedDuration is { } value)
        {
            options.PageTransitionDuration = value;
        }

        options.UsePersistedPageTransition = usePersistedPreference;
        return this;
    }

    public IMotionBuilder SetNavigationPanelTransition(
        bool enabled = true,
        NavigationPanelTransition transition = NavigationPanelTransition.Resize,
        TimeSpan? duration = null,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateEnum(transition, nameof(transition));
        var validatedDuration = ValidateOptionalDuration(duration, nameof(duration));

        options.NavigationPanelTransition = enabled
            ? transition
            : NavigationPanelTransition.None;
        if (validatedDuration is { } value)
        {
            options.NavigationPanelTransitionDuration = value;
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
        var validatedDuration = ValidateOptionalDuration(duration, nameof(duration));

        options.IsHoverRevealEnabled = enabled;
        if (validatedDuration is { } value)
        {
            options.HoverRevealAnimationDuration = value;
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

    private static TimeSpan? ValidateOptionalDuration(
        TimeSpan? duration,
        string parameterName
    )
    {
        if (duration is not { } value)
        {
            return null;
        }

        if (value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Duration must be greater than zero."
            );
        }

        return value;
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
