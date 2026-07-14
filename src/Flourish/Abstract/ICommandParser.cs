namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Defines mappings between command keys and handlers when the Flourish host starts.
/// </summary>
/// <remarks>
/// Register implementations with
/// <see cref="FlourishServiceCollectionExtensions.AddCommandParser{TParser}" />.
/// Flourish activates each parser at startup and removes all mapped handlers when the
/// host stops.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// internal sealed class ReportCommands(ReportService reports)
///     : ICommandParser
/// {
///     public void RegisterCommands(ICommandRegistrar commands)
///     {
///         commands.Register("reports.refresh", async (_, token) =>
///         {
///             await reports.RefreshAsync(token);
///             return CommandResult.Handled;
///         });
///     }
/// }
/// ]]></code>
/// </example>
public interface ICommandParser
{
    /// <summary>
    /// Defines the command-key mappings owned by this parser.
    /// </summary>
    /// <param name="commands">The host-managed command registrar.</param>
    void RegisterCommands(ICommandRegistrar commands);
}
