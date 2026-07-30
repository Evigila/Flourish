using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Interaction;
using WpfPanel = System.Windows.Controls.Panel;
using UserControl = System.Windows.Controls.UserControl;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishStatusBar : UserControl
{
    public FlourishStatusBar()
    {
        InitializeComponent();
    }

    internal event EventHandler<StatusBarAnchorRequestedEventArgs>? AnchorRequested;

    internal event EventHandler? InteractionStarted;

    internal FrameworkElement QueueAnchor => BackgroundTaskQueueButton;

    internal FrameworkElement SystemAnchor => SystemStatusButton;

    internal StatusItemViewCache CreateStatusItemViewCache() => new(StatusItemsHost);

    internal bool UpdateVisibility(
        FlourishStatusBarSnapshot snapshot,
        bool hasBackgroundTasks
    )
    {
        var showConfiguredContent = snapshot.IsEnabled;
        var showStatusBar = showConfiguredContent || hasBackgroundTasks;
        Visibility = showStatusBar ? Visibility.Visible : Visibility.Collapsed;
        StatusItemsHost.Visibility =
            showConfiguredContent && StatusItemsHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        SystemStatusButton.Visibility =
            showConfiguredContent
            && (snapshot.IsLanStatusEnabled || snapshot.IsPowerStatusEnabled)
                ? Visibility.Visible
                : Visibility.Collapsed;
        FooterStartRegionHost.Visibility =
            showConfiguredContent && FooterStartRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        FooterEndRegionHost.Visibility =
            showConfiguredContent && FooterEndRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        return showStatusBar;
    }

    internal void SetBackgroundTaskButtons(IReadOnlyList<UIElement> buttons) =>
        SynchronizeChildren(BackgroundTaskItemsHost, buttons);

    internal void SetQueueState(int count, string automationName)
    {
        BackgroundTaskQueueButton.Visibility =
            count > 0 ? Visibility.Visible : Visibility.Collapsed;
        BackgroundTaskQueueCountText.Text = count.ToString();
        AutomationProperties.SetName(BackgroundTaskQueueButton, automationName);
    }

    internal void SetSystemAutomationName(string name) =>
        AutomationProperties.SetName(SystemStatusButton, name);

    internal FrameworkElement? GetBackgroundTaskAnchor(Guid taskId) =>
        BackgroundTaskItemsHost
            .Children.OfType<FrameworkElement>()
            .FirstOrDefault(element => element.Tag is Guid id && id == taskId);

    internal void SetRegionContent(
        bool isStart,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        var host = isStart ? FooterStartRegionHost : FooterEndRegionHost;
        SynchronizeChildren(host, elements);
        host.Visibility = elements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BackgroundTaskQueueButton_MouseEnter(
        object sender,
        MouseEventArgs e
    ) => RaiseAnchorRequested(StatusBarAnchorKind.BackgroundTasks, focusRequested: false);

    private void BackgroundTaskQueueButton_Click(object sender, RoutedEventArgs e) =>
        RaiseAnchorRequested(StatusBarAnchorKind.BackgroundTasks, focusRequested: true);

    private void SystemStatusButton_MouseEnter(object sender, MouseEventArgs e) =>
        RaiseAnchorRequested(StatusBarAnchorKind.System, focusRequested: false);

    private void SystemStatusButton_Click(object sender, RoutedEventArgs e) =>
        RaiseAnchorRequested(StatusBarAnchorKind.System, focusRequested: true);

    private void StatusBarBorder_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    ) => InteractionStarted?.Invoke(this, EventArgs.Empty);

    private void RaiseAnchorRequested(StatusBarAnchorKind kind, bool focusRequested) =>
        AnchorRequested?.Invoke(
            this,
            new StatusBarAnchorRequestedEventArgs(
                kind,
                kind == StatusBarAnchorKind.System
                    ? SystemStatusButton
                    : BackgroundTaskQueueButton,
                focusRequested
            )
        );

    private static void SynchronizeChildren(
        WpfPanel host,
        IReadOnlyList<UIElement> desiredChildren
    )
    {
        for (var index = 0; index < desiredChildren.Count; index++)
        {
            var desired = desiredChildren[index];
            if (index < host.Children.Count && ReferenceEquals(host.Children[index], desired))
            {
                continue;
            }

            var existingIndex = host.Children.IndexOf(desired);
            if (existingIndex >= 0)
            {
                host.Children.RemoveAt(existingIndex);
            }

            host.Children.Insert(index, desired);
        }

        while (host.Children.Count > desiredChildren.Count)
        {
            host.Children.RemoveAt(host.Children.Count - 1);
        }
    }
}

internal enum StatusBarAnchorKind
{
    BackgroundTasks,
    System,
}

internal sealed class StatusBarAnchorRequestedEventArgs(
    StatusBarAnchorKind kind,
    FrameworkElement anchor,
    bool focusRequested
) : EventArgs
{
    internal StatusBarAnchorKind Kind { get; } = kind;

    internal FrameworkElement Anchor { get; } = anchor;

    internal bool FocusRequested { get; } = focusRequested;
}
