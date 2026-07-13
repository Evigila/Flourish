namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Publishes non-modal notifications to the Flourish shell.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Gets an immutable, creation-ordered snapshot of active notifications.
    /// </summary>
    IReadOnlyList<FlourishNotificationInfo> ActiveNotifications { get; }

    /// <summary>
    /// Occurs synchronously after the active notification collection changes.
    /// </summary>
    /// <remarks>The event may be raised on a non-UI thread.</remarks>
    event EventHandler<FlourishNotificationsChangedEventArgs>? NotificationsChanged;

    /// <summary>
    /// Publishes a new notification with a unique identifier.
    /// </summary>
    /// <param name="notification">The notification definition.</param>
    /// <returns>A handle that can update or dismiss the notification.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Required notification content is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The severity or duration is invalid.</exception>
    /// <exception cref="InvalidOperationException">A notification with the same identifier is already active.</exception>
    /// <exception cref="ObjectDisposedException">The service has been disposed.</exception>
    FlourishNotificationHandle Show(FlourishNotification notification);

    /// <summary>
    /// Publishes a notification or atomically replaces the active notification with the same identifier.
    /// </summary>
    /// <param name="notification">The notification definition.</param>
    /// <returns>A handle that can update or dismiss the resulting notification.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Required notification content is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The severity or duration is invalid.</exception>
    /// <exception cref="ObjectDisposedException">The service has been disposed.</exception>
    FlourishNotificationHandle Upsert(FlourishNotification notification);

    /// <summary>
    /// Dismisses an active notification by identifier.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <returns><see langword="true"/> when a notification was removed; otherwise, <see langword="false"/>.</returns>
    bool Dismiss(string id);

    /// <summary>
    /// Dismisses every active notification and cancels their expiration timers.
    /// </summary>
    void DismissAll();
}
