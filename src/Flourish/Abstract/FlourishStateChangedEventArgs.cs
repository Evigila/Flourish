using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Provides the current immutable state after a Flourish state change.</summary>
/// <typeparam name="TState">The state snapshot type.</typeparam>
public sealed class FlourishStateChangedEventArgs<TState>(TState current) : EventArgs
{
    /// <summary>Gets the state after the change.</summary>
    public TState Current { get; } = current;
}

/// <summary>Provides the immutable states before and after a Flourish state change.</summary>
/// <typeparam name="TState">The state snapshot type.</typeparam>
public sealed class FlourishStateTransitionEventArgs<TState>(TState previous, TState current)
    : EventArgs
{
    /// <summary>Gets the state before the change.</summary>
    public TState Previous { get; } = previous;

    /// <summary>Gets the state after the change.</summary>
    public TState Current { get; } = current;
}
