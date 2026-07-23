using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class WindowRuntimePage : Page
{
    private readonly IWindowService window;
    private readonly ITrayService tray;
    private readonly IWindowCloseService close;
    private readonly IMessageService messages;
    private readonly INotificationService notifications;
    private IWindowCloseGuardRegistration? closeGuard;
    private FlourishNotificationHandle? notificationHandle;
    private bool closeGuardAllows = true;
    private bool isRefreshingCloseBehavior;
    private bool isRefreshingTrayToolTip;

    public WindowRuntimePage(
        IWindowService window,
        ITrayService tray,
        IWindowCloseService close,
        IMessageService messages,
        INotificationService notifications
    )
    {
        this.window = window;
        this.tray = tray;
        this.close = close;
        this.messages = messages;
        this.notifications = notifications;
        InitializeComponent();

        CloseBehaviorBox.ItemsSource = Enum.GetValues<WindowCloseBehavior>();
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshAll();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        window.StateChanged += RuntimeState_Changed;
        tray.StateChanged += RuntimeState_Changed;
        notifications.NotificationsChanged += RuntimeState_Changed;
        RefreshAll();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        window.StateChanged -= RuntimeState_Changed;
        tray.StateChanged -= RuntimeState_Changed;
        notifications.NotificationsChanged -= RuntimeState_Changed;
        closeGuard?.Dispose();
        closeGuard = null;
    }

    private void RuntimeState_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshAll);
    }

    private void SetDemoSize_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () => window.SetSize(1100, 760),
            WindowOutput,
            "Set the shell window size to 1100 × 760."
        );

    private void CenterWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.CenterOnScreen, WindowOutput, "Centered the shell window on screen.");

    private void ToggleTopmost_Click(object sender, RoutedEventArgs e)
    {
        var topmost = !window.Current.IsTopmost;
        Execute(
            () => window.SetTopmost(topmost),
            WindowOutput,
            $"Shell window topmost mode {(topmost ? "enabled" : "disabled")}."
        );
    }

    private void ToggleTaskbar_Click(object sender, RoutedEventArgs e)
    {
        var shown = !window.Current.IsShownInTaskbar;
        Execute(
            () => window.SetShownInTaskbar(shown),
            WindowOutput,
            $"Shell window {(shown ? "shown in" : "removed from")} the taskbar."
        );
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Minimize, WindowOutput, "Minimized the shell window.");

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Maximize, WindowOutput, "Maximized the shell window.");

    private void RestoreWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Restore, WindowOutput, "Restored the shell window.");

    private async void HideBriefly_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            window.Hide();
            await Task.Delay(1000);
            window.Show();
            window.Activate();
            WindowOutput.WriteLine("Restored the shell window after one second.");
        }
        catch (Exception error)
        {
            WindowOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void ToggleTray_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !tray.Current.IsEnabled;
        Execute(
            () => tray.SetEnabled(enabled),
            TrayOutput,
            $"Notification-area tray icon {(enabled ? "enabled" : "disabled")}."
        );
    }

    private void TrayToolTipBox_LostFocus(object sender, RoutedEventArgs e) =>
        ApplyTrayToolTip();

    private void TrayToolTipBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        ApplyTrayToolTip();
        e.Handled = true;
    }

    private void ApplyTrayToolTip()
    {
        if (
            isRefreshingTrayToolTip
            || string.Equals(
                TrayToolTipBox.Text,
                tray.Current.ToolTipText,
                StringComparison.Ordinal
            )
        )
        {
            return;
        }

        Execute(
            () => tray.SetToolTip(TrayToolTipBox.Text),
            TrayOutput,
            $"Tray tooltip set to \"{TrayToolTipBox.Text}\"."
        );
    }

    private void MinimizeToTray_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () =>
            {
                if (!tray.MinimizeToTray())
                {
                    throw new InvalidOperationException("Enable the tray icon before minimizing to it.");
                }
            },
            TrayOutput,
            "Minimized the shell window to the notification area."
        );

    private void RestoreFromTray_Click(object sender, RoutedEventArgs e) =>
        Execute(tray.Restore, TrayOutput, "Restored the shell window from the notification area.");

    private void CloseBehaviorBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (
            !isRefreshingCloseBehavior
            && CloseBehaviorBox.SelectedItem is WindowCloseBehavior behavior
        )
        {
            Execute(
                () => close.SetBehavior(behavior),
                CloseOutput,
                $"Close behavior set to {behavior}."
            );
        }
    }

    private void CloseGuardAllowsBox_Click(object sender, RoutedEventArgs e)
    {
        closeGuardAllows = CloseGuardAllowsBox.IsChecked == true;
        CloseOutput.WriteLine(closeGuard is null
            ? $"The next registered guard will {(closeGuardAllows ? "allow" : "cancel")} close requests."
            : $"The registered guard will now {(closeGuardAllows ? "allow" : "cancel")} close requests.");
    }

    private void RegisterCloseGuard_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                closeGuard?.Dispose();
                closeGuardAllows = CloseGuardAllowsBox.IsChecked == true;
                closeGuard = close.RegisterGuard(
                    "gallery.runtime.guard",
                    (_, _) =>
                        ValueTask.FromResult(
                            closeGuardAllows
                                ? WindowCloseDecision.Allow
                                : WindowCloseDecision.Cancel
                        ),
                    order: 100
                );
            },
            CloseOutput,
            "Close guard registered at order 100."
        );
    }

    private void RemoveCloseGuard_Click(object sender, RoutedEventArgs e)
    {
        closeGuard?.Dispose();
        closeGuard = null;
        CloseOutput.WriteLine("The Gallery close guard was removed.");
    }

    private async void EvaluateClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var allowed = await close.CanCloseAsync(WindowCloseRequestReason.Application);
            CloseOutput.WriteLine($"Current guard evaluation: {(allowed ? "allow" : "cancel")}.");
        }
        catch (Exception error)
        {
            CloseOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private async void RequestClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var closed = await close.RequestCloseAsync(WindowCloseRequestReason.Application);
            CloseOutput.WriteLine(closed
                ? "The close request was accepted."
                : "The close request was canceled.");
        }
        catch (Exception error)
        {
            CloseOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private async void ShowMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await messages.ShowAsync(
                "This dialog was opened and awaited through IMessageService.ShowAsync.",
                "Runtime message",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information
            );
            MessageActivityOutput.WriteLine($"Standard message result: {result}.");
        }
        catch (Exception error)
        {
            MessageActivityOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private async void ShowCustomMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await messages.ShowAsync(
                "Choose a runtime action. Custom options are returned as domain values.",
                "Custom runtime choices",
                new[]
                {
                    new FlourishMessageOption("later", "Later") { IsCancel = true },
                    new FlourishMessageOption("apply", "Apply now")
                    {
                        IsDefault = true,
                        IsPrimary = true,
                    },
                },
                MessageBoxImage.Question
            );
            MessageActivityOutput.WriteLine($"Custom message result: {result?.Id ?? "dismissed"}.");
        }
        catch (Exception error)
        {
            MessageActivityOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void ShowNotification_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                notificationHandle = notifications.Show(CreateNotification());
            },
            MessageActivityOutput,
            () => $"Shown notification: {notificationHandle!.Id}."
        );
    }

    private void UpsertNotification_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                notificationHandle = notifications.Upsert(CreateNotification());
            },
            MessageActivityOutput,
            () => $"Upserted notification: {notificationHandle!.Id}."
        );
    }

    private void DismissNotification_Click(object sender, RoutedEventArgs e)
    {
        var dismissed = false;
        Execute(
            () =>
            {
                dismissed = notifications.Dismiss(NotificationIdBox.Text.Trim());
            },
            MessageActivityOutput,
            () => dismissed
                ? "Notification dismissed."
                : "No active notification matched that ID."
        );
    }

    private void DismissAllNotifications_Click(object sender, RoutedEventArgs e) =>
        Execute(
            notifications.DismissAll,
            MessageActivityOutput,
            "Dismissed all shell notifications."
        );

    private FlourishNotification CreateNotification()
    {
        var id = NotificationIdBox.Text.Trim();
        if (id.Length == 0)
        {
            throw new ArgumentException("Enter a notification ID.");
        }

        return new FlourishNotification(
            id,
            "Runtime Gallery",
            NotificationMessageBox.Text,
            FlourishNotificationSeverity.Success,
            Duration: TimeSpan.FromSeconds(8)
        );
    }

    private void Execute(Action action, OutputCard output, string successMessage) =>
        Execute(action, output, () => successMessage);

    private void Execute(Action action, OutputCard output, Func<string> successMessage)
    {
        try
        {
            action();
            output.WriteLine(successMessage());
            RefreshAll();
        }
        catch (Exception error)
        {
            output.WriteLine($"Error: {error.Message}");
        }
    }

    private void RefreshAll()
    {
        var trayState = tray.Current;
        ToggleTrayButton.Content = trayState.IsEnabled ? "Disable tray" : "Enable tray";
        isRefreshingTrayToolTip = true;
        try
        {
            TrayToolTipBox.Text = trayState.ToolTipText;
        }
        finally
        {
            isRefreshingTrayToolTip = false;
        }
        isRefreshingCloseBehavior = true;
        try
        {
            CloseBehaviorBox.SelectedItem = close.Behavior;
        }
        finally
        {
            isRefreshingCloseBehavior = false;
        }

    }
}
