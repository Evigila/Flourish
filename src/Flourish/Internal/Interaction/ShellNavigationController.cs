using System.Windows.Input;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Internal.Interaction;

internal sealed class ShellNavigationController : IDisposable
{
    private readonly FlourishNavigationPane pane;
    private readonly FlourishShellContentHost contentHost;
    private readonly FlourishTitlebar titlebar;
    private readonly NavigationService navigation;
    private readonly NavigationPanelService panelService;
    private readonly NavigationMenuService menuService;
    private readonly ICommandDispatcher commandDispatcher;
    private readonly FlourishShellOptions options;
    private readonly Dictionary<string, FlourishNavigationItem> itemsByKey = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, FlourishNavigationItem> itemsByPage = [];
    private readonly Dictionary<NavigationTreeKey, FlourishNavigationItem> parentsByKey = [];
    private readonly Dictionary<NavigationTreeKey, List<FlourishNavigationItem>>
        childrenByParentKey = [];
    private FlourishNavigationItem? firstItem;
    private FlourishNavigationItem? selectedItem;
    private FlourishNavigationItem? activeChildParentItem;
    private FlourishTitleBarState titleBarState;
    private bool isInitialized;
    private bool isDisposed;

    internal ShellNavigationController(
        FlourishNavigationPane pane,
        FlourishShellContentHost contentHost,
        FlourishTitlebar titlebar,
        NavigationService navigation,
        NavigationPanelService panelService,
        NavigationMenuService menuService,
        ICommandDispatcher commandDispatcher,
        FlourishShellOptions options,
        FlourishTitleBarState initialTitleBarState
    )
    {
        this.pane = pane ?? throw new ArgumentNullException(nameof(pane));
        this.contentHost =
            contentHost ?? throw new ArgumentNullException(nameof(contentHost));
        this.titlebar = titlebar ?? throw new ArgumentNullException(nameof(titlebar));
        this.navigation =
            navigation ?? throw new ArgumentNullException(nameof(navigation));
        this.panelService =
            panelService ?? throw new ArgumentNullException(nameof(panelService));
        this.menuService =
            menuService ?? throw new ArgumentNullException(nameof(menuService));
        this.commandDispatcher =
            commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        titleBarState =
            initialTitleBarState
            ?? throw new ArgumentNullException(nameof(initialTitleBarState));
    }

    internal event EventHandler<NavigationLayoutRequestedEventArgs>? LayoutRequested;

    internal FlourishNavigationPanelState CurrentPanelState => panelService.Current;

    internal bool IsPanelEnabled => panelService.Current.IsEnabled;

    internal void Init()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        pane.ItemRequested += Pane_ItemRequested;
        titlebar.BackRequested += Titlebar_BackRequested;
        titlebar.ForwardRequested += Titlebar_ForwardRequested;
        titlebar.NavigationToggleRequested += Titlebar_NavigationToggleRequested;
        panelService.Changed += PanelService_Changed;
        menuService.Changed += MenuService_Changed;

        BuildItems();
        ApplyPanelView(panelService.Current);
        ApplyTitleBarState(titleBarState);
    }

    internal void NavigateInitial()
    {
        EnsureInitialized();
        var initialItem =
            GetItem(options.InitialNavigationKey)
            ?? (
                options.InitialNavigationPageType is null
                    ? null
                    : itemsByPage.GetValueOrDefault(options.InitialNavigationPageType)
            )
            ?? firstItem;

        if (initialItem is null)
        {
            return;
        }

        SelectItem(initialItem);
        ActivateItem(initialItem, addToBackStack: false, toggleChildren: false);
    }

    internal void OnNavigated(FlourishNavigatedEventArgs e)
    {
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(e);
        UpdateTitlebarBreadcrumbNavigation();
        UpdateBreadcrumb(e.SourcePageType);
        SelectItem(e.SourcePageType);
    }

    internal void ApplyTitleBarState(FlourishTitleBarState state)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        titleBarState = state ?? throw new ArgumentNullException(nameof(state));
        titlebar.SetNavigationToggleVisibility(
            state.IsEnabled && state.IsNavigationToggleVisible && panelService.Current.IsEnabled
        );
        UpdateTitlebarBreadcrumbNavigation();
        if (navigation.CurrentSourcePageType is { } pageType)
        {
            UpdateBreadcrumb(pageType);
        }
        else
        {
            contentHost.SetBreadcrumbVisibility(IsBreadcrumbFeatureEnabled());
        }
    }

    internal void CommitPaneChrome(bool effectiveOpen)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        pane.SetCompact(!effectiveOpen);
    }

    internal void RecordOpenWidth(double width)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        panelService.RecordOpenWidth(width);
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        if (isInitialized)
        {
            pane.ItemRequested -= Pane_ItemRequested;
            titlebar.BackRequested -= Titlebar_BackRequested;
            titlebar.ForwardRequested -= Titlebar_ForwardRequested;
            titlebar.NavigationToggleRequested -= Titlebar_NavigationToggleRequested;
            panelService.Changed -= PanelService_Changed;
            menuService.Changed -= MenuService_Changed;
        }

        if (activeChildParentItem is not null)
        {
            activeChildParentItem.IsActiveChildParent = false;
            activeChildParentItem = null;
        }

        pane.SetSelectedItem(null);
        LayoutRequested = null;
    }

    private void BuildItems()
    {
        var selectedNavigationKey = navigation.CurrentNavigationKey;
        itemsByKey.Clear();
        itemsByPage.Clear();
        parentsByKey.Clear();
        childrenByParentKey.Clear();
        activeChildParentItem = null;
        firstItem = null;
        selectedItem = null;

        foreach (var item in options.NavigationItems.Concat(options.FixedNavigationItems))
        {
            item.Validate();
            if (!item.IsNavigationItem)
            {
                continue;
            }

            if (!itemsByKey.ContainsKey(item.Key))
            {
                itemsByKey[item.Key] = item;
            }

            IndexTreeItem(item);
            if (item.PageType is not null)
            {
                firstItem ??= item;
                itemsByPage[item.PageType] = item;
            }
        }

        pane.SetItems(options.NavigationItems, options.FixedNavigationItems);
        if (
            selectedNavigationKey is not null
            && GetItem(selectedNavigationKey) is { } selected
        )
        {
            SelectItem(selected);
        }
        else
        {
            RestoreSelectedItem();
        }
    }

    private void IndexTreeItem(FlourishNavigationItem item)
    {
        if (item.ParentId != 0)
        {
            parentsByKey[CreateTreeKey(item, item.ParentId)] = item;
        }

        if (item.ChildId == 0)
        {
            return;
        }

        var childKey = CreateTreeKey(item, item.ChildId);
        if (!childrenByParentKey.TryGetValue(childKey, out var children))
        {
            children = [];
            childrenByParentKey[childKey] = children;
        }

        children.Add(item);
    }

    private void PanelService_Changed(
        object? sender,
        FlourishNavigationPanelChangedEventArgs e
    )
    {
        Dispatch(() =>
        {
            ApplyPanelView(e.Current);
            titlebar.SetNavigationToggleVisibility(
                titleBarState.IsEnabled
                && titleBarState.IsNavigationToggleVisible
                && e.Current.IsEnabled
            );
            LayoutRequested?.Invoke(
                this,
                new NavigationLayoutRequestedEventArgs(e.Current, e.Animate)
            );
        });
    }

    private void MenuService_Changed(
        object? sender,
        FlourishNavigationMenuChangedEventArgs e
    )
    {
        Dispatch(() =>
        {
            BuildItems();
            if (navigation.CurrentSourcePageType is { } pageType)
            {
                UpdateBreadcrumb(pageType);
            }
        });
    }

    private void ApplyPanelView(FlourishNavigationPanelState state)
    {
        pane.SetEnabled(state.IsEnabled);
        pane.SetDirection(state.Direction);
    }

    private void Pane_ItemRequested(object? sender, NavigationItemRequestedEventArgs e)
    {
        var item = e.Item;
        if (!item.IsNavigationItem || !item.IsEnabled)
        {
            if (e.Kind == NavigationItemRequestKind.Selection)
            {
                RestoreSelectedItem();
            }
            return;
        }

        OpenPaneForCollapsedParent(item);
        if (e.Kind == NavigationItemRequestKind.Invoke)
        {
            if (item.IsCommandItem)
            {
                ActivateCommandItem(item);
                return;
            }

            SelectItem(item);
            ActivateItem(item, addToBackStack: true);
            return;
        }

        if (!item.IsPageItem)
        {
            RestoreSelectedItem();
            return;
        }

        SelectItem(item);
        ActivateItem(item, addToBackStack: true, toggleChildren: false);
    }

    private void Titlebar_BackRequested(object? sender, EventArgs e)
    {
        if (navigation.CanGoBack)
        {
            navigation.GoBack();
        }
    }

    private void Titlebar_ForwardRequested(object? sender, EventArgs e)
    {
        if (navigation.CanGoForward)
        {
            navigation.GoForward();
        }
    }

    private void Titlebar_NavigationToggleRequested(object? sender, EventArgs e)
    {
        var state = panelService.Current;
        if (!state.IsEnabled)
        {
            return;
        }

        var shouldOpen = !state.IsOpen;
        if (!shouldOpen)
        {
            CollapseAllChildren();
        }

        panelService.Toggle(animate: true);
        if (shouldOpen)
        {
            RestoreSelectedItem();
        }
    }

    private void ActivateCommandItem(FlourishNavigationItem item)
    {
        ActivateItem(item, addToBackStack: true);
        RestoreSelectedItem();
        Keyboard.ClearFocus();
    }

    private void ActivateItem(
        FlourishNavigationItem item,
        bool addToBackStack,
        bool toggleChildren = true
    )
    {
        if (!item.IsEnabled)
        {
            return;
        }

        if (toggleChildren)
        {
            OpenPaneForCollapsedParent(item);
        }

        if (toggleChildren && item.HasChildren)
        {
            ToggleChildren(item);
        }

        if (item.IsCommandItem)
        {
            if (!item.HasChildren && !string.IsNullOrWhiteSpace(item.CommandKey))
            {
                _ = commandDispatcher
                    .ExecuteAsync(item.CommandKey, source: CommandSource.Navigation)
                    .AsTask();
            }
            return;
        }

        if (item.PageType is null || navigation.CurrentNavigationKey == item.Key)
        {
            return;
        }

        navigation.Navigate(item.Key, addToBackStack: addToBackStack);
        if (!addToBackStack && navigation.CanGoBack)
        {
            navigation.ClearBackStack();
            UpdateTitlebarBreadcrumbNavigation();
        }
    }

    private void ToggleChildren(FlourishNavigationItem parent)
    {
        parent.IsExpanded = !parent.IsExpanded;
        menuService.RecordExpansion(parent.Id, parent.IsExpanded);
        SetChildItemsVisibility(parent, parent.IsExpanded);
    }

    private void OpenPaneForCollapsedParent(FlourishNavigationItem item)
    {
        if (panelService.Current.IsOpen || !item.HasChildren)
        {
            return;
        }

        panelService.Open(animate: true);
    }

    private void CollapseAllChildren()
    {
        foreach (var parent in parentsByKey.Values)
        {
            parent.IsExpanded = false;
            menuService.RecordExpansion(parent.Id, expanded: false);
            SetChildItemsVisibility(parent, isVisible: false);
        }
    }

    private void ExpandAncestorsForSelection(FlourishNavigationItem item)
    {
        if (item.ChildId == 0 || FindParentItem(item) is not { } parent)
        {
            return;
        }

        parent.IsExpanded = true;
        menuService.RecordExpansion(parent.Id, expanded: true);
        SetChildItemsVisibility(parent, isVisible: true);
    }

    private void SetChildItemsVisibility(FlourishNavigationItem parent, bool isVisible)
    {
        foreach (var child in GetChildItems(parent))
        {
            child.IsTreeVisible = isVisible;
        }
    }

    private IEnumerable<FlourishNavigationItem> GetChildItems(
        FlourishNavigationItem parent
    )
    {
        return
            parent.ParentId != 0
            && childrenByParentKey.TryGetValue(
                CreateTreeKey(parent, parent.ParentId),
                out var children
            )
            ? children
            : [];
    }

    private FlourishNavigationItem? FindParentItem(FlourishNavigationItem child)
    {
        return child.ChildId == 0
            ? null
            : parentsByKey.GetValueOrDefault(CreateTreeKey(child, child.ChildId));
    }

    private FlourishNavigationItem? GetItem(string? key)
    {
        return key is not null && itemsByKey.TryGetValue(key, out var item) ? item : null;
    }

    private void SelectItem(Type pageType)
    {
        if (itemsByPage.TryGetValue(pageType, out var item))
        {
            SelectItem(item);
        }
    }

    private void SelectItem(FlourishNavigationItem item)
    {
        if (!item.IsPageItem)
        {
            RestoreSelectedItem();
            return;
        }

        selectedItem = item;
        ExpandAncestorsForSelection(item);
        UpdateActiveChildParent(item);
        pane.SetSelectedItem(item);
    }

    private void RestoreSelectedItem()
    {
        if (selectedItem is not null)
        {
            SelectItem(selectedItem);
            return;
        }

        pane.SetSelectedItem(null);
    }

    private void UpdateActiveChildParent(FlourishNavigationItem activeItem)
    {
        var parent =
            activeItem.IsPageItem && activeItem.ChildId != 0
                ? FindParentItem(activeItem)
                : null;
        if (activeChildParentItem == parent)
        {
            return;
        }

        if (activeChildParentItem is not null)
        {
            activeChildParentItem.IsActiveChildParent = false;
        }

        activeChildParentItem = parent;
        if (activeChildParentItem is not null)
        {
            activeChildParentItem.IsActiveChildParent = true;
        }
    }

    private void UpdateBreadcrumb(Type sourcePageType)
    {
        if (
            !IsBreadcrumbFeatureEnabled()
            || (
                titleBarState.BreadcrumbMode == BreadcrumbShowOption.Auto
                && !HasBreadcrumbNavigation()
            )
        )
        {
            contentHost.SetBreadcrumbVisibility(isVisible: false);
            return;
        }

        var label = itemsByPage.GetValueOrDefault(sourcePageType)?.Label ?? sourcePageType.Name;
        contentHost.SetBreadcrumb($"{titleBarState.ApplicationTitle} / {label}");
    }

    private void UpdateTitlebarBreadcrumbNavigation()
    {
        if (!titleBarState.IsEnabled)
        {
            titlebar.SetBreadcrumbNavigationState(
                isVisible: false,
                canGoBack: false,
                canGoForward: false
            );
            return;
        }

        var isVisible =
            IsBreadcrumbFeatureEnabled()
            && (
                titleBarState.BreadcrumbMode == BreadcrumbShowOption.Always
                || HasBreadcrumbNavigation()
            );
        titlebar.SetBreadcrumbNavigationState(
            isVisible,
            navigation.CanGoBack,
            navigation.CanGoForward
        );
    }

    private bool IsBreadcrumbFeatureEnabled()
    {
        return titleBarState.IsEnabled
            && titleBarState.IsBreadcrumbVisible
            && titleBarState.BreadcrumbMode != BreadcrumbShowOption.Hidden;
    }

    private bool HasBreadcrumbNavigation()
    {
        return navigation.CanGoBack || navigation.CanGoForward;
    }

    private void Dispatch(Action action)
    {
        void ExecuteIfActive()
        {
            if (
                isDisposed
                || pane.Dispatcher.HasShutdownStarted
                || pane.Dispatcher.HasShutdownFinished
            )
            {
                return;
            }
            action();
        }

        if (pane.Dispatcher.CheckAccess())
        {
            ExecuteIfActive();
            return;
        }

        if (
            isDisposed
            || pane.Dispatcher.HasShutdownStarted
            || pane.Dispatcher.HasShutdownFinished
        )
        {
            return;
        }

        _ = pane.Dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(ExecuteIfActive)
        );
    }

    private void EnsureInitialized()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (!isInitialized)
        {
            throw new InvalidOperationException(
                "The navigation controller has not been initialized."
            );
        }
    }

    private static NavigationTreeKey CreateTreeKey(
        FlourishNavigationItem item,
        int relationshipId
    ) =>
        new(item.IsFixed, item.GroupId, relationshipId);

    private readonly record struct NavigationTreeKey(
        bool IsFixed,
        int GroupId,
        int RelationshipId
    );
}

internal sealed class NavigationLayoutRequestedEventArgs(
    FlourishNavigationPanelState state,
    bool animate
) : EventArgs
{
    internal FlourishNavigationPanelState State { get; } =
        state ?? throw new ArgumentNullException(nameof(state));

    internal bool Animate { get; } = animate;
}
