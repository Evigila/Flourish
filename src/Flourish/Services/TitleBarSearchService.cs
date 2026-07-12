using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Flourish.Services;

internal sealed class TitleBarSearchService(
    FlourishShellOptions options,
    IServiceProvider serviceProvider,
    ILogger<TitleBarSearchService> logger
) : ITitleBarSearchService, IDisposable
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, Func<FlourishTitleBarSearchChangedEventArgs, CancellationToken, ValueTask>> handlers = [];
    private CancellationTokenSource queryCancellation = new();
    private string text = string.Empty;
    private bool focusRequested;
    private long version;
    private bool isDisposed;

    public event EventHandler<FlourishTitleBarSearchStateChangedEventArgs>? StateChanged;

    public event EventHandler<FlourishTitleBarSearchChangedEventArgs>? QueryChanged;

    public FlourishTitleBarSearchState Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

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

    public IDisposable Subscribe(
        Func<FlourishTitleBarSearchChangedEventArgs, CancellationToken, ValueTask> handler
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
        Func<FlourishTitleBarSearchChangedEventArgs, CancellationToken, ValueTask>[] subscribers;
        FlourishTitleBarSearchChangedEventArgs args;
        FlourishTitleBarSearchState state;
        CancellationToken token;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            text = value;
            focusRequested = false;
            version++;
            queryCancellation.Cancel();
            queryCancellation.Dispose();
            queryCancellation = new CancellationTokenSource();
            token = queryCancellation.Token;
            subscribers = handlers.Values.ToArray();
            args = new FlourishTitleBarSearchChangedEventArgs(value, version);
            state = CreateSnapshot();
        }

        try
        {
            options.TitlebarSearchTextChanged?.Invoke(serviceProvider, value);
        }
        catch (Exception error)
        {
            logger.LogError(error, "The configured title bar search handler failed.");
        }

        StateChanged?.Invoke(this, new FlourishTitleBarSearchStateChangedEventArgs(state));
        QueryChanged?.Invoke(this, args);
        _ = DispatchAsync(subscribers, args, token);
    }

    internal void AcknowledgeFocusRequest()
    {
        lock (gate)
        {
            focusRequested = false;
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            handlers.Clear();
            queryCancellation.Cancel();
            queryCancellation.Dispose();
        }
    }

    private async Task DispatchAsync(
        IReadOnlyList<Func<FlourishTitleBarSearchChangedEventArgs, CancellationToken, ValueTask>> subscribers,
        FlourishTitleBarSearchChangedEventArgs args,
        CancellationToken cancellationToken
    )
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

    private void UpdateState(Action update)
    {
        FlourishTitleBarSearchState state;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            var previous = CreateSnapshot();
            update();
            var candidate = CreateSnapshot();
            if (candidate == previous)
            {
                return;
            }

            version++;
            state = CreateSnapshot();
        }

        StateChanged?.Invoke(this, new FlourishTitleBarSearchStateChangedEventArgs(state));
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

    private sealed class Subscription(TitleBarSearchService owner, Guid id) : IDisposable
    {
        private TitleBarSearchService? owner = owner;

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.Unsubscribe(id);
        }
    }
}
