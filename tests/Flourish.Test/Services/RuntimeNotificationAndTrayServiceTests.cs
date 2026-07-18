using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeNotificationAndTrayServiceTests
{
    [Fact]
    public void NotificationService_ShowsUpdatesUpsertsAndDismissesSnapshots()
    {
        using var sut = CreateNotificationService();
        var snapshots = new List<IReadOnlyList<FlourishNotificationInfo>>();
        var versions = new List<long>();
        sut.NotificationsChanged += (_, args) =>
        {
            snapshots.Add(args.Notifications);
            versions.Add(args.Version);
        };
        using var first = sut.Show(new FlourishNotification("one", "One", "Initial"));
        using var second = sut.Show(new FlourishNotification("two", "Two", "Second"));

        first.Update(
            new FlourishNotification(
                "one",
                "One updated",
                "Updated",
                FlourishNotificationSeverity.Success
            )
        );
        using var replacement = sut.Upsert(
            new FlourishNotification("two", "Two updated", "Replacement")
        );

        Assert.Equal(["one", "two"], sut.ActiveNotifications.Select(x => x.Notification.Id));
        Assert.Equal("One updated", sut.ActiveNotifications[0].Notification.Title);
        Assert.Equal("Two updated", sut.ActiveNotifications[1].Notification.Title);
        Assert.True(sut.Dismiss("one"));
        Assert.False(sut.Dismiss("missing"));
        sut.DismissAll();
        Assert.Empty(sut.ActiveNotifications);
        Assert.Equal(6, snapshots.Count);
        Assert.Equal([1, 2, 3, 4, 5, 6], versions);
    }

    [Fact]
    public void NotificationService_RejectsDuplicatesAndInvalidDefinitions()
    {
        using var sut = CreateNotificationService();
        using var handle = sut.Show(new FlourishNotification("same", "Title", "Body"));

        Assert.Throws<InvalidOperationException>(() =>
            sut.Show(new FlourishNotification("same", "Again", "Body"))
        );
        Assert.Throws<ArgumentException>(() =>
            sut.Show(new FlourishNotification("", "Title", "Body"))
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.Show(
                new FlourishNotification(
                    "duration",
                    "Title",
                    "Body",
                    Duration: TimeSpan.Zero
                )
            )
        );
        Assert.Throws<ArgumentException>(() =>
            handle.Update(new FlourishNotification("different", "Title", "Body"))
        );
    }

    [Fact]
    public async Task NotificationService_UpsertReplacesExpirationTimer()
    {
        using var sut = CreateNotificationService();
        var expired = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        sut.NotificationsChanged += (_, args) =>
        {
            if (args.Notifications.Count == 0)
            {
                expired.TrySetResult();
            }
        };
        sut.Show(
            new FlourishNotification(
                "timed",
                "Title",
                "First",
                Duration: TimeSpan.FromMilliseconds(80)
            )
        );
        await Task.Delay(30);
        sut.Upsert(
            new FlourishNotification(
                "timed",
                "Title",
                "Replacement",
                Duration: TimeSpan.FromMilliseconds(180)
            )
        );

        await Task.Delay(90);
        Assert.Single(sut.ActiveNotifications);
        Assert.Equal("Replacement", sut.ActiveNotifications[0].Notification.Message);
        await expired.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Empty(sut.ActiveNotifications);
    }

    [Fact]
    public async Task TrayService_UsesClosePipelineWithTrayReason()
    {
        var options = new FlourishShellOptions();
        var close = new WindowCloseService(options, new Mock<IServiceProvider>().Object);
        var requested = new TaskCompletionSource<WindowCloseRequestReason>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        close.Attach((reason, _) =>
        {
            requested.SetResult(reason);
            return ValueTask.FromResult(true);
        });
        using var sut = CreateTrayService(options, close);
        ITrayService tray = sut;

        tray.SetEnabled(true);
        tray.SetToolTip(new string('x', 80));
        tray.Exit();

        Assert.Equal(
            WindowCloseRequestReason.Tray,
            await requested.Task.WaitAsync(TimeSpan.FromSeconds(2))
        );
        Assert.True(tray.Current.IsEnabled);
        Assert.False(tray.Current.IsIconVisible);
        Assert.True(tray.Current.IsExitRequested);
        Assert.Equal(63, tray.Current.ToolTipText.Length);
    }

    [Fact]
    public async Task TrayService_CanceledGuardResetsExitRequest()
    {
        var options = new FlourishShellOptions();
        var close = new WindowCloseService(options, new Mock<IServiceProvider>().Object);
        close.Attach((_, _) => ValueTask.FromResult(true));
        using var guard = close.RegisterGuard(
            "cancel",
            (_, _) => ValueTask.FromResult(WindowCloseDecision.Cancel)
        );
        using var sut = CreateTrayService(options, close);
        var reset = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var observedExit = false;
        sut.StateChanged += (_, args) =>
        {
            observedExit |= args.State.IsExitRequested;
            if (observedExit && !args.State.IsExitRequested)
            {
                reset.TrySetResult();
            }
        };

        ((ITrayService)sut).Exit();

        await reset.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.False(sut.Current.IsExitRequested);
    }

    [Fact]
    public void TrayService_DisablingMinimizeToTrayRestoresPromptCloseBehavior()
    {
        var options = new FlourishShellOptions();
        var close = new WindowCloseService(options, new Mock<IServiceProvider>().Object);
        close.SetBehavior(WindowCloseBehavior.MinimizeToTray);
        using var sut = CreateTrayService(options, close);
        var states = new List<FlourishTrayState>();
        sut.StateChanged += (_, args) => states.Add(args.State);

        ((ITrayService)sut).SetEnabled(false);

        Assert.Equal(WindowCloseBehavior.Prompt, close.Behavior);
        Assert.False(options.IsTrayExitEnabled);
        Assert.False(sut.Current.IsEnabled);
        Assert.Collection(states, state => Assert.False(state.IsEnabled));
    }

    [Fact]
    public async Task MessageService_PreCanceledAsyncCallsReturnCanceledTasks()
    {
        var localization = new FlourishLocalizationService(new FlourishDataOptions());
        IMessageService sut = new MessageService(localization);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var standardTask = sut.ShowAsync("Never shown", cancellationToken: cancellation.Token);
        var customTask = sut.ShowAsync(
            "Never shown",
            "Test",
            [],
            cancellationToken: cancellation.Token
        );

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => standardTask);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => customTask);
    }

    private static NotificationService CreateNotificationService() =>
        new(NullLogger<NotificationService>.Instance);

    private static TrayIconService CreateTrayService(
        FlourishShellOptions options,
        WindowCloseService close
    ) =>
        new(
            options,
            new FlourishLocalizationService(new FlourishDataOptions()),
            close,
            NullLogger<TrayIconService>.Instance
        );
}
