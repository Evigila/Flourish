using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class WindowRuntimePage : Page
{
    private readonly IWindowService window;
    private readonly ITrayService tray;
    private readonly IWindowCloseService close;
    private readonly IMessageService messages;
    private readonly INotificationService notifications;
    private readonly IGalleryLocalization localization;
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
        INotificationService notifications,
        IGalleryLocalization localization
    )
    {
        this.window = window;
        this.tray = tray;
        this.close = close;
        this.messages = messages;
        this.notifications = notifications;
        this.localization = localization;
        InitializeComponent();

        CloseBehaviorBox.ItemsSource = Enum.GetValues<WindowCloseBehavior>();
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshAll();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        window.Changed += RuntimeState_Changed;
        tray.Changed += RuntimeState_Changed;
        notifications.NotificationsChanged += RuntimeState_Changed;
        RefreshAll();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        window.Changed -= RuntimeState_Changed;
        tray.Changed -= RuntimeState_Changed;
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
            localization.Get("Set the shell window size to 1100 × 760.")
        );

    private void CenterWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.CenterOnScreen,
            WindowOutput,
            localization.Get("Centered the shell window on screen.")
        );

    private void ToggleTopmost_Click(object sender, RoutedEventArgs e)
    {
        var topmost = !window.Current.IsTopmost;
        Execute(
            () => window.SetTopmost(topmost),
            WindowOutput,
            localization.Format(
                "Shell window topmost mode {0}.",
                localization.Get(topmost ? "enabled" : "disabled")
            )
        );
    }

    private void ToggleTaskbar_Click(object sender, RoutedEventArgs e)
    {
        var shown = !window.Current.IsShownInTaskbar;
        Execute(
            () => window.SetShownInTaskbar(shown),
            WindowOutput,
            localization.Get(
                shown
                    ? "Shell window shown in the taskbar."
                    : "Shell window removed from the taskbar."
            )
        );
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Minimize, WindowOutput, localization.Get("Minimized the shell window."));

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Maximize, WindowOutput, localization.Get("Maximized the shell window."));

    private void RestoreWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Restore, WindowOutput, localization.Get("Restored the shell window."));

    private async void HideBriefly_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            window.Hide();
            await Task.Delay(1000);
            window.Show();
            window.Activate();
            WindowOutput.WriteLine(
                localization.Get("Restored the shell window after one second.")
            );
        }
        catch (Exception error)
        {
            WindowOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }
    }

    private void ToggleTray_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !tray.Current.IsEnabled;
        Execute(
            () => tray.SetEnabled(enabled),
            TrayOutput,
            localization.Format(
                "Notification-area tray icon {0}.",
                localization.Get(enabled ? "enabled" : "disabled")
            )
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
            localization.Format("Tray tooltip set to \"{0}\".", TrayToolTipBox.Text)
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
            localization.Get("Minimized the shell window to the notification area.")
        );

    private void RestoreFromTray_Click(object sender, RoutedEventArgs e) =>
        Execute(
            tray.Restore,
            TrayOutput,
            localization.Get("Restored the shell window from the notification area.")
        );

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
                localization.Format("Close behavior set to {0}.", behavior)
            );
        }
    }

    private void CloseGuardAllowsBox_Click(object sender, RoutedEventArgs e)
    {
        closeGuardAllows = CloseGuardAllowsBox.IsChecked == true;
        CloseOutput.WriteLine(
            closeGuard is null
                ? localization.Format(
                    "The next registered guard will {0} close requests.",
                    localization.Get(closeGuardAllows ? "allow" : "cancel")
                )
                : localization.Format(
                    "The registered guard will now {0} close requests.",
                    localization.Get(closeGuardAllows ? "allow" : "cancel")
                )
        );
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
            localization.Format("Close guard registered at order {0}.", 100)
        );
    }

    private void RemoveCloseGuard_Click(object sender, RoutedEventArgs e)
    {
        closeGuard?.Dispose();
        closeGuard = null;
        CloseOutput.WriteLine(localization.Get("The Gallery close guard was removed."));
    }

    private async void EvaluateClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var allowed = await close.CanCloseAsync(WindowCloseRequestReason.Application);
            CloseOutput.WriteLine(
                localization.Format(
                    "Current guard evaluation: {0}.",
                    localization.Get(allowed ? "allow" : "cancel")
                )
            );
        }
        catch (Exception error)
        {
            CloseOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }
    }

    private async void RequestClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var closed = await close.RequestCloseAsync(WindowCloseRequestReason.Application);
            CloseOutput.WriteLine(
                closed
                    ? localization.Get("The close request was accepted.")
                    : localization.Get("The close request was canceled.")
            );
        }
        catch (Exception error)
        {
            CloseOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }
    }

    private async void ShowMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await messages.ShowAsync(
                localization.Get(
                    "This dialog was opened and awaited through IMessageService.ShowAsync."
                ),
                localization.Get("Runtime message"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information
            );
            MessageActivityOutput.WriteLine(
                localization.Format("Standard message result: {0}.", result)
            );
        }
        catch (Exception error)
        {
            MessageActivityOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }
    }

    private async void ShowCustomMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await messages.ShowAsync(
                localization.Get(
                    "Choose a runtime action. Custom options are returned as domain values."
                ),
                localization.Get("Custom runtime choices"),
                new[]
                {
                    new FlourishMessageOption("later", localization.Get("Later"))
                    {
                        IsCancel = true,
                    },
                    new FlourishMessageOption("apply", localization.Get("Apply now"))
                    {
                        IsDefault = true,
                        IsPrimary = true,
                    },
                },
                MessageBoxImage.Question
            );
            MessageActivityOutput.WriteLine(
                localization.Format(
                    "Custom message result: {0}.",
                    result?.Id ?? localization.Get("dismissed")
                )
            );
        }
        catch (Exception error)
        {
            MessageActivityOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
            () => localization.Format("Shown notification: {0}.", notificationHandle!.Id)
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
            () => localization.Format("Upserted notification: {0}.", notificationHandle!.Id)
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
                ? localization.Get("Notification dismissed.")
                : localization.Get("No active notification matched that ID.")
        );
    }

    private void DismissAllNotifications_Click(object sender, RoutedEventArgs e) =>
        Execute(
            notifications.DismissAll,
            MessageActivityOutput,
            localization.Get("Dismissed all shell notifications.")
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
            localization.Get("Runtime Gallery"),
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
            output.WriteLine(localization.Format("Error: {0}", error.Message));
        }
    }

    private void RefreshAll()
    {
        var trayState = tray.Current;
        ToggleTrayButton.Content = localization.Get(
            trayState.IsEnabled ? "Disable tray" : "Enable tray"
        );
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
