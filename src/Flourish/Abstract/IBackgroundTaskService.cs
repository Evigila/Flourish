using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Queues and executes asynchronous background work with bounded concurrency.
/// </summary>
public interface IBackgroundTaskService
{
    /// <summary>
    /// Gets the maximum number of task delegates that can run concurrently.
    /// </summary>
    int MaxConcurrency { get; }

    /// <summary>
    /// Gets immutable snapshots of all queued, running, and cancelling tasks.
    /// </summary>
    IReadOnlyList<BackgroundTaskInfo> Current { get; }

    /// <summary>
    /// Occurs when the active task collection, a task state, or task progress changes.
    /// </summary>
    event EventHandler<
        StateChangedEventArgs<IReadOnlyList<BackgroundTaskInfo>>
    >? Changed;

    /// <summary>
    /// Submits asynchronous background work without a return value.
    /// </summary>
    /// <param name="metadata">The metadata displayed while the task is active.</param>
    /// <param name="task">The asynchronous task delegate.</param>
    /// <returns>A handle used to cancel the task and await its captured outcome.</returns>
    BackgroundTaskHandle QueueTask(
        BackgroundTaskMetadata metadata,
        Func<BackgroundTaskContext, ValueTask> task
    );

    /// <summary>
    /// Submits asynchronous background work with a return value.
    /// </summary>
    /// <typeparam name="TResult">The task return value type.</typeparam>
    /// <param name="metadata">The metadata displayed while the task is active.</param>
    /// <param name="task">The asynchronous task delegate.</param>
    /// <returns>A handle used to cancel the task and await its captured outcome and value.</returns>
    BackgroundTaskHandle<TResult> QueueTask<TResult>(
        BackgroundTaskMetadata metadata,
        Func<BackgroundTaskContext, ValueTask<TResult>> task
    );

    /// <summary>
    /// Requests cancellation of an active task by identifier.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <returns><see langword="true" /> when this call changed the task state; otherwise, <see langword="false" />.</returns>
    bool CancelTask(Guid taskId);
}
