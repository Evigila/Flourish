using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Gallery.Views;

public partial class BackgroundTasksPage : Page
{
    private readonly IBackgroundTaskService backgroundTasks;
    private readonly ObservableCollection<string> outcomes = [];
    private Guid? lastTaskId;
    private int taskSequence;

    public BackgroundTasksPage(IBackgroundTaskService backgroundTasks)
    {
        this.backgroundTasks = backgroundTasks;
        InitializeComponent();

        OutcomeList.ItemsSource = outcomes;
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshActiveTasks(backgroundTasks.ActiveTasks);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        backgroundTasks.TasksChanged -= BackgroundTasks_TasksChanged;
        backgroundTasks.TasksChanged += BackgroundTasks_TasksChanged;
        RefreshActiveTasks(backgroundTasks.ActiveTasks);
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        backgroundTasks.TasksChanged -= BackgroundTasks_TasksChanged;
    }

    private void BackgroundTasks_TasksChanged(
        object? sender,
        FlourishBackgroundTasksChangedEventArgs e
    )
    {
        Dispatcher.BeginInvoke(() => RefreshActiveTasks(e.Tasks));
    }

    private void AddProgressTask_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var taskName = AddProgressTask("Interactive progress task", 150);
            ServiceOutput.WriteLine($"Queued {taskName}.");
        }
        catch (Exception error)
        {
            WriteError(error);
        }
    }

    private void AddResultTask_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var sequence = Interlocked.Increment(ref taskSequence);
            var handle = backgroundTasks.AddTask(
                new FlourishBackgroundTaskMetadata(
                    $"Result task {sequence}",
                    "Calculates a value and returns it through the typed handle.",
                    "\uE945"
                ),
                async context =>
                {
                    var total = 0;
                    for (var step = 1; step <= 10; step++)
                    {
                        await Task.Delay(120, context.CancellationToken);
                        total += step;
                        context.ReportProgress(step / 10d);
                    }

                    return total;
                }
            );

            lastTaskId = handle.Id;
            ServiceOutput.WriteLine($"Queued typed result task {handle.Id}.");
            _ = ObserveResultTaskAsync(handle);
        }
        catch (Exception error)
        {
            WriteError(error);
        }
    }

    private void AddBurst_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            for (var index = 1; index <= 4; index++)
            {
                AddProgressTask($"Burst item {index}", 90 + (index * 30));
            }

            ServiceOutput.WriteLine(
                $"Queued four tasks. The configured concurrency limit is {backgroundTasks.MaxConcurrency}."
            );
        }
        catch (Exception error)
        {
            WriteError(error);
        }
    }

    private void CancelLast_Click(object sender, RoutedEventArgs e)
    {
        if (lastTaskId is not Guid id)
        {
            ServiceOutput.WriteLine("No task has been submitted by this page yet.");
            return;
        }

        try
        {
            ServiceOutput.WriteLine(
                backgroundTasks.CancelTask(id)
                    ? $"Cancellation requested for {id}."
                    : $"Task {id} is no longer active."
            );
        }
        catch (Exception error)
        {
            WriteError(error);
        }
    }

    private void CancelSelected_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveTaskList.SelectedItem is not ActiveTaskRow row)
        {
            ServiceOutput.WriteLine("Select an active task first.");
            return;
        }

        try
        {
            ServiceOutput.WriteLine(
                backgroundTasks.CancelTask(row.Id)
                    ? $"Cancellation requested for {row.Name}."
                    : $"{row.Name} is no longer active."
            );
        }
        catch (Exception error)
        {
            WriteError(error);
        }
    }

    private string AddProgressTask(string name, int delayMilliseconds)
    {
        var sequence = Interlocked.Increment(ref taskSequence);
        var handle = backgroundTasks.AddTask(
            new FlourishBackgroundTaskMetadata(
                $"{name} {sequence}",
                "Reports progress and observes cooperative cancellation.",
                "\uE895"
            ),
            async context =>
            {
                for (var step = 1; step <= 20; step++)
                {
                    await Task.Delay(delayMilliseconds, context.CancellationToken);
                    context.ReportProgress(step / 20d);
                }
            }
        );

        lastTaskId = handle.Id;
        _ = ObserveTaskAsync(handle);
        return handle.Snapshot.Metadata.Name;
    }

    private async Task ObserveTaskAsync(FlourishBackgroundTaskHandle handle)
    {
        var result = await handle.Completion;
        await Dispatcher.InvokeAsync(() => AddOutcome(result.Info, null));
    }

    private async Task ObserveResultTaskAsync(FlourishBackgroundTaskHandle<int> handle)
    {
        var result = await handle.Completion;
        await Dispatcher.InvokeAsync(() => AddOutcome(result.Info, result.Value));
    }

    private void AddOutcome(FlourishBackgroundTaskInfo info, object? value)
    {
        var valueText = value is null ? string.Empty : $"  |  value {value}";
        var errorText = info.Exception is null ? string.Empty : $"  |  {info.Exception.Message}";
        outcomes.Insert(0, $"{info.Metadata.Name}  |  {info.State}{valueText}{errorText}");
        while (outcomes.Count > 20)
        {
            outcomes.RemoveAt(outcomes.Count - 1);
        }
    }

    private void RefreshActiveTasks(IReadOnlyList<FlourishBackgroundTaskInfo> tasks)
    {
        ActiveTaskList.ItemsSource = tasks.Select(info => new ActiveTaskRow(info)).ToArray();
    }

    private void WriteError(Exception error) => ServiceOutput.WriteLine($"Error: {error.Message}");

    private sealed record ActiveTaskRow(Guid Id, string Name, FlourishBackgroundTaskState State, double? Progress)
    {
        public ActiveTaskRow(FlourishBackgroundTaskInfo info)
            : this(info.Id, info.Metadata.Name, info.State, info.Progress) { }

        public override string ToString()
        {
            var progress = Progress is null ? "waiting" : $"{Progress:P0}";
            return $"{Name}  |  {State}  |  {progress}";
        }
    }
}
