using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Hosting;

/// <summary>Keeps command registrations inside the current Blazor scope.</summary>
internal sealed class CommandRuntime : ICommandRegistrar, ICommandDispatcher
{
    private readonly Dictionary<string, List<Registration>> registrations = new(StringComparer.Ordinal);
    private long sequence;

    public CommandRuntime(IEnumerable<ICommandParser> parsers)
    {
        foreach (var parser in parsers) parser.RegisterCommands(this);
    }

    public void Register(
        string commandKey,
        CommandExecutionHandler executeAsync,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandKey);
        ArgumentNullException.ThrowIfNull(executeAsync);
        options ??= new CommandRegistrationOptions();
        if (!Enum.IsDefined(options.DuplicatePolicy)) throw new ArgumentOutOfRangeException(nameof(options));

        if (!registrations.TryGetValue(commandKey, out var entries))
        {
            entries = [];
            registrations.Add(commandKey, entries);
        }
        else if (entries.Count > 0)
        {
            if (options.DuplicatePolicy == CommandDuplicatePolicy.Reject)
                throw new InvalidOperationException($"Command key '{commandKey}' is already registered.");
            if (options.DuplicatePolicy == CommandDuplicatePolicy.Replace) entries.Clear();
        }

        entries.Add(new(executeAsync, canExecute, options.Priority, sequence++));
        entries.Sort(static (left, right) =>
        {
            var priority = right.Priority.CompareTo(left.Priority);
            return priority != 0 ? priority : left.Sequence.CompareTo(right.Sequence);
        });
    }

    public void Register(
        string commandKey,
        Action execute,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        Register(commandKey, (_, _) =>
        {
            execute();
            return ValueTask.FromResult(CommandResult.Handled);
        }, canExecute, options);
    }

    public bool CanExecute(string commandKey, object? parameter = null, CommandSource source = CommandSource.Application)
    {
        if (string.IsNullOrWhiteSpace(commandKey) || !registrations.TryGetValue(commandKey, out var entries)) return false;
        var context = new CommandContext(commandKey, parameter, source);
        return entries.Any(entry => CanExecute(entry, context));
    }

    public async ValueTask<CommandResult> ExecuteAsync(
        string commandKey,
        object? parameter = null,
        CommandSource source = CommandSource.Application,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(commandKey) || !registrations.TryGetValue(commandKey, out var entries))
            return CommandResult.NotHandled;
        if (cancellationToken.IsCancellationRequested) return CommandResult.Canceled;

        var context = new CommandContext(commandKey, parameter, source);
        var executable = false;
        foreach (var entry in entries)
        {
            bool allowed;
            try { allowed = entry.CanExecute?.Invoke(context) != false; }
            catch (Exception error) { return CommandResult.Failed(error); }
            if (!allowed) continue;
            executable = true;

            try
            {
                var result = await entry.ExecuteAsync(context, cancellationToken);
                if (result.Status != CommandExecutionStatus.NotHandled) return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return CommandResult.Canceled;
            }
            catch (Exception error)
            {
                return CommandResult.Failed(error);
            }
        }

        return executable ? CommandResult.NotHandled : CommandResult.Disabled;
    }

    private static bool CanExecute(Registration entry, CommandContext context)
    {
        try { return entry.CanExecute?.Invoke(context) != false; }
        catch { return false; }
    }

    private sealed record Registration(
        CommandExecutionHandler ExecuteAsync,
        CommandCanExecuteHandler? CanExecute,
        int Priority,
        long Sequence);
}
