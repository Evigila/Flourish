using System;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls and observes a submitted background task without a return value.
/// </summary>
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

    /// <summary>
    /// Gets the unique task identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets a task that always completes successfully with the captured task outcome.
    /// </summary>
    public Task<BackgroundTaskResult> Completion { get; }

    /// <summary>
    /// Gets the latest immutable task snapshot.
    /// </summary>
    public BackgroundTaskInfo Snapshot => getSnapshot();

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

    /// <summary>
    /// Gets the unique task identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets a task that always completes successfully with the captured task outcome and value.
    /// </summary>
    public Task<BackgroundTaskResult<TResult>> Completion { get; }

    /// <summary>
    /// Gets the latest immutable task snapshot.
    /// </summary>
    public BackgroundTaskInfo Snapshot => getSnapshot();

    /// <summary>
    /// Requests cooperative cancellation.
    /// </summary>
    /// <returns><see langword="true" /> when this call changed the task state; otherwise, <see langword="false" />.</returns>
    public bool Cancel() => cancel();
}
