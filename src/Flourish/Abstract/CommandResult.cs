using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the captured outcome of a command dispatch operation.
/// </summary>
public readonly record struct CommandResult
{
    private CommandResult(
        CommandExecutionStatus status,
        object? value = null,
        Exception? exception = null
    )
    {
        Status = status;
        Value = value;
        Exception = exception;
    }

    /// <summary>
    /// Gets the command execution status.
    /// </summary>
    public CommandExecutionStatus Status { get; }

    /// <summary>
    /// Gets an optional value returned by a successful command.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the exception captured from a failed handler or predicate.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets a value indicating whether a handler completed the command successfully.
    /// </summary>
    public bool IsHandled => Status == CommandExecutionStatus.Handled;

    /// <summary>
    /// Gets the result used when no handler accepted a command.
    /// </summary>
    public static CommandResult NotHandled { get; } =
        new(CommandExecutionStatus.NotHandled);

    /// <summary>
    /// Gets the result used when a command completed successfully without a return value.
    /// </summary>
    public static CommandResult Handled { get; } = new(CommandExecutionStatus.Handled);

    /// <summary>
    /// Gets the result used when a registered command is currently disabled.
    /// </summary>
    public static CommandResult Disabled { get; } = new(CommandExecutionStatus.Disabled);

    /// <summary>
    /// Gets the result used when dispatch was canceled.
    /// </summary>
    public static CommandResult Canceled { get; } = new(CommandExecutionStatus.Canceled);

    /// <summary>
    /// Creates a successful command result with a return value.
    /// </summary>
    /// <param name="value">The value returned by the command.</param>
    /// <returns>A successful command result.</returns>
    public static CommandResult HandledWith(object? value)
    {
        return new CommandResult(CommandExecutionStatus.Handled, value);
    }

    /// <summary>
    /// Creates a failed command result that captures an exception.
    /// </summary>
    /// <param name="exception">The exception raised by the command handler or predicate.</param>
    /// <returns>A failed command result.</returns>
    public static CommandResult Failed(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new CommandResult(CommandExecutionStatus.Failed, exception: exception);
    }
}
