using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents an immutable active-notification snapshot.
/// </summary>
/// <param name="Notification">The active notification definition.</param>
/// <param name="CreatedAt">The UTC instant at which this notification version became active.</param>
/// <param name="Version">The monotonically increasing service version assigned to this notification version.</param>
public sealed record ActiveNotificationInfo(
    Notification Notification,
    DateTimeOffset CreatedAt,
    long Version
);
