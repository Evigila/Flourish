using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the active notification collection and its monotonically increasing version.
/// </summary>
/// <param name="Notifications">The immutable, creation-ordered active notifications.</param>
/// <param name="Version">The version represented by this state.</param>
public sealed record FlourishNotificationState(
    IReadOnlyList<FlourishNotificationInfo> Notifications,
    long Version
);
