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
            localization.Get(GalleryLocaleKeys.RuntimeSetTheShellWindowSizeTo1100760_BEBC5F4A)
        );

    private void CenterWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.CenterOnScreen,
            WindowOutput,
            localization.Get(GalleryLocaleKeys.RuntimeCenteredTheShellWindowOnScreen_3C0BE7A4)
        );

    private void ToggleTopmost_Click(object sender, RoutedEventArgs e)
    {
        var topmost = !window.Current.IsTopmost;
        Execute(
            () => window.SetTopmost(topmost),
            WindowOutput,
            localization.Format(
                GalleryLocaleKeys.RuntimeShellWindowTopmostMode0_B8A0BA3C,
                localization.Get(
                    topmost
                        ? GalleryLocaleKeys.RuntimeEnabled_FB9CF756
                        : GalleryLocaleKeys.RuntimeDisabled_17EB3C01
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
            localization.Get(
                shown
                    ? GalleryLocaleKeys.RuntimeShellWindowShownInTheTaskbar_9ABA9C6D
                    : GalleryLocaleKeys.RuntimeShellWindowRemovedFromTheTaskbar_92D9DF14
            )
        );
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.Minimize,
            WindowOutput,
            localization.Get(GalleryLocaleKeys.RuntimeMinimizedTheShellWindow_478BE911)
        );

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.Maximize,
            WindowOutput,
            localization.Get(GalleryLocaleKeys.RuntimeMaximizedTheShellWindow_1A48B139)
        );

    private void RestoreWindow_Click(object sender, RoutedEventArgs e) =>
        Execute(
            window.Restore,
            WindowOutput,
            localization.Get(GalleryLocaleKeys.RuntimeRestoredTheShellWindow_02648753)
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
                localization.Get(
                    GalleryLocaleKeys.RuntimeRestoredTheShellWindowAfterOneSecond_876F4D37
                )
            );
        }
        catch (Exception error)
        {
            WindowOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }

    private void ToggleTray_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !tray.Current.IsEnabled;
        Execute(
            () => tray.SetEnabled(enabled),
            TrayOutput,
            localization.Format(
                GalleryLocaleKeys.RuntimeNotificationAreaTrayIcon0_E862BD7A,
                localization.Get(
                    enabled
                        ? GalleryLocaleKeys.RuntimeEnabled_FB9CF756
                        : GalleryLocaleKeys.RuntimeDisabled_17EB3C01
                )
            )
        );
    }

    private void TrayToolTipBox_LostFocus(object sender, RoutedEventArgs e) => ApplyTrayToolTip();

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
            localization.Format(
                GalleryLocaleKeys.RuntimeTrayTooltipSetTo0_D5C91582,
                TrayToolTipBox.Text
            )
        );
    }

    private void MinimizeToTray_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () =>
            {
                if (!tray.MinimizeToTray())
                {
                    throw new InvalidOperationException(
                        "Enable the tray icon before minimizing to it."
                    );
                }
            },
            TrayOutput,
            localization.Get(
                GalleryLocaleKeys.RuntimeMinimizedTheShellWindowToTheNotificationArea_E26BDC65
            )
        );

    private void RestoreFromTray_Click(object sender, RoutedEventArgs e) =>
        Execute(
            tray.Restore,
            TrayOutput,
            localization.Get(
                GalleryLocaleKeys.RuntimeRestoredTheShellWindowFromTheNotificationArea_3144C274
            )
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
                localization.Format(GalleryLocaleKeys.RuntimeCloseBehaviorSetTo0_E715B552, behavior)
            );
        }
    }

    private void CloseGuardAllowsBox_Click(object sender, RoutedEventArgs e)
    {
        closeGuardAllows = CloseGuardAllowsBox.IsChecked == true;
        CloseOutput.WriteLine(
            closeGuard is null
                ? localization.Format(
                    GalleryLocaleKeys.RuntimeTheNextRegisteredGuardWill0CloseRequests_99AAEB54,
                    localization.Get(
                        closeGuardAllows
                            ? GalleryLocaleKeys.RuntimeAllow_41008373
                            : GalleryLocaleKeys.RuntimeCancel_2374D917
                    )
                )
                : localization.Format(
                    GalleryLocaleKeys.RuntimeTheRegisteredGuardWillNow0CloseRequests_7DC92137,
                    localization.Get(
                        closeGuardAllows
                            ? GalleryLocaleKeys.RuntimeAllow_41008373
                            : GalleryLocaleKeys.RuntimeCancel_2374D917
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
            localization.Format(GalleryLocaleKeys.RuntimeCloseGuardRegisteredAtOrder0_E0EA9354, 100)
        );
    }

    private void RemoveCloseGuard_Click(object sender, RoutedEventArgs e)
    {
        closeGuard?.Dispose();
        closeGuard = null;
        CloseOutput.WriteLine(
            localization.Get(GalleryLocaleKeys.RuntimeTheGalleryCloseGuardWasRemoved_5525779C)
        );
    }

    private async void EvaluateClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var allowed = await close.CanCloseAsync(WindowCloseRequestReason.Application);
            CloseOutput.WriteLine(
                localization.Format(
                    GalleryLocaleKeys.RuntimeCurrentGuardEvaluation0_98F8FDC5,
                    localization.Get(
                        allowed
                            ? GalleryLocaleKeys.RuntimeAllow_41008373
                            : GalleryLocaleKeys.RuntimeCancel_2374D917
                    )
                )
            );
        }
        catch (Exception error)
        {
            CloseOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
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
                    ? localization.Get(GalleryLocaleKeys.RuntimeTheCloseRequestWasAccepted_5EF3EC41)
                    : localization.Get(GalleryLocaleKeys.RuntimeTheCloseRequestWasCanceled_28CDC2C5)
            );
        }
        catch (Exception error)
        {
            CloseOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }

    private async void ShowMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await messages.ShowAsync(
                localization.Get(
                    GalleryLocaleKeys.RuntimeThisDialogWasOpenedAndAwaitedThroughIMessageServiceShowAsync_201018F9
                ),
                localization.Get(GalleryLocaleKeys.RuntimeRuntimeMessage_7E67DEE4),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information
            );
            MessageActivityOutput.WriteLine(
                localization.Format(
                    GalleryLocaleKeys.RuntimeStandardMessageResult0_6DDDFBE8,
                    result
                )
            );
        }
        catch (Exception error)
        {
            MessageActivityOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }

    private async void ShowCustomMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await messages.ShowAsync(
                localization.Get(
                    GalleryLocaleKeys.RuntimeChooseARuntimeActionCustomOptionsAreReturnedAsDomainValues_7353BDBC
                ),
                localization.Get(GalleryLocaleKeys.RuntimeCustomRuntimeChoices_380A5D74),
                new[]
                {
                    new FlourishMessageOption(
                        "later",
                        localization.Get(GalleryLocaleKeys.RuntimeLater_73B6E48A)
                    )
                    {
                        IsCancel = true,
                    },
                    new FlourishMessageOption(
                        "apply",
                        localization.Get(GalleryLocaleKeys.RuntimeApplyNow_3F0C9286)
                    )
                    {
                        IsDefault = true,
                        IsPrimary = true,
                    },
                },
                MessageBoxImage.Question
            );
            MessageActivityOutput.WriteLine(
                localization.Format(
                    GalleryLocaleKeys.RuntimeCustomMessageResult0_443E6D0A,
                    result?.Id ?? localization.Get(GalleryLocaleKeys.RuntimeDismissed_71116847)
                )
            );
        }
        catch (Exception error)
        {
            MessageActivityOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
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
                localization.Format(
                    GalleryLocaleKeys.RuntimeShownNotification0_5506309A,
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
                localization.Format(
                    GalleryLocaleKeys.RuntimeUpsertedNotification0_232DD227,
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
                    ? localization.Get(GalleryLocaleKeys.RuntimeNotificationDismissed_3FC448EB)
                    : localization.Get(
                        GalleryLocaleKeys.RuntimeNoActiveNotificationMatchedThatID_A05E0448
                    )
        );
    }

    private void DismissAllNotifications_Click(object sender, RoutedEventArgs e) =>
        Execute(
            notifications.DismissAll,
            MessageActivityOutput,
            localization.Get(GalleryLocaleKeys.RuntimeDismissedAllShellNotifications_3A0952B6)
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
            localization.Get(GalleryLocaleKeys.RuntimeRuntimeGallery_D19C2E76),
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
            output.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }

    private void RefreshAll()
    {
        var trayState = tray.Current;
        ToggleTrayButton.Content = localization.Get(
            trayState.IsEnabled
                ? GalleryLocaleKeys.RuntimeDisableTray_9AAE0B05
                : GalleryLocaleKeys.RuntimeEnableTray_A0D89F7F
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
