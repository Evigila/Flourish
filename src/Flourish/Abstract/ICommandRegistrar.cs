namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Adds command handlers whose lifetime is managed by the Flourish host.
/// </summary>
/// <remarks>
/// A registrar is supplied to <see cref="ICommandParser.RegisterCommands" />
/// during host startup. Parsers must complete registration synchronously and must not retain
/// the registrar. Use <see cref="ICommandRegistry" /> directly for registrations whose lifetime
/// changes while the application is running.
/// A registration that uses <see cref="CommandDuplicatePolicy.Replace" /> cannot restore handlers
/// it replaced if startup registration later fails, so parsers should normally use the default
/// <see cref="CommandDuplicatePolicy.Reject" /> policy.
/// </remarks>
public interface ICommandRegistrar
{
    /// <summary>
    /// Registers an asynchronous command handler for the lifetime of the running host.
    /// </summary>
    /// <param name="commandKey">The non-empty stable command key.</param>
    /// <param name="executeAsync">The asynchronous handler.</param>
    /// <param name="canExecute">An optional availability predicate.</param>
    /// <param name="options">Optional duplicate and priority behavior.</param>
    void Register(
        string commandKey,
        CommandExecutionHandler executeAsync,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null
    );

    /// <summary>
    /// Registers a synchronous command handler for the lifetime of the running host.
    /// </summary>
    /// <param name="commandKey">The non-empty stable command key.</param>
    /// <param name="execute">The synchronous action.</param>
    /// <param name="canExecute">An optional availability predicate.</param>
    /// <param name="options">Optional duplicate and priority behavior.</param>
    void Register(
        string commandKey,
        Action execute,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null
    );
}
