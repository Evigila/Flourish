---
title: Background tasks
description: Run bounded asynchronous work with metadata, cancellation, progress, results, and status-bar integration.
---

# Background tasks

Flourish registers `IBackgroundTaskService` as a singleton runtime service and starts it with the Generic Host. Resolve it through dependency injection, submit work with `AddTask`, and keep the returned handle when the caller needs to cancel the work or observe its final outcome.

```csharp
public sealed class ExportViewModel(IBackgroundTaskService backgroundTasks)
{
    public FlourishBackgroundTaskHandle Export()
    {
        var metadata = new FlourishBackgroundTaskMetadata(
            name: "Export report",
            description: "Writes the current report to disk.",
            iconGlyph: "\uE74E");

        return backgroundTasks.AddTask(metadata, async context =>
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

`AddTask` returns immediately, and its delegate runs outside the WPF UI thread. Do not access WPF controls from the delegate; marshal UI work to the appropriate `Dispatcher` when necessary.

## Metadata and status-bar integration

Every submission requires `FlourishBackgroundTaskMetadata`. `Name` must contain text; `Description` and `IconGlyph` are optional. Supply user-facing metadata before submitting the task because the Shell uses it for tooltips, task rows, automation names, and status icons.

While work is active, the left side of the status bar shows one icon for each running or cancelling task. Hovering an icon shows its metadata, state, and reported progress; clicking it opens the background-task flyout. If all execution slots are occupied, later submissions remain queued and a queue icon displays their count. The queue provides cancellation actions for queued or running work.

Task status and queue details remain available when the application omits `UseTips()`. The task and queue buttons support pointer and keyboard interaction.

Active work temporarily reveals the status bar even when `UseStatusBar()` was not configured. Completed, failed, and cancelled tasks leave the active list and their icons are removed; use the returned handle when an application needs the final outcome or its own completed-task record.

## Concurrency and the waiting queue

`MaxConcurrency` reports how many delegates can run concurrently. Additional tasks remain in the waiting queue in submission order until an execution slot is available.

`ActiveTasks` returns immutable snapshots of queued, running, and cancelling tasks. `TasksChanged` publishes a new immutable list when collection membership, state, or progress changes. The event can be raised from a non-UI thread, so event handlers that update application UI must dispatch back to the UI thread.

`FlourishBackgroundTaskState` reports the lifecycle:

| State | Meaning |
| --- | --- |
| `Queued` | Waiting for an execution slot. |
| `Running` | The delegate is executing. |
| `Cancelling` | Cancellation was requested and the running delegate is finishing. |
| `Succeeded` | Completed successfully. |
| `Canceled` | Completed as cancelled. |
| `Failed` | Completed with a captured exception. |

Only the first three states appear in `ActiveTasks`; terminal state remains available through the handle and result.

## ValueTask delegates

The two submission overloads accept:

- `Func<FlourishBackgroundTaskContext, ValueTask>` for work without a return value.
- `Func<FlourishBackgroundTaskContext, ValueTask<TResult>>` for work that produces a value.

Async lambdas bind naturally to these overloads. Do not wrap asynchronous I/O in `Task.Run`; await it directly and pass `context.CancellationToken` to APIs that support cancellation.

## Cancellation and Host shutdown

`FlourishBackgroundTaskContext.CancellationToken` is cancelled when `handle.Cancel()`, `CancelTask(handle.Id)`, or Host shutdown requests cancellation.

- Cancelling queued work removes it without invoking its delegate.
- Cancelling running work changes its state to `Cancelling`. Cancellation is cooperative, so the delegate should observe the token and finish promptly.
- A repeated or terminal cancellation request returns `false`.
- Host shutdown stops accepting new submissions, cancels active work, and waits for active delegates to finish. A delegate that ignores cancellation can therefore delay shutdown.

`OperationCanceledException` associated with the context token becomes a cancelled result. Other exceptions are captured as failed results.

## Progress

Call `context.ReportProgress(value)` with a finite value from `0` through `1`. The latest value appears in `FlourishBackgroundTaskInfo.Progress`; it is `null` until progress is first reported. Values outside that range throw `ArgumentOutOfRangeException`.

## Results and exceptions

`handle.Completion` always completes successfully with a `FlourishBackgroundTaskResult` object. A task delegate failure does not fault `Completion`; inspect `Succeeded`, `Canceled`, `Exception`, and the final `Info` snapshot instead.

The generic overload also carries the successful return value:

```csharp
public async Task<int?> CountFilesAsync(IBackgroundTaskService backgroundTasks)
{
    var handle = backgroundTasks.AddTask<int>(
        new FlourishBackgroundTaskMetadata(
            "Count files",
            "Counts files in the selected workspace.",
            "\uE8B7"),
        async context =>
        {
            var files = await fileCatalog.ListAsync(context.CancellationToken);
            context.ReportProgress(1);
            return files.Count;
        });

    FlourishBackgroundTaskResult<int> result = await handle.Completion;
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

- [Status bar](status-bar.md) describes running indicators, the waiting queue, and the system-status flyout.
- [Dependency injection](configure-services.md) explains how application services and `IBackgroundTaskService` are resolved.
- [Application data](configure-data.md) lists the built-in task UI localization keys.
