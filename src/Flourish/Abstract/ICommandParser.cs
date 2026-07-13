namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Handles command keys synchronously for Flourish UI surfaces.
/// </summary>
/// <remarks>
/// Register implementations during service configuration. <see cref="ICommandDispatcher" />
/// evaluates handlers from <see cref="ICommandRegistry" /> first. It invokes registered parsers
/// in registration order when no runtime registration exists or when executable runtime handlers
/// return <see cref="CommandResult.NotHandled" />.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// services.AddSingleton<ICommandParser, AppCommandParser>();
/// ]]></code>
/// </example>
public interface ICommandParser
{
    /// <summary>
    /// Attempts to parse and handle a command key.
    /// </summary>
    /// <param name="commandKey">The command key associated with the requested action.</param>
    /// <returns><see langword="true" /> if the command was recognized and handled; otherwise, <see langword="false" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// public bool TryParse(string commandKey)
    /// {
    ///     return commandKey switch
    ///     {
    ///         "reports.export" => ExportReports(),
    ///         _ => false
    ///     };
    /// }
    /// ]]></code>
    /// </example>
    bool TryParse(string commandKey);
}
