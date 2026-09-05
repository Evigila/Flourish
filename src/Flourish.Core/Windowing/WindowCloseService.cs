using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Windowing;

internal sealed class WindowCloseService : IWindowCloseService
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, GuardEntry> guards = new(StringComparer.Ordinal);
    private readonly Queue<WindowCloseState> pendingStates = new();
    private readonly IServiceProvider services;
    private Func<WindowCloseRequestReason, CancellationToken, ValueTask<bool>>? requestClose;
    private WindowCloseBehavior behavior;
    private WindowCloseState current;
    private bool isPublishing;

    internal WindowCloseService(
        IServiceProvider services,
        WindowCloseBehavior initialBehavior = WindowCloseBehavior.Prompt
    )
    {
        this.services = services ?? throw new ArgumentNullException(nameof(services));
        ValidateBehavior(initialBehavior, nameof(initialBehavior));
        behavior = initialBehavior;
        current = new WindowCloseState(initialBehavior);
    }

    public WindowCloseState Current => Volatile.Read(ref current);

    public event EventHandler<StateChangedEventArgs<WindowCloseState>>? Changed;

    public void SetBehavior(WindowCloseBehavior behavior)
    {
        ValidateBehavior(behavior, nameof(behavior));

        lock (gate)
        {
            if (this.behavior == behavior)
            {
                return;
            }

            this.behavior = behavior;
            var state = new WindowCloseState(behavior);
            Volatile.Write(ref current, state);
            pendingStates.Enqueue(state);
        }

        PublishQueuedChanges();
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
        GuardEntry entry = new(id, order, guard);
        lock (gate)
        {
            if (!guards.TryAdd(id, entry))
            {
                throw new InvalidOperationException(
                    $"A close guard with ID '{id}' is already registered."
                );
            }
        }

        return new Registration(this, entry);
    }

    public async ValueTask<bool> CanCloseAsync(
        WindowCloseRequestReason reason,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateReason(reason);

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
            cancellationToken.ThrowIfCancellationRequested();
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
        ValidateReason(reason);

        if (!await CanCloseAsync(reason, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        cancellationToken.ThrowIfCancellationRequested();
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

    private void Unregister(GuardEntry entry)
    {
        lock (gate)
        {
            if (
                guards.TryGetValue(entry.Id, out var active)
                && ReferenceEquals(active, entry)
            )
            {
                guards.Remove(entry.Id);
            }
        }
    }

    private void PublishQueuedChanges()
    {
        lock (gate)
        {
            if (isPublishing || pendingStates.Count == 0)
            {
                return;
            }

            isPublishing = true;
        }

        while (true)
        {
            WindowCloseState state;
            EventHandler<StateChangedEventArgs<WindowCloseState>>? handlers;
            lock (gate)
            {
                if (pendingStates.Count == 0)
                {
                    isPublishing = false;
                    return;
                }

                state = pendingStates.Dequeue();
                handlers = Changed;
            }

            if (handlers is null)
            {
                continue;
            }

            var eventArgs = new StateChangedEventArgs<WindowCloseState>(state);
            foreach (
                EventHandler<StateChangedEventArgs<WindowCloseState>> handler in handlers.GetInvocationList()
            )
            {
                try
                {
                    handler(this, eventArgs);
                }
                catch (Exception error)
                {
                    Debug.WriteLine($"Window close state listener failed: {error}");
                }
            }
        }
    }

    private static void ValidateBehavior(WindowCloseBehavior behavior, string parameterName)
    {
        if (!Enum.IsDefined(behavior))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                behavior,
                "Unknown close behavior."
            );
        }
    }

    private static void ValidateReason(WindowCloseRequestReason reason)
    {
        if (!Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "Unknown close request reason."
            );
        }
    }

    private sealed class GuardEntry(
        string id,
        int order,
        Func<WindowCloseContext, CancellationToken, ValueTask<WindowCloseDecision>> guard
    )
    {
        public string Id { get; } = id;

        public int Order { get; } = order;

        public Func<
            WindowCloseContext,
            CancellationToken,
            ValueTask<WindowCloseDecision>
        > Guard { get; } = guard;
    }

    private sealed class Registration(WindowCloseService owner, GuardEntry entry)
        : IRegistration
    {
        private WindowCloseService? owner = owner;

        public bool IsRegistered => Volatile.Read(ref owner) is not null;

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.Unregister(entry);
        }
    }
}
