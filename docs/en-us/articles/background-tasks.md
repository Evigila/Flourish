---
title: Background tasks
description: Run bounded asynchronous work with metadata, cancellation, progress, results, and status-bar integration.
---

# Background tasks

Use the singleton `IBackgroundTaskService` to queue, cancel, and observe work outside the WPF UI thread.

```csharp
public sealed class ExportViewModel(IBackgroundTaskService backgroundTasks)
{
    public BackgroundTaskHandle Export()
    {
        var metadata = new BackgroundTaskMetadata(
            name: "Export report",
            description: "Writes the current report to disk.",
            iconGlyph: "\uE74E");

        return backgroundTasks.QueueTask(metadata, async context =>
        {
            for (var step = 1; step <= 10; step++)
            {
                await Task.Delay(200, context.CancellationToken);
                context.ReportProgress(step / 10d);
            }
        });
    }
}
```

`QueueTask` returns immediately, and its delegate runs outside the WPF UI thread. Do not access WPF controls from the delegate; marshal UI work to the appropriate `Dispatcher` when necessary.

## Metadata and status-bar integration

Every task requires `BackgroundTaskMetadata`: `Name` must contain text, while `Description` and `IconGlyph` are optional. The Shell uses this metadata for task UI and automation names.

The status bar shows each running or cancelling task. Hover for metadata, state, and progress; click to open the task flyout. When all slots are occupied, a number shows the queued count, and the flyout can cancel queued or running work.

Task details remain available without `ConfigureToolTips`, and task controls support pointer and keyboard input.

Active work temporarily reveals the status bar even without `IStatusBarBuilder.SetEnabled()`. Terminal tasks leave the active list; use the handle for their outcome.

## Concurrency and the waiting queue

`MaxConcurrency` reports how many delegates can run concurrently. Additional tasks remain in the waiting queue in submission order until an execution slot is available.

`Current` is an immutable snapshot of queued, running, and cancelling tasks. `Changed` publishes it through `args.Current` after membership, state, or progress changes. The event may run off the UI thread.

`BackgroundTaskState` reports the lifecycle:

| State | Meaning |
| --- | --- |
| `Queued` | Waiting for an execution slot. |
| `Running` | The delegate is executing. |
| `Cancelling` | Cancellation was requested and the running delegate is finishing. |
| `Succeeded` | Completed successfully. |
| `Canceled` | Completed as cancelled. |
| `Failed` | Completed with a captured exception. |

Only the first three states appear in `Current`; terminal state remains available through the handle and result.

## ValueTask delegates

The two submission overloads accept:

- `Func<BackgroundTaskContext, ValueTask>` for work without a return value.
- `Func<BackgroundTaskContext, ValueTask<TResult>>` for work that produces a value.

Await asynchronous I/O directly and pass `context.CancellationToken` to cancellable APIs.

## Cancellation and Host shutdown

`BackgroundTaskContext.CancellationToken` is cancelled when `handle.Cancel()`, `CancelTask(handle.Id)`, or Host shutdown requests cancellation.

- Cancelling queued work removes it without invoking its delegate.
- Cancelling running work changes its state to `Cancelling`. Cancellation is cooperative, so the delegate should observe the token and finish promptly.
- A repeated or terminal cancellation request returns `false`.
- Host shutdown stops accepting new submissions, cancels active work, and waits for active delegates to finish. A delegate that ignores cancellation can therefore delay shutdown.

`OperationCanceledException` associated with the context token becomes a cancelled result. Other exceptions are captured as failed results.

## Progress

Call `context.ReportProgress(value)` with a finite value from `0` through `1`. The latest value appears in `BackgroundTaskInfo.Progress`; it is `null` until progress is first reported. Values outside that range throw `ArgumentOutOfRangeException`.

## Results and exceptions

`handle.Completion` returns `BackgroundTaskResult` even when the delegate fails. Inspect `Succeeded`, `Canceled`, `Exception`, and the final `Info` snapshot.

The generic overload also carries the successful return value:

```csharp
public async Task<int?> CountFilesAsync(IBackgroundTaskService backgroundTasks)
{
    var handle = backgroundTasks.QueueTask<int>(
        new BackgroundTaskMetadata(
            "Count files",
            "Counts files in the selected workspace.",
            "\uE8B7"),
        async context =>
        {
            var files = await fileCatalog.ListAsync(context.CancellationToken);
            context.ReportProgress(1);
            return files.Count;
        });

    BackgroundTaskResult<int> result = await handle.Completion;
    if (result.Succeeded)
    {
        return result.Value;
    }

    if (result.Exception is not null)
    {
        logger.LogError(result.Exception, "File counting failed.");
    }

    return null;
}
```

`Value` is the default value when the task is cancelled or fails. `handle.Snapshot` provides the latest state while the task is active and remains available with its terminal state after completion.

## Related features

- [Status bar](status-bar.md)
- [Dependency injection](configure-services.md)
- [Application data](configure-data.md)
