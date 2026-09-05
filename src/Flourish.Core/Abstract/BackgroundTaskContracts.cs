using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Describes the lifecycle state of a background task.</summary>
public enum BackgroundTaskState
{
    /// <summary>The task is waiting for an execution slot.</summary>
    Queued,

    /// <summary>The task delegate is running.</summary>
    Running,

    /// <summary>Cancellation was requested and the delegate is finishing.</summary>
    Cancelling,

    /// <summary>The task completed successfully.</summary>
    Succeeded,

    /// <summary>The task was canceled.</summary>
    Canceled,

    /// <summary>The task failed with an exception.</summary>
    Failed,
}

/// <summary>Describes a background task before it is submitted.</summary>
public sealed class BackgroundTaskMetadata
{
    /// <summary>Initializes background task metadata.</summary>
    public BackgroundTaskMetadata(
        string name,
        string? description = null,
        string? iconGlyph = null
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Background task name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
        Description = NormalizeOptionalValue(description);
        IconGlyph = NormalizeOptionalValue(iconGlyph);
    }

    /// <summary>Gets the task name displayed to the user.</summary>
    public string Name { get; }

    /// <summary>Gets the optional task description.</summary>
    public string? Description { get; }

    /// <summary>Gets the optional task icon glyph.</summary>
    public string? IconGlyph { get; }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

/// <summary>Provides an immutable snapshot of a background task.</summary>
public sealed class BackgroundTaskInfo
{
    internal BackgroundTaskInfo(
        Guid id,
        BackgroundTaskMetadata metadata,
        BackgroundTaskState state,
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

    /// <summary>Gets the unique task identifier.</summary>
    public Guid Id { get; }

    /// <summary>Gets the task metadata.</summary>
    public BackgroundTaskMetadata Metadata { get; }

    /// <summary>Gets the current task state.</summary>
    public BackgroundTaskState State { get; }

    /// <summary>Gets progress from zero to one, or null when unreported.</summary>
    public double? Progress { get; }

    /// <summary>Gets the UTC time at which the task was queued.</summary>
    public DateTimeOffset QueuedAt { get; }

    /// <summary>Gets the UTC time at which execution started.</summary>
    public DateTimeOffset? StartedAt { get; }

    /// <summary>Gets the UTC time at which the task reached a terminal state.</summary>
    public DateTimeOffset? CompletedAt { get; }

    /// <summary>Gets the captured exception.</summary>
    public Exception? Exception { get; }
}

/// <summary>Represents the terminal outcome of a background task.</summary>
public sealed class BackgroundTaskResult
{
    internal BackgroundTaskResult(BackgroundTaskInfo info)
    {
        Info = info;
    }

    /// <summary>Gets the final task snapshot.</summary>
    public BackgroundTaskInfo Info { get; }

    /// <summary>Gets whether the task succeeded.</summary>
    public bool Succeeded => Info.State == BackgroundTaskState.Succeeded;

    /// <summary>Gets whether the task was canceled.</summary>
    public bool Canceled => Info.State == BackgroundTaskState.Canceled;

    /// <summary>Gets the captured exception.</summary>
    public Exception? Exception => Info.Exception;
}

/// <summary>Represents the terminal outcome and value of a background task.</summary>
public sealed class BackgroundTaskResult<TResult>
{
    internal BackgroundTaskResult(BackgroundTaskInfo info, TResult? value)
    {
        Info = info;
        Value = value;
    }

    /// <summary>Gets the final task snapshot.</summary>
    public BackgroundTaskInfo Info { get; }

    /// <summary>Gets the result value when the task succeeded.</summary>
    public TResult? Value { get; }

    /// <summary>Gets whether the task succeeded.</summary>
    public bool Succeeded => Info.State == BackgroundTaskState.Succeeded;

    /// <summary>Gets whether the task was canceled.</summary>
    public bool Canceled => Info.State == BackgroundTaskState.Canceled;

    /// <summary>Gets the captured exception.</summary>
    public Exception? Exception => Info.Exception;
}

/// <summary>Controls and observes a submitted background task.</summary>
public sealed class BackgroundTaskHandle
{
    private readonly Func<bool> cancel;
    private readonly Func<BackgroundTaskInfo> getSnapshot;

    internal BackgroundTaskHandle(
        Guid id,
        Task<BackgroundTaskResult> completion,
        Func<bool> cancel,
        Func<BackgroundTaskInfo> getSnapshot
    )
    {
        Id = id;
        Completion = completion;
        this.cancel = cancel;
        this.getSnapshot = getSnapshot;
    }

    /// <summary>Gets the unique task identifier.</summary>
    public Guid Id { get; }

    /// <summary>Gets the completion carrying the captured outcome.</summary>
    public Task<BackgroundTaskResult> Completion { get; }

    /// <summary>Gets the latest immutable task snapshot.</summary>
    public BackgroundTaskInfo Snapshot => getSnapshot();

    /// <summary>Requests cooperative cancellation.</summary>
    public bool Cancel() => cancel();
}

/// <summary>Controls and observes a submitted background task with a value.</summary>
public sealed class BackgroundTaskHandle<TResult>
{
    private readonly Func<bool> cancel;
    private readonly Func<BackgroundTaskInfo> getSnapshot;

    internal BackgroundTaskHandle(
        Guid id,
        Task<BackgroundTaskResult<TResult>> completion,
        Func<bool> cancel,
        Func<BackgroundTaskInfo> getSnapshot
    )
    {
        Id = id;
        Completion = completion;
        this.cancel = cancel;
        this.getSnapshot = getSnapshot;
    }

    /// <summary>Gets the unique task identifier.</summary>
    public Guid Id { get; }

    /// <summary>Gets the completion carrying the captured outcome and value.</summary>
    public Task<BackgroundTaskResult<TResult>> Completion { get; }

    /// <summary>Gets the latest immutable task snapshot.</summary>
    public BackgroundTaskInfo Snapshot => getSnapshot();

    /// <summary>Requests cooperative cancellation.</summary>
    public bool Cancel() => cancel();
}

/// <summary>Supplies cooperative cancellation and progress reporting.</summary>
public sealed class BackgroundTaskContext
{
    private readonly Action<double> reportProgress;

    internal BackgroundTaskContext(
        CancellationToken cancellationToken,
        Action<double> reportProgress
    )
    {
        CancellationToken = cancellationToken;
        this.reportProgress = reportProgress;
    }

    /// <summary>Gets the task cancellation token.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>Reports progress in the inclusive range from zero to one.</summary>
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

/// <summary>Queues and executes asynchronous work with bounded concurrency.</summary>
public interface IBackgroundTaskService
{
    /// <summary>Gets the maximum number of concurrent delegates.</summary>
    int MaxConcurrency { get; }

    /// <summary>Gets queued, running, and cancelling task snapshots.</summary>
    IReadOnlyList<BackgroundTaskInfo> Current { get; }

    /// <summary>Occurs when task state or progress changes.</summary>
    event EventHandler<
        StateChangedEventArgs<IReadOnlyList<BackgroundTaskInfo>>
    >? Changed;

    /// <summary>Submits asynchronous work without a return value.</summary>
    BackgroundTaskHandle QueueTask(
        BackgroundTaskMetadata metadata,
        Func<BackgroundTaskContext, ValueTask> task
    );

    /// <summary>Submits asynchronous work with a return value.</summary>
    BackgroundTaskHandle<TResult> QueueTask<TResult>(
        BackgroundTaskMetadata metadata,
        Func<BackgroundTaskContext, ValueTask<TResult>> task
    );

    /// <summary>Requests cancellation of an active task.</summary>
    bool CancelTask(Guid taskId);
}
