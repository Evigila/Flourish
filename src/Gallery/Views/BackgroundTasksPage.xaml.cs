using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CKey = Arkheide.Essential.Culture.Key;
using Localizer = Arkheide.Essential.Culture.Localizer;
using ArkheideSystem.Flourish.Abstract;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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
        RefreshActiveTasks(backgroundTasks.Current);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        backgroundTasks.Changed -= BackgroundTasks_Changed;
        backgroundTasks.Changed += BackgroundTasks_Changed;
        RefreshActiveTasks(backgroundTasks.Current);
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        backgroundTasks.Changed -= BackgroundTasks_Changed;
    }

    private void BackgroundTasks_Changed(
        object? sender,
        FlourishStateChangedEventArgs<IReadOnlyList<FlourishBackgroundTaskInfo>> e
    )
    {
        Dispatcher.BeginInvoke(() => RefreshActiveTasks(e.Current));
    }

    private void AddProgressTask_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var taskName = AddProgressTask(CKey.Runtime_InteractiveProgressTask0_77FBD62C, 150);
            ServiceOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_Queued0_06E215EA, taskName)
            );
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
            var handle = backgroundTasks.QueueTask(
                new FlourishBackgroundTaskMetadata(
                    Localizer.Parse(CKey.Runtime_ResultTask0_3715E242, sequence),
                    Localizer.Parse(
                        CKey.Runtime_CalculatesAValueAndReturnsItThroughTheTypedHandle_CB4074C6
                    ),
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
            ServiceOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_QueuedTypedResultTask0_0150B8DE, handle.Id)
            );
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
                AddProgressTask(CKey.Runtime_BurstItem0_D1B9E728, 90 + (index * 30));
            }

            ServiceOutput.WriteLine(
                Localizer.Parse(
                    CKey.Runtime_QueuedFourTasksTheConfiguredConcurrencyLimitIs0_03B406E3,
                    backgroundTasks.MaxConcurrency
                )
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
            ServiceOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_NoTaskHasBeenSubmittedByThisPageYet_7BF12FBA)
            );
            return;
        }

        try
        {
            ServiceOutput.WriteLine(
                backgroundTasks.CancelTask(id)
                    ? Localizer.Parse(CKey.Runtime_CancellationRequestedFor0_E0FD5F68, id)
                    : Localizer.Parse(CKey.Runtime_Task0IsNoLongerActive_FA5562AC, id)
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
            ServiceOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_SelectAnActiveTaskFirst_648362F8)
            );
            return;
        }

        try
        {
            ServiceOutput.WriteLine(
                backgroundTasks.CancelTask(row.Id)
                    ? Localizer.Parse(
                        CKey.Runtime_CancellationRequestedFor0_E0FD5F68,
                        row.Name
                    )
                    : Localizer.Parse(CKey.Runtime_Text0IsNoLongerActive_471A7907, row.Name)
            );
        }
        catch (Exception error)
        {
            WriteError(error);
        }
    }

    private string AddProgressTask(string nameFormatKey, int delayMilliseconds)
    {
        var sequence = Interlocked.Increment(ref taskSequence);
        var handle = backgroundTasks.QueueTask(
            new FlourishBackgroundTaskMetadata(
                Localizer.Parse(nameFormatKey, sequence),
                Localizer.Parse(
                    CKey.Runtime_ReportsProgressAndObservesCooperativeCancellation_11E9A330
                ),
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
        var valueText = value is null
            ? string.Empty
            : Localizer.Parse(CKey.Runtime_Value0_D2183E4C, value);
        var errorText = info.Exception is null ? string.Empty : $"  |  {info.Exception.Message}";
        outcomes.Insert(0, $"{info.Metadata.Name}  |  {info.State}{valueText}{errorText}");
        while (outcomes.Count > 20)
        {
            outcomes.RemoveAt(outcomes.Count - 1);
        }
    }

    private void RefreshActiveTasks(IReadOnlyList<FlourishBackgroundTaskInfo> tasks)
    {
        ActiveTaskList.ItemsSource = tasks
            .Select(info => new ActiveTaskRow(info))
            .ToArray();
    }

    private void WriteError(Exception error) =>
        ServiceOutput.WriteLine(
            Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
        );

    private sealed record ActiveTaskRow(
        Guid Id,
        string Name,
        FlourishBackgroundTaskState State,
        double? Progress
    )
    {
        public ActiveTaskRow(FlourishBackgroundTaskInfo info)
            : this(info.Id, info.Metadata.Name, info.State, info.Progress) { }

        public override string ToString()
        {
            var progress = Progress is null
                ? Localizer.Parse(CKey.Runtime_Waiting_80CFA3E7)
                : $"{Progress:P0}";
            return $"{Name}  |  {State}  |  {progress}";
        }
    }
}
