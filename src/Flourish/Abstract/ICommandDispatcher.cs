namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Queries and dispatches command keys to registered handlers.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Determines whether a command is currently eligible for dispatch.
    /// </summary>
    /// <param name="commandKey">The command key to query.</param>
    /// <param name="parameter">An optional command parameter.</param>
    /// <param name="source">The source requesting the command.</param>
    /// <returns><see langword="true" /> when the command can be dispatched; otherwise, <see langword="false" />.</returns>
    bool CanExecute(
        string commandKey,
        object? parameter = null,
        CommandSource source = CommandSource.Application
    );

    /// <summary>
    /// Executes a command asynchronously and captures handler failures in the returned result.
    /// </summary>
    /// <param name="commandKey">The command key to execute.</param>
    /// <param name="parameter">An optional command parameter.</param>
    /// <param name="source">The source requesting the command.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>The captured command outcome.</returns>
    ValueTask<CommandResult> ExecuteAsync(
        string commandKey,
        object? parameter = null,
        CommandSource source = CommandSource.Application,
        CancellationToken cancellationToken = default
    );
}
