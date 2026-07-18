namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides the latest active notification collection.
/// </summary>
/// <param name="notifications">An immutable, creation-ordered snapshot of active notifications.</param>
/// <param name="version">The collection version represented by the snapshot.</param>
public sealed class FlourishNotificationsChangedEventArgs(
    IReadOnlyList<FlourishNotificationInfo> notifications,
    long version = 0
) : EventArgs
{
    /// <summary>
    /// Gets an immutable, creation-ordered snapshot of active notifications.
    /// </summary>
    public IReadOnlyList<FlourishNotificationInfo> Notifications { get; } = notifications;

    /// <summary>Gets the collection version represented by this snapshot.</summary>
    public long Version { get; } = version;
}
