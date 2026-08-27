namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes the lifecycle state of a Flourish background task.
/// </summary>
public enum BackgroundTaskState
{
    /// <summary>
    /// The task is waiting for an execution slot.
    /// </summary>
    Queued,

    /// <summary>
    /// The task delegate is running.
    /// </summary>
    Running,

    /// <summary>
    /// Cancellation was requested and the running delegate is finishing.
    /// </summary>
    Cancelling,

    /// <summary>
    /// The task completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The task was canceled.
    /// </summary>
    Canceled,

    /// <summary>
    /// The task failed with an exception.
    /// </summary>
    Failed,
}
