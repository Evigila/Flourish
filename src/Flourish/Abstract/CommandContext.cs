using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a command invocation supplied to runtime command handlers.
/// </summary>
public sealed class CommandContext
{
    /// <summary>
    /// Initializes a command invocation context.
    /// </summary>
    /// <param name="commandKey">The stable command key being dispatched.</param>
    /// <param name="parameter">An optional parameter supplied by the caller.</param>
    /// <param name="source">The surface or component that requested the command.</param>
    public CommandContext(
        string commandKey,
        object? parameter = null,
        CommandSource source = CommandSource.Application
    )
    {
        if (string.IsNullOrWhiteSpace(commandKey))
        {
            throw new ArgumentException("Command key cannot be empty.", nameof(commandKey));
        }

        CommandKey = commandKey;
        Parameter = parameter;
        Source = source;
    }

    /// <summary>
    /// Gets the stable command key being dispatched.
    /// </summary>
    public string CommandKey { get; }

    /// <summary>
    /// Gets the optional parameter supplied by the caller.
    /// </summary>
    public object? Parameter { get; }

    /// <summary>
    /// Gets the surface or component that requested the command.
    /// </summary>
    public CommandSource Source { get; }
}
