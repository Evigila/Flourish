using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides an immutable snapshot of a background task.
/// </summary>
public sealed class FlourishBackgroundTaskInfo
{
    internal FlourishBackgroundTaskInfo(
        Guid id,
        FlourishBackgroundTaskMetadata metadata,
        FlourishBackgroundTaskState state,
        double? progress,
        DateTimeOffset queuedAt,
        DateTimeOffset? startedAt,
        DateTimeOffset? completedAt,
        Exception? exception
    )
    {
        Id = id;
        Metadata = metadata;
        State = state;
        Progress = progress;
        QueuedAt = queuedAt;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Exception = exception;
    }

    /// <summary>
    /// Gets the unique task identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the task metadata supplied when the task was submitted.
    /// </summary>
    public FlourishBackgroundTaskMetadata Metadata { get; }

    /// <summary>
    /// Gets the current task state.
    /// </summary>
    public FlourishBackgroundTaskState State { get; }

    /// <summary>
    /// Gets reported progress in the inclusive range from zero to one, or <see langword="null" /> when no progress has been reported.
    /// </summary>
    public double? Progress { get; }

    /// <summary>
    /// Gets the UTC time at which the task was queued.
    /// </summary>
    public DateTimeOffset QueuedAt { get; }

    /// <summary>
    /// Gets the UTC time at which task execution started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; }

    /// <summary>
    /// Gets the UTC time at which the task reached a terminal state.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; }

    /// <summary>
    /// Gets the exception captured from a failed task.
    /// </summary>
    public Exception? Exception { get; }
}
