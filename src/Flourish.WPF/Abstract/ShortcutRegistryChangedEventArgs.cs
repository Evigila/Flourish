using System;
using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides an immutable keyboard shortcut snapshot after a structural change.
/// </summary>
public sealed class ShortcutRegistryChangedEventArgs : EventArgs
{
    internal ShortcutRegistryChangedEventArgs(
        long version,
        CollectionChangeKind changeKind,
        ShortcutRegistrationInfo affectedShortcut,
        IReadOnlyList<ShortcutRegistrationInfo> current
    )
    {
        Version = version;
        ChangeKind = changeKind;
        AffectedShortcut = affectedShortcut;
        Current = current;
    }

    /// <summary>
    /// Gets the monotonically increasing shortcut registry version.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// Gets the kind of structural change.
    /// </summary>
    public CollectionChangeKind ChangeKind { get; }

    /// <summary>
    /// Gets the shortcut added or removed by the change.
    /// </summary>
    public ShortcutRegistrationInfo AffectedShortcut { get; }

    /// <summary>
    /// Gets all active shortcuts in registration order.
    /// </summary>
    public IReadOnlyList<ShortcutRegistrationInfo> Current { get; }
}
