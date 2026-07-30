using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Interaction;
using ArkheideSystem.Flourish.Services;
using Border = System.Windows.Controls.Border;
using Button = ArkheideSystem.Flourish.Controls.Button;
using ColumnDefinition = System.Windows.Controls.ColumnDefinition;
using Grid = System.Windows.Controls.Grid;
using StackPanel = System.Windows.Controls.StackPanel;
using TextBlock = ArkheideSystem.Flourish.Controls.FlourishTextBlock;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace ArkheideSystem.Flourish.Views.Windows;

internal sealed class ShellStatusSurfaceController : IDisposable
{
    private readonly FlourishStatusService statusService;
    private readonly IBackgroundTaskService backgroundTaskService;
    private readonly FlourishLocalizationService localizationService;
    private readonly FlourishStatusBar statusBar;
    private readonly StatusOverlay overlay;
    private readonly Dispatcher dispatcher;
    private readonly StatusItemViewCache statusItemViews;
    private readonly Dictionary<Guid, BackgroundTaskIconView> backgroundTaskIconsById = [];
    private readonly Dictionary<Guid, BackgroundTaskRowView> backgroundTaskRowsById = [];
    private readonly Lock backgroundTaskRefreshGate = new();
    private readonly Lock statusRefreshGate = new();
    private readonly DispatcherTimer backgroundTaskRefreshTimer;
    private FlourishStatusBarSnapshot statusBarSnapshot;
    private IReadOnlyList<FlourishBackgroundTaskInfo> backgroundTasks = [];
    private IReadOnlyList<FlourishBackgroundTaskInfo> pendingBackgroundTasks = [];
    private FlourishStatusBarChangedEventArgs? pendingStatusChange;
    private TextBlock? backgroundTaskEmptyText;
    private FrameworkElement? anchor;
    private Guid? anchorTaskId;
    private StatusSurfaceKind kind;
    private bool backgroundTaskRefreshPending;
    private bool backgroundTaskRefreshLoopActive;
    private bool statusRefreshPending;
    private bool started;
    private int disposed;

    internal ShellStatusSurfaceController(
        FlourishStatusService statusService,
        IBackgroundTaskService backgroundTaskService,
        FlourishLocalizationService localizationService,
        FlourishStatusBar statusBar,
        StatusOverlay overlay,
        Dispatcher dispatcher
    )
    {
        this.statusService = statusService;
        this.backgroundTaskService = backgroundTaskService;
        this.localizationService = localizationService;
        this.statusBar = statusBar;
        this.overlay = overlay;
        this.dispatcher = dispatcher;
        statusItemViews = statusBar.CreateStatusItemViewCache();
        statusBarSnapshot = statusService.Current;
        backgroundTaskRefreshTimer = new DispatcherTimer(
            DispatcherPriority.Background,
            dispatcher
        )
        {
            Interval = TimeSpan.FromMilliseconds(33),
        };
        backgroundTaskRefreshTimer.Tick += BackgroundTaskRefreshTimer_Tick;
    }

    internal event EventHandler<StatusSurfaceOpenRequestedEventArgs>? OpenRequested;

    internal event EventHandler<StatusSurfaceCloseRequestedEventArgs>? CloseRequested;

    internal event EventHandler? VisualStateChanged;

    internal event EventHandler<StatusSurfaceAnchorChangedEventArgs>? AnchorChanged;

    internal event EventHandler? ContentFocusInvalidated;

    internal bool IsOpen => overlay.IsOpen;

    internal FrameworkElement? Anchor => anchor;

    internal bool ContainsKeyboardFocus => overlay.ContainsKeyboardFocus;

    internal double CardActualWidth => overlay.CardActualWidth;

    internal double CardActualHeight => overlay.CardActualHeight;

    internal double CardWidth => overlay.CardWidth;

    internal int ActiveTaskCount => backgroundTaskService.ActiveTasks.Count;

    internal void Start()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (started)
        {
            return;
        }

        started = true;
        statusService.Changed += StatusService_Changed;
        backgroundTaskService.TasksChanged += BackgroundTaskService_TasksChanged;
        localizationService.Changed += LocalizationService_Changed;
        statusBar.AnchorRequested += StatusBar_AnchorRequested;
        statusBar.InteractionStarted += StatusBar_InteractionStarted;
        overlay.DismissRequested += StatusOverlay_DismissRequested;
        overlay.PlacementInvalidated += StatusOverlay_PlacementInvalidated;

        statusBarSnapshot = statusService.Current;
        statusItemViews.Synchronize(statusBarSnapshot);
        RefreshBackgroundTaskStatus(backgroundTaskService.ActiveTasks);
        RefreshLocale();
        RefreshVisibility();
    }

    internal void Open()
    {
        if (IsDisposed || anchor is null || kind == StatusSurfaceKind.None)
        {
            return;
        }

        overlay.PlacementTarget = anchor;
        overlay.Variant =
            kind == StatusSurfaceKind.System
                ? OverlayVariant.Temporary
                : OverlayVariant.Strong;
        overlay.Open();
    }

    internal FrameworkElement? Close()
    {
        var previousAnchor = anchor;
        overlay.Close();
        SetAnchor(null);
        anchorTaskId = null;
        kind = StatusSurfaceKind.None;
        return previousAnchor;
    }

    internal void FocusContent()
    {
        if (kind == StatusSurfaceKind.BackgroundTasks)
        {
            foreach (var task in backgroundTasks)
            {
                if (
                    backgroundTaskRowsById.TryGetValue(task.Id, out var row)
                    && row.CancelButton.IsEnabled
                    && row.CancelButton.Focus()
                )
                {
                    return;
                }
            }
        }

        overlay.FocusFallback();
    }

    internal void SetLayout(double left, double top, double maxWidth, double maxHeight) =>
        overlay.SetLayout(left, top, maxWidth, maxHeight);

    internal void RefreshVisibility()
    {
        var showStatusBar = statusBar.UpdateVisibility(
            statusBarSnapshot,
            backgroundTasks.Count > 0
        );
        if (!showStatusBar && overlay.IsOpen)
        {
            RequestClose(restoreFocus: false);
        }
    }

    internal void SetRegionContent(
        bool isStart,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        statusBar.SetRegionContent(isStart, elements);
        RefreshVisibility();
    }

    internal void CancelActiveTasks()
    {
        foreach (var task in backgroundTaskService.ActiveTasks)
        {
            backgroundTaskService.CancelTask(task.Id);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        if (started)
        {
            statusService.Changed -= StatusService_Changed;
            backgroundTaskService.TasksChanged -= BackgroundTaskService_TasksChanged;
            localizationService.Changed -= LocalizationService_Changed;
            statusBar.AnchorRequested -= StatusBar_AnchorRequested;
            statusBar.InteractionStarted -= StatusBar_InteractionStarted;
            overlay.DismissRequested -= StatusOverlay_DismissRequested;
            overlay.PlacementInvalidated -= StatusOverlay_PlacementInvalidated;
        }

        lock (backgroundTaskRefreshGate)
        {
            backgroundTaskRefreshPending = false;
            backgroundTaskRefreshLoopActive = false;
            pendingBackgroundTasks = [];
        }

        lock (statusRefreshGate)
        {
            pendingStatusChange = null;
            statusRefreshPending = false;
        }

        backgroundTaskRefreshTimer.Stop();
        backgroundTaskRefreshTimer.Tick -= BackgroundTaskRefreshTimer_Tick;
        foreach (var view in backgroundTaskIconsById.Values)
        {
            view.Button.Click -= BackgroundTaskButton_Click;
        }

        foreach (var view in backgroundTaskRowsById.Values)
        {
            view.CancelButton.Click -= CancelBackgroundTaskButton_Click;
        }

        backgroundTaskIconsById.Clear();
        backgroundTaskRowsById.Clear();
    }

    private bool IsDisposed => Volatile.Read(ref disposed) != 0;

    private void StatusService_Changed(object? sender, FlourishStatusBarChangedEventArgs e)
    {
        lock (statusRefreshGate)
        {
            if (
                IsDisposed
                || (
                    pendingStatusChange is not null
                    && pendingStatusChange.Current.Version >= e.Current.Version
                )
            )
            {
                return;
            }

            pendingStatusChange = e;
            if (statusRefreshPending)
            {
                return;
            }

            statusRefreshPending = true;
        }

        if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
        {
            return;
        }

        _ = dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(FlushPendingStatusChange)
        );
    }

    private void FlushPendingStatusChange()
    {
        FlourishStatusBarChangedEventArgs? change;
        lock (statusRefreshGate)
        {
            change = pendingStatusChange;
            pendingStatusChange = null;
            statusRefreshPending = false;
        }

        if (IsDisposed || change is null || !statusItemViews.Apply(change))
        {
            return;
        }

        statusBarSnapshot = change.Current;
        RefreshVisibility();
        if (kind == StatusSurfaceKind.System && overlay.IsOpen)
        {
            BuildSystemStatusFlyoutContent();
            VisualStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void BackgroundTaskService_TasksChanged(
        object? sender,
        FlourishBackgroundTasksChangedEventArgs e
    )
    {
        var shouldStartRefreshLoop = false;
        lock (backgroundTaskRefreshGate)
        {
            if (IsDisposed)
            {
                return;
            }

            pendingBackgroundTasks = e.Tasks;
            backgroundTaskRefreshPending = true;
            if (!backgroundTaskRefreshLoopActive)
            {
                backgroundTaskRefreshLoopActive = true;
                shouldStartRefreshLoop = true;
            }
        }

        if (!shouldStartRefreshLoop)
        {
            return;
        }

        if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
        {
            ResetBackgroundTaskRefreshLoop();
            return;
        }

        try
        {
            dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(StartBackgroundTaskRefreshTimer)
            );
        }
        catch (InvalidOperationException)
        {
            ResetBackgroundTaskRefreshLoop();
        }
    }

    private void BackgroundTaskRefreshTimer_Tick(object? sender, EventArgs e)
    {
        IReadOnlyList<FlourishBackgroundTaskInfo> tasks;
        lock (backgroundTaskRefreshGate)
        {
            if (IsDisposed)
            {
                backgroundTaskRefreshTimer.Stop();
                return;
            }

            if (backgroundTaskRefreshPending)
            {
                tasks = pendingBackgroundTasks;
                backgroundTaskRefreshPending = false;
            }
            else
            {
                backgroundTaskRefreshLoopActive = false;
                backgroundTaskRefreshTimer.Stop();
                return;
            }
        }

        RefreshBackgroundTaskStatus(tasks);
    }

    private void StartBackgroundTaskRefreshTimer()
    {
        lock (backgroundTaskRefreshGate)
        {
            if (
                IsDisposed
                || !backgroundTaskRefreshLoopActive
                || dispatcher.HasShutdownStarted
            )
            {
                return;
            }
        }

        backgroundTaskRefreshTimer.Start();
    }

    private void ResetBackgroundTaskRefreshLoop()
    {
        lock (backgroundTaskRefreshGate)
        {
            backgroundTaskRefreshPending = false;
            backgroundTaskRefreshLoopActive = false;
            pendingBackgroundTasks = [];
        }
    }

    private void RefreshBackgroundTaskStatus(
        IReadOnlyList<FlourishBackgroundTaskInfo> tasks
    )
    {
        if (IsDisposed)
        {
            return;
        }

        backgroundTasks = tasks;
        var runningTaskIds = new HashSet<Guid>(backgroundTasks.Count);
        var desiredTaskButtons = new List<UIElement>(backgroundTasks.Count);
        var queuedTaskCount = 0;
        foreach (var task in backgroundTasks)
        {
            if (task.State == FlourishBackgroundTaskState.Queued)
            {
                queuedTaskCount++;
                continue;
            }

            if (
                task.State
                is not (
                    FlourishBackgroundTaskState.Running
                    or FlourishBackgroundTaskState.Cancelling
                )
            )
            {
                continue;
            }

            runningTaskIds.Add(task.Id);
            if (!backgroundTaskIconsById.TryGetValue(task.Id, out var iconView))
            {
                iconView = CreateBackgroundTaskIconView(task);
                backgroundTaskIconsById.Add(task.Id, iconView);
            }

            UpdateBackgroundTaskIconView(iconView, task);
            desiredTaskButtons.Add(iconView.Button);
        }

        statusBar.SetBackgroundTaskButtons(desiredTaskButtons);
        RemoveStaleBackgroundTaskViews(backgroundTaskIconsById, runningTaskIds);
        statusBar.SetQueueState(
            queuedTaskCount,
            localizationService.Format(
                FlourishLocaleKeys.BackgroundTaskWaitingCount,
                queuedTaskCount
            )
        );
        RefreshVisibility();

        if (kind != StatusSurfaceKind.BackgroundTasks || !overlay.IsOpen)
        {
            return;
        }

        if (backgroundTasks.Count == 0)
        {
            RequestClose(restoreFocus: false);
            return;
        }

        BuildBackgroundTaskFlyoutContent();
        FrameworkElement? nextAnchor;
        if (
            anchorTaskId is { } taskId
            && backgroundTaskIconsById.TryGetValue(taskId, out var anchorIcon)
        )
        {
            nextAnchor = anchorIcon.Button;
        }
        else
        {
            anchorTaskId = null;
            nextAnchor =
                queuedTaskCount > 0
                    ? statusBar.QueueAnchor
                    : desiredTaskButtons.Count > 0
                        ? (FrameworkElement)desiredTaskButtons[0]
                        : null;
        }

        SetAnchor(nextAnchor);
        if (!overlay.ContainsKeyboardFocus)
        {
            ContentFocusInvalidated?.Invoke(this, EventArgs.Empty);
        }

        VisualStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private BackgroundTaskIconView CreateBackgroundTaskIconView(
        FlourishBackgroundTaskInfo task
    )
    {
        var icon = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        BindIconTypography(icon, "FlourishIconFontSizeStatusBarBackgroundTask");
        var toolTipName = new TextBlock { FontWeight = FontWeights.Bold };
        var toolTipDescription = new TextBlock
        {
            Margin = new Thickness(0, 3, 0, 0),
            MaxWidth = 270,
            TextWrapping = TextWrapping.Wrap,
        };
        toolTipDescription.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        var toolTipState = new TextBlock { Margin = new Thickness(0, 4, 0, 0) };
        toolTipState.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        var toolTip = new StackPanel();
        toolTip.Children.Add(toolTipName);
        toolTip.Children.Add(toolTipDescription);
        toolTip.Children.Add(toolTipState);

        var button = new Button
        {
            Width = 26,
            Height = 22,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Thickness(),
            VerticalAlignment = VerticalAlignment.Center,
            Icon = icon,
            Variant = ButtonVariant.Text,
            Tag = task.Id,
            ToolTip = toolTip,
        };
        button.Click += BackgroundTaskButton_Click;
        return new BackgroundTaskIconView(
            button,
            icon,
            toolTipName,
            toolTipDescription,
            toolTipState
        );
    }

    private void UpdateBackgroundTaskIconView(
        BackgroundTaskIconView view,
        FlourishBackgroundTaskInfo task
    )
    {
        view.Icon.Text = task.Metadata.IconGlyph ?? "\uE895";
        view.ToolTipName.Text = task.Metadata.Name;
        view.ToolTipDescription.Text = task.Metadata.Description ?? string.Empty;
        view.ToolTipDescription.Visibility =
            task.Metadata.Description is null
                ? Visibility.Collapsed
                : Visibility.Visible;
        view.ToolTipState.Text = FormatBackgroundTaskState(task);
        AutomationProperties.SetName(
            view.Button,
            $"{task.Metadata.Name}, {GetBackgroundTaskStateText(task.State)}"
        );
    }

    private void BackgroundTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement taskAnchor)
        {
            RequestBackgroundTaskFlyout(taskAnchor, focusRequested: true);
        }
    }

    private void StatusBar_AnchorRequested(
        object? sender,
        StatusBarAnchorRequestedEventArgs e
    )
    {
        if (e.Kind == StatusBarAnchorKind.System)
        {
            RequestSystemStatusFlyout(e.FocusRequested);
        }
        else
        {
            RequestBackgroundTaskFlyout(e.Anchor, e.FocusRequested);
        }
    }

    private void RequestBackgroundTaskFlyout(
        FrameworkElement requestedAnchor,
        bool focusRequested
    )
    {
        anchorTaskId = requestedAnchor.Tag is Guid taskId ? taskId : null;
        kind = StatusSurfaceKind.BackgroundTasks;
        SetAnchor(requestedAnchor);
        BuildBackgroundTaskFlyoutContent();
        OpenRequested?.Invoke(
            this,
            new StatusSurfaceOpenRequestedEventArgs(requestedAnchor, focusRequested)
        );
    }

    private void BuildBackgroundTaskFlyoutContent()
    {
        overlay.SetTitle(localizationService.Get(FlourishLocaleKeys.BackgroundTaskTitle));
        var activeTaskIds = new HashSet<Guid>(backgroundTasks.Count);
        var desiredRows = new List<UIElement>(Math.Max(1, backgroundTasks.Count));
        if (backgroundTasks.Count == 0)
        {
            backgroundTaskEmptyText ??= new TextBlock();
            backgroundTaskEmptyText.Text = localizationService.Get(
                FlourishLocaleKeys.BackgroundTaskNoActiveTasks
            );
            backgroundTaskEmptyText.SetResourceReference(
                TextBlock.ForegroundProperty,
                "FlourishNeutralForeground2Brush"
            );
            desiredRows.Add(backgroundTaskEmptyText);
        }
        else
        {
            foreach (var task in backgroundTasks)
            {
                activeTaskIds.Add(task.Id);
                if (!backgroundTaskRowsById.TryGetValue(task.Id, out var rowView))
                {
                    rowView = CreateBackgroundTaskRowView(task);
                    backgroundTaskRowsById.Add(task.Id, rowView);
                }

                UpdateBackgroundTaskRowView(rowView, task);
                desiredRows.Add(rowView.Container);
            }
        }

        overlay.SetItems(desiredRows);
        RemoveStaleBackgroundTaskViews(backgroundTaskRowsById, activeTaskIds);
    }

    private BackgroundTaskRowView CreateBackgroundTaskRowView(
        FlourishBackgroundTaskInfo task
    )
    {
        var row = new Border
        {
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(10, 8, 10, 8),
        };
        row.SetResourceReference(Border.BackgroundProperty, "FlourishNeutralBackground2Brush");
        row.SetResourceReference(Border.BorderBrushProperty, "FlourishSurfaceStrokeBrush");
        row.SetResourceReference(Border.BorderThicknessProperty, "FlourishSurfaceBorderThickness");
        row.SetResourceReference(Border.CornerRadiusProperty, "FlourishSurfaceCornerRadius");

        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        layout.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var icon = new TextBlock { VerticalAlignment = VerticalAlignment.Top };
        BindIconTypography(icon, "FlourishIconFontSizeBackgroundTaskView");
        icon.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        layout.Children.Add(icon);

        var details = new StackPanel();
        Grid.SetColumn(details, 1);
        var name = new TextBlock
        {
            FontWeight = FontWeights.Bold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        BindTextSize(name, "FlourishFontSizeStandard");
        details.Children.Add(name);
        var description = new TextBlock
        {
            Margin = new Thickness(0, 2, 8, 0),
            MaxWidth = 220,
            TextWrapping = TextWrapping.Wrap,
        };
        description.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        details.Children.Add(description);
        var state = new TextBlock { Margin = new Thickness(0, 3, 8, 0) };
        BindTextSize(state, "FlourishFontSizeStandard");
        state.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        details.Children.Add(state);
        layout.Children.Add(details);

        var cancelButton = new Button
        {
            MinWidth = 58,
            Padding = new Thickness(10, 0, 10, 0),
            Tag = task.Id,
            Content = localizationService.Get(FlourishLocaleKeys.BackgroundTaskCancel),
        };
        Grid.SetColumn(cancelButton, 2);
        cancelButton.Click += CancelBackgroundTaskButton_Click;
        layout.Children.Add(cancelButton);
        row.Child = layout;
        return new BackgroundTaskRowView(row, icon, name, description, state, cancelButton);
    }

    private void UpdateBackgroundTaskRowView(
        BackgroundTaskRowView view,
        FlourishBackgroundTaskInfo task
    )
    {
        view.Icon.Text = task.Metadata.IconGlyph ?? "\uE895";
        view.Name.Text = task.Metadata.Name;
        view.Description.Text = task.Metadata.Description ?? string.Empty;
        view.Description.Visibility =
            task.Metadata.Description is null
                ? Visibility.Collapsed
                : Visibility.Visible;
        view.State.Text = FormatBackgroundTaskState(task);
        view.CancelButton.Tag = task.Id;
        view.CancelButton.IsEnabled =
            task.State != FlourishBackgroundTaskState.Cancelling;
        view.CancelButton.Content = localizationService.Get(
            FlourishLocaleKeys.BackgroundTaskCancel
        );
        AutomationProperties.SetName(
            view.CancelButton,
            $"{localizationService.Get(FlourishLocaleKeys.BackgroundTaskCancel)} {task.Metadata.Name}"
        );
    }

    private void CancelBackgroundTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Guid taskId })
        {
            backgroundTaskService.CancelTask(taskId);
        }
    }

    private string FormatBackgroundTaskState(FlourishBackgroundTaskInfo task)
    {
        var text = GetBackgroundTaskStateText(task.State);
        return task.Progress is { } progress ? $"{text} · {progress:P0}" : text;
    }

    private string GetBackgroundTaskStateText(FlourishBackgroundTaskState state) =>
        localizationService.Get(
            state switch
            {
                FlourishBackgroundTaskState.Queued =>
                    FlourishLocaleKeys.BackgroundTaskQueued,
                FlourishBackgroundTaskState.Cancelling =>
                    FlourishLocaleKeys.BackgroundTaskCancelling,
                _ => FlourishLocaleKeys.BackgroundTaskRunning,
            }
        );

    private void RequestSystemStatusFlyout(bool focusRequested)
    {
        kind = StatusSurfaceKind.System;
        anchorTaskId = null;
        SetAnchor(statusBar.SystemAnchor);
        BuildSystemStatusFlyoutContent();
        OpenRequested?.Invoke(
            this,
            new StatusSurfaceOpenRequestedEventArgs(statusBar.SystemAnchor, focusRequested)
        );
    }

    private void BuildSystemStatusFlyoutContent()
    {
        overlay.SetTitle(localizationService.Get(FlourishLocaleKeys.SystemStatusTitle));
        overlay.ClearItems();
        if (statusBarSnapshot.IsLanStatusEnabled)
        {
            var networkState = localizationService.Get(
                FlourishLocaleKeys.SystemStatusUnknown
            );
            try
            {
                networkState = localizationService.Get(
                    System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
                        ? FlourishLocaleKeys.StatusConnected
                        : FlourishLocaleKeys.StatusDisconnected
                );
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Flourish network status query failed: {error}"
                );
            }

            overlay.AppendItem(
                CreateStatusDetailRow(
                    "\uE701",
                    localizationService.Get(FlourishLocaleKeys.SystemStatusNetwork),
                    networkState
                )
            );
        }

        if (statusBarSnapshot.IsPowerStatusEnabled)
        {
            var powerSource = GetPowerStatusText();
            overlay.AppendItem(
                CreateStatusDetailRow(
                    "\uE850",
                    localizationService.Get(FlourishLocaleKeys.SystemStatusPower),
                    powerSource
                )
            );
        }
    }

    private string GetPowerStatusText()
    {
        var powerSource = localizationService.Get(
            FlourishLocaleKeys.SystemStatusUnknown
        );
        try
        {
            var powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;
            powerSource = powerStatus.PowerLineStatus switch
            {
                System.Windows.Forms.PowerLineStatus.Online => localizationService.Get(
                    FlourishLocaleKeys.SystemStatusAC
                ),
                System.Windows.Forms.PowerLineStatus.Offline => localizationService.Get(
                    FlourishLocaleKeys.SystemStatusBattery
                ),
                _ => localizationService.Get(FlourishLocaleKeys.SystemStatusUnknown),
            };
            var batteryStatus = powerStatus.BatteryChargeStatus;
            var hasNoSystemBattery = batteryStatus.HasFlag(
                System.Windows.Forms.BatteryChargeStatus.NoSystemBattery
            );
            var hasUsableBattery =
                !hasNoSystemBattery
                && !batteryStatus.HasFlag(
                    System.Windows.Forms.BatteryChargeStatus.Unknown
                );
            if (
                hasNoSystemBattery
                && powerStatus.PowerLineStatus
                    != System.Windows.Forms.PowerLineStatus.Online
            )
            {
                powerSource = localizationService.Get(
                    FlourishLocaleKeys.SystemStatusUnknown
                );
            }

            if (hasUsableBattery && powerStatus.BatteryLifePercent is >= 0 and <= 1)
            {
                powerSource = $"{powerSource} · {powerStatus.BatteryLifePercent:P0}";
            }
        }
        catch (Exception error)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Flourish power status query failed: {error}"
            );
        }

        return powerSource;
    }

    private static FrameworkElement CreateStatusDetailRow(
        string iconGlyph,
        string label,
        string value
    )
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        row.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );
        var icon = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = iconGlyph,
        };
        BindIconTypography(icon, "FlourishIconFontSizeSystemStatusView");
        icon.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        row.Children.Add(icon);

        var text = new StackPanel();
        Grid.SetColumn(text, 1);
        var labelText = new TextBlock { Text = label };
        BindTextSize(labelText, "FlourishFontSizeStandard");
        labelText.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );
        text.Children.Add(labelText);
        var valueText = new TextBlock
        {
            Margin = new Thickness(0, 2, 0, 0),
            Text = value,
        };
        BindTextSize(valueText, "FlourishFontSizeStandard");
        valueText.SetResourceReference(
            TextBlock.ForegroundProperty,
            "FlourishNeutralForeground1Brush"
        );
        text.Children.Add(valueText);
        row.Children.Add(text);
        return row;
    }

    private void LocalizationService_Changed(
        object? sender,
        FlourishLocalizationChangedEventArgs e
    )
    {
        if (!dispatcher.CheckAccess())
        {
            _ = dispatcher.BeginInvoke(new Action(RefreshLocale));
            return;
        }

        RefreshLocale();
    }

    private void RefreshLocale()
    {
        if (IsDisposed)
        {
            return;
        }

        statusBar.SetSystemAutomationName(
            localizationService.Get(FlourishLocaleKeys.SystemStatusTitle)
        );
        RefreshBackgroundTaskStatus(backgroundTaskService.ActiveTasks);
        if (kind == StatusSurfaceKind.System && overlay.IsOpen)
        {
            BuildSystemStatusFlyoutContent();
            VisualStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void StatusBar_InteractionStarted(object? sender, EventArgs e)
    {
        if (overlay.IsOpen && anchor?.IsMouseOver != true)
        {
            RequestClose(restoreFocus: false);
        }
    }

    private void StatusOverlay_DismissRequested(object? sender, EventArgs e) =>
        RequestClose(restoreFocus: true);

    private void StatusOverlay_PlacementInvalidated(object? sender, EventArgs e) =>
        VisualStateChanged?.Invoke(this, EventArgs.Empty);

    private void RequestClose(bool restoreFocus) =>
        CloseRequested?.Invoke(
            this,
            new StatusSurfaceCloseRequestedEventArgs(restoreFocus)
        );

    private void SetAnchor(FrameworkElement? value)
    {
        if (ReferenceEquals(anchor, value))
        {
            return;
        }

        var previous = anchor;
        anchor = value;
        AnchorChanged?.Invoke(
            this,
            new StatusSurfaceAnchorChangedEventArgs(previous, value)
        );
    }

    private static void RemoveStaleBackgroundTaskViews<TView>(
        Dictionary<Guid, TView> viewsById,
        HashSet<Guid> activeIds
    )
    {
        List<Guid>? staleIds = null;
        foreach (var taskId in viewsById.Keys)
        {
            if (!activeIds.Contains(taskId))
            {
                (staleIds ??= []).Add(taskId);
            }
        }

        if (staleIds is null)
        {
            return;
        }

        foreach (var taskId in staleIds)
        {
            viewsById.Remove(taskId);
        }
    }

    private static void BindIconTypography(TextBlock element, string fontSizeResource)
    {
        element.SetResourceReference(
            TextBlock.FontFamilyProperty,
            "FlourishIconFontFamily"
        );
        element.SetResourceReference(TextBlock.FontSizeProperty, fontSizeResource);
        element.SetResourceReference(TextBlock.LineHeightProperty, fontSizeResource);
    }

    private static void BindTextSize(TextBlock element, string fontSizeResource) =>
        element.SetResourceReference(TextBlock.FontSizeProperty, fontSizeResource);

    private enum StatusSurfaceKind
    {
        None,
        BackgroundTasks,
        System,
    }

    private sealed record BackgroundTaskIconView(
        Button Button,
        TextBlock Icon,
        TextBlock ToolTipName,
        TextBlock ToolTipDescription,
        TextBlock ToolTipState
    );

    private sealed record BackgroundTaskRowView(
        Border Container,
        TextBlock Icon,
        TextBlock Name,
        TextBlock Description,
        TextBlock State,
        Button CancelButton
    );
}

internal sealed class StatusSurfaceOpenRequestedEventArgs(
    FrameworkElement anchor,
    bool focusRequested
) : EventArgs
{
    internal FrameworkElement Anchor { get; } = anchor;

    internal bool FocusRequested { get; } = focusRequested;
}

internal sealed class StatusSurfaceCloseRequestedEventArgs(bool restoreFocus)
    : EventArgs
{
    internal bool RestoreFocus { get; } = restoreFocus;
}

internal sealed class StatusSurfaceAnchorChangedEventArgs(
    FrameworkElement? previousAnchor,
    FrameworkElement? anchor
) : EventArgs
{
    internal FrameworkElement? PreviousAnchor { get; } = previousAnchor;

    internal FrameworkElement? Anchor { get; } = anchor;
}
