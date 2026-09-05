using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArkheideSystem.Flourish.BackgroundTasks;

internal sealed class BackgroundTaskService : IBackgroundTaskService, IHostedService
{
    private const int DefaultMaxConcurrency = 3;
    private readonly Lock gate = new();
    private readonly Lock lifecycleGate = new();
    private readonly Lock notificationGate = new();
    private readonly ILogger<BackgroundTaskService> logger;
    private readonly Channel<BackgroundOperation> queue;
    private readonly Dictionary<Guid, BackgroundOperation> activeOperationsById = [];
    private readonly List<BackgroundOperation> activeOperations = [];
    private TaskCompletionSource workersDrained = CreateCompletedSignal();
    private Task? stopTask;
    private bool hasStarted;
    private bool isAcceptingTasks = true;
    private bool isRaisingTasksChanged;
    private IReadOnlyList<BackgroundTaskInfo> current = Array.Empty<BackgroundTaskInfo>();
    private int activeWorkerCount;
    private int pendingTaskNotifications;
    private int pendingQueueItems;
    private int workerStartCount;
    private int workersAwaitingFirstRead;

    public BackgroundTaskService()
        : this(NullLogger<BackgroundTaskService>.Instance, DefaultMaxConcurrency) { }

    public BackgroundTaskService(ILogger<BackgroundTaskService> logger)
        : this(logger, DefaultMaxConcurrency) { }

    internal BackgroundTaskService(int maxConcurrency)
        : this(NullLogger<BackgroundTaskService>.Instance, maxConcurrency) { }

    internal BackgroundTaskService(
        ILogger<BackgroundTaskService> logger,
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

    public IReadOnlyList<BackgroundTaskInfo> Current => Volatile.Read(ref current);

    public event EventHandler<
        StateChangedEventArgs<IReadOnlyList<BackgroundTaskInfo>>
    >? Changed;

    public BackgroundTaskHandle QueueTask(
        BackgroundTaskMetadata metadata,
        Func<BackgroundTaskContext, ValueTask> task
    )
    {
        ArgumentNullException.ThrowIfNull(task);

        var operation = QueueTaskCore(metadata, ExecuteAsync);
        return new BackgroundTaskHandle(
            operation.Id,
            ConvertCompletionAsync(operation.Completion.Task),
            () => CancelTask(operation.Id),
            () => GetSnapshot(operation)
        );

        async ValueTask<object?> ExecuteAsync(BackgroundTaskContext context)
        {
            await task(context).ConfigureAwait(false);
            return null;
        }
    }

    public BackgroundTaskHandle<TResult> QueueTask<TResult>(
        BackgroundTaskMetadata metadata,
        Func<BackgroundTaskContext, ValueTask<TResult>> task
    )
    {
        ArgumentNullException.ThrowIfNull(task);

        var operation = QueueTaskCore(metadata, ExecuteAsync);
        return new BackgroundTaskHandle<TResult>(
            operation.Id,
            ConvertCompletionAsync<TResult>(operation.Completion.Task),
            () => CancelTask(operation.Id),
            () => GetSnapshot(operation)
        );

        async ValueTask<object?> ExecuteAsync(BackgroundTaskContext context)
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
                case BackgroundTaskState.Queued:
                    operation.State = BackgroundTaskState.Canceled;
                    operation.CompletedAt = DateTimeOffset.UtcNow;
                    RemoveActiveOperationLocked(operation);
                    completion = new OperationCompletion(operation.CreateSnapshot(), null);
                    break;

                case BackgroundTaskState.Running:
                    operation.State = BackgroundTaskState.Cancelling;
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
            _ = CancelWithoutBlockingAsync(operation.CancellationSource, cancellationDispatch);
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
                stopTask = Task.Run(() => StopCoreAsync(operations, workerDrainTask));
            }

            currentStopTask = stopTask;
        }

        return cancellationToken.CanBeCanceled
            ? currentStopTask.WaitAsync(cancellationToken)
            : currentStopTask;
    }

    private BackgroundOperation QueueTaskCore(
        BackgroundTaskMetadata metadata,
        Func<BackgroundTaskContext, ValueTask<object?>> task
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
                    if (operation.State == BackgroundTaskState.Queued)
                    {
                        operation.State = BackgroundTaskState.Running;
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
        var terminalState = BackgroundTaskState.Succeeded;
        var context = new BackgroundTaskContext(
            operation.CancellationSource.Token,
            progress => ReportProgress(operation, progress)
        );

        try
        {
            value = await operation.ExecuteAsync(context).ConfigureAwait(false);
            if (operation.CancellationSource.IsCancellationRequested)
            {
                terminalState = BackgroundTaskState.Canceled;
                value = null;
            }
        }
        catch (OperationCanceledException)
            when (operation.CancellationSource.IsCancellationRequested)
        {
            terminalState = BackgroundTaskState.Canceled;
        }
        catch (Exception error)
        {
            terminalState = BackgroundTaskState.Failed;
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
                is BackgroundTaskState.Running
                    or BackgroundTaskState.Cancelling
            )
            {
                if (operation.Progress == progress)
                {
                    return;
                }

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
        BackgroundTaskState terminalState,
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
                terminalState == BackgroundTaskState.Succeeded
                && operation.State == BackgroundTaskState.Cancelling
            )
            {
                terminalState = BackgroundTaskState.Canceled;
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

    private BackgroundTaskInfo GetSnapshot(BackgroundOperation operation)
    {
        lock (gate)
        {
            return operation.CreateSnapshot();
        }
    }

    private StateChangedEventArgs<
        IReadOnlyList<BackgroundTaskInfo>
    > CreateTasksChangedEventArgsLocked()
    {
        IReadOnlyList<BackgroundTaskInfo> snapshot =
            new ReadOnlyCollection<BackgroundTaskInfo>(
                activeOperations.Select(operation => operation.CreateSnapshot()).ToArray()
            );
        Volatile.Write(ref current, snapshot);
        return new StateChangedEventArgs<IReadOnlyList<BackgroundTaskInfo>>(snapshot);
    }

    private void RemoveActiveOperationLocked(BackgroundOperation operation)
    {
        activeOperationsById.Remove(operation.Id);
        activeOperations.Remove(operation);
    }

    private async Task StopCoreAsync(BackgroundOperation[] operations, Task workerDrainTask)
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
                .Select(operation => operation.CancellationDispatchTask ?? Task.CompletedTask)
                .ToArray();
        }

        await Task.WhenAll(cancellationDispatchTasks).ConfigureAwait(false);
    }

    private static TaskCompletionSource CreateCompletedSignal()
    {
        var signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
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

            StateChangedEventArgs<IReadOnlyList<BackgroundTaskInfo>> eventArgs;
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
            Debug.WriteLine($"Background task failure logging failed: {loggingError}");
        }
    }

    private void RaiseTasksChangedCore(
        StateChangedEventArgs<IReadOnlyList<BackgroundTaskInfo>> eventArgs
    )
    {
        var handlers = Changed;
        if (handlers is null)
        {
            return;
        }

        foreach (
            EventHandler<
                StateChangedEventArgs<IReadOnlyList<BackgroundTaskInfo>>
            > handler in handlers.GetInvocationList()
        )
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception error)
            {
                Debug.WriteLine($"Background task event handler failed: {error}");
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
            Debug.WriteLine($"Background task cancellation callback failed: {error}");
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
            // The task completed between its state transition and cancellation dispatch.
        }
        catch (Exception error)
        {
            Debug.WriteLine($"Background task cancellation callback failed: {error}");
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

    private static async Task<BackgroundTaskResult> ConvertCompletionAsync(
        Task<OperationCompletion> completionTask
    )
    {
        var completion = await completionTask.ConfigureAwait(false);
        return new BackgroundTaskResult(completion.Info);
    }

    private static async Task<BackgroundTaskResult<TResult>> ConvertCompletionAsync<TResult>(
        Task<OperationCompletion> completionTask
    )
    {
        var completion = await completionTask.ConfigureAwait(false);
        var value = completion.Value is null ? default : (TResult)completion.Value;
        return new BackgroundTaskResult<TResult>(completion.Info, value);
    }

    private sealed class BackgroundOperation(
        BackgroundTaskMetadata metadata,
        Func<BackgroundTaskContext, ValueTask<object?>> executeAsync
    )
    {
        public Guid Id { get; } = Guid.NewGuid();

        public BackgroundTaskMetadata Metadata { get; } = metadata;

        public Func<BackgroundTaskContext, ValueTask<object?>> ExecuteAsync { get; } =
            executeAsync;

        public CancellationTokenSource CancellationSource { get; } = new();

        public TaskCompletionSource<OperationCompletion> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public BackgroundTaskState State { get; set; } = BackgroundTaskState.Queued;

        public double? Progress { get; set; }

        public DateTimeOffset QueuedAt { get; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public Exception? Exception { get; set; }

        public Task? CancellationDispatchTask { get; set; }

        public BackgroundTaskInfo CreateSnapshot()
        {
            return new BackgroundTaskInfo(
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

    private sealed record OperationCompletion(BackgroundTaskInfo Info, object? Value);
}
