using System.Linq;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Flourish.Shell.TitleBar;

internal sealed class TitleBarSearchService : IDisposable
{
    private readonly Lock gate = new();
    private readonly FlourishTitleBarOptions options;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<TitleBarSearchService> logger;
    private readonly Dictionary<
        Guid,
        Func<FlourishTitleBarSearchQuery, CancellationToken, ValueTask>
    > handlers = [];
    private QueryDispatch? activeQueryDispatch;
    private FlourishTitleBarSearchState current;
    private string text = string.Empty;
    private bool focusRequested;
    private long version;
    private bool isDisposed;

    public TitleBarSearchService(
        FlourishTitleBarOptions options,
        IServiceProvider serviceProvider,
        ILogger<TitleBarSearchService> logger
    )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        current = CreateSnapshot();
    }

    internal event EventHandler<FlourishStateChangedEventArgs<FlourishTitleBarSearchState>>? Changed;

    internal event EventHandler<FlourishStateChangedEventArgs<FlourishTitleBarSearchState>>? ProgrammaticStateChanged;

    public FlourishTitleBarSearchState Current => Volatile.Read(ref current);

    public void SetVisible(bool visible)
    {
        UpdateState(() => options.IsTitlebarSearchEnabled = visible);
    }

    public void SetPlaceholder(string placeholder)
    {
        if (string.IsNullOrWhiteSpace(placeholder))
        {
            throw new ArgumentException("Search placeholder cannot be empty.", nameof(placeholder));
        }

        UpdateState(() => options.SearchPlaceholder = placeholder.Trim());
    }

    public void SetText(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        UpdateState(() => text = value);
    }

    public void Clear() => SetText(string.Empty);

    public void Focus()
    {
        UpdateState(() => focusRequested = true);
    }

    public IRegistration Subscribe(
        Func<FlourishTitleBarSearchQuery, CancellationToken, ValueTask> handler
    )
    {
        ArgumentNullException.ThrowIfNull(handler);
        var id = Guid.NewGuid();
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            handlers.Add(id, handler);
        }

        return new Subscription(this, id);
    }

    internal void PublishFromView(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Func<FlourishTitleBarSearchQuery, CancellationToken, ValueTask>[]? subscribers;
        EventHandler<FlourishStateChangedEventArgs<FlourishTitleBarSearchState>>? stateChanged;
        FlourishTitleBarSearchState? state;
        QueryDispatch? dispatch = null;
        QueryDispatch? previousDispatch;
        long sequence;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            text = value;
            focusRequested = false;
            version++;
            var snapshot = CreateSnapshot();
            Volatile.Write(ref current, snapshot);
            sequence = version;
            previousDispatch = activeQueryDispatch;
            if (handlers.Count == 0)
            {
                activeQueryDispatch = null;
                subscribers = null;
            }
            else
            {
                dispatch = new QueryDispatch();
                activeQueryDispatch = dispatch;
                subscribers = handlers.Values.ToArray();
            }

            stateChanged = Changed;
            state = stateChanged is null ? null : snapshot;
        }

        CancelDispatch(previousDispatch);

        try
        {
            options.TitlebarSearchTextChanged?.Invoke(serviceProvider, value);
        }
        catch (Exception error)
        {
            logger.LogError(error, "The configured title bar search handler failed.");
        }

        var args = new FlourishTitleBarSearchQuery(value, sequence);
        try
        {
            if (stateChanged is not null)
            {
                stateChanged(this, new FlourishStateChangedEventArgs<FlourishTitleBarSearchState>(state!));
            }

        }
        catch
        {
            if (dispatch is not null)
            {
                CompleteDispatch(dispatch);
            }

            throw;
        }

        if (subscribers is not null)
        {
            _ = DispatchAsync(subscribers, args, dispatch!);
        }
    }

    internal void AcknowledgeFocusRequest()
    {
        lock (gate)
        {
            if (!focusRequested)
            {
                return;
            }

            focusRequested = false;
            Volatile.Write(ref current, CreateSnapshot());
        }
    }

    internal bool IsCurrentVersion(long candidateVersion)
    {
        return Volatile.Read(ref current).Version == candidateVersion;
    }

    public void Dispose()
    {
        QueryDispatch? dispatch;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            handlers.Clear();
            dispatch = activeQueryDispatch;
            activeQueryDispatch = null;
        }

        CancelDispatch(dispatch);
    }

    private async Task DispatchAsync(
        IReadOnlyList<
            Func<FlourishTitleBarSearchQuery, CancellationToken, ValueTask>
        > subscribers,
        FlourishTitleBarSearchQuery args,
        QueryDispatch dispatch
    )
    {
        var cancellationToken = dispatch.Token;
        try
        {
            foreach (var subscriber in subscribers)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await subscriber(args, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception error)
                {
                    logger.LogError(error, "A runtime title bar search handler failed.");
                }
            }
        }
        finally
        {
            CompleteDispatch(dispatch);
        }
    }

    private void UpdateState(Action update)
    {
        EventHandler<FlourishStateChangedEventArgs<FlourishTitleBarSearchState>>? programmaticStateChanged;
        EventHandler<FlourishStateChangedEventArgs<FlourishTitleBarSearchState>>? stateChanged;
        FlourishTitleBarSearchState? state;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            var previousText = text;
            var previousPlaceholder = options.SearchPlaceholder;
            var wasVisible = options.IsTitlebarSearchEnabled;
            var wasFocusRequested = focusRequested;
            update();
            if (
                string.Equals(previousText, text, StringComparison.Ordinal)
                && string.Equals(
                    previousPlaceholder,
                    options.SearchPlaceholder,
                    StringComparison.Ordinal
                )
                && wasVisible == options.IsTitlebarSearchEnabled
                && wasFocusRequested == focusRequested
            )
            {
                return;
            }

            version++;
            var snapshot = CreateSnapshot();
            Volatile.Write(ref current, snapshot);
            programmaticStateChanged = ProgrammaticStateChanged;
            stateChanged = Changed;
            state =
                programmaticStateChanged is null && stateChanged is null ? null : snapshot;
        }

        if (state is null)
        {
            return;
        }

        var args = new FlourishStateChangedEventArgs<FlourishTitleBarSearchState>(state);
        programmaticStateChanged?.Invoke(this, args);
        stateChanged?.Invoke(this, args);
    }

    private void CancelDispatch(QueryDispatch? dispatch)
    {
        if (dispatch is null)
        {
            return;
        }

        try
        {
            dispatch.Cancel();
        }
        catch (Exception error)
        {
            logger.LogError(error, "Canceling a stale title bar search query failed.");
        }
    }

    private void CompleteDispatch(QueryDispatch dispatch)
    {
        lock (gate)
        {
            if (ReferenceEquals(activeQueryDispatch, dispatch))
            {
                activeQueryDispatch = null;
            }
        }

        dispatch.Complete();
    }

    private FlourishTitleBarSearchState CreateSnapshot()
    {
        return new FlourishTitleBarSearchState(
            text,
            options.SearchPlaceholder,
            options.IsTitlebarSearchEnabled,
            focusRequested,
            version
        );
    }

    private void Unsubscribe(Guid id)
    {
        lock (gate)
        {
            handlers.Remove(id);
        }
    }

    private sealed class Subscription(TitleBarSearchService owner, Guid id) : IRegistration
    {
        private TitleBarSearchService? owner = owner;

        public bool IsRegistered => Volatile.Read(ref owner) is not null;

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.Unsubscribe(id);
        }
    }

    private sealed class QueryDispatch
    {
        private readonly Lock gate = new();
        private CancellationTokenSource? cancellation = new();

        internal CancellationToken Token
        {
            get
            {
                lock (gate)
                {
                    return cancellation?.Token ?? new CancellationToken(canceled: true);
                }
            }
        }

        internal void Cancel()
        {
            lock (gate)
            {
                cancellation?.Cancel();
            }
        }

        internal void Complete()
        {
            CancellationTokenSource? completed;
            lock (gate)
            {
                completed = cancellation;
                cancellation = null;
            }

            completed?.Dispose();
        }
    }
}
