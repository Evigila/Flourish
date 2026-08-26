using System;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationService
{
    private readonly INavigationPageProvider pageProvider;
    private readonly PageHistoryService pageHistoryService;
    private readonly NavigationRouteRegistry routeRegistry;
    private readonly Lock navigationGate = new();
    private INavigationContentHost? contentHost;
    private Dispatcher? dispatcher;
    private Type? currentSourcePageType;
    private string? currentNavigationKey;
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
        lock (navigationGate)
        {
            lastAppliedRouteVersion = Math.Max(
                lastAppliedRouteVersion,
                routeRegistry.Current.Version
            );
        }
    }

    public event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    public event EventHandler<EventArgs>? StateChanged;

    public bool CanGoBack
    {
        get
        {
            lock (navigationGate)
            {
                return pageHistoryService.CanGoBack;
            }
        }
    }

    public bool CanGoForward
    {
        get
        {
            lock (navigationGate)
            {
                return pageHistoryService.CanGoForward;
            }
        }
    }

    public Type? CurrentSourcePageType
    {
        get
        {
            lock (navigationGate)
            {
                return currentSourcePageType;
            }
        }
    }

    public string? CurrentNavigationKey
    {
        get
        {
            lock (navigationGate)
            {
                return currentNavigationKey;
            }
        }
    }

    public object? CurrentParameter
    {
        get
        {
            lock (navigationGate)
            {
                return currentParameter;
            }
        }
    }

    internal void Attach(Frame contentFrame)
    {
        ArgumentNullException.ThrowIfNull(contentFrame);
        lock (navigationGate)
        {
            dispatcher = contentFrame.Dispatcher;
            contentHost = new FrameNavigationContentHost(contentFrame);
        }
    }

    internal void Attach(INavigationContentHost contentHost)
    {
        ArgumentNullException.ThrowIfNull(contentHost);
        lock (navigationGate)
        {
            this.contentHost = contentHost;
        }
    }

    public bool Navigate(string navigationKey, object? parameter = null, bool addToBackStack = true)
    {
        NavigationNotifications? notifications;
        bool navigated;
        lock (navigationGate)
        {
            navigated = NavigateCore(
                navigationKey,
                parameter,
                addToBackStack,
                out notifications
            );
        }

        RaiseNotifications(notifications);
        return navigated;
    }

    public bool CanNavigate(string navigationKey)
    {
        return routeRegistry.Get(navigationKey) is not null;
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
        Dispatcher? currentDispatcher;
        lock (navigationGate)
        {
            currentDispatcher = dispatcher;
        }

        if (currentDispatcher is null || currentDispatcher.CheckAccess())
        {
            return Navigate(navigationKey, parameter, addToBackStack);
        }

        var operation = currentDispatcher.InvokeAsync(
            () => Navigate(navigationKey, parameter, addToBackStack),
            DispatcherPriority.Normal,
            cancellationToken
        );
        return await operation.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public bool GoBack()
    {
        NavigationNotifications? notifications;
        bool navigated;
        lock (navigationGate)
        {
            if (!pageHistoryService.TryPopBack(out var entry))
            {
                return false;
            }

            var currentEntry = CreateCurrentEntry();
            navigated = NavigateCore(
                entry.NavigationKey,
                entry.Parameter,
                false,
                out notifications,
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

        RaiseNotifications(notifications);
        return navigated;
    }

    public bool GoForward()
    {
        NavigationNotifications? notifications;
        bool navigated;
        lock (navigationGate)
        {
            if (!pageHistoryService.TryPopForward(out var entry))
            {
                return false;
            }

            var currentEntry = CreateCurrentEntry();
            navigated = NavigateCore(
                entry.NavigationKey,
                entry.Parameter,
                false,
                out notifications,
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

        RaiseNotifications(notifications);
        return navigated;
    }

    public void ClearBackStack()
    {
        EventArgs stateChanged;
        lock (navigationGate)
        {
            pageHistoryService.ClearBack();
            stateChanged = CreateStateChangedEventArgsLocked();
        }

        StateChanged?.Invoke(this, stateChanged);
    }

    public void ClearForwardStack()
    {
        EventArgs stateChanged;
        lock (navigationGate)
        {
            pageHistoryService.ClearForward();
            stateChanged = CreateStateChangedEventArgsLocked();
        }

        StateChanged?.Invoke(this, stateChanged);
    }

    public void ClearHistory()
    {
        EventArgs stateChanged;
        lock (navigationGate)
        {
            pageHistoryService.Clear();
            stateChanged = CreateStateChangedEventArgsLocked();
        }

        StateChanged?.Invoke(this, stateChanged);
    }

    private bool NavigateCore(
        string navigationKey,
        object? parameter,
        bool addToBackStack,
        out NavigationNotifications? notifications,
        Action? commitHistory = null,
        Action? rollbackHistory = null
    )
    {
        notifications = null;
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

            if (currentNavigationKey == navigationKey && Equals(currentParameter, parameter))
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
            currentSourcePageType = sourcePageType;
            currentNavigationKey = navigationKey;
            currentParameter = parameter;

            notifications = new NavigationNotifications(
                new FlourishNavigatedEventArgs(navigationKey, sourcePageType, page, parameter),
                CreateStateChangedEventArgsLocked()
            );
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
        return currentNavigationKey is null
            ? null
            : new FlourishPageStackEntry(currentNavigationKey, currentParameter);
    }

    private void RouteRegistry_Changed(object? sender, FlourishNavigationRoutesChangedEventArgs e)
    {
        EventArgs? stateChanged = null;
        lock (navigationGate)
        {
            if (e.Current.Version <= lastAppliedRouteVersion)
            {
                return;
            }

            lastAppliedRouteVersion = e.Current.Version;
            var historyChanged = pageHistoryService.RemoveWhere(entry =>
                !e.Current.Routes.ContainsKey(entry.NavigationKey)
            );
            if (historyChanged || e.ChangeKind == FlourishRuntimeChangeKind.Removed)
            {
                stateChanged = CreateStateChangedEventArgsLocked();
            }
        }

        if (stateChanged is not null)
        {
            StateChanged?.Invoke(this, stateChanged);
        }
    }

    private EventArgs CreateStateChangedEventArgsLocked()
    {
        return EventArgs.Empty;
    }

    private void RaiseNotifications(NavigationNotifications? notifications)
    {
        if (notifications is not { } value)
        {
            return;
        }

        Navigated?.Invoke(this, value.Navigated);
        StateChanged?.Invoke(this, value.StateChanged);
    }

    private sealed record NavigationNotifications(
        FlourishNavigatedEventArgs Navigated,
        EventArgs StateChanged
    );
}
