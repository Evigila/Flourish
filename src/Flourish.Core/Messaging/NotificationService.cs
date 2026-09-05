using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Flourish.Messaging;

internal sealed class NotificationService(ILogger<NotificationService> logger)
    : INotificationService,
        IDisposable
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, NotificationEntry> entries = new(StringComparer.Ordinal);
    private readonly Queue<NotificationState> pendingStates = new();
    private NotificationState current = new(
        Array.Empty<ActiveNotificationInfo>(),
        0
    );
    private long version;
    private bool isDisposed;
    private bool isPublishing;

    public event EventHandler<StateChangedEventArgs<NotificationState>>? Changed;

    public NotificationState Current => Volatile.Read(ref current);

    public NotificationHandle Show(Notification notification)
    {
        Validate(notification);

        NotificationEntry entry;
        NotificationEntry? previous;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (entries.ContainsKey(notification.Id))
            {
                throw new InvalidOperationException(
                    $"Notification '{notification.Id}' is already active."
                );
            }

            (entry, previous) = AddOrReplaceLocked(
                notification,
                new NotificationLease()
            );
        }

        CancelAndDispose(previous);
        PublishQueuedChanges();
        return CreateHandle(entry);
    }

    public NotificationHandle Upsert(Notification notification)
    {
        Validate(notification);

        NotificationEntry entry;
        NotificationEntry? previous;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            (entry, previous) = AddOrReplaceLocked(
                notification,
                new NotificationLease()
            );
        }

        CancelAndDispose(previous);
        PublishQueuedChanges();
        return CreateHandle(entry);
    }

    public bool Dismiss(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        NotificationEntry? removed;
        lock (gate)
        {
            if (!entries.Remove(id, out removed))
            {
                return false;
            }

            removed.Lease.Deactivate();
            CommitStateLocked();
        }

        CancelAndDispose(removed);
        PublishQueuedChanges();
        return true;
    }

    public void DismissAll()
    {
        NotificationEntry[] removed;
        lock (gate)
        {
            if (entries.Count == 0)
            {
                return;
            }

            removed = entries.Values.ToArray();
            entries.Clear();
            foreach (var entry in removed)
            {
                entry.Lease.Deactivate();
            }

            CommitStateLocked();
        }

        CancelAndDispose(removed);
        PublishQueuedChanges();
    }

    public void Dispose()
    {
        NotificationEntry[] removed;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            removed = entries.Values.ToArray();
            entries.Clear();
            foreach (var entry in removed)
            {
                entry.Lease.Deactivate();
            }

            version++;
            Volatile.Write(
                ref current,
                new NotificationState(Array.Empty<ActiveNotificationInfo>(), version)
            );
            pendingStates.Clear();
            Changed = null;
        }

        CancelAndDispose(removed);
    }

    private (NotificationEntry Entry, NotificationEntry? Previous) AddOrReplaceLocked(
        Notification notification,
        NotificationLease lease
    )
    {
        NotificationEntry? previous = null;
        if (entries.Remove(notification.Id, out previous))
        {
            if (!ReferenceEquals(previous.Lease, lease))
            {
                previous.Lease.Deactivate();
            }
        }

        var cancellation = new CancellationTokenSource();
        var entry = new NotificationEntry(
            notification,
            DateTimeOffset.UtcNow,
            ++version,
            cancellation,
            lease
        );
        entries.Add(notification.Id, entry);
        EnqueueCurrentStateLocked();

        if (notification.Duration is { } duration)
        {
            _ = DismissAfterAsync(
                notification.Id,
                entry.Version,
                lease,
                duration,
                cancellation.Token
            );
        }

        return (entry, previous);
    }

    private async Task DismissAfterAsync(
        string id,
        long expectedVersion,
        NotificationLease expectedLease,
        TimeSpan duration,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await Task.Delay(duration, cancellationToken).ConfigureAwait(false);

            NotificationEntry? removed = null;
            lock (gate)
            {
                if (
                    entries.TryGetValue(id, out var active)
                    && active.Version == expectedVersion
                    && ReferenceEquals(active.Lease, expectedLease)
                    && entries.Remove(id)
                )
                {
                    removed = active;
                    removed.Lease.Deactivate();
                    CommitStateLocked();
                }
            }

            if (removed is not null)
            {
                removed.Cancellation.Dispose();
                PublishQueuedChanges();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception error)
        {
            LogError(error, id);
        }
    }

    private void Update(Notification notification, NotificationLease lease)
    {
        Validate(notification);

        NotificationEntry? previous;
        lock (gate)
        {
            if (
                isDisposed
                || !lease.IsActive
                || !entries.TryGetValue(notification.Id, out var active)
                || !ReferenceEquals(active.Lease, lease)
            )
            {
                return;
            }

            (_, previous) = AddOrReplaceLocked(notification, lease);
        }

        CancelAndDispose(previous);
        PublishQueuedChanges();
    }

    private void Dismiss(string id, NotificationLease lease)
    {
        lease.Deactivate();

        NotificationEntry? removed = null;
        lock (gate)
        {
            if (
                entries.TryGetValue(id, out var active)
                && ReferenceEquals(active.Lease, lease)
                && entries.Remove(id)
            )
            {
                removed = active;
                CommitStateLocked();
            }
        }

        if (removed is null)
        {
            return;
        }

        CancelAndDispose(removed);
        PublishQueuedChanges();
    }

    private NotificationHandle CreateHandle(NotificationEntry entry)
    {
        return new NotificationHandle(
            entry.Notification.Id,
            notification => Update(notification, entry.Lease),
            () => Dismiss(entry.Notification.Id, entry.Lease)
        );
    }

    private void CommitStateLocked()
    {
        version++;
        EnqueueCurrentStateLocked();
    }

    private void EnqueueCurrentStateLocked()
    {
        var state = new NotificationState(CreateSnapshotLocked(), version);
        Volatile.Write(ref current, state);
        pendingStates.Enqueue(state);
    }

    private void PublishQueuedChanges()
    {
        lock (gate)
        {
            if (isDisposed || isPublishing || pendingStates.Count == 0)
            {
                return;
            }

            isPublishing = true;
        }

        while (true)
        {
            NotificationState state;
            EventHandler<StateChangedEventArgs<NotificationState>>? handlers;
            lock (gate)
            {
                if (isDisposed || pendingStates.Count == 0)
                {
                    isPublishing = false;
                    return;
                }

                state = pendingStates.Dequeue();
                handlers = Changed;
            }

            if (handlers is null)
            {
                continue;
            }

            var eventArgs = new StateChangedEventArgs<NotificationState>(state);
            foreach (
                EventHandler<StateChangedEventArgs<NotificationState>> handler in handlers.GetInvocationList()
            )
            {
                try
                {
                    handler(this, eventArgs);
                }
                catch (Exception error)
                {
                    LogError(error, notificationId: null);
                }
            }
        }
    }

    private IReadOnlyList<ActiveNotificationInfo> CreateSnapshotLocked()
    {
        var snapshot = entries
            .Values.OrderBy(entry => entry.CreatedAt)
            .ThenBy(entry => entry.Version)
            .Select(entry => new ActiveNotificationInfo(
                entry.Notification,
                entry.CreatedAt,
                entry.Version
            ))
            .ToArray();
        return new ReadOnlyCollection<ActiveNotificationInfo>(snapshot);
    }

    private void LogError(Exception error, string? notificationId)
    {
        try
        {
            if (notificationId is null)
            {
                logger.LogError(error, "A notification state listener failed.");
            }
            else
            {
                logger.LogError(
                    error,
                    "Failed to expire notification {NotificationId}.",
                    notificationId
                );
            }
        }
        catch (Exception loggingError)
        {
            Debug.WriteLine($"Notification diagnostic logging failed: {loggingError}");
        }
    }

    private static void CancelAndDispose(NotificationEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        entry.Cancellation.Cancel();
        entry.Cancellation.Dispose();
    }

    private static void CancelAndDispose(IEnumerable<NotificationEntry> entries)
    {
        foreach (var entry in entries)
        {
            CancelAndDispose(entry);
        }
    }

    private static void Validate(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (string.IsNullOrWhiteSpace(notification.Id))
        {
            throw new ArgumentException("A notification requires an ID.", nameof(notification));
        }

        if (string.IsNullOrWhiteSpace(notification.Title))
        {
            throw new ArgumentException("A notification requires a title.", nameof(notification));
        }

        if (string.IsNullOrWhiteSpace(notification.Message))
        {
            throw new ArgumentException("A notification requires a message.", nameof(notification));
        }

        if (!Enum.IsDefined(notification.Severity))
        {
            throw new ArgumentOutOfRangeException(
                nameof(notification),
                "Unknown notification severity."
            );
        }

        if (notification.Duration is { } duration && duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(notification),
                "Notification duration must be positive."
            );
        }
    }

    private sealed record NotificationEntry(
        Notification Notification,
        DateTimeOffset CreatedAt,
        long Version,
        CancellationTokenSource Cancellation,
        NotificationLease Lease
    );

    private sealed class NotificationLease
    {
        private int isActive = 1;

        public bool IsActive => Volatile.Read(ref isActive) != 0;

        public void Deactivate()
        {
            Interlocked.Exchange(ref isActive, 0);
        }
    }
}
