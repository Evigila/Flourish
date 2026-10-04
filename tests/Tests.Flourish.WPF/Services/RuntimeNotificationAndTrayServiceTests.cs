using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Messaging;
using ArkheideSystem.Flourish.Views.Windows;
using ArkheideSystem.Flourish.Windowing;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ArkheideSystem.Tests.Flourish.WPF.Services;

public sealed class RuntimeNotificationAndTrayServiceTests
{
    [Fact]
    public void NotificationService_ShowsUpdatesUpsertsAndDismissesSnapshots()
    {
        using var sut = CreateNotificationService();
        var snapshots = new List<IReadOnlyList<ActiveNotificationInfo>>();
        var versions = new List<long>();
        sut.Changed += (_, args) =>
        {
            snapshots.Add(args.Current.Notifications);
            versions.Add(args.Current.Version);
        };
        using var first = sut.Show(new Notification("one", "One", "Initial"));
        using var second = sut.Show(new Notification("two", "Two", "Second"));

        first.Update(
            new Notification(
                "one",
                "One updated",
                "Updated",
                NotificationSeverity.Success
            )
        );
        using var replacement = sut.Upsert(
            new Notification("two", "Two updated", "Replacement")
        );

        Assert.Equal(["one", "two"], sut.Current.Notifications.Select(x => x.Notification.Id));
        Assert.Equal("One updated", sut.Current.Notifications[0].Notification.Title);
        Assert.Equal("Two updated", sut.Current.Notifications[1].Notification.Title);
        Assert.True(sut.Dismiss("one"));
        Assert.False(sut.Dismiss("missing"));
        sut.DismissAll();
        Assert.Empty(sut.Current.Notifications);
        Assert.Equal(6, snapshots.Count);
        Assert.Equal([1, 2, 3, 4, 5, 6], versions);
    }

    [Fact]
    public void NotificationService_RejectsDuplicatesAndInvalidDefinitions()
    {
        using var sut = CreateNotificationService();
        using var handle = sut.Show(new Notification("same", "Title", "Body"));

        Assert.Throws<InvalidOperationException>(() =>
            sut.Show(new Notification("same", "Again", "Body"))
        );
        Assert.Throws<ArgumentException>(() =>
            sut.Show(new Notification("", "Title", "Body"))
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.Show(new Notification("duration", "Title", "Body", Duration: TimeSpan.Zero))
        );
        Assert.Throws<ArgumentException>(() =>
            handle.Update(new Notification("different", "Title", "Body"))
        );
    }

    [Fact]
    public async Task NotificationService_UpsertReplacesExpirationTimer()
    {
        using var sut = CreateNotificationService();
        var expired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        sut.Changed += (_, args) =>
        {
            if (args.Current.Notifications.Count == 0)
            {
                expired.TrySetResult();
            }
        };
        sut.Show(
            new Notification(
                "timed",
                "Title",
                "First",
                Duration: TimeSpan.FromMilliseconds(80)
            )
        );
        await Task.Delay(30);
        sut.Upsert(
            new Notification(
                "timed",
                "Title",
                "Replacement",
                Duration: TimeSpan.FromMilliseconds(180)
            )
        );

        await Task.Delay(90);
        Assert.Single(sut.Current.Notifications);
        Assert.Equal("Replacement", sut.Current.Notifications[0].Notification.Message);
        await expired.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Empty(sut.Current.Notifications);
    }

    [Fact]
    public async Task TrayService_UsesClosePipelineWithTrayReason()
    {
        var options = new WindowOptions();
        var close = new WindowCloseService(new Mock<IServiceProvider>().Object);
        var requested = new TaskCompletionSource<WindowCloseRequestReason>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        close.Attach(
            (reason, _) =>
            {
                requested.SetResult(reason);
                return ValueTask.FromResult(true);
            }
        );
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
        var options = new WindowOptions();
        var close = new WindowCloseService(new Mock<IServiceProvider>().Object);
        close.Attach((_, _) => ValueTask.FromResult(true));
        using var guard = close.RegisterGuard(
            "cancel",
            (_, _) => ValueTask.FromResult(WindowCloseDecision.Cancel)
        );
        using var sut = CreateTrayService(options, close);
        var reset = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var observedExit = false;
        sut.Changed += (_, args) =>
        {
            observedExit |= args.Current.IsExitRequested;
            if (observedExit && !args.Current.IsExitRequested)
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
        var options = new WindowOptions();
        var close = new WindowCloseService(new Mock<IServiceProvider>().Object);
        close.SetBehavior(WindowCloseBehavior.MinimizeToTray);
        using var sut = CreateTrayService(options, close);
        var states = new List<TrayState>();
        sut.Changed += (_, args) => states.Add(args.Current);

        ((ITrayService)sut).SetEnabled(false);

        Assert.Equal(WindowCloseBehavior.Prompt, close.Current.Behavior);
        Assert.False(options.IsTrayExitEnabled);
        Assert.False(sut.Current.IsEnabled);
        Assert.Collection(states, state => Assert.False(state.IsEnabled));
    }

    [Fact]
    public async Task MessageService_PreCanceledAsyncCallsReturnCanceledTasks()
    {
        var localization = new LocalizationService(new ApplicationDataOptions());
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
        WindowOptions options,
        WindowCloseService close
    ) =>
        new(
            options,
            new LocalizationService(new ApplicationDataOptions()),
            close,
            NullLogger<TrayIconService>.Instance
        );
}
