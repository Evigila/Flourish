using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.BackgroundTasks;

using Microsoft.Extensions.Logging;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.BackgroundTasks;

public sealed class BackgroundTaskServiceTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task StartAsync_WithoutPendingTasksDoesNotStartWorkers()
    {
        var service = new BackgroundTaskService(maxConcurrency: 3);

        await service.StartAsync(CancellationToken.None);

        Assert.Equal(0, service.ActiveWorkerCount);
        Assert.Equal(0, service.WorkerStartCount);
        await service.WaitForWorkersIdleAsync().WaitAsync(Timeout);

        await service.StopAsync(CancellationToken.None).WaitAsync(Timeout);
        Assert.Equal(0, service.ActiveWorkerCount);
        Assert.Equal(0, service.WorkerStartCount);
    }

    [Fact]
    public async Task IdleWorker_ExitsAndRestartsForLaterWork()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var firstStarted = CreateSignal();
        var releaseFirst = CreateSignal();
        var first = service.QueueTask(
            new BackgroundTaskMetadata("First burst"),
            async _ =>
            {
                firstStarted.TrySetResult();
                await releaseFirst.Task;
            }
        );

        await firstStarted.Task.WaitAsync(Timeout);
        Assert.Equal(1, service.ActiveWorkerCount);
        Assert.Equal(1, service.WorkerStartCount);
        releaseFirst.TrySetResult();
        Assert.True((await first.Completion.WaitAsync(Timeout)).Succeeded);
        await service.WaitForWorkersIdleAsync().WaitAsync(Timeout);
        Assert.Equal(0, service.ActiveWorkerCount);

        var secondStarted = CreateSignal();
        var second = service.QueueTask(
            new BackgroundTaskMetadata("Second burst"),
            _ =>
            {
                secondStarted.TrySetResult();
                return ValueTask.CompletedTask;
            }
        );

        await secondStarted.Task.WaitAsync(Timeout);
        Assert.True((await second.Completion.WaitAsync(Timeout)).Succeeded);
        await service.WaitForWorkersIdleAsync().WaitAsync(Timeout);
        Assert.Equal(0, service.ActiveWorkerCount);
        Assert.Equal(2, service.WorkerStartCount);
        await service.StopAsync(CancellationToken.None).WaitAsync(Timeout);
    }

    [Fact]
    public async Task QueueTask_RespectsMaximumConcurrencyAndPreservesQueue()
    {
        var service = new BackgroundTaskService(maxConcurrency: 3);
        await service.StartAsync(CancellationToken.None);
        var release = CreateSignal();
        var threeStarted = CreateSignal();
        var running = 0;
        var maximumRunning = 0;
        var handles = Enumerable
            .Range(1, 4)
            .Select(index =>
                service.QueueTask(
                    new BackgroundTaskMetadata($"Task {index}"),
                    async _ =>
                    {
                        var current = Interlocked.Increment(ref running);
                        UpdateMaximum(ref maximumRunning, current);
                        if (current == 3)
                        {
                            threeStarted.TrySetResult();
                        }

                        await release.Task;
                        Interlocked.Decrement(ref running);
                    }
                )
            )
            .ToArray();

        await threeStarted.Task.WaitAsync(Timeout);

        Assert.Equal(
            3,
            service.Current.Count(task => task.State == BackgroundTaskState.Running)
        );
        Assert.Single(service.Current, task => task.State == BackgroundTaskState.Queued);
        Assert.Equal(3, Volatile.Read(ref maximumRunning));

        release.TrySetResult();
        var results = await Task.WhenAll(handles.Select(handle => handle.Completion))
            .WaitAsync(Timeout);
        Assert.All(results, result => Assert.True(result.Succeeded));
        Assert.Empty(service.Current);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CancelTask_WhileQueuedDoesNotInvokeDelegate()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var release = CreateSignal();
        var firstStarted = CreateSignal();
        var secondInvoked = false;
        var first = service.QueueTask(
            new BackgroundTaskMetadata("First"),
            async _ =>
            {
                firstStarted.TrySetResult();
                await release.Task;
            }
        );
        await firstStarted.Task.WaitAsync(Timeout);
        var second = service.QueueTask(
            new BackgroundTaskMetadata("Second"),
            _ =>
            {
                secondInvoked = true;
                return ValueTask.CompletedTask;
            }
        );

        Assert.True(service.CancelTask(second.Id));
        Assert.False(second.Cancel());
        Assert.True((await second.Completion.WaitAsync(Timeout)).Canceled);

        release.TrySetResult();
        Assert.True((await first.Completion.WaitAsync(Timeout)).Succeeded);
        Assert.False(secondInvoked);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CancelTask_WhileRunningTransitionsThroughCancelling()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var started = CreateSignal();
        var cancellationObserved = CreateSignal();
        var finish = CreateSignal();
        var handle = service.QueueTask(
            new BackgroundTaskMetadata("Cancelable"),
            async context =>
            {
                started.TrySetResult();
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    await Task.Yield();
                }

                cancellationObserved.TrySetResult();
                await finish.Task;
            }
        );
        await started.Task.WaitAsync(Timeout);

        Assert.True(handle.Cancel());
        await cancellationObserved.Task.WaitAsync(Timeout);
        Assert.Equal(BackgroundTaskState.Cancelling, handle.Snapshot.State);
        Assert.False(service.CancelTask(handle.Id));

        finish.TrySetResult();
        var result = await handle.Completion.WaitAsync(Timeout);
        Assert.True(result.Canceled);
        Assert.Equal(BackgroundTaskState.Canceled, result.Info.State);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Failure_IsCapturedLoggedAndDoesNotFaultCompletion()
    {
        var logger = new RecordingLogger<BackgroundTaskService>();
        var service = new BackgroundTaskService(logger, maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var failure = new InvalidOperationException("Expected failure");
        var handle = service.QueueTask(
            new BackgroundTaskMetadata("Import catalog"),
            _ => ValueTask.FromException(failure)
        );

        var result = await handle.Completion.WaitAsync(Timeout);
        var log = await logger.Logged.Task.WaitAsync(Timeout);

        Assert.Equal(BackgroundTaskState.Failed, result.Info.State);
        Assert.Same(failure, result.Exception);
        Assert.False(handle.Completion.IsFaulted);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Same(failure, log.Exception);
        Assert.Contains(handle.Id.ToString(), log.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Import catalog", log.Message);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task GenericTask_ReturnsValueOnlyOnSuccess()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var handle = service.QueueTask(
            new BackgroundTaskMetadata("Return value"),
            _ => ValueTask.FromResult(42)
        );

        var result = await handle.Completion.WaitAsync(Timeout);

        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Value);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Changed_ReportsQueuedRunningProgressAndRemovalSnapshots()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        var snapshots = new List<IReadOnlyList<BackgroundTaskInfo>>();
        var eventGate = new object();
        service.Changed += (_, args) =>
        {
            lock (eventGate)
            {
                snapshots.Add(args.Current);
            }
        };
        await service.StartAsync(CancellationToken.None);
        var handle = service.QueueTask(
            new BackgroundTaskMetadata("Progress"),
            context =>
            {
                context.ReportProgress(0.5);
                return ValueTask.CompletedTask;
            }
        );

        await handle.Completion.WaitAsync(Timeout);
        IReadOnlyList<IReadOnlyList<BackgroundTaskInfo>> captured;
        lock (eventGate)
        {
            captured = snapshots.ToArray();
        }

        Assert.Contains(
            captured.SelectMany(tasks => tasks),
            task => task.State == BackgroundTaskState.Queued
        );
        Assert.Contains(
            captured.SelectMany(tasks => tasks),
            task => task.State == BackgroundTaskState.Running
        );
        Assert.Contains(captured.SelectMany(tasks => tasks), task => task.Progress == 0.5);
        Assert.Empty(captured[^1]);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RepeatedProgress_NotifiesOnlyForDistinctValues()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        var snapshots = new List<IReadOnlyList<BackgroundTaskInfo>>();
        var eventGate = new object();
        service.Changed += (_, args) =>
        {
            lock (eventGate)
            {
                snapshots.Add(args.Current);
            }
        };
        await service.StartAsync(CancellationToken.None);
        var handle = service.QueueTask(
            new BackgroundTaskMetadata("Repeated progress"),
            context =>
            {
                context.ReportProgress(0.25);
                context.ReportProgress(0.25);
                context.ReportProgress(0.75);
                return ValueTask.CompletedTask;
            }
        );

        Assert.True((await handle.Completion.WaitAsync(Timeout)).Succeeded);
        await service.WaitForWorkersIdleAsync().WaitAsync(Timeout);
        IReadOnlyList<IReadOnlyList<BackgroundTaskInfo>> captured;
        lock (eventGate)
        {
            captured = snapshots.ToArray();
        }

        Assert.Single(
            captured,
            tasks => tasks.Any(task => task.Id == handle.Id && task.Progress == 0.25)
        );
        Assert.Single(
            captured,
            tasks => tasks.Any(task => task.Id == handle.Id && task.Progress == 0.75)
        );
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Changed_ReentrantCancellationDoesNotRegressStateForLaterSubscribers()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        var finish = CreateSignal();
        var cancellingObserved = CreateSignal();
        var recordedStates = new List<BackgroundTaskState>();
        var requestedCancellation = false;
        service.Changed += (_, args) =>
        {
            var task = args.Current.SingleOrDefault();
            if (task?.State == BackgroundTaskState.Running && !requestedCancellation)
            {
                requestedCancellation = true;
                service.CancelTask(task.Id);
            }
        };
        service.Changed += (_, args) =>
        {
            var task = args.Current.SingleOrDefault();
            if (task is null)
            {
                return;
            }

            recordedStates.Add(task.State);
            if (task.State == BackgroundTaskState.Cancelling)
            {
                cancellingObserved.TrySetResult();
            }
        };
        var handle = service.QueueTask(
            new BackgroundTaskMetadata("Reentrant cancellation"),
            async _ => await finish.Task
        );

        await service.StartAsync(CancellationToken.None);
        await cancellingObserved.Task.WaitAsync(Timeout);
        finish.TrySetResult();
        Assert.True((await handle.Completion.WaitAsync(Timeout)).Canceled);

        var runningIndex = recordedStates.IndexOf(BackgroundTaskState.Running);
        var cancellingIndex = recordedStates.IndexOf(BackgroundTaskState.Cancelling);
        Assert.True(runningIndex >= 0);
        Assert.True(cancellingIndex > runningIndex);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StopAsync_CancelsRunningAndQueuedTasksAndRejectsNewWork()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var started = CreateSignal();
        var running = service.QueueTask(
            new BackgroundTaskMetadata("Running"),
            async context =>
            {
                started.TrySetResult();
                await Task.Delay(
                    System.Threading.Timeout.InfiniteTimeSpan,
                    context.CancellationToken
                );
            }
        );
        await started.Task.WaitAsync(Timeout);
        var queuedInvoked = false;
        var queued = service.QueueTask(
            new BackgroundTaskMetadata("Queued"),
            _ =>
            {
                queuedInvoked = true;
                return ValueTask.CompletedTask;
            }
        );

        await service.StopAsync(CancellationToken.None).WaitAsync(Timeout);
        var results = await Task.WhenAll(running.Completion, queued.Completion).WaitAsync(Timeout);

        Assert.All(results, result => Assert.True(result.Canceled));
        Assert.False(queuedInvoked);
        Assert.Empty(service.Current);
        await service.WaitForWorkersIdleAsync().WaitAsync(Timeout);
        Assert.Equal(0, service.ActiveWorkerCount);
        Assert.Throws<InvalidOperationException>(() =>
            service.QueueTask(
                new BackgroundTaskMetadata("Rejected"),
                _ => ValueTask.CompletedTask
            )
        );
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartAsync(CancellationToken.None)
        );
    }

    [Fact]
    public async Task StopAsync_WaitsForAsynchronousCancellationDispatch()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var started = CreateSignal();
        var callbackStarted = CreateSignal();
        var releaseCallback = CreateSignal();
        var handle = service.QueueTask(
            new BackgroundTaskMetadata("Slow cancellation callback"),
            async context =>
            {
                using var registration = context.CancellationToken.Register(() =>
                {
                    callbackStarted.TrySetResult();
                    releaseCallback.Task.GetAwaiter().GetResult();
                });
                started.TrySetResult();
                await Task.Delay(
                    System.Threading.Timeout.InfiniteTimeSpan,
                    context.CancellationToken
                );
            }
        );
        await started.Task.WaitAsync(Timeout);

        var stopTask = service.StopAsync(CancellationToken.None);
        await callbackStarted.Task.WaitAsync(Timeout);

        Assert.False(stopTask.IsCompleted);
        releaseCallback.TrySetResult();
        await stopTask.WaitAsync(Timeout);
        Assert.True((await handle.Completion.WaitAsync(Timeout)).Canceled);
    }

    [Fact]
    public async Task StopAsync_CallerCancellationDoesNotCancelSharedStopOperation()
    {
        var service = new BackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var started = CreateSignal();
        var finish = CreateSignal();
        var handle = service.QueueTask(
            new BackgroundTaskMetadata("Slow stop"),
            async context =>
            {
                started.TrySetResult();
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    await Task.Yield();
                }

                await finish.Task;
            }
        );
        await started.Task.WaitAsync(Timeout);
        using var callerCancellation = new CancellationTokenSource();
        callerCancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.StopAsync(callerCancellation.Token)
        );
        finish.TrySetResult();
        await service.StopAsync(CancellationToken.None).WaitAsync(Timeout);

        Assert.True((await handle.Completion.WaitAsync(Timeout)).Canceled);
    }

    [Fact]
    public void MetadataProgressAndConcurrency_ValidateInputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BackgroundTaskService(0));
        Assert.Throws<ArgumentException>(() => new BackgroundTaskMetadata("  "));
        var metadata = new BackgroundTaskMetadata(" Task ", " ", " I ");
        Assert.Equal("Task", metadata.Name);
        Assert.Null(metadata.Description);
        Assert.Equal("I", metadata.IconGlyph);

        var context = new BackgroundTaskContext(CancellationToken.None, _ => { });
        Assert.Throws<ArgumentOutOfRangeException>(() => context.ReportProgress(-0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => context.ReportProgress(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => context.ReportProgress(1.1));
    }

    private static TaskCompletionSource CreateSignal()
    {
        return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static void UpdateMaximum(ref int maximum, int value)
    {
        var observed = Volatile.Read(ref maximum);
        while (value > observed)
        {
            var previous = Interlocked.CompareExchange(ref maximum, value, observed);
            if (previous == observed)
            {
                return;
            }

            observed = previous;
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public TaskCompletionSource<LogEntry> Logged { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return EmptyScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            Logged.TrySetResult(new LogEntry(logLevel, formatter(state, exception), exception));
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

    private sealed class EmptyScope : IDisposable
    {
        public static EmptyScope Instance { get; } = new();

        public void Dispose() { }
    }
}
