using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Flourish.Services;

internal sealed class NotificationService(ILogger<NotificationService> logger)
    : INotificationService,
        IDisposable
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, NotificationEntry> entries = new(StringComparer.Ordinal);
    private long version;
    private bool isDisposed;

    public event EventHandler<FlourishNotificationsChangedEventArgs>? NotificationsChanged;

    internal long CurrentVersion
    {
        get
        {
            lock (gate)
            {
                return version;
            }
        }
    }

    public IReadOnlyList<FlourishNotificationInfo> ActiveNotifications
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshotLocked();
            }
        }
    }

    public FlourishNotificationHandle Show(FlourishNotification notification)
    {
        Validate(notification);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (entries.ContainsKey(notification.Id))
            {
                throw new InvalidOperationException(
                    $"Notification '{notification.Id}' is already active."
                );
            }

            AddOrReplaceLocked(notification);
        }

        NotifyChanged();
        return CreateHandle(notification.Id);
    }

    public FlourishNotificationHandle Upsert(FlourishNotification notification)
    {
        Validate(notification);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            AddOrReplaceLocked(notification);
        }

        NotifyChanged();
        return CreateHandle(notification.Id);
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

            version++;
        }

        removed.Cancellation.Cancel();
        removed.Cancellation.Dispose();
        NotifyChanged();
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
            version++;
        }

        foreach (var entry in removed)
        {
            entry.Cancellation.Cancel();
            entry.Cancellation.Dispose();
        }

        NotifyChanged();
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
        }

        foreach (var entry in removed)
        {
            entry.Cancellation.Cancel();
            entry.Cancellation.Dispose();
        }
    }

    private void AddOrReplaceLocked(FlourishNotification notification)
    {
        if (entries.Remove(notification.Id, out var previous))
        {
            previous.Cancellation.Cancel();
            previous.Cancellation.Dispose();
        }

        var cancellation = new CancellationTokenSource();
        var entry = new NotificationEntry(
            notification,
            DateTimeOffset.UtcNow,
            ++version,
            cancellation
        );
        entries.Add(notification.Id, entry);

        if (notification.Duration is { } duration)
        {
            _ = DismissAfterAsync(notification.Id, entry.Version, duration, cancellation.Token);
        }
    }

    private async Task DismissAfterAsync(
        string id,
        long expectedVersion,
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
                    entries.TryGetValue(id, out var current)
                    && current.Version == expectedVersion
                    && entries.Remove(id)
                )
                {
                    removed = current;
                    version++;
                }
            }

            if (removed is not null)
            {
                removed.Cancellation.Dispose();
                NotifyChanged();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception error)
        {
            logger.LogError(error, "Failed to expire notification {NotificationId}.", id);
        }
    }

    private void Update(FlourishNotification notification)
    {
        Validate(notification);
        lock (gate)
        {
            if (!entries.ContainsKey(notification.Id))
            {
                return;
            }

            AddOrReplaceLocked(notification);
        }

        NotifyChanged();
    }

    private FlourishNotificationHandle CreateHandle(string id)
    {
        return new FlourishNotificationHandle(id, Update, () => Dismiss(id));
    }

    private void NotifyChanged()
    {
        IReadOnlyList<FlourishNotificationInfo> notifications;
        long currentVersion;
        lock (gate)
        {
            notifications = CreateSnapshotLocked();
            currentVersion = version;
        }

        NotificationsChanged?.Invoke(
            this,
            new FlourishNotificationsChangedEventArgs(notifications, currentVersion)
        );
    }

    private IReadOnlyList<FlourishNotificationInfo> CreateSnapshotLocked()
    {
        return entries
            .Values.OrderBy(entry => entry.CreatedAt)
            .Select(entry => new FlourishNotificationInfo(
                entry.Notification,
                entry.CreatedAt,
                entry.Version
            ))
            .ToArray();
    }

    private static void Validate(FlourishNotification notification)
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
        FlourishNotification Notification,
        DateTimeOffset CreatedAt,
        long Version,
        CancellationTokenSource Cancellation
    );
}
