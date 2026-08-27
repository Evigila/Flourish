using System.Threading;

using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Updates or dismisses a runtime Flourish notification.
/// </summary>
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

    /// <summary>
    /// Gets the notification identifier controlled by this handle.
    /// </summary>
    public string Id { get; } = string.IsNullOrWhiteSpace(id)
        ? throw new ArgumentException("A notification handle requires an ID.", nameof(id))
        : id;

    /// <summary>
    /// Replaces the active notification content while preserving its identifier.
    /// </summary>
    /// <param name="notification">The replacement notification.</param>
    /// <remarks>The call has no effect after this handle is dismissed or when its notification is no longer active.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="notification"/> uses a different identifier.</exception>
    public void Update(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (!string.Equals(notification.Id, Id, StringComparison.Ordinal))
        {
            throw new ArgumentException("A notification handle cannot change its notification ID.", nameof(notification));
        }

        Volatile.Read(ref update)?.Invoke(notification);
    }

    /// <summary>
    /// Requests dismissal and deactivates this handle.
    /// </summary>
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

    /// <summary>
    /// Requests dismissal and deactivates this handle.
    /// </summary>
    public void Dispose() => Dismiss();
}
