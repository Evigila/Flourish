using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides an immutable snapshot of a runtime command registration.
/// </summary>
public sealed class CommandRegistrationInfo
{
    internal CommandRegistrationInfo(Guid id, string commandKey, int priority)
    {
        Id = id;
        CommandKey = commandKey;
        Priority = priority;
    }

    /// <summary>
    /// Gets the unique registration identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the stable command key.
    /// </summary>
    public string CommandKey { get; }

    /// <summary>
    /// Gets the handler priority.
    /// </summary>
    public int Priority { get; }
}
