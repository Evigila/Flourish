using System;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.ToolTips;

internal sealed class ToolTipBuilder(ToolTipOptions options)
    : BuilderMutationGuard,
        IToolTipBuilder
{
    public IToolTipBuilder SetEnabled(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsTipsEnabled = enabled;
        return this;
    }

    public IToolTipBuilder SetSettings(
        int initialShowDelayMilliseconds = 200,
        double spawnableMargin = 5
    )
    {
        ThrowIfFrozen();
        if (initialShowDelayMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialShowDelayMilliseconds),
                initialShowDelayMilliseconds,
                "Tooltip delay cannot be negative."
            );
        }

        ValueValidation.NonNegativeFinite(spawnableMargin, nameof(spawnableMargin));
        options.InitialShowDelayMilliseconds = initialShowDelayMilliseconds;
        options.SpawnableMargin = spawnableMargin;
        return this;
    }
}
