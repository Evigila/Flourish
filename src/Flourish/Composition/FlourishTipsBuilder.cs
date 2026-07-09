using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishTipsBuilder(FlourishTipOptions options) : IFlourishTipsBuilder
{
    public IFlourishTipsBuilder SetDelay(int milliseconds = 200)
    {
        if (milliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(milliseconds),
                milliseconds,
                "Tooltip delay cannot be negative."
            );
        }

        options.InitialShowDelayMilliseconds = milliseconds;
        return this;
    }

    public IFlourishTipsBuilder SetSpawnableMargin(double margin = 5)
    {
        if (double.IsNaN(margin) || double.IsInfinity(margin) || margin < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(margin),
                margin,
                "Tooltip spawnable margin must be a non-negative finite value."
            );
        }

        options.SpawnableMargin = margin;
        return this;
    }
}
