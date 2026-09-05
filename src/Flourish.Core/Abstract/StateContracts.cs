using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Owns the lifetime of a runtime registration.</summary>
public interface IRegistration : IDisposable
{
    /// <summary>Gets whether the registration is still active.</summary>
    bool IsRegistered { get; }
}

/// <summary>Describes the kind of mutation that caused a runtime collection to change.</summary>
public enum CollectionChangeKind
{
    /// <summary>The complete state was replaced.</summary>
    Reset,

    /// <summary>An entry was added.</summary>
    Added,

    /// <summary>An existing entry was updated.</summary>
    Updated,

    /// <summary>An entry was removed.</summary>
    Removed,

    /// <summary>One or more entries were reordered.</summary>
    Moved,
}

/// <summary>Provides the current immutable state after a state change.</summary>
/// <typeparam name="TState">The state snapshot type.</typeparam>
public sealed class StateChangedEventArgs<TState>(TState current) : EventArgs
{
    /// <summary>Gets the state after the change.</summary>
    public TState Current { get; } = current;
}

/// <summary>Provides the immutable states before and after a state change.</summary>
/// <typeparam name="TState">The state snapshot type.</typeparam>
public sealed class StateTransitionEventArgs<TState>(TState previous, TState current)
    : EventArgs
{
    /// <summary>Gets the state before the change.</summary>
    public TState Previous { get; } = previous;

    /// <summary>Gets the state after the change.</summary>
    public TState Current { get; } = current;
}
