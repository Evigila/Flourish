namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>
/// Registers and removes command handlers while an application is running.
/// </summary>
public interface ICommandRegistry
{
    /// <summary>
    /// Gets immutable snapshots of all active runtime registrations.
    /// </summary>
    IReadOnlyList<CommandRegistrationInfo> Registrations { get; }

    /// <summary>
    /// Occurs after the runtime registration collection changes.
    /// </summary>
    event EventHandler<CommandRegistryChangedEventArgs>? Changed;

    /// <summary>
    /// Occurs when one or all commands should have their availability queried again.
    /// </summary>
    event EventHandler<CommandCanExecuteChangedEventArgs>? CanExecuteChanged;

    /// <summary>
    /// Registers a runtime command handler.
    /// </summary>
    /// <param name="commandKey">The non-empty stable command key.</param>
    /// <param name="executeAsync">The asynchronous handler.</param>
    /// <param name="canExecute">An optional availability predicate.</param>
    /// <param name="options">Optional duplicate and priority behavior.</param>
    /// <returns>A lease that unregisters only this handler when disposed.</returns>
    ICommandRegistration Register(
        string commandKey,
        CommandExecutionHandler executeAsync,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null
    );

    /// <summary>
    /// Determines whether a command key has at least one runtime registration.
    /// </summary>
    /// <param name="commandKey">The command key to inspect.</param>
    /// <returns><see langword="true" /> when the key has a runtime registration; otherwise, <see langword="false" />.</returns>
    bool Contains(string commandKey);

    /// <summary>
    /// Notifies listeners that command availability should be queried again.
    /// </summary>
    /// <param name="commandKey">The affected command key, or <see langword="null" /> for every command.</param>
    void NotifyCanExecuteChanged(string? commandKey = null);
}
