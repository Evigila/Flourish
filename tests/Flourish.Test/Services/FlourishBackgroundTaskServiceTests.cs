using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishBackgroundTaskServiceTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task StartAsync_WithoutPendingTasksDoesNotStartWorkers()
    {
        var service = new FlourishBackgroundTaskService(maxConcurrency: 3);

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
        var service = new FlourishBackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var releaseFirst = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var first = service.AddTask(
            new FlourishBackgroundTaskMetadata("First burst"),
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

        var secondStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var second = service.AddTask(
            new FlourishBackgroundTaskMetadata("Second burst"),
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
    public async Task StopAsync_DrainsWorkersAndPreservesNonRestartableLifecycle()
    {
        var service = new FlourishBackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var started = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var handle = service.AddTask(
            new FlourishBackgroundTaskMetadata("Stop worker"),
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

        await service.StopAsync(CancellationToken.None).WaitAsync(Timeout);

        Assert.True((await handle.Completion.WaitAsync(Timeout)).Canceled);
        await service.WaitForWorkersIdleAsync().WaitAsync(Timeout);
        Assert.Equal(0, service.ActiveWorkerCount);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartAsync(CancellationToken.None)
        );
    }

    [Fact]
    public async Task AddTask_RunsAtMostThreeTasksAndLeavesFourthQueued()
    {
        var service = new FlourishBackgroundTaskService();
        await service.StartAsync(CancellationToken.None);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var threeStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var running = 0;
        var maximumRunning = 0;

        var handles = Enumerable
            .Range(1, 4)
            .Select(index =>
                service.AddTask(
                    new FlourishBackgroundTaskMetadata($"Task {index}"),
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
        var activeTasks = service.ActiveTasks;

        Assert.Equal(3, service.MaxConcurrency);
        Assert.Equal(3, activeTasks.Count(task => task.State == FlourishBackgroundTaskState.Running));
        Assert.Single(activeTasks, task => task.State == FlourishBackgroundTaskState.Queued);
        Assert.Equal(3, Volatile.Read(ref maximumRunning));

        release.TrySetResult();
        var results = await Task.WhenAll(handles.Select(handle => handle.Completion)).WaitAsync(Timeout);
        Assert.All(results, result => Assert.True(result.Succeeded));
        Assert.Empty(service.ActiveTasks);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CancelTask_WhileQueued_DoesNotInvokeDelegate()
    {
        var service = new FlourishBackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var secondInvoked = false;
        var first = service.AddTask(
            new FlourishBackgroundTaskMetadata("First"),
            async _ =>
            {
                firstStarted.TrySetResult();
                await release.Task;
            }
        );
        await firstStarted.Task.WaitAsync(Timeout);
        var second = service.AddTask(
            new FlourishBackgroundTaskMetadata("Second"),
            _ =>
            {
                secondInvoked = true;
                return ValueTask.CompletedTask;
            }
        );

        Assert.True(service.CancelTask(second.Id));
        Assert.False(second.Cancel());
        var secondResult = await second.Completion.WaitAsync(Timeout);
        Assert.True(secondResult.Canceled);
        Assert.Equal(FlourishBackgroundTaskState.Canceled, secondResult.Info.State);

        release.TrySetResult();
        Assert.True((await first.Completion.WaitAsync(Timeout)).Succeeded);
        Assert.False(secondInvoked);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CancelTask_WhileRunning_EntersCancellingAndEndsCanceled()
    {
        var service = new FlourishBackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellationObserved = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var finish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = service.AddTask(
            new FlourishBackgroundTaskMetadata("Cancelable"),
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
        Assert.Equal(FlourishBackgroundTaskState.Cancelling, handle.Snapshot.State);
        Assert.False(service.CancelTask(handle.Id));

        finish.TrySetResult();
        var result = await handle.Completion.WaitAsync(Timeout);
        Assert.True(result.Canceled);
        Assert.Equal(FlourishBackgroundTaskState.Canceled, result.Info.State);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task FailedTask_CapturesExceptionAndCompletionDoesNotFault()
    {
        var service = new FlourishBackgroundTaskService();
        await service.StartAsync(CancellationToken.None);
        var expected = new InvalidOperationException("Expected failure");
        var handle = service.AddTask(
            new FlourishBackgroundTaskMetadata("Fail"),
            _ => ValueTask.FromException(expected)
        );

        var result = await handle.Completion.WaitAsync(Timeout);

        Assert.False(result.Succeeded);
        Assert.Same(expected, result.Exception);
        Assert.Equal(FlourishBackgroundTaskState.Failed, result.Info.State);
        Assert.False(handle.Completion.IsFaulted);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task FailedTask_LogsTaskIdentityAtErrorLevel()
    {
        var logger = new RecordingLogger<FlourishBackgroundTaskService>();
        var service = new FlourishBackgroundTaskService(logger, maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var expected = new InvalidOperationException("Logged failure");
        var handle = service.AddTask(
            new FlourishBackgroundTaskMetadata("Import catalog"),
            _ => ValueTask.FromException(expected)
        );

        var result = await handle.Completion.WaitAsync(Timeout);
        var log = await logger.Logged.Task.WaitAsync(Timeout);

        Assert.Equal(FlourishBackgroundTaskState.Failed, result.Info.State);
        Assert.Same(expected, result.Exception);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Same(expected, log.Exception);
        Assert.Contains(handle.Id.ToString(), log.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Import catalog", log.Message);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task GenericTask_ReturnsValueInSuccessfulResult()
    {
        var service = new FlourishBackgroundTaskService();
        await service.StartAsync(CancellationToken.None);
        var handle = service.AddTask(
            new FlourishBackgroundTaskMetadata("Return value"),
            _ => ValueTask.FromResult(42)
        );

        var result = await handle.Completion.WaitAsync(Timeout);

        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Value);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TasksChanged_ReportsStateAndProgressSnapshots()
    {
        var service = new FlourishBackgroundTaskService(maxConcurrency: 1);
        var snapshots = new List<IReadOnlyList<FlourishBackgroundTaskInfo>>();
        var eventGate = new object();
        service.TasksChanged += (_, args) =>
        {
            lock (eventGate)
            {
                snapshots.Add(args.Tasks);
            }
        };
        await service.StartAsync(CancellationToken.None);
        var handle = service.AddTask(
            new FlourishBackgroundTaskMetadata("Progress"),
            context =>
            {
                context.ReportProgress(0.5);
                return ValueTask.CompletedTask;
            }
        );

        await handle.Completion.WaitAsync(Timeout);
        IReadOnlyList<IReadOnlyList<FlourishBackgroundTaskInfo>> captured;
        lock (eventGate)
        {
            captured = snapshots.ToArray();
        }

        Assert.Contains(
            captured.SelectMany(tasks => tasks),
            task => task.State == FlourishBackgroundTaskState.Queued
        );
        Assert.Contains(
            captured.SelectMany(tasks => tasks),
            task => task.State == FlourishBackgroundTaskState.Running
        );
        Assert.Contains(
            captured.SelectMany(tasks => tasks),
            task => task.Progress == 0.5
        );
        Assert.Empty(captured[^1]);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TasksChanged_ReentrantCancellationDoesNotRegressLaterSubscribers()
    {
        var service = new FlourishBackgroundTaskService(maxConcurrency: 1);
        var finish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellingObserved = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var recordedStates = new List<FlourishBackgroundTaskState>();
        var requestedCancellation = false;
        service.TasksChanged += (_, args) =>
        {
            var task = args.Tasks.SingleOrDefault();
            if (
                task?.State == FlourishBackgroundTaskState.Running
                && !requestedCancellation
            )
            {
                requestedCancellation = true;
                service.CancelTask(task.Id);
            }
        };
        service.TasksChanged += (_, args) =>
        {
            var task = args.Tasks.SingleOrDefault();
            if (task is null)
            {
                return;
            }

            recordedStates.Add(task.State);
            if (task.State == FlourishBackgroundTaskState.Cancelling)
            {
                cancellingObserved.TrySetResult();
            }
        };
        var handle = service.AddTask(
            new FlourishBackgroundTaskMetadata("Reentrant cancellation"),
            async _ => await finish.Task
        );

        await service.StartAsync(CancellationToken.None);
        await cancellingObserved.Task.WaitAsync(Timeout);
        finish.TrySetResult();
        Assert.True((await handle.Completion.WaitAsync(Timeout)).Canceled);

        var runningIndex = recordedStates.IndexOf(FlourishBackgroundTaskState.Running);
        var cancellingIndex = recordedStates.IndexOf(FlourishBackgroundTaskState.Cancelling);
        Assert.True(runningIndex >= 0);
        Assert.True(cancellingIndex > runningIndex);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StopAsync_StopsAcceptingAndCancelsRunningAndQueuedTasks()
    {
        var service = new FlourishBackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var running = service.AddTask(
            new FlourishBackgroundTaskMetadata("Running"),
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
        var queued = service.AddTask(
            new FlourishBackgroundTaskMetadata("Queued"),
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
        Assert.Empty(service.ActiveTasks);
        Assert.Throws<InvalidOperationException>(() =>
            service.AddTask(
                new FlourishBackgroundTaskMetadata("Rejected"),
                _ => ValueTask.CompletedTask
            )
        );
    }

    [Fact]
    public async Task StopAsync_WaitsForAsynchronousCancellationDispatch()
    {
        var service = new FlourishBackgroundTaskService(maxConcurrency: 1);
        await service.StartAsync(CancellationToken.None);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var callbackStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var releaseCallback = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var handle = service.AddTask(
            new FlourishBackgroundTaskMetadata("Slow cancellation callback"),
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
    public void Composition_RegistersOneInstanceForPublicAndHostedServices()
    {
        using var flourish = FlourishBuilder.CreateDefaultBuilder([]).Build();

        var publicService = flourish.GetRequiredService<IBackgroundTaskService>();
        var hostedService = Assert.Single(
            flourish
                .GetRequiredService<IEnumerable<IHostedService>>()
                .OfType<FlourishBackgroundTaskService>()
        );

        Assert.Same(publicService, hostedService);
    }

    [Fact]
    public void MetadataAndProgress_ValidateInputs()
    {
        Assert.Throws<ArgumentException>(() => new FlourishBackgroundTaskMetadata("  "));
        var metadata = new FlourishBackgroundTaskMetadata(" Task ", " ", " I ");
        Assert.Equal("Task", metadata.Name);
        Assert.Null(metadata.Description);
        Assert.Equal("I", metadata.IconGlyph);

        var context = new FlourishBackgroundTaskContext(CancellationToken.None, _ => { });
        Assert.Throws<ArgumentOutOfRangeException>(() => context.ReportProgress(-0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => context.ReportProgress(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => context.ReportProgress(1.1));
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
            Logged.TrySetResult(
                new LogEntry(logLevel, formatter(state, exception), exception)
            );
        }
    }

    private sealed record LogEntry(
        LogLevel Level,
        string Message,
        Exception? Exception
    );

    private sealed class EmptyScope : IDisposable
    {
        public static EmptyScope Instance { get; } = new();

        public void Dispose() { }
    }
}
