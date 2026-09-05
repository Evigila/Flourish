using System;
using System.Collections.Generic;
using System.Threading;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Specifies the visual importance of a runtime notification.</summary>
public enum NotificationSeverity
{
    /// <summary>Neutral information that does not require special attention.</summary>
    Information,

    /// <summary>Confirmation that an operation completed successfully.</summary>
    Success,

    /// <summary>A recoverable condition that may require attention.</summary>
    Warning,

    /// <summary>A failed operation or condition requiring attention.</summary>
    Error,
}

/// <summary>Defines a notification shown by the application shell.</summary>
/// <param name="Id">A non-empty identifier used for updates and dismissal.</param>
/// <param name="Title">The non-empty notification heading.</param>
/// <param name="Message">The non-empty notification body.</param>
/// <param name="Severity">The visual severity.</param>
/// <param name="IconGlyph">An optional icon glyph understood by the shell theme.</param>
/// <param name="CommandKey">An optional runtime command invoked when the notification is activated.</param>
/// <param name="Duration">An optional positive lifetime after which the notification is dismissed automatically.</param>
public sealed record Notification(
    string Id,
    string Title,
    string Message,
    NotificationSeverity Severity = NotificationSeverity.Information,
    string? IconGlyph = null,
    string? CommandKey = null,
    TimeSpan? Duration = null
);

/// <summary>Represents an immutable active-notification snapshot.</summary>
/// <param name="Notification">The active notification definition.</param>
/// <param name="CreatedAt">The UTC instant at which this notification version became active.</param>
/// <param name="Version">The monotonically increasing service version assigned to this notification version.</param>
public sealed record ActiveNotificationInfo(
    Notification Notification,
    DateTimeOffset CreatedAt,
    long Version
);

/// <summary>Represents the active notification collection and its monotonically increasing version.</summary>
/// <param name="Notifications">The immutable, creation-ordered active notifications.</param>
/// <param name="Version">The version represented by this state.</param>
public sealed record NotificationState(
    IReadOnlyList<ActiveNotificationInfo> Notifications,
    long Version
);

/// <summary>Publishes non-modal notifications to the application shell.</summary>
public interface INotificationService
{
    /// <summary>Gets the current immutable notification state.</summary>
    NotificationState Current { get; }

    /// <summary>Occurs synchronously after the active notification collection changes.</summary>
    /// <remarks>The event may be raised on a non-UI thread.</remarks>
    event EventHandler<StateChangedEventArgs<NotificationState>>? Changed;

    /// <summary>Publishes a new notification with a unique identifier.</summary>
    /// <param name="notification">The notification definition.</param>
    /// <returns>A handle that can update or dismiss the notification.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Required notification content is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The severity or duration is invalid.</exception>
    /// <exception cref="InvalidOperationException">A notification with the same identifier is already active.</exception>
    /// <exception cref="ObjectDisposedException">The service has been disposed.</exception>
    NotificationHandle Show(Notification notification);

    /// <summary>Publishes a notification or atomically replaces the active notification with the same identifier.</summary>
    /// <param name="notification">The notification definition.</param>
    /// <returns>A handle that can update or dismiss the resulting notification.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Required notification content is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The severity or duration is invalid.</exception>
    /// <exception cref="ObjectDisposedException">The service has been disposed.</exception>
    NotificationHandle Upsert(Notification notification);

    /// <summary>Dismisses an active notification by identifier.</summary>
    /// <param name="id">The notification identifier.</param>
    /// <returns><see langword="true"/> when a notification was removed; otherwise, <see langword="false"/>.</returns>
    bool Dismiss(string id);

    /// <summary>Dismisses every active notification and cancels their expiration timers.</summary>
    void DismissAll();
}

/// <summary>Updates or dismisses a runtime notification.</summary>
/// <param name="id">The notification identifier controlled by this handle.</param>
/// <param name="update">The callback used to update an active notification.</param>
/// <param name="dismiss">The callback used to request dismissal.</param>
public sealed class NotificationHandle(
    string id,
    Action<Notification> update,
    Action dismiss
) : IDisposable
{
    private Action<Notification>? update = update
        ?? throw new ArgumentNullException(nameof(update));
    private Action? dismiss = dismiss ?? throw new ArgumentNullException(nameof(dismiss));

    /// <summary>Gets the notification identifier controlled by this handle.</summary>
    public string Id { get; } = string.IsNullOrWhiteSpace(id)
        ? throw new ArgumentException("A notification handle requires an ID.", nameof(id))
        : id;

    /// <summary>Replaces the active notification content while preserving its identifier.</summary>
    /// <param name="notification">The replacement notification.</param>
    /// <remarks>The call has no effect after this handle is dismissed or when its notification is no longer active.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="notification"/> uses a different identifier.</exception>
    public void Update(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (!string.Equals(notification.Id, Id, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A notification handle cannot change its notification ID.",
                nameof(notification)
            );
        }

        Volatile.Read(ref update)?.Invoke(notification);
    }

    /// <summary>Requests dismissal and deactivates this handle.</summary>
    /// <returns>
    /// <see langword="true"/> when this was the handle's first dismissal request; otherwise,
    /// <see langword="false"/>. The notification may already have expired or been dismissed elsewhere.
    /// </returns>
    public bool Dismiss()
    {
        var callback = Interlocked.Exchange(ref dismiss, null);
        Interlocked.Exchange(ref update, null);
        if (callback is null)
        {
            return false;
        }

        callback();
        return true;
    }

    /// <summary>Requests dismissal and deactivates this handle.</summary>
    public void Dispose() => Dismiss();
}
