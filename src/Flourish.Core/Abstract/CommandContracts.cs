using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Identifies the component that requested a command.</summary>
public enum CommandSource
{
    /// <summary>The command source is not known.</summary>
    Unknown = 0,

    /// <summary>The command was requested directly by application code.</summary>
    Application,

    /// <summary>The command was requested by a navigation item.</summary>
    Navigation,

    /// <summary>The command was requested by a toolbar item.</summary>
    Toolbar,

    /// <summary>The command was requested by a title bar item.</summary>
    TitleBar,

    /// <summary>The command was requested by a status bar item.</summary>
    StatusBar,

    /// <summary>The command was requested by a shell notification.</summary>
    Notification,

    /// <summary>The command was requested by a keyboard shortcut.</summary>
    Shortcut,

    /// <summary>The command was requested by custom region content.</summary>
    CustomRegion,
}

/// <summary>Describes the outcome of a command dispatch operation.</summary>
public enum CommandExecutionStatus
{
    /// <summary>No handler accepted the command.</summary>
    NotHandled = 0,

    /// <summary>A handler completed the command successfully.</summary>
    Handled,

    /// <summary>The command is registered but is currently disabled.</summary>
    Disabled,

    /// <summary>A handler or can-execute predicate failed.</summary>
    Failed,

    /// <summary>Dispatch was canceled.</summary>
    Canceled,
}

/// <summary>Specifies how registration handles an existing command key.</summary>
public enum CommandDuplicatePolicy
{
    /// <summary>Reject the new registration.</summary>
    Reject = 0,

    /// <summary>Replace all existing registrations for the command key.</summary>
    Replace,

    /// <summary>Keep existing handlers and append the new handler.</summary>
    Append,
}

/// <summary>Describes a command invocation supplied to runtime handlers.</summary>
public sealed class CommandContext
{
    /// <summary>Initializes a command invocation context.</summary>
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

    /// <summary>Gets the stable command key being dispatched.</summary>
    public string CommandKey { get; }

    /// <summary>Gets the optional parameter supplied by the caller.</summary>
    public object? Parameter { get; }

    /// <summary>Gets the component that requested the command.</summary>
    public CommandSource Source { get; }
}

/// <summary>Represents the captured outcome of command dispatch.</summary>
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

    /// <summary>Gets the command execution status.</summary>
    public CommandExecutionStatus Status { get; }

    /// <summary>Gets an optional value returned by a successful command.</summary>
    public object? Value { get; }

    /// <summary>Gets the captured handler or predicate exception.</summary>
    public Exception? Exception { get; }

    /// <summary>Gets whether a handler completed the command successfully.</summary>
    public bool IsHandled => Status == CommandExecutionStatus.Handled;

    /// <summary>Gets the result used when no handler accepted a command.</summary>
    public static CommandResult NotHandled { get; } =
        new(CommandExecutionStatus.NotHandled);

    /// <summary>Gets a successful result without a return value.</summary>
    public static CommandResult Handled { get; } = new(CommandExecutionStatus.Handled);

    /// <summary>Gets the result used when a command is disabled.</summary>
    public static CommandResult Disabled { get; } = new(CommandExecutionStatus.Disabled);

    /// <summary>Gets the result used when dispatch was canceled.</summary>
    public static CommandResult Canceled { get; } = new(CommandExecutionStatus.Canceled);

    /// <summary>Creates a successful command result with a return value.</summary>
    public static CommandResult HandledWith(object? value)
    {
        return new CommandResult(CommandExecutionStatus.Handled, value);
    }

    /// <summary>Creates a failed command result that captures an exception.</summary>
    public static CommandResult Failed(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new CommandResult(CommandExecutionStatus.Failed, exception: exception);
    }
}

/// <summary>Executes a runtime command.</summary>
public delegate ValueTask<CommandResult> CommandExecutionHandler(
    CommandContext context,
    CancellationToken cancellationToken
);

/// <summary>Determines whether a runtime command can execute.</summary>
public delegate bool CommandCanExecuteHandler(CommandContext context);

/// <summary>Configures a runtime command registration.</summary>
public sealed class CommandRegistrationOptions
{
    /// <summary>Gets the policy applied when the command key already exists.</summary>
    public CommandDuplicatePolicy DuplicatePolicy { get; init; } =
        CommandDuplicatePolicy.Reject;

    /// <summary>Gets the handler priority. Higher values run first.</summary>
    public int Priority { get; init; }
}

/// <summary>Provides an immutable snapshot of a command registration.</summary>
public sealed class CommandRegistrationInfo
{
    internal CommandRegistrationInfo(Guid id, string commandKey, int priority)
    {
        Id = id;
        CommandKey = commandKey;
        Priority = priority;
    }

    /// <summary>Gets the unique registration identifier.</summary>
    public Guid Id { get; }

    /// <summary>Gets the stable command key.</summary>
    public string CommandKey { get; }

    /// <summary>Gets the handler priority.</summary>
    public int Priority { get; }
}

/// <summary>Provides a command registry snapshot after a structural change.</summary>
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

    /// <summary>Gets the monotonically increasing registry version.</summary>
    public long Version { get; }

    /// <summary>Gets the kind of structural change.</summary>
    public CollectionChangeKind ChangeKind { get; }

    /// <summary>Gets the affected command key.</summary>
    public string CommandKey { get; }

    /// <summary>Gets active registrations in registration order.</summary>
    public IReadOnlyList<CommandRegistrationInfo> Current { get; }
}

/// <summary>Identifies command availability that should be queried again.</summary>
public sealed class CommandCanExecuteChangedEventArgs : EventArgs
{
    internal CommandCanExecuteChangedEventArgs(string? commandKey)
    {
        CommandKey = commandKey;
    }

    /// <summary>Gets the affected key, or null when all commands should be queried.</summary>
    public string? CommandKey { get; }
}

/// <summary>Queries and dispatches command keys to registered handlers.</summary>
public interface ICommandDispatcher
{
    /// <summary>Determines whether a command is eligible for dispatch.</summary>
    bool CanExecute(
        string commandKey,
        object? parameter = null,
        CommandSource source = CommandSource.Application
    );

    /// <summary>Executes a command and captures handler failures.</summary>
    ValueTask<CommandResult> ExecuteAsync(
        string commandKey,
        object? parameter = null,
        CommandSource source = CommandSource.Application,
        CancellationToken cancellationToken = default
    );
}

/// <summary>Adds command handlers managed by the application host.</summary>
public interface ICommandRegistrar
{
    /// <summary>Registers an asynchronous command handler.</summary>
    void Register(
        string commandKey,
        CommandExecutionHandler executeAsync,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null
    );

    /// <summary>Registers a synchronous command handler.</summary>
    void Register(
        string commandKey,
        Action execute,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null
    );
}

/// <summary>Registers and removes handlers while an application is running.</summary>
public interface ICommandRegistry
{
    /// <summary>Gets immutable snapshots of active registrations.</summary>
    IReadOnlyList<CommandRegistrationInfo> Current { get; }

    /// <summary>Occurs after the registration collection changes.</summary>
    event EventHandler<CommandRegistryChangedEventArgs>? Changed;

    /// <summary>Occurs when command availability should be queried again.</summary>
    event EventHandler<CommandCanExecuteChangedEventArgs>? CanExecuteChanged;

    /// <summary>Registers a runtime command handler.</summary>
    IRegistration Register(
        string commandKey,
        CommandExecutionHandler executeAsync,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null
    );

    /// <summary>Determines whether a key has an active registration.</summary>
    bool Contains(string commandKey);

    /// <summary>Notifies listeners that command availability should be queried again.</summary>
    void NotifyCanExecuteChanged(string? commandKey = null);
}

/// <summary>Defines command mappings when the application host starts.</summary>
public interface ICommandParser
{
    /// <summary>Defines the mappings owned by this parser.</summary>
    void RegisterCommands(ICommandRegistrar commands);
}
