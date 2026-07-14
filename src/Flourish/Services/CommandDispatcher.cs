using System.Collections.ObjectModel;
using System.Diagnostics;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed class CommandDispatcher : ICommandRegistry, ICommandDispatcher
{
    private readonly object gate = new();
    private readonly Dictionary<string, List<RegistrationEntry>> registrationsByKey =
        new(StringComparer.Ordinal);
    private long nextSequence;
    private long version;

    public IReadOnlyList<CommandRegistrationInfo> Registrations
    {
        get
        {
            lock (gate)
            {
                return CreateRegistrationSnapshotLocked();
            }
        }
    }

    public event EventHandler<CommandRegistryChangedEventArgs>? Changed;

    public event EventHandler<CommandCanExecuteChangedEventArgs>? CanExecuteChanged;

    public ICommandRegistration Register(
        string commandKey,
        CommandExecutionHandler executeAsync,
        CommandCanExecuteHandler? canExecute = null,
        CommandRegistrationOptions? options = null
    )
    {
        ValidateCommandKey(commandKey);
        ArgumentNullException.ThrowIfNull(executeAsync);

        options ??= new CommandRegistrationOptions();
        if (!Enum.IsDefined(options.DuplicatePolicy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.DuplicatePolicy,
                "The command duplicate policy is not defined."
            );
        }

        RegistrationEntry entry;
        CommandRegistryChangedEventArgs changed;
        lock (gate)
        {
            registrationsByKey.TryGetValue(commandKey, out var existing);
            var hasExisting = existing is { Count: > 0 };
            if (hasExisting && options.DuplicatePolicy == CommandDuplicatePolicy.Reject)
            {
                throw new InvalidOperationException(
                    $"Command key '{commandKey}' is already registered."
                );
            }

            var changeKind = CommandRegistryChangeKind.Registered;
            if (hasExisting && options.DuplicatePolicy == CommandDuplicatePolicy.Replace)
            {
                foreach (var replaced in existing!)
                {
                    replaced.TryDeactivate();
                }

                existing!.Clear();
                changeKind = CommandRegistryChangeKind.Replaced;
            }

            if (existing is null)
            {
                existing = [];
                registrationsByKey.Add(commandKey, existing);
            }

            entry = new RegistrationEntry(
                commandKey,
                executeAsync,
                canExecute,
                options.Priority,
                nextSequence++
            );
            existing.Add(entry);
            changed = CreateChangedEventArgsLocked(changeKind, commandKey);
        }

        RaiseChanged(changed);
        return new CommandRegistration(this, entry);
    }

    public bool Contains(string commandKey)
    {
        if (string.IsNullOrWhiteSpace(commandKey))
        {
            return false;
        }

        lock (gate)
        {
            return registrationsByKey.TryGetValue(commandKey, out var entries)
                && entries.Any(entry => entry.IsRegistered);
        }
    }

    public void NotifyCanExecuteChanged(string? commandKey = null)
    {
        if (commandKey is not null)
        {
            ValidateCommandKey(commandKey);
        }

        RaiseCanExecuteChanged(new CommandCanExecuteChangedEventArgs(commandKey));
    }

    public bool CanExecute(
        string commandKey,
        object? parameter = null,
        CommandSource source = CommandSource.Application
    )
    {
        if (string.IsNullOrWhiteSpace(commandKey))
        {
            return false;
        }

        var context = new CommandContext(commandKey, parameter, source);
        var entries = GetDispatchEntries(commandKey);

        foreach (var entry in entries)
        {
            if (!entry.IsRegistered)
            {
                continue;
            }

            try
            {
                if (entry.CanExecute?.Invoke(context) != false)
                {
                    return true;
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine(
                    $"Flourish command can-execute handler failed for '{commandKey}': {error}"
                );
            }
        }

        return false;
    }

    public async ValueTask<CommandResult> ExecuteAsync(
        string commandKey,
        object? parameter = null,
        CommandSource source = CommandSource.Application,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(commandKey))
        {
            return CommandResult.NotHandled;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return CommandResult.Canceled;
        }

        var context = new CommandContext(commandKey, parameter, source);
        var entries = GetDispatchEntries(commandKey);
        var hasActiveRegistration = false;
        var hasExecutableRegistration = false;

        foreach (var entry in entries)
        {
            if (!entry.IsRegistered)
            {
                continue;
            }

            hasActiveRegistration = true;
            bool canExecute;
            try
            {
                canExecute = entry.CanExecute?.Invoke(context) != false;
            }
            catch (Exception error)
            {
                return CommandResult.Failed(error);
            }

            if (!canExecute)
            {
                continue;
            }

            hasExecutableRegistration = true;
            if (cancellationToken.IsCancellationRequested)
            {
                return CommandResult.Canceled;
            }

            CommandResult result;
            try
            {
                result = await entry
                    .ExecuteAsync(context, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return CommandResult.Canceled;
            }
            catch (Exception error)
            {
                return CommandResult.Failed(error);
            }

            if (result.Status != CommandExecutionStatus.NotHandled)
            {
                return result;
            }
        }

        if (hasActiveRegistration && !hasExecutableRegistration)
        {
            return CommandResult.Disabled;
        }

        return CommandResult.NotHandled;
    }

    private RegistrationEntry[] GetDispatchEntries(string commandKey)
    {
        lock (gate)
        {
            if (!registrationsByKey.TryGetValue(commandKey, out var entries))
            {
                return [];
            }

            return entries
                .Where(entry => entry.IsRegistered)
                .OrderByDescending(entry => entry.Priority)
                .ThenBy(entry => entry.Sequence)
                .ToArray();
        }
    }

    private void Unregister(RegistrationEntry entry)
    {
        CommandRegistryChangedEventArgs? changed = null;
        lock (gate)
        {
            if (
                !entry.IsRegistered
                || !registrationsByKey.TryGetValue(entry.CommandKey, out var entries)
                || !entries.Contains(entry)
            )
            {
                return;
            }

            entry.TryDeactivate();
            entries.Remove(entry);
            if (entries.Count == 0)
            {
                registrationsByKey.Remove(entry.CommandKey);
            }

            changed = CreateChangedEventArgsLocked(
                CommandRegistryChangeKind.Unregistered,
                entry.CommandKey
            );
        }

        RaiseChanged(changed);
    }

    private ReadOnlyCollection<CommandRegistrationInfo> CreateRegistrationSnapshotLocked()
    {
        return new ReadOnlyCollection<CommandRegistrationInfo>(
            registrationsByKey
                .Values
                .SelectMany(entries => entries)
                .Where(entry => entry.IsRegistered)
                .OrderBy(entry => entry.Sequence)
                .Select(entry => entry.CreateSnapshot())
                .ToArray()
        );
    }

    private CommandRegistryChangedEventArgs CreateChangedEventArgsLocked(
        CommandRegistryChangeKind changeKind,
        string commandKey
    )
    {
        version++;
        return new CommandRegistryChangedEventArgs(
            version,
            changeKind,
            commandKey,
            CreateRegistrationSnapshotLocked()
        );
    }

    private void RaiseChanged(CommandRegistryChangedEventArgs eventArgs)
    {
        var handlers = Changed;
        if (handlers is null)
        {
            return;
        }

        foreach (
            EventHandler<CommandRegistryChangedEventArgs> handler in handlers.GetInvocationList()
        )
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception error)
            {
                Debug.WriteLine($"Flourish command registry event handler failed: {error}");
            }
        }
    }

    private void RaiseCanExecuteChanged(CommandCanExecuteChangedEventArgs eventArgs)
    {
        var handlers = CanExecuteChanged;
        if (handlers is null)
        {
            return;
        }

        foreach (
            EventHandler<CommandCanExecuteChangedEventArgs> handler in handlers.GetInvocationList()
        )
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception error)
            {
                Debug.WriteLine(
                    $"Flourish command can-execute event handler failed: {error}"
                );
            }
        }
    }

    private static void ValidateCommandKey(string commandKey)
    {
        if (string.IsNullOrWhiteSpace(commandKey))
        {
            throw new ArgumentException("Command key cannot be empty.", nameof(commandKey));
        }
    }

    private sealed class RegistrationEntry(
        string commandKey,
        CommandExecutionHandler executeAsync,
        CommandCanExecuteHandler? canExecute,
        int priority,
        long sequence
    )
    {
        private int isRegistered = 1;

        public Guid Id { get; } = Guid.NewGuid();

        public string CommandKey { get; } = commandKey;

        public CommandExecutionHandler ExecuteAsync { get; } = executeAsync;

        public CommandCanExecuteHandler? CanExecute { get; } = canExecute;

        public int Priority { get; } = priority;

        public long Sequence { get; } = sequence;

        public bool IsRegistered => Volatile.Read(ref isRegistered) != 0;

        public bool TryDeactivate()
        {
            return Interlocked.Exchange(ref isRegistered, 0) != 0;
        }

        public CommandRegistrationInfo CreateSnapshot()
        {
            return new CommandRegistrationInfo(Id, CommandKey, Priority);
        }
    }

    private sealed class CommandRegistration(
        CommandDispatcher owner,
        RegistrationEntry entry
    ) : ICommandRegistration
    {
        public Guid Id => entry.Id;

        public string CommandKey => entry.CommandKey;

        public bool IsRegistered => entry.IsRegistered;

        public void NotifyCanExecuteChanged()
        {
            if (entry.IsRegistered)
            {
                owner.NotifyCanExecuteChanged(entry.CommandKey);
            }
        }

        public void Dispose()
        {
            owner.Unregister(entry);
        }
    }
}
