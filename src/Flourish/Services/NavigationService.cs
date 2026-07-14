using System.Windows.Controls;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed class NavigationService : INavigationService, IFrameNavigationService
{
    private readonly INavigationPageProvider pageProvider;
    private readonly PageHistoryService pageHistoryService;
    private readonly NavigationRouteRegistry routeRegistry;
    private readonly object routeChangeGate = new();
    private INavigationContentHost? contentHost;
    private Dispatcher? dispatcher;
    private object? currentParameter;
    private long lastAppliedRouteVersion = -1;

    public NavigationService(
        PageCacheService pageCacheService,
        PageHistoryService pageHistoryService,
        NavigationRouteRegistry routeRegistry
    )
        : this((INavigationPageProvider)pageCacheService, pageHistoryService, routeRegistry) { }

    internal NavigationService(
        INavigationPageProvider pageProvider,
        PageHistoryService pageHistoryService,
        NavigationRouteRegistry routeRegistry
    )
    {
        this.pageProvider = pageProvider ?? throw new ArgumentNullException(nameof(pageProvider));
        this.pageHistoryService =
            pageHistoryService ?? throw new ArgumentNullException(nameof(pageHistoryService));
        this.routeRegistry =
            routeRegistry ?? throw new ArgumentNullException(nameof(routeRegistry));
        routeRegistry.Changed += RouteRegistry_Changed;
        lock (routeChangeGate)
        {
            lastAppliedRouteVersion = Math.Max(
                lastAppliedRouteVersion,
                routeRegistry.Current.Version
            );
        }
    }

    public event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    public event EventHandler<FlourishNavigationStateChangedEventArgs>? StateChanged;

    public bool CanGoBack => pageHistoryService.CanGoBack;

    public bool CanGoForward => pageHistoryService.CanGoForward;

    public Type? CurrentSourcePageType { get; private set; }

    public string? CurrentNavigationKey { get; private set; }

    public object? CurrentParameter => currentParameter;

    public IReadOnlyCollection<string> Routes => routeRegistry.Current.Routes.Keys.ToArray();

    public void Initialize(Frame contentFrame)
    {
        ArgumentNullException.ThrowIfNull(contentFrame);
        dispatcher = contentFrame.Dispatcher;
        Initialize(new FrameNavigationContentHost(contentFrame));
    }

    internal void Initialize(INavigationContentHost contentHost)
    {
        this.contentHost = contentHost ?? throw new ArgumentNullException(nameof(contentHost));
    }

    public bool Navigate(string navigationKey, object? parameter = null, bool addToBackStack = true)
    {
        return NavigateCore(navigationKey, parameter, addToBackStack);
    }

    public bool CanNavigate(string navigationKey)
    {
        return routeRegistry.Contains(navigationKey);
    }

    public bool Navigate<TPage>(object? parameter = null, bool addToBackStack = true)
        where TPage : Page
    {
        if (!routeRegistry.TryGet(typeof(TPage), out var route))
        {
            throw new InvalidOperationException(
                $"Page type '{typeof(TPage).FullName}' is not registered for navigation."
            );
        }

        return Navigate(route.NavigationKey, parameter, addToBackStack);
    }

    public async Task<bool> NavigateAsync(
        string navigationKey,
        object? parameter = null,
        bool addToBackStack = true,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            return Navigate(navigationKey, parameter, addToBackStack);
        }

        var operation = dispatcher.InvokeAsync(
            () => Navigate(navigationKey, parameter, addToBackStack),
            DispatcherPriority.Normal,
            cancellationToken
        );
        return await operation.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public bool GoBack()
    {
        if (!pageHistoryService.TryPopBack(out var entry))
        {
            return false;
        }

        var currentEntry = CreateCurrentEntry();
        return NavigateCore(
            entry.NavigationKey,
            entry.Parameter,
            false,
            commitHistory: () =>
            {
                if (currentEntry is not null)
                {
                    pageHistoryService.PushForward(currentEntry);
                }
            },
            rollbackHistory: () => pageHistoryService.Push(entry)
        );
    }

    public bool GoForward()
    {
        if (!pageHistoryService.TryPopForward(out var entry))
        {
            return false;
        }

        var currentEntry = CreateCurrentEntry();
        return NavigateCore(
            entry.NavigationKey,
            entry.Parameter,
            false,
            commitHistory: () =>
            {
                if (currentEntry is not null)
                {
                    pageHistoryService.Push(currentEntry);
                }
            },
            rollbackHistory: () => pageHistoryService.PushForward(entry)
        );
    }

    public void ClearBackStack()
    {
        pageHistoryService.ClearBack();
        RaiseStateChanged();
    }

    public void ClearForwardStack()
    {
        pageHistoryService.ClearForward();
        RaiseStateChanged();
    }

    public void ClearHistory()
    {
        pageHistoryService.Clear();
        RaiseStateChanged();
    }

    private bool NavigateCore(
        string navigationKey,
        object? parameter,
        bool addToBackStack,
        Action? commitHistory = null,
        Action? rollbackHistory = null
    )
    {
        var historyCommitted = false;

        try
        {
            if (contentHost is null)
            {
                throw new InvalidOperationException(
                    "NavigationService must be initialized with a frame."
                );
            }

            if (!routeRegistry.TryGet(navigationKey, out var route))
            {
                throw new InvalidOperationException(
                    $"Navigation key '{navigationKey}' is not registered. Check its spelling and casing. Keys are generated from Page class names by removing the trailing, case-sensitive 'Page' suffix (for example, SettingsPage becomes 'Settings')."
                );
            }


            var sourcePageType = route.PageType;

            if (CurrentNavigationKey == navigationKey && Equals(currentParameter, parameter))
            {
                rollbackHistory?.Invoke();
                return false;
            }

            var page = pageProvider.GetPage(sourcePageType);
            if (!contentHost.Navigate(page))
            {
                rollbackHistory?.Invoke();
                return false;
            }

            commitHistory?.Invoke();
            if (addToBackStack && CreateCurrentEntry() is { } currentEntry)
            {
                pageHistoryService.Push(currentEntry);
                pageHistoryService.ClearForward();
            }

            historyCommitted = true;
            CurrentSourcePageType = sourcePageType;
            CurrentNavigationKey = navigationKey;
            currentParameter = parameter;

            Navigated?.Invoke(
                this,
                new FlourishNavigatedEventArgs(navigationKey, sourcePageType, page, parameter)
            );
            RaiseStateChanged();
            return true;
        }
        catch
        {
            if (!historyCommitted)
            {
                rollbackHistory?.Invoke();
            }

            throw;
        }
    }

    private FlourishPageStackEntry? CreateCurrentEntry()
    {
        return CurrentNavigationKey is null
            ? null
            : new FlourishPageStackEntry(CurrentNavigationKey, currentParameter);
    }

    private void RouteRegistry_Changed(
        object? sender,
        FlourishNavigationRoutesChangedEventArgs e
    )
    {
        var historyChanged = false;
        lock (routeChangeGate)
        {
            if (e.Current.Version <= lastAppliedRouteVersion)
            {
                return;
            }

            lastAppliedRouteVersion = e.Current.Version;
            var registeredKeys = e.Current.Routes.Keys.ToHashSet(StringComparer.Ordinal);
            var staleHistoryKeys = pageHistoryService
                .BackStack.Concat(pageHistoryService.ForwardStack)
                .Select(entry => entry.NavigationKey)
                .Where(key => !registeredKeys.Contains(key))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            foreach (var staleHistoryKey in staleHistoryKeys)
            {
                pageHistoryService.Remove(staleHistoryKey);
            }

            historyChanged =
                staleHistoryKeys.Length > 0
                || e.ChangeKind == FlourishRuntimeChangeKind.Removed;
        }

        if (historyChanged)
        {
            RaiseStateChanged();
        }
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(
            this,
            new FlourishNavigationStateChangedEventArgs(
                new FlourishNavigationState(
                    CurrentNavigationKey,
                    CurrentSourcePageType,
                    CanGoBack,
                    CanGoForward
                )
            )
        );
    }
}
