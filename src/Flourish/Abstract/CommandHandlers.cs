using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Executes a runtime command.
/// </summary>
/// <param name="context">The command invocation context.</param>
/// <param name="cancellationToken">A token that requests cancellation.</param>
/// <returns>The captured command outcome.</returns>
public delegate ValueTask<CommandResult> CommandExecutionHandler(
    CommandContext context,
    CancellationToken cancellationToken
);

/// <summary>
/// Determines whether a runtime command can execute for an invocation context.
/// </summary>
/// <param name="context">The command invocation context.</param>
/// <returns><see langword="true" /> when the handler can execute; otherwise, <see langword="false" />.</returns>
public delegate bool CommandCanExecuteHandler(CommandContext context);
