namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Parses command keys raised by Flourish UI surfaces such as toolbar items.
/// </summary>
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
