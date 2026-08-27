using System;
using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides an immutable command registry snapshot after a structural change.
/// </summary>
public sealed class CommandRegistryChangedEventArgs : EventArgs
{
    internal CommandRegistryChangedEventArgs(
        long version,
        CollectionChangeKind changeKind,
        string commandKey,
        IReadOnlyList<CommandRegistrationInfo> current
    )
    {
        Version = version;
        ChangeKind = changeKind;
        CommandKey = commandKey;
        Current = current;
    }

    /// <summary>
    /// Gets the monotonically increasing registry version.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// Gets the kind of structural change.
    /// </summary>
    public CollectionChangeKind ChangeKind { get; }

    /// <summary>
    /// Gets the command key affected by the change.
    /// </summary>
    public string CommandKey { get; }

    /// <summary>
    /// Gets all active runtime registrations in registration order.
    /// </summary>
    public IReadOnlyList<CommandRegistrationInfo> Current { get; }
}
