using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Messaging;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Messaging;

public sealed class NotificationServiceTests
{
    [Fact]
    public void ShowUpdateUpsertAndDismiss_PublishVersionedImmutableSnapshots()
    {
        using var sut = CreateService();
        var snapshots = new List<NotificationState>();
        sut.Changed += (_, args) => snapshots.Add(args.Current);
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

        Assert.Equal(["one", "two"], sut.Current.Notifications.Select(ItemId));
        Assert.Equal("One updated", sut.Current.Notifications[0].Notification.Title);
        Assert.Equal("Two updated", sut.Current.Notifications[1].Notification.Title);
        var mutableView = Assert.IsAssignableFrom<IList<ActiveNotificationInfo>>(
            sut.Current.Notifications
        );
        Assert.Throws<NotSupportedException>(() => mutableView.Clear());

        Assert.True(sut.Dismiss("one"));
        Assert.False(sut.Dismiss("missing"));
        sut.DismissAll();

        Assert.Empty(sut.Current.Notifications);
        Assert.Equal([1, 2, 3, 4, 5, 6], snapshots.Select(state => state.Version));
        Assert.Equal(6, sut.Current.Version);
    }

    [Fact]
    public void ShowAndHandleUpdate_RejectInvalidDefinitions()
    {
        using var sut = CreateService();
        using var handle = sut.Show(new Notification("same", "Title", "Body"));

        Assert.Throws<ArgumentNullException>(() => sut.Show(null!));
        Assert.Throws<InvalidOperationException>(() =>
            sut.Show(new Notification("same", "Again", "Body"))
        );
        Assert.Throws<ArgumentException>(() =>
            sut.Show(new Notification("", "Title", "Body"))
        );
        Assert.Throws<ArgumentException>(() =>
            sut.Show(new Notification("title", " ", "Body"))
        );
        Assert.Throws<ArgumentException>(() =>
            sut.Show(new Notification("message", "Title", " "))
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.Show(
                new Notification(
                    "severity",
                    "Title",
                    "Body",
                    (NotificationSeverity)99
                )
            )
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.Show(
                new Notification(
                    "duration",
                    "Title",
                    "Body",
                    Duration: TimeSpan.Zero
                )
            )
        );
        Assert.Throws<ArgumentException>(() =>
            handle.Update(new Notification("different", "Title", "Body"))
        );
    }

    [Fact]
    public async Task Upsert_ReplacesExpirationTimer()
    {
        using var sut = CreateService();
        var expired = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
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
                Duration: TimeSpan.FromMilliseconds(220)
            )
        );

        await Task.Delay(100);
        Assert.Single(sut.Current.Notifications);
        Assert.Equal("Replacement", sut.Current.Notifications[0].Notification.Message);

        await expired.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Empty(sut.Current.Notifications);
        Assert.Equal(3, sut.Current.Version);
    }

    [Fact]
    public void StaleHandle_CannotUpdateOrDismissUpsertedNotification()
    {
        using var sut = CreateService();
        var original = sut.Show(new Notification("shared", "Original", "First"));
        var replacement = sut.Upsert(
            new Notification("shared", "Replacement", "Second")
        );

        original.Update(new Notification("shared", "Stale", "Ignored"));
        Assert.True(original.Dismiss());
        Assert.False(original.Dismiss());

        var active = Assert.Single(sut.Current.Notifications);
        Assert.Equal("Replacement", active.Notification.Title);
        Assert.Equal(2, sut.Current.Version);

        replacement.Update(new Notification("shared", "Current", "Updated"));
        Assert.Equal("Current", Assert.Single(sut.Current.Notifications).Notification.Title);
        Assert.True(replacement.Dismiss());
        Assert.Empty(sut.Current.Notifications);
        Assert.Equal(4, sut.Current.Version);
    }

    [Fact]
    public async Task ExpiredHandle_CannotResurrectNotification()
    {
        using var sut = CreateService();
        var expired = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        sut.Changed += (_, args) =>
        {
            if (args.Current.Notifications.Count == 0)
            {
                expired.TrySetResult();
            }
        };
        var handle = sut.Show(
            new Notification(
                "short",
                "Title",
                "Body",
                Duration: TimeSpan.FromMilliseconds(40)
            )
        );

        await expired.Task.WaitAsync(TimeSpan.FromSeconds(2));
        handle.Update(new Notification("short", "Late", "Ignored"));

        Assert.Empty(sut.Current.Notifications);
        Assert.Equal(2, sut.Current.Version);
        Assert.True(handle.Dismiss());
        Assert.False(handle.Dismiss());
    }

    [Fact]
    public async Task ConcurrentMutations_PublishEveryVersionInOrder()
    {
        using var sut = CreateService();
        var observedVersions = new List<long>();
        sut.Changed += (_, args) => observedVersions.Add(args.Current.Version);

        var shows = Enumerable.Range(0, 64)
            .Select(index =>
                Task.Run(() =>
                    sut.Show(
                        new Notification(
                            $"item-{index}",
                            $"Item {index}",
                            "Body"
                        )
                    )
                )
            );
        var upserts = Enumerable.Range(0, 64)
            .Select(index =>
                Task.Run(() =>
                    sut.Upsert(
                        new Notification(
                            "shared",
                            $"Revision {index}",
                            "Body"
                        )
                    )
                )
            );

        await Task.WhenAll(shows.Concat(upserts));

        Assert.Equal(65, sut.Current.Notifications.Count);
        Assert.Equal(128, sut.Current.Version);
        Assert.Equal(
            Enumerable.Range(1, 128).Select(value => (long)value),
            observedVersions
        );

        sut.DismissAll();
        Assert.Empty(sut.Current.Notifications);
        Assert.Equal(129, sut.Current.Version);
    }

    [Fact]
    public async Task Dispose_CancelsTimersClearsStateAndRejectsNewNotifications()
    {
        var sut = CreateService();
        var handle = sut.Show(
            new Notification(
                "timed",
                "Title",
                "Body",
                Duration: TimeSpan.FromMilliseconds(80)
            )
        );
        var changesAfterDispose = 0;
        sut.Changed += (_, _) => Interlocked.Increment(ref changesAfterDispose);

        sut.Dispose();
        sut.Dispose();
        handle.Update(new Notification("timed", "Late", "Ignored"));
        Assert.True(handle.Dismiss());
        Assert.False(handle.Dismiss());
        Assert.False(sut.Dismiss("timed"));
        sut.DismissAll();

        Assert.Empty(sut.Current.Notifications);
        Assert.Equal(2, sut.Current.Version);
        Assert.Throws<ObjectDisposedException>(() =>
            sut.Show(new Notification("new", "Title", "Body"))
        );
        Assert.Throws<ObjectDisposedException>(() =>
            sut.Upsert(new Notification("new", "Title", "Body"))
        );

        await Task.Delay(150);
        Assert.Equal(0, Volatile.Read(ref changesAfterDispose));
        Assert.Equal(2, sut.Current.Version);
    }

    [Fact]
    public void ThrowingListener_DoesNotBlockLaterListenersOrMutations()
    {
        using var sut = CreateService();
        var observed = new List<long>();
        sut.Changed += (_, _) => throw new InvalidOperationException("listener failed");
        sut.Changed += (_, args) => observed.Add(args.Current.Version);

        sut.Show(new Notification("one", "One", "Body"));
        sut.Show(new Notification("two", "Two", "Body"));

        Assert.Equal([1L, 2L], observed);
        Assert.Equal(2, sut.Current.Notifications.Count);
    }

    private static string ItemId(ActiveNotificationInfo info) => info.Notification.Id;

    private static NotificationService CreateService() =>
        new(NullLogger<NotificationService>.Instance);
}
