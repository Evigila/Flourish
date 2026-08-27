using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the terminal outcome of a background task without a return value.
/// </summary>
public sealed class BackgroundTaskResult
{
    internal BackgroundTaskResult(BackgroundTaskInfo info)
    {
        Info = info;
    }

    /// <summary>
    /// Gets the final task snapshot.
    /// </summary>
    public BackgroundTaskInfo Info { get; }

    /// <summary>
    /// Gets a value indicating whether the task completed successfully.
    /// </summary>
    public bool Succeeded => Info.State == BackgroundTaskState.Succeeded;

    /// <summary>
    /// Gets a value indicating whether the task was canceled.
    /// </summary>
    public bool Canceled => Info.State == BackgroundTaskState.Canceled;

    /// <summary>
    /// Gets the exception captured from a failed task.
    /// </summary>
    public Exception? Exception => Info.Exception;
}

/// <summary>
/// Represents the terminal outcome and return value of a background task.
/// </summary>
/// <typeparam name="TResult">The task return value type.</typeparam>
public sealed class BackgroundTaskResult<TResult>
{
    internal BackgroundTaskResult(BackgroundTaskInfo info, TResult? value)
    {
        Info = info;
        Value = value;
    }

    /// <summary>
    /// Gets the final task snapshot.
    /// </summary>
    public BackgroundTaskInfo Info { get; }

    /// <summary>
    /// Gets the task return value when the task succeeded; otherwise, the default value.
    /// </summary>
    public TResult? Value { get; }

    /// <summary>
    /// Gets a value indicating whether the task completed successfully.
    /// </summary>
    public bool Succeeded => Info.State == BackgroundTaskState.Succeeded;

    /// <summary>
    /// Gets a value indicating whether the task was canceled.
    /// </summary>
    public bool Canceled => Info.State == BackgroundTaskState.Canceled;

    /// <summary>
    /// Gets the exception captured from a failed task.
    /// </summary>
    public Exception? Exception => Info.Exception;
}
