using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.ComponentModel;
using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;
using AckSS.Flourish.Services;
using Button = System.Windows.Controls.Button;
using FontFamily = System.Windows.Media.FontFamily;
using ListBox = System.Windows.Controls.ListBox;
using Orientation = System.Windows.Controls.Orientation;
using WpfPanel = System.Windows.Controls.Panel;

namespace AckSS.Flourish.Windows;

internal partial class FlourishShellWindow : Window
{
    private readonly INavigationService navigationService;
    private readonly IFrameNavigationService frameNavigationService;
    private readonly FlourishToolbarService toolbarService;
    private readonly FlourishStatusService statusService;
    private readonly IMessageService messageService;
    private readonly TrayIconService trayIconService;
    private readonly CommandParser commandParser;
    private readonly FontService fontService;
    private readonly MaterialEffectService materialEffectService;
    private readonly ThemeService themeService;
    private readonly FlourishMotionService motionService;
    private readonly WindowFrameFixService windowFrameFixService;
    private readonly IServiceProvider serviceProvider;
    private readonly FlourishShellOptions options;
    private readonly Dictionary<string, FlourishNavigationItem> navigationItemsByKey = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, FlourishNavigationItem> navigationItemsByPage = [];
    private readonly Dictionary<NavigationTreeKey, FlourishNavigationItem> navigationParentsByKey =
        [];
    private readonly Dictionary<
        NavigationTreeKey,
        List<FlourishNavigationItem>
    > navigationChildrenByParentKey = [];
    private readonly Dictionary<Type, IReadOnlyList<Button>> toolbarButtonsByPageType = [];
    private IReadOnlyList<Button>? defaultToolbarButtons;
    private FlourishNavigationItem? firstNavigationItem;
    private Style toolbarButtonStyle = null!;
    private Style navigationListBoxItemStyle = null!;
    private FontFamily iconFontFamily = null!;
    private Type? activeToolbarPageType;
    private FlourishNavigationItem? selectedNavigationItem;
    private FlourishNavigationItem? activeChildParentItem;
    private bool isDefaultToolbarActive;
    private bool isPaneOpen = true;
    private bool suppressNavigationSelection;
    private double navigationPaneDragStartWidth;

    private readonly record struct NavigationTreeKey(bool IsFixed, int GroupId, int RelationshipId);

    public FlourishShellWindow(
        INavigationService navigationService,
        IFrameNavigationService frameNavigationService,
        FlourishToolbarService toolbarService,
        FlourishStatusService statusService,
        IMessageService messageService,
        TrayIconService trayIconService,
        CommandParser commandParser,
        FontService fontService,
        MaterialEffectService materialEffectService,
        ThemeService themeService,
        FlourishMotionService motionService,
        WindowFrameFixService windowFrameFixService,
        IServiceProvider serviceProvider,
        FlourishShellOptions options
    )
    {
        themeService.Initialize(System.Windows.Application.Current);
        InitializeComponent();

        this.navigationService = navigationService;
        this.frameNavigationService = frameNavigationService;
        this.toolbarService = toolbarService;
        this.statusService = statusService;
        this.messageService = messageService;
        this.trayIconService = trayIconService;
        this.commandParser = commandParser;
        this.fontService = fontService;
        this.materialEffectService = materialEffectService;
        this.themeService = themeService;
        this.motionService = motionService;
        this.windowFrameFixService = windowFrameFixService;
        this.serviceProvider = serviceProvider;
        this.options = options;

        fontService.Apply(this);
        CacheResources();
        ApplyOptions();
        BuildRegionContents();
        BuildToolbarItems();
        BuildNavigationItems();
        BuildStatusItems();
        AttachTitlebarEvents();

        themeService.ThemeChanged += ThemeService_ThemeChanged;
        StateChanged += MainWindow_StateChanged;
        Closing += ShellWindow_Closing;
        Closed += ShellWindow_Closed;
        frameNavigationService.Initialize(RootFrame);
        navigationService.Navigated += RootFrame_Navigated;

        NavigateToInitialPage();
    }

    private void CacheResources()
    {
        toolbarButtonStyle = (Style)FindResource("FlourishToolbarButtonStyle");
        navigationListBoxItemStyle = (Style)FindResource("FlourishNavigationListBoxItemStyle");
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
            options.IsTitlebarThemeToggleEnabled && options.IsThemeEnabled,
            options.IsTitlebarProfileEnabled
        );
        StatusBarBorder.Visibility = options.IsStatusBarEnabled || HasAnyRegionContent(
                FlourishRegion.StatusStart,
                FlourishRegion.StatusEnd
            )
            ? Visibility.Visible
            : Visibility.Collapsed;
        StatusTextBlock.Text = statusService.StatusText;
        NavigationPaneBorder.Visibility = options.IsNavigationPanelEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        BreadcrumbHost.Visibility = IsBreadcrumbFeatureEnabled()
            ? Visibility.Visible
            : Visibility.Collapsed;
        UpdateTitlebarBreadcrumbNavigation();
        ApplyMotionResources();
        ApplyToolTipResources();

        NormalizeNavigationPaneWidths();
        ApplyNavigationPanelPlacement();
        isPaneOpen = options.IsNavigationPanelInitiallyOpen;
        ApplyNavigationPaneState();
        windowFrameFixService.Attach(this);
        materialEffectService.Attach(this, options.MaterialEffect);
        themeService.Attach(this);
        ApplyThemeState();
        trayIconService.Initialize(this, options.Title);
    }

    private void ApplyMotionResources()
    {
        Resources["FlourishHoverRevealEnabled"] =
            options.Motion.IsEnabled
            && options.Motion.IsHoverRevealEnabled
            && (
                !options.Motion.RespectSystemReducedMotion
                || SystemParameters.ClientAreaAnimation
            );
    }

    private void ApplyToolTipResources()
    {
        Resources["FlourishToolTipInitialShowDelay"] =
            options.Tips.InitialShowDelayMilliseconds;
        Resources["FlourishToolTipSpawnableMargin"] = options.Tips.SpawnableMargin;
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
        ApplyWindowChrome();
        Titlebar.SetMaximizeEnabled(ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip);
    }

    private void ApplyWindowChrome()
    {
        if (!options.IsTitlebarEnabled)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowChrome.SetWindowChrome(this, null);
            ShellBorder.BorderThickness = new Thickness();
            Titlebar.Visibility = Visibility.Collapsed;
            return;
        }

        WindowStyle = WindowStyle.None;
        WindowChrome.SetWindowChrome(
            this,
            new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(),
                GlassFrameThickness = new Thickness(),
                ResizeBorderThickness = new Thickness(6),
                UseAeroCaptionButtons = false,
            }
        );
        ShellBorder.BorderThickness = new Thickness(1);
        Titlebar.Visibility = Visibility.Visible;
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
        Titlebar.ThemeToggleRequested += Titlebar_ThemeToggleRequested;
    }

    private void ApplyNavigationPanelPlacement()
    {
        if (options.NavigationPanelDirection == NavigationPanelDirection.Right)
        {
            Grid.SetColumn(ContentAreaGrid, 0);
            Grid.SetColumn(NavigationPaneBorder, 1);
            Grid.SetColumn(NavigationPaneSplitter, 1);
            NavigationPaneSplitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            NavigationPaneSplitter.ResizeBehavior = GridResizeBehavior.PreviousAndCurrent;
            NavigationPaneBorder.BorderThickness = new Thickness(1, 0, 0, 0);
            return;
        }

        Grid.SetColumn(NavigationPaneBorder, 0);
        Grid.SetColumn(ContentAreaGrid, 1);
        Grid.SetColumn(NavigationPaneSplitter, 0);
        NavigationPaneSplitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        NavigationPaneSplitter.ResizeBehavior = GridResizeBehavior.CurrentAndNext;
        NavigationPaneBorder.BorderThickness = new Thickness(0, 0, 1, 0);
    }

    private ColumnDefinition GetNavigationPaneColumn()
    {
        return options.NavigationPanelDirection == NavigationPanelDirection.Right
            ? ContentColumn
            : PaneColumn;
    }

    private ColumnDefinition GetContentColumn()
    {
        return options.NavigationPanelDirection == NavigationPanelDirection.Right
            ? PaneColumn
            : ContentColumn;
    }

    private void PreparePaneColumnsForAnimation()
    {
        GetContentColumn().Width = new GridLength(1, GridUnitType.Star);
    }

    private void SetNavigationPaneWidth(double width)
    {
        GetNavigationPaneColumn().Width = new GridLength(width);
        GetContentColumn().Width = new GridLength(1, GridUnitType.Star);
    }

    private void NormalizeNavigationPaneWidths()
    {
        options.OpenPaneWidth = CoerceOpenPaneWidth(options.OpenPaneWidth);
        options.ClosedPaneWidth = Math.Min(options.ClosedPaneWidth, options.OpenPaneWidth);
    }

    private double CoerceOpenPaneWidth(double width)
    {
        return Math.Min(
            Math.Max(width, options.NavigationPaneMinWidth),
            options.NavigationPaneMaxWidth
        );
    }

    private void ApplyNavigationPaneColumnConstraints(bool isOpen)
    {
        var paneColumn = GetNavigationPaneColumn();
        paneColumn.MinWidth = isOpen ? options.NavigationPaneMinWidth : 0;
        paneColumn.MaxWidth = isOpen
            ? options.NavigationPaneMaxWidth
            : double.PositiveInfinity;
    }

    private void UpdateNavigationPaneSplitterState()
    {
        var isSplitterEnabled = options.IsNavigationPanelEnabled && isPaneOpen;
        NavigationPaneSplitter.IsEnabled = isSplitterEnabled;
        NavigationPaneSplitter.Visibility = isSplitterEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ApplyNavigationPaneState(bool animate = false)
    {
        var isNavigationVisible = options.IsNavigationPanelEnabled;
        var isOpen = isNavigationVisible && isPaneOpen;
        var paneWidth = !isNavigationVisible
            ? 0
            : isOpen
                ? CoerceOpenPaneWidth(options.OpenPaneWidth)
                : options.ClosedPaneWidth;

        if (isOpen || animate)
        {
            ApplyNavigationPaneChrome(isOpen);
        }

        if (!animate)
        {
            ApplyNavigationPaneColumnConstraints(isOpen);
            SetNavigationPaneWidth(paneWidth);
            ApplyNavigationPaneChrome(isOpen);
            UpdateNavigationPaneSplitterState();
            return;
        }

        ApplyNavigationPaneColumnConstraints(false);
        NavigationPaneSplitter.IsEnabled = false;
        NavigationPaneSplitter.Visibility = Visibility.Collapsed;
        PreparePaneColumnsForAnimation();
        var paneColumn = GetNavigationPaneColumn();
        var fromWidth = paneColumn.ActualWidth > 0 ? paneColumn.ActualWidth : paneColumn.Width.Value;

        motionService.AnimateNavigationPane(
            paneColumn,
            fromWidth,
            paneWidth,
            () =>
            {
                ApplyNavigationPaneColumnConstraints(isOpen);
                SetNavigationPaneWidth(paneWidth);
                ApplyNavigationPaneChrome(isOpen);
                UpdateNavigationPaneSplitterState();
            }
        );
    }

    private void NavigationPaneSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (!options.IsNavigationPanelEnabled || !isPaneOpen)
        {
            return;
        }

        var horizontalChange =
            options.NavigationPanelDirection == NavigationPanelDirection.Right
                ? -e.HorizontalChange
                : e.HorizontalChange;
        var paneWidth = e.Canceled
            ? navigationPaneDragStartWidth
            : CoerceOpenPaneWidth(navigationPaneDragStartWidth + horizontalChange);

        options.OpenPaneWidth = paneWidth;
        ApplyNavigationPaneColumnConstraints(isOpen: true);
        SetNavigationPaneWidth(paneWidth);
        RefreshWorkAreaLayout();
    }

    private void NavigationPaneSplitter_DragStarted(object sender, DragStartedEventArgs e)
    {
        navigationPaneDragStartWidth = CoerceOpenPaneWidth(
            GetNavigationPaneColumn().ActualWidth
        );
    }

    private void RefreshWorkAreaLayout()
    {
        WorkAreaGrid.InvalidateMeasure();
        WorkAreaGrid.InvalidateArrange();
        WorkAreaGrid.UpdateLayout();
    }

    private void ApplyNavigationPaneChrome(bool isOpen)
    {
        NavigationItemsHost.Tag = isOpen;
        FixedNavigationItemsHost.Tag = isOpen;
    }

    private void BuildToolbarItems(Type? pageType = null)
    {
        if (activeToolbarPageType == pageType && isDefaultToolbarActive == (pageType is null))
        {
            return;
        }

        ToolbarItemsHost.Children.Clear();
        var toolbarButtons = GetToolbarButtons(pageType);

        foreach (var button in toolbarButtons)
        {
            ToolbarItemsHost.Children.Add(button);
        }

        ToolbarHostBorder.Visibility =
            toolbarButtons.Count > 0
            || ToolbarStartRegionHost.Children.Count > 0
            || ToolbarEndRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        activeToolbarPageType = pageType;
        isDefaultToolbarActive = pageType is null;
    }

    private void BuildRegionContents()
    {
        foreach (var region in Enum.GetValues<FlourishRegion>())
        {
            var elements = options
                .RegionContents.Where(content => content.Region == region)
                .OrderBy(content => content.Order)
                .Select(CreateRegionElement)
                .ToList();

            SetRegionContent(region, elements);
        }
    }

    private FrameworkElement CreateRegionElement(FlourishRegionContent content)
    {
        var element = content.CreateContent(serviceProvider);
        if (element.Parent is not null)
        {
            throw new InvalidOperationException(
                $"The content factory for region {content.Region} returned an element that already has a parent."
            );
        }

        return element;
    }

    private void SetRegionContent(
        FlourishRegion region,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        switch (region)
        {
            case FlourishRegion.TitlebarStart:
            case FlourishRegion.TitlebarCenter:
            case FlourishRegion.TitlebarEnd:
            case FlourishRegion.TitlebarProfile:
                Titlebar.SetRegionContent(region, elements);
                break;
            case FlourishRegion.NavigationHeader:
                SetPanelContent(NavigationHeaderRegionHost, elements);
                break;
            case FlourishRegion.NavigationFooter:
                SetPanelContent(NavigationFooterRegionHost, elements);
                break;
            case FlourishRegion.ContentHeader:
                SetPanelContent(ContentHeaderRegionHost, elements);
                break;
            case FlourishRegion.ContentFooter:
                SetPanelContent(ContentFooterRegionHost, elements);
                break;
            case FlourishRegion.ContentOverlay:
                SetPanelContent(ContentOverlayRegionHost, elements);
                break;
            case FlourishRegion.ToolbarStart:
                SetPanelContent(ToolbarStartRegionHost, elements);
                break;
            case FlourishRegion.ToolbarEnd:
                SetPanelContent(ToolbarEndRegionHost, elements);
                break;
            case FlourishRegion.StatusStart:
                SetPanelContent(StatusStartRegionHost, elements);
                break;
            case FlourishRegion.StatusEnd:
                SetPanelContent(StatusEndRegionHost, elements);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown shell region.");
        }
    }

    private bool HasAnyRegionContent(params FlourishRegion[] regions)
    {
        return options.RegionContents.Any(content => regions.Contains(content.Region));
    }

    private static void SetPanelContent(
        WpfPanel host,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        host.Children.Clear();
        foreach (var element in elements)
        {
            host.Children.Add(element);
        }

        host.Visibility = elements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private IReadOnlyList<Button> GetToolbarButtons(Type? pageType)
    {
        if (pageType is null)
        {
            return defaultToolbarButtons ??= CreateToolbarButtons(
                toolbarService.GetToolbarItems(),
                showIconOnly: false
            );
        }

        if (!toolbarButtonsByPageType.TryGetValue(pageType, out var toolbarButtons))
        {
            toolbarButtons = CreateToolbarButtons(
                toolbarService.GetToolbarItems(pageType),
                ShouldShowToolbarIconOnly(pageType)
            );
            toolbarButtonsByPageType[pageType] = toolbarButtons;
        }

        return toolbarButtons;
    }

    private IReadOnlyList<Button> CreateToolbarButtons(
        IReadOnlyList<FlourishToolbarItem> items,
        bool showIconOnly
    )
    {
        var buttons = new List<Button>(items.Count);
        foreach (var item in items)
        {
            var useIconOnly = showIconOnly && !string.IsNullOrWhiteSpace(item.IconGlyph);
            var button = new Button
            {
                Content = useIconOnly
                    ? CreateIconContent(item.IconGlyph)
                    : CreateIconTextContent(item.IconGlyph, item.DisplayName),
                Margin = buttons.Count > 0 ? new Thickness(2, 0, 0, 0) : new Thickness(),
                ToolTip = item.DisplayName,
                Style = toolbarButtonStyle,
                Tag = item.CommandKey,
                Width = useIconOnly ? 30 : double.NaN,
            };
            button.Click += ToolbarButton_Click;
            buttons.Add(button);
        }

        return buttons;
    }

    private bool ShouldShowToolbarIconOnly(Type pageType)
    {
        return options.DynamicToolbarIconModes.GetValueOrDefault(pageType, true);
    }

    private void ToolbarButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string commandKey })
        {
            commandParser.Parse(commandKey);
        }
    }

    private void BuildNavigationItems()
    {
        navigationItemsByKey.Clear();
        navigationItemsByPage.Clear();
        navigationParentsByKey.Clear();
        navigationChildrenByParentKey.Clear();
        activeChildParentItem = null;
        firstNavigationItem = null;

        foreach (var item in options.NavigationItems.Concat(options.FixedNavigationItems))
        {
            item.Validate();
            if (!item.IsNavigationItem)
            {
                continue;
            }

            if (!navigationItemsByKey.ContainsKey(item.Key))
            {
                navigationItemsByKey[item.Key] = item;
            }

            IndexNavigationTreeItem(item);

            if (item.PageType is not null)
            {
                firstNavigationItem ??= item;
                navigationItemsByPage[item.PageType] = item;
            }
        }

        NavigationItemsHost.ItemContainerStyle = navigationListBoxItemStyle;
        NavigationItemsHost.ItemsSource = options.NavigationItems;
        FixedNavigationItemsHost.ItemContainerStyle = navigationListBoxItemStyle;
        FixedNavigationItemsHost.ItemsSource = options.FixedNavigationItems;
        FixedNavigationItemsBorder.Visibility =
            options.FixedNavigationItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void IndexNavigationTreeItem(FlourishNavigationItem item)
    {
        if (item.ParentId != 0)
        {
            navigationParentsByKey[CreateNavigationTreeKey(item, item.ParentId)] = item;
        }

        if (item.ChildId == 0)
        {
            return;
        }

        var childKey = CreateNavigationTreeKey(item, item.ChildId);
        if (!navigationChildrenByParentKey.TryGetValue(childKey, out var children))
        {
            children = [];
            navigationChildrenByParentKey[childKey] = children;
        }

        children.Add(item);
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
            var iconText = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = iconFontFamily,
                FontSize = statusFontSize,
                Text = item.IconGlyph,
            };
            iconText.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            status.Children.Add(iconText);

            var labelText = new TextBlock
            {
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = statusFontSize,
                Text = item.Text,
            };
            labelText.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            status.Children.Add(labelText);

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
                FontSize = (double)FindResource("FlourishFontSizeTitlebarIcon"),
                Text = iconGlyph,
            }
        );
        content.Children.Add(
            new TextBlock
            {
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = label,
            }
        );
        return content;
    }

    private TextBlock CreateIconContent(string iconGlyph)
    {
        return new TextBlock
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontFamily = iconFontFamily,
            FontSize = (double)FindResource("FlourishFontSizeTitlebarIcon"),
            Text = iconGlyph,
            TextAlignment = System.Windows.TextAlignment.Center,
        };
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
        ActivateNavigationItem(initialItem, false, toggleChildren: false);
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

        var shouldOpen = !isPaneOpen;
        if (!shouldOpen)
        {
            CollapseAllNavigationChildren();
        }

        isPaneOpen = shouldOpen;
        ApplyNavigationPaneState(animate: true);

        if (shouldOpen)
        {
            RestoreSelectedNavigationItem();
        }
    }

    private void NavigationItemsHost_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (suppressNavigationSelection || sender is not ListBox)
        {
            return;
        }

        if (GetNavigationItemFromInputSource(e.OriginalSource) is not { } item)
        {
            return;
        }

        e.Handled = true;
        if (!item.IsNavigationItem)
        {
            return;
        }

        OpenNavigationPaneForCollapsedParent(item);

        if (item.IsCommandItem)
        {
            ActivateCommandNavigationItem(item);
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true);
    }

    private void NavigationItemsHost_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (suppressNavigationSelection || sender is not ListBox listBox)
        {
            return;
        }

        if (e.Key is not (Key.Enter or Key.Space))
        {
            return;
        }

        var item =
            GetNavigationItemFromInputSource(e.OriginalSource)
            ?? listBox.SelectedItem as FlourishNavigationItem;
        if (item is null || !item.IsNavigationItem)
        {
            return;
        }

        e.Handled = true;
        OpenNavigationPaneForCollapsedParent(item);

        if (item.IsCommandItem)
        {
            ActivateCommandNavigationItem(item);
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true);
    }

    private void NavigationItemsHost_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (suppressNavigationSelection || sender is not ListBox listBox)
        {
            return;
        }

        if (listBox.SelectedItem is not FlourishNavigationItem item)
        {
            return;
        }

        OpenNavigationPaneForCollapsedParent(item);

        if (!item.IsPageItem)
        {
            RestoreSelectedNavigationItem();
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true, toggleChildren: false);
    }

    private void ActivateCommandNavigationItem(FlourishNavigationItem item)
    {
        ActivateNavigationItem(item, true);
        RestoreSelectedNavigationItem();
        Keyboard.ClearFocus();
    }

    private void RootFrame_Navigated(object? sender, FlourishNavigatedEventArgs e)
    {
        UpdateTitlebarBreadcrumbNavigation();
        BuildToolbarItems(e.SourcePageType);
        UpdateBreadcrumb(e.SourcePageType);

        SelectNavigationItem(e.SourcePageType);
        motionService.AnimatePageEntrance(RootFrame);
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

    private void ActivateNavigationItem(
        FlourishNavigationItem item,
        bool addToBackStack,
        bool toggleChildren = true
    )
    {
        if (toggleChildren)
        {
            OpenNavigationPaneForCollapsedParent(item);
        }

        if (toggleChildren && item.HasChildren)
        {
            ToggleChildItems(item);
        }

        if (item.IsCommandItem)
        {
            if (!item.HasChildren)
            {
                commandParser.Parse(item.CommandKey);
            }

            return;
        }

        if (item.PageType is null || navigationService.CurrentSourcePageType == item.PageType)
        {
            return;
        }

        navigationService.Navigate(item.PageType, addToBackStack: addToBackStack);

        if (!addToBackStack && navigationService.CanGoBack)
        {
            navigationService.ClearBackStack();
            UpdateTitlebarBreadcrumbNavigation();
        }
    }

    private void ToggleChildItems(FlourishNavigationItem parent)
    {
        parent.IsExpanded = !parent.IsExpanded;
        SetChildItemsVisibility(parent, parent.IsExpanded);
    }

    private void OpenNavigationPaneForCollapsedParent(FlourishNavigationItem item)
    {
        if (isPaneOpen || !item.HasChildren)
        {
            return;
        }

        isPaneOpen = true;
        ApplyNavigationPaneState(animate: true);
    }

    private void CollapseAllNavigationChildren()
    {
        foreach (var parent in navigationParentsByKey.Values)
        {
            parent.IsExpanded = false;
            SetChildItemsVisibility(parent, false);
        }
    }

    private void ExpandAncestorsForSelection(FlourishNavigationItem item)
    {
        if (item.ChildId == 0)
        {
            return;
        }

        var parent = FindParentNavigationItem(item);
        if (parent is null)
        {
            return;
        }

        parent.IsExpanded = true;
        SetChildItemsVisibility(parent, true);
    }

    private void SetChildItemsVisibility(FlourishNavigationItem parent, bool isVisible)
    {
        foreach (var child in GetChildNavigationItems(parent))
        {
            child.IsVisible = isVisible;
        }
    }

    private IEnumerable<FlourishNavigationItem> GetChildNavigationItems(
        FlourishNavigationItem parent
    )
    {
        return parent.ParentId != 0
            && navigationChildrenByParentKey.TryGetValue(
                CreateNavigationTreeKey(parent, parent.ParentId),
                out var children
            )
            ? children
            : [];
    }

    private FlourishNavigationItem? FindParentNavigationItem(FlourishNavigationItem child)
    {
        return child.ChildId == 0
            ? null
            : navigationParentsByKey.GetValueOrDefault(
                CreateNavigationTreeKey(child, child.ChildId)
            );
    }

    private static NavigationTreeKey CreateNavigationTreeKey(
        FlourishNavigationItem item,
        int relationshipId
    )
    {
        return new NavigationTreeKey(item.IsFixed, item.GroupId, relationshipId);
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
        if (!options.IsTitlebarEnabled)
        {
            return;
        }

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
        if (!ConfirmCloseRequest())
        {
            return;
        }

        if (trayIconService.MinimizeToTray())
        {
            return;
        }

        Close();
    }

    private void Titlebar_ThemeToggleRequested(object? sender, EventArgs e)
    {
        themeService.ToggleTheme();
    }

    private void ThemeService_ThemeChanged(object? sender, FlourishThemeChangedEventArgs e)
    {
        ApplyThemeState();
    }

    private void ApplyThemeState()
    {
        materialEffectService.SetDarkMode(this, themeService.IsDark);
        Titlebar.SetThemeToggleState(themeService.CurrentTheme, themeService.EffectiveTheme);
    }

    private bool ConfirmCloseRequest()
    {
        FlourishMessageOption[] closeOptions =
        [
            new("no", "No") { IsDefault = true, IsCancel = true },
            new("yes", "Yes") { IsPrimary = true },
        ];

        return messageService.Show(
            this,
            "Are you sure you want to close this window?",
            "Close",
            closeOptions,
            MessageBoxImage.Question
        )?.Id == "yes";
    }

    private void ShellWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!trayIconService.IsEnabled || trayIconService.IsExitRequested)
        {
            return;
        }

        e.Cancel = trayIconService.MinimizeToTray();
    }

    private void ShellWindow_Closed(object? sender, EventArgs e)
    {
        themeService.ThemeChanged -= ThemeService_ThemeChanged;
        themeService.Detach(this);
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (!options.IsTitlebarEnabled)
        {
            return;
        }

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
        if (!item.IsPageItem)
        {
            RestoreSelectedNavigationItem();
            return;
        }

        selectedNavigationItem = item;
        suppressNavigationSelection = true;
        try
        {
            ExpandAncestorsForSelection(item);
            UpdateActiveChildParent(item);

            if (item.IsFixed)
            {
                NavigationItemsHost.SelectedItem = null;
                FixedNavigationItemsHost.SelectedItem = item;
                return;
            }

            FixedNavigationItemsHost.SelectedItem = null;
            NavigationItemsHost.SelectedItem = item;
        }
        finally
        {
            suppressNavigationSelection = false;
        }
    }

    private void RestoreSelectedNavigationItem()
    {
        if (selectedNavigationItem is not null)
        {
            SelectNavigationItem(selectedNavigationItem);
            return;
        }

        suppressNavigationSelection = true;
        try
        {
            NavigationItemsHost.SelectedItem = null;
            FixedNavigationItemsHost.SelectedItem = null;
        }
        finally
        {
            suppressNavigationSelection = false;
        }
    }

    private void UpdateActiveChildParent(FlourishNavigationItem activeItem)
    {
        var parent = activeItem.IsPageItem && activeItem.ChildId != 0
            ? FindParentNavigationItem(activeItem)
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

    private static T? FindAncestor<T>(DependencyObject source)
        where T : DependencyObject
    {
        var current = source;
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static FlourishNavigationItem? GetNavigationItemFromInputSource(object source)
    {
        return source is DependencyObject dependencyObject
            && FindAncestor<ListBoxItem>(dependencyObject)?.DataContext is FlourishNavigationItem item
            ? item
            : null;
    }
}
