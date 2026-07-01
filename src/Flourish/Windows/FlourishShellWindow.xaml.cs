using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;
using AcksheedSys.Flourish.Services;
using Brush = System.Windows.Media.Brush;
using Button = System.Windows.Controls.Button;
using FontFamily = System.Windows.Media.FontFamily;
using Orientation = System.Windows.Controls.Orientation;

namespace AcksheedSys.Flourish.Windows;

internal partial class FlourishShellWindow : Window
{
    private readonly INavigationService navigationService;
    private readonly IFrameNavigationService frameNavigationService;
    private readonly IFlourishToolbarService toolbarService;
    private readonly IFlourishStatusService statusService;
    private readonly ITrayIconService trayIconService;
    private readonly FontService fontService;
    private readonly MaterialEffectService materialEffectService;
    private readonly WindowFrameFixService windowFrameFixService;
    private readonly FlourishShellOptions options;
    private readonly Dictionary<string, FlourishNavigationItem> navigationItemsByKey = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, FlourishNavigationItem> navigationItemsByPage = [];
    private readonly Dictionary<Type, IReadOnlyList<Button>> toolbarButtonsByPageType = [];
    private IReadOnlyList<Button>? defaultToolbarButtons;
    private FlourishNavigationItem? firstNavigationItem;
    private Style toolbarButtonStyle = null!;
    private Style navigationListBoxItemStyle = null!;
    private Brush mutedTextBrush = null!;
    private FontFamily iconFontFamily = null!;
    private Type? activeToolbarPageType;
    private bool isDefaultToolbarActive;
    private bool isPaneOpen = true;
    private bool suppressNavigationSelection;

    public FlourishShellWindow(
        INavigationService navigationService,
        IFrameNavigationService frameNavigationService,
        IFlourishToolbarService toolbarService,
        IFlourishStatusService statusService,
        ITrayIconService trayIconService,
        FontService fontService,
        MaterialEffectService materialEffectService,
        WindowFrameFixService windowFrameFixService,
        FlourishShellOptions options
    )
    {
        InitializeComponent();

        this.navigationService = navigationService;
        this.frameNavigationService = frameNavigationService;
        this.toolbarService = toolbarService;
        this.statusService = statusService;
        this.trayIconService = trayIconService;
        this.fontService = fontService;
        this.materialEffectService = materialEffectService;
        this.windowFrameFixService = windowFrameFixService;
        this.options = options;

        fontService.Apply(this);
        CacheResources();
        ApplyOptions();
        BuildToolbarItems();
        BuildNavigationItems();
        BuildStatusItems();
        AttachTitlebarEvents();

        StateChanged += MainWindow_StateChanged;
        Closing += ShellWindow_Closing;
        frameNavigationService.Initialize(RootFrame);
        navigationService.Navigated += RootFrame_Navigated;

        NavigateToInitialPage();
    }

    private void CacheResources()
    {
        toolbarButtonStyle = (Style)FindResource("FlourishToolbarButtonStyle");
        navigationListBoxItemStyle = (Style)FindResource("FlourishNavigationListBoxItemStyle");
        mutedTextBrush = (Brush)FindResource("MutedTextBrush");
        iconFontFamily = (FontFamily)FindResource("FlourishIconFontFamily");
    }

    private void ApplyOptions()
    {
        ApplyWindowOptions();
        Title = options.Title;
        Titlebar.SetTitle(options.Title);
        Titlebar.SetSubtitle(options.Subtitle);
        Titlebar.SetSearchPlaceholder(options.SearchPlaceholder);
        Titlebar.SetLogo(options.LogoSource, options.LogoFallbackText);
        Titlebar.ConfigureVisibility(
            options.IsTitlebarSearchEnabled,
            IsBreadcrumbFeatureEnabled(),
            options.IsTitlebarNavigationToggleEnabled && options.IsNavigationPanelEnabled,
            options.IsTitlebarLogoEnabled,
            options.IsTitlebarTitleEnabled,
            options.IsTitlebarSubtitleEnabled,
            options.IsTitlebarProfileEnabled
        );
        PaneTitle.Text = options.PaneTitle;
        StatusTextBlock.Text = statusService.StatusText;
        NavigationPaneBorder.Visibility = options.IsNavigationPanelEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        BreadcrumbHost.Visibility = IsBreadcrumbFeatureEnabled()
            ? Visibility.Visible
            : Visibility.Collapsed;
        UpdateTitlebarBreadcrumbNavigation();

        ApplyNavigationPanelPlacement();
        isPaneOpen = options.IsNavigationPanelInitiallyOpen;
        ApplyNavigationPaneState();
        windowFrameFixService.Attach(this);
        materialEffectService.Attach(this, options.MaterialEffect);
        trayIconService.Initialize(this, options.Title);
    }

    private void ApplyWindowOptions()
    {
        MinWidth = options.WindowMinWidth;
        MinHeight = options.WindowMinHeight;
        MaxWidth = options.WindowMaxWidth;
        MaxHeight = options.WindowMaxHeight;
        Width = options.WindowWidth;
        Height = options.WindowHeight;
        WindowStartupLocation = options.WindowStartupLocation;
        ResizeMode = options.WindowResizeMode;
        Topmost = options.WindowTopmost;
        ShowInTaskbar = options.WindowShowInTaskbar;

        if (options.WindowLeft is { } left)
        {
            Left = left;
        }

        if (options.WindowTop is { } top)
        {
            Top = top;
        }

        WindowState = options.WindowState;
        Titlebar.SetMaximizeEnabled(ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip);
    }

    private void AttachTitlebarEvents()
    {
        Titlebar.BackRequested += Titlebar_BackRequested;
        Titlebar.ForwardRequested += Titlebar_ForwardRequested;
        Titlebar.NavigationToggleRequested += Titlebar_NavigationToggleRequested;
        Titlebar.MinimizeRequested += Titlebar_MinimizeRequested;
        Titlebar.MaximizeRequested += Titlebar_MaximizeRequested;
        Titlebar.CloseRequested += Titlebar_CloseRequested;
        Titlebar.DragRequested += Titlebar_DragRequested;
        Titlebar.ToggleWindowStateRequested += Titlebar_ToggleWindowStateRequested;
    }

    private void ApplyNavigationPanelPlacement()
    {
        if (options.NavigationPanelDirection == NavigationPanelDirection.Right)
        {
            Grid.SetColumn(ContentAreaGrid, 0);
            Grid.SetColumn(NavigationPaneBorder, 1);
            NavigationPaneBorder.BorderThickness = new Thickness(1, 0, 0, 0);
            return;
        }

        Grid.SetColumn(NavigationPaneBorder, 0);
        Grid.SetColumn(ContentAreaGrid, 1);
        NavigationPaneBorder.BorderThickness = new Thickness(0, 0, 1, 0);
    }

    private void SetPaneWidth(double width)
    {
        if (options.NavigationPanelDirection == NavigationPanelDirection.Right)
        {
            PaneColumn.Width = new GridLength(1, GridUnitType.Star);
            ContentColumn.Width = new GridLength(width);
            return;
        }

        PaneColumn.Width = new GridLength(width);
        ContentColumn.Width = new GridLength(1, GridUnitType.Star);
    }

    private void ApplyNavigationPaneState()
    {
        var isNavigationVisible = options.IsNavigationPanelEnabled;
        var isOpen = isNavigationVisible && isPaneOpen;
        var paneWidth = !isNavigationVisible
            ? 0
            : isOpen
                ? options.OpenPaneWidth
                : options.ClosedPaneWidth;

        SetPaneWidth(paneWidth);
        PaneTitle.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
        NavigationItemsHost.Tag = isOpen;
    }

    private void BuildToolbarItems(Type? pageType = null)
    {
        if (activeToolbarPageType == pageType && isDefaultToolbarActive == (pageType is null))
        {
            return;
        }

        ToolbarItemsHost.Children.Clear();
        var toolbarButtons = GetToolbarButtons(pageType);
        ToolbarHostBorder.Visibility =
            toolbarButtons.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        foreach (var button in toolbarButtons)
        {
            ToolbarItemsHost.Children.Add(button);
        }

        activeToolbarPageType = pageType;
        isDefaultToolbarActive = pageType is null;
    }

    private IReadOnlyList<Button> GetToolbarButtons(Type? pageType)
    {
        if (pageType is null)
        {
            return defaultToolbarButtons ??= CreateToolbarButtons(toolbarService.GetToolbarItems());
        }

        if (!toolbarButtonsByPageType.TryGetValue(pageType, out var toolbarButtons))
        {
            toolbarButtons = CreateToolbarButtons(toolbarService.GetToolbarItems(pageType));
            toolbarButtonsByPageType[pageType] = toolbarButtons;
        }

        return toolbarButtons;
    }

    private IReadOnlyList<Button> CreateToolbarButtons(IReadOnlyList<FlourishToolbarItem> items)
    {
        var buttons = new List<Button>(items.Count);
        foreach (var item in items)
        {
            buttons.Add(
                new Button
                {
                    Content = CreateIconTextContent(item.IconGlyph, item.DisplayName),
                    Margin = buttons.Count > 0 ? new Thickness(6, 0, 0, 0) : new Thickness(),
                    Style = toolbarButtonStyle,
                    Tag = item.CommandKey,
                }
            );
        }

        return buttons;
    }

    private void BuildNavigationItems()
    {
        navigationItemsByKey.Clear();
        navigationItemsByPage.Clear();
        firstNavigationItem = null;

        foreach (var item in options.NavigationItems)
        {
            item.Validate();
            firstNavigationItem ??= item;
            navigationItemsByKey[item.Key] = item;
            navigationItemsByPage[item.PageType] = item;
        }

        NavigationItemsHost.ItemContainerStyle = navigationListBoxItemStyle;
        NavigationItemsHost.ItemsSource = options.NavigationItems;
    }

    private void BuildStatusItems()
    {
        StatusItemsHost.Children.Clear();
        var statusFontSize = (double)FindResource("FlourishFontSizeCaption");

        foreach (var item in statusService.StatusItems)
        {
            var status = new StackPanel
            {
                Margin =
                    StatusItemsHost.Children.Count > 0
                        ? new Thickness(14, 0, 0, 0)
                        : new Thickness(),
                Orientation = Orientation.Horizontal,
            };
            status.Children.Add(
                new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = iconFontFamily,
                    FontSize = statusFontSize,
                    Foreground = mutedTextBrush,
                    Text = item.IconGlyph,
                }
            );
            status.Children.Add(
                new TextBlock
                {
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = statusFontSize,
                    Text = item.Text,
                }
            );

            StatusItemsHost.Children.Add(status);
        }
    }

    private StackPanel CreateIconTextContent(string iconGlyph, string label)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(
            new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = iconFontFamily,
                Text = iconGlyph,
            }
        );
        content.Children.Add(
            new TextBlock
            {
                Margin = new Thickness(7, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = label,
            }
        );
        return content;
    }

    private void NavigateToInitialPage()
    {
        var initialItem =
            options.InitialNavigationPageType is null
                ? GetNavigationItem(options.InitialNavigationKey) ?? firstNavigationItem
                : navigationItemsByPage.GetValueOrDefault(options.InitialNavigationPageType);

        if (initialItem is null)
        {
            return;
        }

        SelectNavigationItem(initialItem);
        NavigateToKey(initialItem.Key, false);
    }

    private FlourishNavigationItem? GetNavigationItem(string? key)
    {
        return key is not null && navigationItemsByKey.TryGetValue(key, out var item) ? item : null;
    }

    private void Titlebar_BackRequested(object? sender, EventArgs e)
    {
        if (navigationService.CanGoBack)
        {
            navigationService.GoBack();
        }
    }

    private void Titlebar_ForwardRequested(object? sender, EventArgs e)
    {
        if (navigationService.CanGoForward)
        {
            navigationService.GoForward();
        }
    }

    private void Titlebar_NavigationToggleRequested(object? sender, EventArgs e)
    {
        if (!options.IsNavigationPanelEnabled)
        {
            return;
        }

        isPaneOpen = !isPaneOpen;
        ApplyNavigationPaneState();
    }

    private void NavigationItemsHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (
            suppressNavigationSelection
            || NavigationItemsHost.SelectedItem is not FlourishNavigationItem item
        )
        {
            return;
        }

        NavigateToKey(item.Key, true);
    }

    private void RootFrame_Navigated(object? sender, FlourishNavigatedEventArgs e)
    {
        UpdateTitlebarBreadcrumbNavigation();
        BuildToolbarItems(e.SourcePageType);
        UpdateBreadcrumb(e.SourcePageType);

        SelectNavigationItem(e.SourcePageType);
    }

    private void UpdateBreadcrumb(Type sourcePageType)
    {
        if (
            !IsBreadcrumbFeatureEnabled()
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        if (
            options.BreadcrumbShowOption == BreadcrumbShowOption.Auto
            && !HasBreadcrumbNavigation()
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        var label =
            navigationItemsByPage.GetValueOrDefault(sourcePageType)?.Label
            ?? sourcePageType.Name;

        BreadcrumbText.Text = $"{options.Title} / {label}";
        BreadcrumbHost.Visibility = Visibility.Visible;
    }

    private void NavigateToKey(string key, bool addToBackStack)
    {
        if (!navigationItemsByKey.TryGetValue(key, out var targetItem))
        {
            targetItem = firstNavigationItem;
        }

        if (targetItem is null || navigationService.CurrentSourcePageType == targetItem.PageType)
        {
            return;
        }

        navigationService.Navigate(targetItem.PageType, addToBackStack: addToBackStack);

        if (!addToBackStack && navigationService.CanGoBack)
        {
            navigationService.ClearBackStack();
            UpdateTitlebarBreadcrumbNavigation();
        }
    }

    private bool IsBreadcrumbFeatureEnabled()
    {
        return options.IsBreadcrumbEnabled
            && options.BreadcrumbShowOption != BreadcrumbShowOption.Hidden;
    }

    private bool HasBreadcrumbNavigation()
    {
        return navigationService.CanGoBack || navigationService.CanGoForward;
    }

    private void UpdateTitlebarBreadcrumbNavigation()
    {
        var isVisible = IsBreadcrumbFeatureEnabled()
            && (
                options.BreadcrumbShowOption == BreadcrumbShowOption.Always
                || HasBreadcrumbNavigation()
            );

        Titlebar.SetBreadcrumbNavigationState(
            isVisible,
            navigationService.CanGoBack,
            navigationService.CanGoForward
        );
    }

    private void Titlebar_DragRequested(object? sender, EventArgs e)
    {
        DragMove();
    }

    private void Titlebar_ToggleWindowStateRequested(object? sender, EventArgs e)
    {
        ToggleWindowState();
    }

    private void Titlebar_MinimizeRequested(object? sender, EventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Titlebar_MaximizeRequested(object? sender, EventArgs e)
    {
        if (ResizeMode is not (ResizeMode.CanResize or ResizeMode.CanResizeWithGrip))
        {
            return;
        }

        ToggleWindowState();
    }

    private void Titlebar_CloseRequested(object? sender, EventArgs e)
    {
        if (trayIconService.MinimizeToTray())
        {
            return;
        }

        Close();
    }

    private void ShellWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!trayIconService.IsEnabled || trayIconService.IsExitRequested)
        {
            return;
        }

        e.Cancel = trayIconService.MinimizeToTray();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        Titlebar.SetMaximized(WindowState == WindowState.Maximized);
    }

    private void ToggleWindowState()
    {
        if (ResizeMode is not (ResizeMode.CanResize or ResizeMode.CanResizeWithGrip))
        {
            return;
        }

        WindowState =
            WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void SelectNavigationItem(Type pageType)
    {
        if (navigationItemsByPage.TryGetValue(pageType, out var item))
        {
            SelectNavigationItem(item);
        }
    }

    private void SelectNavigationItem(FlourishNavigationItem item)
    {
        suppressNavigationSelection = true;
        try
        {
            NavigationItemsHost.SelectedItem = item;
        }
        finally
        {
            suppressNavigationSelection = false;
        }
    }
}
