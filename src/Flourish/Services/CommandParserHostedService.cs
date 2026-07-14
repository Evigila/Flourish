using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Services;

internal sealed class CommandParserHostedService : IHostedService, IDisposable
{
    private readonly object lifecycleGate = new();
    private readonly ICommandRegistry commandRegistry;
    private readonly IReadOnlyList<ICommandParser> parsers;
    private List<ICommandRegistration>? activeRegistrations;
    private LifecycleState lifecycleState;

    public CommandParserHostedService(
        ICommandRegistry commandRegistry,
        IEnumerable<ICommandParser> parsers
    )
    {
        this.commandRegistry = commandRegistry
            ?? throw new ArgumentNullException(nameof(commandRegistry));
        ArgumentNullException.ThrowIfNull(parsers);
        this.parsers = parsers.ToArray();
        if (this.parsers.Any(parser => parser is null))
        {
            throw new ArgumentException(
                "The command parser collection cannot contain null entries.",
                nameof(parsers)
            );
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (lifecycleGate)
        {
            ObjectDisposedException.ThrowIf(
                lifecycleState == LifecycleState.Disposed,
                this
            );
            if (lifecycleState == LifecycleState.Started)
            {
                return Task.CompletedTask;
            }

            if (lifecycleState == LifecycleState.Starting)
            {
                throw new InvalidOperationException(
                    "Command registration is already starting."
                );
            }

            lifecycleState = LifecycleState.Starting;
        }

        List<ICommandRegistration> registrations = [];
        try
        {
            foreach (var parser in parsers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var registrar = new CommandRegistrar(commandRegistry, registrations);
                try
                {
                    parser.RegisterCommands(registrar);
                }
                finally
                {
                    registrar.Complete();
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (Exception startError)
        {
            try
            {
                DisposeRegistrations(registrations);
            }
            catch (Exception cleanupError)
            {
                throw new AggregateException(
                    "Command registration failed and rollback did not complete cleanly.",
                    startError,
                    cleanupError
                );
            }
            finally
            {
                lock (lifecycleGate)
                {
                    if (lifecycleState == LifecycleState.Starting)
                    {
                        lifecycleState = LifecycleState.Stopped;
                    }
                }
            }

            throw;
        }

        lock (lifecycleGate)
        {
            if (lifecycleState != LifecycleState.Disposed)
            {
                activeRegistrations = registrations;
                lifecycleState = LifecycleState.Started;
                return Task.CompletedTask;
            }
        }

        DisposeRegistrations(registrations);
        throw new ObjectDisposedException(nameof(CommandParserHostedService));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        List<ICommandRegistration>? registrations;
        lock (lifecycleGate)
        {
            if (lifecycleState == LifecycleState.Starting)
            {
                throw new InvalidOperationException(
                    "Command registration cannot stop while it is starting."
                );
            }

            registrations = activeRegistrations;
            activeRegistrations = null;
            if (lifecycleState != LifecycleState.Disposed)
            {
                lifecycleState = LifecycleState.Stopped;
            }
        }

        DisposeRegistrations(registrations);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        List<ICommandRegistration>? registrations;
        lock (lifecycleGate)
        {
            if (lifecycleState == LifecycleState.Disposed)
            {
                return;
            }

            lifecycleState = LifecycleState.Disposed;
            registrations = activeRegistrations;
            activeRegistrations = null;
        }

        DisposeRegistrations(registrations);
    }

    private static void DisposeRegistrations(
        IReadOnlyList<ICommandRegistration>? registrations
    )
    {
        if (registrations is null)
        {
            return;
        }

        List<Exception>? errors = null;
        for (var index = registrations.Count - 1; index >= 0; index--)
        {
            try
            {
                registrations[index].Dispose();
            }
            catch (Exception error)
            {
                errors ??= [];
                errors.Add(error);
            }
        }

        if (errors is not null)
        {
            throw new AggregateException(
                "One or more command registrations could not be removed.",
                errors
            );
        }
    }

    private enum LifecycleState
    {
        Stopped,
        Starting,
        Started,
        Disposed,
    }

    private sealed class CommandRegistrar(
        ICommandRegistry commandRegistry,
        List<ICommandRegistration> registrations
    ) : ICommandRegistrar
    {
        private readonly object gate = new();
        private bool isActive = true;

        public void Register(
            string commandKey,
            CommandExecutionHandler executeAsync,
            CommandCanExecuteHandler? canExecute = null,
            CommandRegistrationOptions? options = null
        )
        {
            ArgumentNullException.ThrowIfNull(executeAsync);
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(!isActive, this);
                var registration = commandRegistry.Register(
                    commandKey,
                    executeAsync,
                    canExecute,
                    options
                );
                try
                {
                    registrations.Add(registration);
                }
                catch
                {
                    registration.Dispose();
                    throw;
                }
            }
        }

        public void Register(
            string commandKey,
            Action execute,
            CommandCanExecuteHandler? canExecute = null,
            CommandRegistrationOptions? options = null
        )
        {
            ArgumentNullException.ThrowIfNull(execute);
            Register(
                commandKey,
                (_, _) =>
                {
                    execute();
                    return ValueTask.FromResult(CommandResult.Handled);
                },
                canExecute,
                options
            );
        }

        public void Complete()
        {
            lock (gate)
            {
                isActive = false;
            }
        }
    }
}
