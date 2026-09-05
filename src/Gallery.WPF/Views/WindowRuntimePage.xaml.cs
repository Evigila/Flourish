using System;
using System.Threading.Tasks;

using CKey = ArkheideSystem.Essential.Culture.Key;
using Localizer = ArkheideSystem.Essential.Culture.Localizer;
using InputKey = System.Windows.Input.Key;
using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class WindowRuntimePage : Page
{
    private readonly IWindowService window;
    private readonly ITrayService tray;
    private readonly IWindowCloseService close;
    private readonly IMessageService messages;
    private readonly INotificationService notifications;
    private IRegistration? closeGuard;
    private NotificationHandle? notificationHandle;
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
        window.Changed += RuntimeState_Changed;
        tray.Changed += RuntimeState_Changed;
        notifications.Changed += RuntimeState_Changed;
        RefreshAll();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        window.Changed -= RuntimeState_Changed;
        tray.Changed -= RuntimeState_Changed;
        notifications.Changed -= RuntimeState_Changed;
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
            Localizer.Parse(CKey.Runtime_SetTheShellWindowSizeTo1100760_BEBC5F4A)
        );

    private void CenterWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.CenterOnScreen,
            WindowOutput,
            Localizer.Parse(CKey.Runtime_CenteredTheShellWindowOnScreen_3C0BE7A4)
        );

    private void ToggleTopmost_Click(object sender, RoutedEventArgs e)
    {
        var topmost = !window.Current.IsTopmost;
        Execute(
            () => window.SetTopmost(topmost),
            WindowOutput,
            Localizer.Parse(
                CKey.Runtime_ShellWindowTopmostMode0_B8A0BA3C,
                Localizer.Parse(
                    topmost ? CKey.Runtime_Enabled_FB9CF756 : CKey.Runtime_Disabled_17EB3C01
                )
            )
        );
    }

    private void ToggleTaskbar_Click(object sender, RoutedEventArgs e)
    {
        var shown = !window.Current.IsShownInTaskbar;
        Execute(
            () => window.SetShownInTaskbar(shown),
            WindowOutput,
            Localizer.Parse(
                shown
                    ? CKey.Runtime_ShellWindowShownInTheTaskbar_9ABA9C6D
                    : CKey.Runtime_ShellWindowRemovedFromTheTaskbar_92D9DF14
            )
        );
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.Minimize,
            WindowOutput,
            Localizer.Parse(CKey.Runtime_MinimizedTheShellWindow_478BE911)
        );

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.Maximize,
            WindowOutput,
            Localizer.Parse(CKey.Runtime_MaximizedTheShellWindow_1A48B139)
        );

    private void RestoreWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.Restore,
            WindowOutput,
            Localizer.Parse(CKey.Runtime_RestoredTheShellWindow_02648753)
        );

    private async void HideBriefly_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            window.Hide();
            await Task.Delay(1000);
            window.Show();
            window.Activate();
            WindowOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_RestoredTheShellWindowAfterOneSecond_876F4D37)
            );
        }
        catch (Exception error)
        {
            WindowOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void ToggleTray_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !tray.Current.IsEnabled;
        Execute(
            () => tray.SetEnabled(enabled),
            TrayOutput,
            Localizer.Parse(
                CKey.Runtime_NotificationAreaTrayIcon0_E862BD7A,
                Localizer.Parse(
                    enabled ? CKey.Runtime_Enabled_FB9CF756 : CKey.Runtime_Disabled_17EB3C01
                )
            )
        );
    }

    private void TrayToolTipBox_LostFocus(object sender, RoutedEventArgs e) => ApplyTrayToolTip();

    private void TrayToolTipBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != InputKey.Enter)
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
            Localizer.Parse(CKey.Runtime_TrayTooltipSetTo0_D5C91582, TrayToolTipBox.Text)
        );
    }

    private void MinimizeToTray_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () =>
            {
                if (!tray.MinimizeToTray())
                {
                    throw new InvalidOperationException("Enable the tray icon first.");
                }
            },
            TrayOutput,
            Localizer.Parse(CKey.Runtime_MinimizedTheShellWindowToTheNotificationArea_E26BDC65)
        );

    private void RestoreFromTray_Click(object sender, RoutedEventArgs e) =>
        Execute(
            tray.Restore,
            TrayOutput,
            Localizer.Parse(CKey.Runtime_RestoredTheShellWindowFromTheNotificationArea_3144C274)
        );

    private void CloseBehaviorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (
            !isRefreshingCloseBehavior
            && CloseBehaviorBox.SelectedItem is WindowCloseBehavior behavior
        )
        {
            Execute(
                () => close.SetBehavior(behavior),
                CloseOutput,
                Localizer.Parse(CKey.Runtime_CloseBehaviorSetTo0_E715B552, behavior)
            );
        }
    }

    private void CloseGuardAllowsBox_Click(object sender, RoutedEventArgs e)
    {
        closeGuardAllows = CloseGuardAllowsBox.IsChecked == true;
        CloseOutput.WriteLine(
            closeGuard is null
                ? Localizer.Parse(
                    CKey.Runtime_TheNextRegisteredGuardWill0CloseRequests_99AAEB54,
                    Localizer.Parse(
                        closeGuardAllows
                            ? CKey.Runtime_Allow_41008373
                            : CKey.Runtime_Cancel_2374D917
                    )
                )
                : Localizer.Parse(
                    CKey.Runtime_TheRegisteredGuardWillNow0CloseRequests_7DC92137,
                    Localizer.Parse(
                        closeGuardAllows
                            ? CKey.Runtime_Allow_41008373
                            : CKey.Runtime_Cancel_2374D917
                    )
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
            Localizer.Parse(CKey.Runtime_CloseGuardRegisteredAtOrder0_E0EA9354, 100)
        );
    }

    private void RemoveCloseGuard_Click(object sender, RoutedEventArgs e)
    {
        closeGuard?.Dispose();
        closeGuard = null;
        CloseOutput.WriteLine(
            Localizer.Parse(CKey.Runtime_TheGalleryCloseGuardWasRemoved_5525779C)
        );
    }

    private async void EvaluateClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var allowed = await close.CanCloseAsync(WindowCloseRequestReason.Application);
            CloseOutput.WriteLine(
                Localizer.Parse(
                    CKey.Runtime_CurrentGuardEvaluation0_98F8FDC5,
                    Localizer.Parse(
                        allowed ? CKey.Runtime_Allow_41008373 : CKey.Runtime_Cancel_2374D917
                    )
                )
            );
        }
        catch (Exception error)
        {
            CloseOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private async void RequestClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var closed = await close.RequestCloseAsync(WindowCloseRequestReason.Application);
            CloseOutput.WriteLine(
                closed
                    ? Localizer.Parse(CKey.Runtime_TheCloseRequestWasAccepted_5EF3EC41)
                    : Localizer.Parse(CKey.Runtime_TheCloseRequestWasCanceled_28CDC2C5)
            );
        }
        catch (Exception error)
        {
            CloseOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private async void ShowMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await messages.ShowAsync(
                Localizer.Parse(
                    CKey.Runtime_ThisDialogWasOpenedAndAwaitedThroughIMessageServiceShowAsync_201018F9
                ),
                Localizer.Parse(CKey.Runtime_RuntimeMessage_7E67DEE4),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information
            );
            MessageActivityOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_StandardMessageResult0_6DDDFBE8, result)
            );
        }
        catch (Exception error)
        {
            MessageActivityOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private async void ShowCustomMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await messages.ShowAsync(
                Localizer.Parse(
                    CKey.Runtime_ChooseARuntimeActionCustomOptionsAreReturnedAsDomainValues_7353BDBC
                ),
                Localizer.Parse(CKey.Runtime_CustomRuntimeChoices_380A5D74),
                new[]
                {
                    new MessageDialogOption(
                        "later",
                        Localizer.Parse(CKey.Runtime_Later_73B6E48A)
                    )
                    {
                        IsCancel = true,
                    },
                    new MessageDialogOption(
                        "apply",
                        Localizer.Parse(CKey.Runtime_ApplyNow_3F0C9286)
                    )
                    {
                        IsDefault = true,
                        IsPrimary = true,
                    },
                },
                MessageBoxImage.Question
            );
            MessageActivityOutput.WriteLine(
                Localizer.Parse(
                    CKey.Runtime_CustomMessageResult0_443E6D0A,
                    result?.Id ?? Localizer.Parse(CKey.Runtime_Dismissed_71116847)
                )
            );
        }
        catch (Exception error)
        {
            MessageActivityOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
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
            () =>
                Localizer.Parse(
                    CKey.Runtime_ShownNotification0_5506309A,
                    notificationHandle!.Id
                )
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
            () =>
                Localizer.Parse(
                    CKey.Runtime_UpsertedNotification0_232DD227,
                    notificationHandle!.Id
                )
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
            () =>
                dismissed
                    ? Localizer.Parse(CKey.Runtime_NotificationDismissed_3FC448EB)
                    : Localizer.Parse(CKey.Runtime_NoActiveNotificationMatchedThatID_A05E0448)
        );
    }

    private void DismissAllNotifications_Click(object sender, RoutedEventArgs e) =>
        Execute(
            notifications.DismissAll,
            MessageActivityOutput,
            Localizer.Parse(CKey.Runtime_DismissedAllShellNotifications_3A0952B6)
        );

    private Notification CreateNotification()
    {
        var id = NotificationIdBox.Text.Trim();
        if (id.Length == 0)
        {
            throw new ArgumentException("Enter a notification ID.");
        }

        return new Notification(
            id,
            Localizer.Parse(CKey.Runtime_RuntimeGallery_D19C2E76),
            NotificationMessageBox.Text,
            NotificationSeverity.Success,
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
            output.WriteLine(Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message));
        }
    }

    private void RefreshAll()
    {
        var trayState = tray.Current;
        ToggleTrayButton.Content = Localizer.Parse(
            trayState.IsEnabled
                ? CKey.Runtime_DisableTray_9AAE0B05
                : CKey.Runtime_EnableTray_A0D89F7F
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
            CloseBehaviorBox.SelectedItem = close.Current.Behavior;
        }
        finally
        {
            isRefreshingCloseBehavior = false;
        }
    }
}
