using System;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls and observes a submitted background task without a return value.
/// </summary>
public sealed class FlourishBackgroundTaskHandle
{
    private readonly Func<bool> cancel;
    private readonly Func<FlourishBackgroundTaskInfo> getSnapshot;

    internal FlourishBackgroundTaskHandle(
        Guid id,
        Task<FlourishBackgroundTaskResult> completion,
        Func<bool> cancel,
        Func<FlourishBackgroundTaskInfo> getSnapshot
    )
    {
        Id = id;
        Completion = completion;
        this.cancel = cancel;
        this.getSnapshot = getSnapshot;
    }

    /// <summary>
    /// Gets the unique task identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets a task that always completes successfully with the captured task outcome.
    /// </summary>
    public Task<FlourishBackgroundTaskResult> Completion { get; }

    /// <summary>
    /// Gets the latest immutable task snapshot.
    /// </summary>
    public FlourishBackgroundTaskInfo Snapshot => getSnapshot();

    /// <summary>
    /// Requests cooperative cancellation.
    /// </summary>
    /// <returns><see langword="true" /> when this call changed the task state; otherwise, <see langword="false" />.</returns>
    public bool Cancel() => cancel();
}

/// <summary>
/// Controls and observes a submitted background task with a return value.
/// </summary>
/// <typeparam name="TResult">The task return value type.</typeparam>
public sealed class FlourishBackgroundTaskHandle<TResult>
{
    private readonly Func<bool> cancel;
    private readonly Func<FlourishBackgroundTaskInfo> getSnapshot;

    internal FlourishBackgroundTaskHandle(
        Guid id,
        Task<FlourishBackgroundTaskResult<TResult>> completion,
        Func<bool> cancel,
        Func<FlourishBackgroundTaskInfo> getSnapshot
    )
    {
        Id = id;
        Completion = completion;
        this.cancel = cancel;
        this.getSnapshot = getSnapshot;
    }

    /// <summary>
    /// Gets the unique task identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets a task that always completes successfully with the captured task outcome and value.
    /// </summary>
    public Task<FlourishBackgroundTaskResult<TResult>> Completion { get; }

    /// <summary>
    /// Gets the latest immutable task snapshot.
    /// </summary>
    public FlourishBackgroundTaskInfo Snapshot => getSnapshot();

    /// <summary>
    /// Requests cooperative cancellation.
    /// </summary>
    /// <returns><see langword="true" /> when this call changed the task state; otherwise, <see langword="false" />.</returns>
    public bool Cancel() => cancel();
}
