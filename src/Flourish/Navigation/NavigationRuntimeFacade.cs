using System.Linq;

using System;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationRuntimeFacade : INavigationService, IDisposable
{
    private readonly Lock gate = new();
    private readonly NavigationService navigation;
    private readonly NavigationRouteRegistry routes;
    private readonly NavigationMenuService menu;
    private readonly NavigationPanelService panel;
    private readonly PageCacheService cache;
    private FlourishNavigationState current;
    private long version;
    private bool isDisposed;

    public NavigationRuntimeFacade(
        NavigationService navigation,
        NavigationRouteRegistry routes,
        NavigationMenuService menu,
        NavigationPanelService panel,
        PageCacheService cache
    )
    {
        this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        this.routes = routes ?? throw new ArgumentNullException(nameof(routes));
        this.menu = menu ?? throw new ArgumentNullException(nameof(menu));
        this.panel = panel ?? throw new ArgumentNullException(nameof(panel));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        current = CaptureCurrent();
        navigation.StateChanged += Source_Changed;
        routes.Changed += Source_Changed;
        menu.Changed += Source_Changed;
        panel.Changed += Source_Changed;
        cache.Changed += Source_Changed;
    }

    public FlourishNavigationState Current
    {
        get
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref isDisposed), this);
            return Volatile.Read(ref current);
        }
    }

    public event EventHandler<FlourishStateChangedEventArgs<FlourishNavigationState>>? Changed;

    public event EventHandler<FlourishNavigatedEventArgs>? Navigated
    {
        add => navigation.Navigated += value;
        remove => navigation.Navigated -= value;
    }

    public bool CanNavigate(string navigationKey) => navigation.CanNavigate(navigationKey);
    public bool Navigate(string navigationKey, object? parameter = null, bool addToBackStack = true) =>
        navigation.Navigate(navigationKey, parameter, addToBackStack);
    public bool Navigate<TPage>(object? parameter = null, bool addToBackStack = true)
        where TPage : Page => navigation.Navigate<TPage>(parameter, addToBackStack);
    public Task<bool> NavigateAsync(
        string navigationKey,
        object? parameter = null,
        bool addToBackStack = true,
        CancellationToken cancellationToken = default
    ) => navigation.NavigateAsync(navigationKey, parameter, addToBackStack, cancellationToken);
    public bool GoBack() => navigation.GoBack();
    public bool GoForward() => navigation.GoForward();
    public void ClearBackStack() => navigation.ClearBackStack();
    public void ClearForwardStack() => navigation.ClearForwardStack();
    public void ClearHistory() => navigation.ClearHistory();

    public IRegistration AddNavigable<TPage>(
        string? navigationKey = null,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Enabled
    )
        where TPage : Page
    {
        navigationKey = string.IsNullOrWhiteSpace(navigationKey)
            ? FlourishServiceCollectionExtensions.CreateDefaultNavigationKey(typeof(TPage))
            : navigationKey.Trim();
        return routes.Set(new FlourishNavigationRoute(navigationKey, typeof(TPage), cacheMode));
    }

    public IRegistration SetNavigable(FlourishNavigationRoute route) => routes.Set(route);
    public bool RemoveNavigable(string navigationKey) => routes.Remove(navigationKey);
    public FlourishNavigationRoute? GetNavigable(string navigationKey) => routes.Get(navigationKey);
    public void SetMenu(Action<INavigationMenuEditor> update) => menu.Set(update);
    public void SetEnabled(bool enabled) => panel.SetEnabled(enabled);
    public void SetDirection(NavigationPanelDirection direction) => panel.SetDirection(direction);
    public void SetPanelWidth(double openWidth, double closedWidth, double maxWidth, double minWidth) =>
        panel.SetPanelWidth(openWidth, closedWidth, maxWidth, minWidth);
    public void Open(bool animate = true) => panel.Open(animate);
    public void Close(bool animate = true) => panel.Close(animate);
    public void Toggle(bool animate = true) => panel.Toggle(animate);
    public void SetCacheMode(Type pageType, FlourishPageCacheMode cacheMode)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        foreach (var route in routes.Current.Routes.Values.Where(route => route.PageType == pageType))
        {
            routes.SetCacheMode(route.NavigationKey, cacheMode);
        }

        cache.SetCacheMode(pageType, cacheMode);
    }
    public bool Evict(Type pageType) => cache.Evict(pageType);
    public void ClearCache() => cache.Clear();
    public bool IsCached(Type pageType) => cache.Contains(pageType);

    public void Dispose()
    {
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
        }

        navigation.StateChanged -= Source_Changed;
        routes.Changed -= Source_Changed;
        menu.Changed -= Source_Changed;
        panel.Changed -= Source_Changed;
        cache.Changed -= Source_Changed;
    }

    private void Source_Changed(object? sender, EventArgs args)
    {
        FlourishNavigationState snapshot;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            version++;
            snapshot = CaptureCurrent();
            Volatile.Write(ref current, snapshot);
        }

        Changed?.Invoke(this, new FlourishStateChangedEventArgs<FlourishNavigationState>(snapshot));
    }

    private FlourishNavigationState CaptureCurrent() => new(
        navigation.CurrentNavigationKey,
        navigation.CurrentSourcePageType,
        navigation.CurrentParameter,
        navigation.CanGoBack,
        navigation.CanGoForward,
        routes.Current,
        menu.Current,
        panel.Current,
        cache.Current,
        version
    );
}
