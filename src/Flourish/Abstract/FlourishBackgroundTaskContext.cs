using System;
using System.Threading;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Supplies cooperative cancellation and progress reporting to a background task.
/// </summary>
public sealed class FlourishBackgroundTaskContext
{
    private readonly Action<double> reportProgress;

    internal FlourishBackgroundTaskContext(
        CancellationToken cancellationToken,
        Action<double> reportProgress
    )
    {
        CancellationToken = cancellationToken;
        this.reportProgress = reportProgress;
    }

    /// <summary>
    /// Gets the token that is canceled when the task or application host is stopped.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Reports task progress in the inclusive range from zero to one.
    /// </summary>
    /// <param name="progress">The completed fraction, from zero to one.</param>
    public void ReportProgress(double progress)
    {
        if (!double.IsFinite(progress) || progress is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(progress),
                progress,
                "Background task progress must be a finite value from zero to one."
            );
        }

        reportProgress(progress);
    }
}
