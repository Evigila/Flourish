using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class WindowRuntimePage : Page
{
    private static Type? initialProfilePageType;

    private readonly IWindowService window;
    private readonly ITrayService tray;
    private readonly IWindowCloseService close;
    private readonly IProfileFlyoutService profileFlyout;
    private readonly IMessageService messages;
    private readonly INotificationService notifications;
    private readonly Type startupProfilePageType;
    private IWindowCloseGuardRegistration? closeGuard;
    private FlourishNotificationHandle? notificationHandle;

    public WindowRuntimePage(
        IWindowService window,
        ITrayService tray,
        IWindowCloseService close,
        IProfileFlyoutService profileFlyout,
        IMessageService messages,
        INotificationService notifications
    )
    {
        this.window = window;
        this.tray = tray;
        this.close = close;
        this.profileFlyout = profileFlyout;
        this.messages = messages;
        this.notifications = notifications;
        var currentProfilePageType = profileFlyout.Current.ContentPageType;
        startupProfilePageType =
            Interlocked.CompareExchange(
                ref initialProfilePageType,
                currentProfilePageType,
                null
            ) ?? currentProfilePageType;
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
        profileFlyout.Changed += RuntimeState_Changed;
        notifications.NotificationsChanged += RuntimeState_Changed;
        RefreshAll();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        window.StateChanged -= RuntimeState_Changed;
        tray.StateChanged -= RuntimeState_Changed;
        profileFlyout.Changed -= RuntimeState_Changed;
        notifications.NotificationsChanged -= RuntimeState_Changed;
        closeGuard?.Dispose();
        closeGuard = null;
    }

    private void RuntimeState_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshAll);
    }

    private void SetDemoSize_Click(object sender, RoutedEventArgs e) =>
        Execute(() => window.SetSize(1100, 760), WindowStatusText);

    private void CenterWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.CenterOnScreen, WindowStatusText);

    private void ToggleTopmost_Click(object sender, RoutedEventArgs e) =>
        Execute(() => window.SetTopmost(!window.Current.IsTopmost), WindowStatusText);

    private void ToggleTaskbar_Click(object sender, RoutedEventArgs e) =>
        Execute(() => window.SetShownInTaskbar(!window.Current.IsShownInTaskbar), WindowStatusText);

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Minimize, WindowStatusText);

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Maximize, WindowStatusText);

    private void RestoreWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(window.Restore, WindowStatusText);

    private async void HideBriefly_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            window.Hide();
            await Task.Delay(1000);
            window.Show();
            window.Activate();
        }
        catch (Exception error)
        {
            WindowStatusText.Text = error.Message;
        }
    }

    private void ToggleTray_Click(object sender, RoutedEventArgs e) =>
        Execute(() => tray.SetEnabled(!tray.Current.IsEnabled), TrayStatusText);

    private void ApplyTrayToolTip_Click(object sender, RoutedEventArgs e) =>
        Execute(() => tray.SetToolTip(TrayToolTipBox.Text), TrayStatusText);

    private void MinimizeToTray_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () =>
            {
                if (!tray.MinimizeToTray())
                {
                    throw new InvalidOperationException("Enable the tray icon before minimizing to it.");
                }
            },
            TrayStatusText
        );

    private void RestoreFromTray_Click(object sender, RoutedEventArgs e) =>
        Execute(tray.Restore, TrayStatusText);

    private void ApplyCloseBehavior_Click(object sender, RoutedEventArgs e)
    {
        if (CloseBehaviorBox.SelectedItem is WindowCloseBehavior behavior)
        {
            Execute(() => close.SetBehavior(behavior), CloseStatusText);
        }
    }

    private void RegisterCloseGuard_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                closeGuard?.Dispose();
                var allowsClose = CloseGuardAllowsBox.IsChecked == true;
                closeGuard = close.RegisterGuard(
                    "gallery.runtime.guard",
                    (_, _) =>
                        ValueTask.FromResult(
                            allowsClose
                                ? WindowCloseDecision.Allow
                                : WindowCloseDecision.Cancel
                        ),
                    order: 100
                );
                CloseStatusText.Text = "Close guard registered at order 100.";
            },
            CloseStatusText
        );
    }

    private void RemoveCloseGuard_Click(object sender, RoutedEventArgs e)
    {
        closeGuard?.Dispose();
        closeGuard = null;
        CloseStatusText.Text = "The Gallery close guard was removed.";
    }

    private async void EvaluateClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var allowed = await close.CanCloseAsync(WindowCloseRequestReason.Application);
            CloseStatusText.Text = $"Current guard evaluation: {(allowed ? "allow" : "cancel")}.";
        }
        catch (Exception error)
        {
            CloseStatusText.Text = error.Message;
        }
    }

    private async void RequestClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var closed = await close.RequestCloseAsync(WindowCloseRequestReason.Application);
            CloseStatusText.Text = closed
                ? "The close request was accepted."
                : "The close request was canceled.";
        }
        catch (Exception error)
        {
            CloseStatusText.Text = error.Message;
        }
    }

    private void ToggleProfileEnabled_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () => profileFlyout.SetEnabled(!profileFlyout.Current.IsEnabled),
            ProfileStatusText
        );

    private void ToggleProfileFlyout_Click(object sender, RoutedEventArgs e) =>
        Execute(profileFlyout.Toggle, ProfileStatusText);

    private void UseConfigurationProfilePage_Click(object sender, RoutedEventArgs e) =>
        Execute(() => profileFlyout.SetContentPage<ConfigurationPage>(), ProfileStatusText);

    private void RestoreProfilePage_Click(object sender, RoutedEventArgs e) =>
        Execute(() => profileFlyout.SetContentPage(startupProfilePageType), ProfileStatusText);

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
            MessageStatusText.Text = $"Standard message result: {result}.";
        }
        catch (Exception error)
        {
            MessageStatusText.Text = error.Message;
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
            MessageStatusText.Text = $"Custom message result: {result?.Id ?? "dismissed"}.";
        }
        catch (Exception error)
        {
            MessageStatusText.Text = error.Message;
        }
    }

    private void ShowNotification_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                notificationHandle = notifications.Show(CreateNotification());
                NotificationStatusText.Text = $"Shown: {notificationHandle.Id}.";
            },
            NotificationStatusText
        );
    }

    private void UpsertNotification_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                notificationHandle = notifications.Upsert(CreateNotification());
                NotificationStatusText.Text = $"Upserted: {notificationHandle.Id}.";
            },
            NotificationStatusText
        );
    }

    private void DismissNotification_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () =>
            {
                var dismissed = notifications.Dismiss(NotificationIdBox.Text.Trim());
                NotificationStatusText.Text = dismissed
                    ? "Notification dismissed."
                    : "No active notification matched that ID.";
            },
            NotificationStatusText
        );

    private void DismissAllNotifications_Click(object sender, RoutedEventArgs e) =>
        Execute(notifications.DismissAll, NotificationStatusText);

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

    private void Execute(Action action, FlourishTextBlock status)
    {
        try
        {
            action();
            RefreshAll();
        }
        catch (Exception error)
        {
            status.Text = error.Message;
        }
    }

    private void RefreshAll()
    {
        var windowState = window.Current;
        WindowStatusText.Text =
            $"{windowState.Bounds.Width:0} × {windowState.Bounds.Height:0} at ({windowState.Bounds.X:0}, {windowState.Bounds.Y:0})  |  {windowState.WindowState}  |  Topmost: {windowState.IsTopmost}  |  Taskbar: {windowState.IsShownInTaskbar}";

        var trayState = tray.Current;
        ToggleTrayButton.Content = trayState.IsEnabled ? "Disable tray" : "Enable tray";
        TrayToolTipBox.Text = trayState.ToolTipText;
        TrayStatusText.Text =
            $"Enabled: {trayState.IsEnabled}  |  Icon visible: {trayState.IsIconVisible}  |  Window hidden: {trayState.IsWindowHidden}";

        CloseBehaviorBox.SelectedItem = close.Behavior;

        var profileState = profileFlyout.Current;
        ToggleProfileEnabledButton.Content = profileState.IsEnabled
            ? "Disable profile"
            : "Enable profile";
        ProfileStatusText.Text =
            $"Enabled: {profileState.IsEnabled}  |  Visible: {profileState.IsVisible}  |  Content: {profileState.ContentPageType.Name}";

        NotificationStatusText.Text =
            $"Active shell notifications: {notifications.ActiveNotifications.Count}.";
    }
}
