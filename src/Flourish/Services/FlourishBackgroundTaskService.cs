using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Channels;
using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishBackgroundTaskService : IBackgroundTaskService, IHostedService
{
    private const int DefaultMaxConcurrency = 3;
    private readonly object gate = new();
    private readonly object lifecycleGate = new();
    private readonly object notificationGate = new();
    private readonly ILogger<FlourishBackgroundTaskService> logger;
    private readonly Channel<BackgroundOperation> queue;
    private readonly Dictionary<Guid, BackgroundOperation> activeOperationsById = [];
    private readonly List<BackgroundOperation> activeOperations = [];
    private TaskCompletionSource workersDrained = CreateCompletedSignal();
    private Task? stopTask;
    private bool hasStarted;
    private bool isAcceptingTasks = true;
    private bool isRaisingTasksChanged;
    private int activeWorkerCount;
    private int pendingTaskNotifications;
    private int pendingQueueItems;
    private int workerStartCount;
    private int workersAwaitingFirstRead;

    public FlourishBackgroundTaskService()
        : this(NullLogger<FlourishBackgroundTaskService>.Instance, DefaultMaxConcurrency) { }

    public FlourishBackgroundTaskService(
        ILogger<FlourishBackgroundTaskService> logger
    )
        : this(logger, DefaultMaxConcurrency) { }

    internal FlourishBackgroundTaskService(int maxConcurrency)
        : this(NullLogger<FlourishBackgroundTaskService>.Instance, maxConcurrency) { }

    internal FlourishBackgroundTaskService(
        ILogger<FlourishBackgroundTaskService> logger,
        int maxConcurrency
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        if (maxConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxConcurrency),
                maxConcurrency,
                "Background task concurrency must be greater than zero."
            );
        }

        this.logger = logger;
        MaxConcurrency = maxConcurrency;
        queue = Channel.CreateUnbounded<BackgroundOperation>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleReader = maxConcurrency == 1,
                SingleWriter = false,
            }
        );
    }

    public int MaxConcurrency { get; }

    internal int ActiveWorkerCount
    {
        get
        {
            lock (lifecycleGate)
            {
                return activeWorkerCount;
            }
        }
    }

    internal int WorkerStartCount
    {
        get
        {
            lock (lifecycleGate)
            {
                return workerStartCount;
            }
        }
    }

    public IReadOnlyList<FlourishBackgroundTaskInfo> ActiveTasks
    {
        get
        {
            lock (gate)
            {
                return CreateActiveTaskSnapshotLocked();
            }
        }
    }

    public event EventHandler<FlourishBackgroundTasksChangedEventArgs>? TasksChanged;

    public FlourishBackgroundTaskHandle AddTask(
        FlourishBackgroundTaskMetadata metadata,
        Func<FlourishBackgroundTaskContext, ValueTask> task
    )
    {
        ArgumentNullException.ThrowIfNull(task);

        var operation = AddTaskCore(metadata, ExecuteAsync);
        return new FlourishBackgroundTaskHandle(
            operation.Id,
            ConvertCompletionAsync(operation.Completion.Task),
            () => CancelTask(operation.Id),
            () => GetSnapshot(operation)
        );

        async ValueTask<object?> ExecuteAsync(FlourishBackgroundTaskContext context)
        {
            await task(context).ConfigureAwait(false);
            return null;
        }
    }

    public FlourishBackgroundTaskHandle<TResult> AddTask<TResult>(
        FlourishBackgroundTaskMetadata metadata,
        Func<FlourishBackgroundTaskContext, ValueTask<TResult>> task
    )
    {
        ArgumentNullException.ThrowIfNull(task);

        var operation = AddTaskCore(metadata, ExecuteAsync);
        return new FlourishBackgroundTaskHandle<TResult>(
            operation.Id,
            ConvertCompletionAsync<TResult>(operation.Completion.Task),
            () => CancelTask(operation.Id),
            () => GetSnapshot(operation)
        );

        async ValueTask<object?> ExecuteAsync(FlourishBackgroundTaskContext context)
        {
            return await task(context).ConfigureAwait(false);
        }
    }

    public bool CancelTask(Guid taskId)
    {
        BackgroundOperation? operation;
        OperationCompletion? completion = null;
        TaskCompletionSource? cancellationDispatch = null;

        lock (gate)
        {
            if (!activeOperationsById.TryGetValue(taskId, out operation))
            {
                return false;
            }

            switch (operation.State)
            {
                case FlourishBackgroundTaskState.Queued:
                    operation.State = FlourishBackgroundTaskState.Canceled;
                    operation.CompletedAt = DateTimeOffset.UtcNow;
                    RemoveActiveOperationLocked(operation);
                    completion = new OperationCompletion(operation.CreateSnapshot(), null);
                    break;

                case FlourishBackgroundTaskState.Running:
                    operation.State = FlourishBackgroundTaskState.Cancelling;
                    cancellationDispatch = new TaskCompletionSource(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    );
                    operation.CancellationDispatchTask = cancellationDispatch.Task;
                    break;

                default:
                    return false;
            }
        }

        if (cancellationDispatch is not null)
        {
            _ = CancelWithoutBlockingAsync(
                operation.CancellationSource,
                cancellationDispatch
            );
        }
        else
        {
            CancelWithoutThrowing(operation.CancellationSource);
        }

        if (completion is not null)
        {
            operation.Completion.TrySetResult(completion);
            operation.CancellationSource.Dispose();
        }

        NotifyTasksChanged();
        return true;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (lifecycleGate)
        {
            if (stopTask is not null)
            {
                throw new InvalidOperationException(
                    "The Flourish background task service cannot be restarted after it has stopped."
                );
            }

            if (hasStarted)
            {
                return Task.CompletedTask;
            }

            hasStarted = true;
            StartWorkersForPendingItemsLocked();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Task currentStopTask;
        lock (lifecycleGate)
        {
            if (stopTask is null)
            {
                BackgroundOperation[] operations;
                lock (gate)
                {
                    isAcceptingTasks = false;
                    queue.Writer.TryComplete();
                    operations = activeOperations.ToArray();
                }

                while (queue.Reader.TryRead(out _))
                {
                    pendingQueueItems--;
                }

                Debug.Assert(pendingQueueItems == 0);
                var workerDrainTask = workersDrained.Task;

                // Run the synchronous cancellation phase outside the caller and the lifecycle
                // lock. Cancellation callbacks and task events are application code and may
                // re-enter StopAsync.
                stopTask = Task.Run(() => StopCoreAsync(operations, workerDrainTask));
            }

            currentStopTask = stopTask;
        }

        return cancellationToken.CanBeCanceled
            ? currentStopTask.WaitAsync(cancellationToken)
            : currentStopTask;
    }

    private BackgroundOperation AddTaskCore(
        FlourishBackgroundTaskMetadata metadata,
        Func<FlourishBackgroundTaskContext, ValueTask<object?>> task
    )
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var operation = new BackgroundOperation(metadata, task);
        lock (lifecycleGate)
        {
            lock (gate)
            {
                if (!isAcceptingTasks)
                {
                    throw new InvalidOperationException(
                        "The Flourish background task service is stopping and no longer accepts tasks."
                    );
                }

                activeOperations.Add(operation);
                activeOperationsById.Add(operation.Id, operation);
                pendingQueueItems++;
                if (!queue.Writer.TryWrite(operation))
                {
                    pendingQueueItems--;
                    RemoveActiveOperationLocked(operation);
                    throw new InvalidOperationException(
                        "The Flourish background task queue is no longer available."
                    );
                }
            }
        }

        NotifyTasksChanged();
        EnsureWorkersStarted();
        return operation;
    }

    private async Task ProcessQueueAsync()
    {
        var hasInitialReservation = true;
        try
        {
            while (TryReadNextOperation(ref hasInitialReservation, out var operation))
            {
                var shouldExecute = false;
                lock (gate)
                {
                    if (operation.State == FlourishBackgroundTaskState.Queued)
                    {
                        operation.State = FlourishBackgroundTaskState.Running;
                        operation.StartedAt = DateTimeOffset.UtcNow;
                        shouldExecute = true;
                    }
                }

                if (!shouldExecute)
                {
                    continue;
                }

                NotifyTasksChanged();
                await ExecuteOperationAsync(operation).ConfigureAwait(false);
            }
        }
        finally
        {
            WorkerExited(hasInitialReservation);
        }
    }

    private bool TryReadNextOperation(
        ref bool hasInitialReservation,
        out BackgroundOperation operation
    )
    {
        lock (lifecycleGate)
        {
            if (hasInitialReservation)
            {
                workersAwaitingFirstRead--;
                hasInitialReservation = false;
            }
            else if (pendingQueueItems <= workersAwaitingFirstRead)
            {
                operation = null!;
                return false;
            }

            if (!queue.Reader.TryRead(out operation!))
            {
                return false;
            }

            pendingQueueItems--;
            return true;
        }
    }

    private void EnsureWorkersStarted()
    {
        lock (lifecycleGate)
        {
            StartWorkersForPendingItemsLocked();
        }
    }

    private void StartWorkersForPendingItemsLocked()
    {
        if (!hasStarted || stopTask is not null)
        {
            return;
        }

        var unreservedItems = pendingQueueItems - workersAwaitingFirstRead;
        var workersToStart = Math.Min(MaxConcurrency - activeWorkerCount, unreservedItems);
        for (var index = 0; index < workersToStart; index++)
        {
            if (activeWorkerCount == 0 && workersDrained.Task.IsCompleted)
            {
                workersDrained = new TaskCompletionSource(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
            }

            activeWorkerCount++;
            workersAwaitingFirstRead++;
            workerStartCount++;
            _ = Task.Run(ProcessQueueAsync);
        }
    }

    private void WorkerExited(bool hasInitialReservation)
    {
        lock (lifecycleGate)
        {
            if (hasInitialReservation)
            {
                workersAwaitingFirstRead--;
            }

            activeWorkerCount--;
            StartWorkersForPendingItemsLocked();
            if (activeWorkerCount == 0)
            {
                workersDrained.TrySetResult();
            }
        }
    }

    internal Task WaitForWorkersIdleAsync()
    {
        lock (lifecycleGate)
        {
            return workersDrained.Task;
        }
    }

    private async Task ExecuteOperationAsync(BackgroundOperation operation)
    {
        object? value = null;
        Exception? exception = null;
        var terminalState = FlourishBackgroundTaskState.Succeeded;
        var context = new FlourishBackgroundTaskContext(
            operation.CancellationSource.Token,
            progress => ReportProgress(operation, progress)
        );

        try
        {
            value = await operation.ExecuteAsync(context).ConfigureAwait(false);
            if (operation.CancellationSource.IsCancellationRequested)
            {
                terminalState = FlourishBackgroundTaskState.Canceled;
                value = null;
            }
        }
        catch (OperationCanceledException)
            when (operation.CancellationSource.IsCancellationRequested)
        {
            terminalState = FlourishBackgroundTaskState.Canceled;
        }
        catch (Exception error)
        {
            terminalState = FlourishBackgroundTaskState.Failed;
            exception = error;
        }

        if (exception is not null)
        {
            LogTaskFailure(operation, exception);
        }

        CompleteOperation(operation, terminalState, value, exception);
    }

    private void ReportProgress(BackgroundOperation operation, double progress)
    {
        var didChange = false;
        lock (gate)
        {
            if (
                operation.State
                is FlourishBackgroundTaskState.Running
                    or FlourishBackgroundTaskState.Cancelling
            )
            {
                operation.Progress = progress;
                didChange = true;
            }
        }

        if (didChange)
        {
            NotifyTasksChanged();
        }
    }

    private void CompleteOperation(
        BackgroundOperation operation,
        FlourishBackgroundTaskState terminalState,
        object? value,
        Exception? exception
    )
    {
        OperationCompletion completion;
        lock (gate)
        {
            if (!activeOperationsById.ContainsKey(operation.Id))
            {
                return;
            }

            if (
                terminalState == FlourishBackgroundTaskState.Succeeded
                && operation.State == FlourishBackgroundTaskState.Cancelling
            )
            {
                terminalState = FlourishBackgroundTaskState.Canceled;
                value = null;
            }

            operation.State = terminalState;
            operation.CompletedAt = DateTimeOffset.UtcNow;
            operation.Exception = exception;
            RemoveActiveOperationLocked(operation);
            completion = new OperationCompletion(operation.CreateSnapshot(), value);
        }

        operation.Completion.TrySetResult(completion);
        if (operation.CancellationDispatchTask is { } cancellationDispatchTask)
        {
            _ = DisposeAfterCancellationAsync(
                operation.CancellationSource,
                cancellationDispatchTask
            );
        }
        else
        {
            operation.CancellationSource.Dispose();
        }

        NotifyTasksChanged();
    }

    private FlourishBackgroundTaskInfo GetSnapshot(BackgroundOperation operation)
    {
        lock (gate)
        {
            return operation.CreateSnapshot();
        }
    }

    private IReadOnlyList<FlourishBackgroundTaskInfo> CreateActiveTaskSnapshotLocked()
    {
        return new ReadOnlyCollection<FlourishBackgroundTaskInfo>(
            activeOperations.Select(operation => operation.CreateSnapshot()).ToArray()
        );
    }

    private FlourishBackgroundTasksChangedEventArgs CreateTasksChangedEventArgsLocked()
    {
        return new FlourishBackgroundTasksChangedEventArgs(
            activeOperations.Select(operation => operation.CreateSnapshot())
        );
    }

    private void RemoveActiveOperationLocked(BackgroundOperation operation)
    {
        activeOperationsById.Remove(operation.Id);
        activeOperations.Remove(operation);
    }

    private async Task StopCoreAsync(
        BackgroundOperation[] operations,
        Task workerDrainTask
    )
    {
        foreach (var operation in operations)
        {
            CancelTask(operation.Id);
        }

        await workerDrainTask.ConfigureAwait(false);
        Task[] cancellationDispatchTasks;
        lock (gate)
        {
            cancellationDispatchTasks = operations
                .Select(operation =>
                    operation.CancellationDispatchTask ?? Task.CompletedTask
                )
                .ToArray();
        }

        await Task.WhenAll(cancellationDispatchTasks).ConfigureAwait(false);
    }

    private static TaskCompletionSource CreateCompletedSignal()
    {
        var signal = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        signal.TrySetResult();
        return signal;
    }

    private void NotifyTasksChanged()
    {
        lock (notificationGate)
        {
            pendingTaskNotifications++;
            if (isRaisingTasksChanged)
            {
                return;
            }

            isRaisingTasksChanged = true;
        }

        while (true)
        {
            lock (notificationGate)
            {
                if (pendingTaskNotifications == 0)
                {
                    isRaisingTasksChanged = false;
                    return;
                }

                pendingTaskNotifications--;
            }

            FlourishBackgroundTasksChangedEventArgs eventArgs;
            lock (gate)
            {
                eventArgs = CreateTasksChangedEventArgsLocked();
            }

            RaiseTasksChangedCore(eventArgs);
        }
    }

    private void LogTaskFailure(BackgroundOperation operation, Exception exception)
    {
        try
        {
            logger.LogError(
                exception,
                "Flourish background task {TaskId} ({TaskName}) failed.",
                operation.Id,
                operation.Metadata.Name
            );
        }
        catch (Exception loggingError)
        {
            Debug.WriteLine($"Flourish background task failure logging failed: {loggingError}");
        }
    }

    private void RaiseTasksChangedCore(FlourishBackgroundTasksChangedEventArgs eventArgs)
    {
        var handlers = TasksChanged;
        if (handlers is null)
        {
            return;
        }

        foreach (
            EventHandler<FlourishBackgroundTasksChangedEventArgs> handler in handlers.GetInvocationList()
        )
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception error)
            {
                Debug.WriteLine($"Flourish background task event handler failed: {error}");
            }
        }
    }

    private static void CancelWithoutThrowing(CancellationTokenSource source)
    {
        try
        {
            source.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // A terminal task may race with a duplicate cancellation request.
        }
        catch (Exception error)
        {
            Debug.WriteLine($"Flourish background task cancellation callback failed: {error}");
        }
    }

    private static async Task CancelWithoutBlockingAsync(
        CancellationTokenSource source,
        TaskCompletionSource dispatchCompletion
    )
    {
        try
        {
            await source.CancelAsync().ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            // The task completed between its state transition and the cancellation dispatch.
        }
        catch (Exception error)
        {
            Debug.WriteLine($"Flourish background task cancellation callback failed: {error}");
        }
        finally
        {
            dispatchCompletion.TrySetResult();
        }
    }

    private static async Task DisposeAfterCancellationAsync(
        CancellationTokenSource source,
        Task cancellationDispatchTask
    )
    {
        await cancellationDispatchTask.ConfigureAwait(false);
        source.Dispose();
    }

    private static async Task<FlourishBackgroundTaskResult> ConvertCompletionAsync(
        Task<OperationCompletion> completionTask
    )
    {
        var completion = await completionTask.ConfigureAwait(false);
        return new FlourishBackgroundTaskResult(completion.Info);
    }

    private static async Task<FlourishBackgroundTaskResult<TResult>> ConvertCompletionAsync<TResult>(
        Task<OperationCompletion> completionTask
    )
    {
        var completion = await completionTask.ConfigureAwait(false);
        var value = completion.Value is null ? default : (TResult)completion.Value;
        return new FlourishBackgroundTaskResult<TResult>(completion.Info, value);
    }

    private sealed class BackgroundOperation(
        FlourishBackgroundTaskMetadata metadata,
        Func<FlourishBackgroundTaskContext, ValueTask<object?>> executeAsync
    )
    {
        public Guid Id { get; } = Guid.NewGuid();

        public FlourishBackgroundTaskMetadata Metadata { get; } = metadata;

        public Func<FlourishBackgroundTaskContext, ValueTask<object?>> ExecuteAsync { get; } =
            executeAsync;

        public CancellationTokenSource CancellationSource { get; } = new();

        public TaskCompletionSource<OperationCompletion> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public FlourishBackgroundTaskState State { get; set; } =
            FlourishBackgroundTaskState.Queued;

        public double? Progress { get; set; }

        public DateTimeOffset QueuedAt { get; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public Exception? Exception { get; set; }

        public Task? CancellationDispatchTask { get; set; }

        public FlourishBackgroundTaskInfo CreateSnapshot()
        {
            return new FlourishBackgroundTaskInfo(
                Id,
                Metadata,
                State,
                Progress,
                QueuedAt,
                StartedAt,
                CompletedAt,
                Exception
            );
        }
    }

    private sealed record OperationCompletion(
        FlourishBackgroundTaskInfo Info,
        object? Value
    );
}
