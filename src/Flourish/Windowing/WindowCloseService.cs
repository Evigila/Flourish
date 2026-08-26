using System.Linq;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Windowing;

internal sealed class WindowCloseService(FlourishWindowOptions options, IServiceProvider services)
    : IWindowCloseService
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, GuardEntry> guards = new(StringComparer.Ordinal);
    private Func<WindowCloseRequestReason, CancellationToken, ValueTask<bool>>? requestClose;
    private WindowCloseBehavior behavior = options.IsTrayExitEnabled
        ? WindowCloseBehavior.MinimizeToTray
        : WindowCloseBehavior.Prompt;
    private FlourishWindowCloseState current = new(
        options.IsTrayExitEnabled
            ? WindowCloseBehavior.MinimizeToTray
            : WindowCloseBehavior.Prompt
    );

    public FlourishWindowCloseState Current => Volatile.Read(ref current);

    public event EventHandler<FlourishStateChangedEventArgs<FlourishWindowCloseState>>? Changed;

    public void SetBehavior(WindowCloseBehavior behavior)
    {
        if (!Enum.IsDefined(behavior))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                behavior,
                "Unknown close behavior."
            );
        }

        FlourishWindowCloseState current;
        lock (gate)
        {
            if (this.behavior == behavior)
            {
                return;
            }

            this.behavior = behavior;
            options.IsTrayExitEnabled = behavior == WindowCloseBehavior.MinimizeToTray;
            current = new FlourishWindowCloseState(behavior);
            Volatile.Write(ref this.current, current);
        }

        Changed?.Invoke(
            this,
            new FlourishStateChangedEventArgs<FlourishWindowCloseState>(current)
        );
    }

    public IRegistration RegisterGuard(
        string id,
        Func<WindowCloseContext, CancellationToken, ValueTask<WindowCloseDecision>> guard,
        int order = 0
    )
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A close guard requires an ID.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(guard);
        id = id.Trim();
        lock (gate)
        {
            if (!guards.TryAdd(id, new GuardEntry(id, order, guard)))
            {
                throw new InvalidOperationException(
                    $"A close guard with ID '{id}' is already registered."
                );
            }
        }

        return new Registration(this, id);
    }

    public async ValueTask<bool> CanCloseAsync(
        WindowCloseRequestReason reason,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "Unknown close request reason."
            );
        }

        GuardEntry[] snapshot;
        lock (gate)
        {
            snapshot = guards
                .Values.OrderBy(entry => entry.Order)
                .ThenBy(entry => entry.Id, StringComparer.Ordinal)
                .ToArray();
        }

        var context = new WindowCloseContext(reason, services);
        foreach (var entry in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var decision = await entry.Guard(context, cancellationToken).ConfigureAwait(false);
            if (!Enum.IsDefined(decision))
            {
                throw new InvalidOperationException(
                    $"Close guard '{entry.Id}' returned an unknown decision: {decision}."
                );
            }

            if (decision == WindowCloseDecision.Cancel)
            {
                return false;
            }
        }

        return true;
    }

    public async ValueTask<bool> RequestCloseAsync(
        WindowCloseRequestReason reason = WindowCloseRequestReason.Application,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "Unknown close request reason."
            );
        }

        if (!await CanCloseAsync(reason, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        Func<WindowCloseRequestReason, CancellationToken, ValueTask<bool>>? request;
        lock (gate)
        {
            request = requestClose;
        }

        if (request is null)
        {
            return false;
        }

        return await request(reason, cancellationToken).ConfigureAwait(false);
    }

    internal void Attach(Func<WindowCloseRequestReason, CancellationToken, ValueTask<bool>> request)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (gate)
        {
            requestClose = request;
        }
    }

    internal void Detach()
    {
        lock (gate)
        {
            requestClose = null;
        }
    }

    private void Unregister(string id)
    {
        lock (gate)
        {
            guards.Remove(id);
        }
    }

    private sealed record GuardEntry(
        string Id,
        int Order,
        Func<WindowCloseContext, CancellationToken, ValueTask<WindowCloseDecision>> Guard
    );

    private sealed class Registration(WindowCloseService owner, string id)
        : IRegistration
    {
        private WindowCloseService? owner = owner;

        public string Id { get; } = id;

        public bool IsRegistered => Volatile.Read(ref owner) is not null;

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.Unregister(Id);
        }
    }
}
