using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;
using AcksheedSys.Flourish.Services;

namespace AcksheedSys.Flourish.Windows;

internal partial class FlourishShellWindow : Window
{
    private readonly INavigationService navigationService;
    private readonly IFrameNavigationService frameNavigationService;
    private readonly IFlourishToolbarService toolbarService;
    private readonly IFlourishStatusService statusService;
    private readonly FlourishShellOptions options;
    private readonly Dictionary<string, FlourishNavigationItem> navigationItemsByKey = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, RadioButton> navigationButtonsByPage = new();
    private readonly List<TextBlock> paneLabels = [];
    private bool isPaneOpen = true;

    public FlourishShellWindow(
        INavigationService navigationService,
        IFrameNavigationService frameNavigationService,
        IFlourishToolbarService toolbarService,
        IFlourishStatusService statusService,
        FlourishShellOptions options
    )
    {
        InitializeComponent();

        this.navigationService = navigationService;
        this.frameNavigationService = frameNavigationService;
        this.toolbarService = toolbarService;
        this.statusService = statusService;
        this.options = options;

        ApplyOptions();
        BuildToolbarItems();
        BuildNavigationItems();
        BuildStatusItems();

        StateChanged += MainWindow_StateChanged;
        frameNavigationService.Initialize(RootFrame);
        navigationService.Navigated += RootFrame_Navigated;

        NavigateToInitialPage();
    }

    private void ApplyOptions()
    {
        Title = options.Title;
        AppTitleText.Text = options.Title;
        AppSubtitleText.Text = options.Subtitle;
        PaneTitle.Text = options.PaneTitle;
        SearchBox.Text = options.SearchPlaceholder;
        StatusTextBlock.Text = statusService.StatusText;
        SearchBoxHost.Visibility = options.IsTitlebarSearchEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        NavigationPaneBorder.Visibility = options.IsNavigationPanelEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        PaneToggleButton.Visibility = options.IsNavigationPanelEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        BreadcrumbHost.Visibility = options.IsBreadcrumbEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;

        ApplyNavigationPanelPlacement();
        SetPaneWidth(options.IsNavigationPanelEnabled ? options.OpenPaneWidth : 0);

        if (options.LogoSource is not null)
        {
            AppLogoImage.Source = options.LogoSource;
            AppLogoImage.Visibility = Visibility.Visible;
            AppLogoFallback.Visibility = Visibility.Collapsed;
            return;
        }

        AppLogoFallback.Text = string.IsNullOrWhiteSpace(options.LogoFallbackText)
            ? "F"
            : options.LogoFallbackText[..1];
        AppLogoFallback.Visibility = Visibility.Visible;
    }

    private void ApplyNavigationPanelPlacement()
    {
        if (options.NavigationPanelDirection == NavigationPanelDirection.Right)
        {
            Grid.SetColumn(RootFrame, 0);
            Grid.SetColumn(NavigationPaneBorder, 1);
            NavigationPaneBorder.BorderThickness = new Thickness(1, 0, 0, 0);
            return;
        }

        Grid.SetColumn(NavigationPaneBorder, 0);
        Grid.SetColumn(RootFrame, 1);
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

    private void BuildToolbarItems(Type? pageType = null)
    {
        ToolbarItemsHost.Children.Clear();
        var toolbarItems = toolbarService.GetToolbarItems(pageType);
        ToolbarHostBorder.Visibility =
            toolbarItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        foreach (var item in toolbarItems)
        {
            var button = new Button
            {
                Content = CreateIconTextContent(item.IconGlyph, item.DisplayName),
                Style = (Style)FindResource("FlourishToolbarButtonStyle"),
                Tag = item.CommandKey,
            };

            if (ToolbarItemsHost.Children.Count > 0)
            {
                button.Margin = new Thickness(6, 0, 0, 0);
            }

            ToolbarItemsHost.Children.Add(button);
        }
    }

    private void BuildNavigationItems()
    {
        NavigationItemsHost.Children.Clear();
        navigationItemsByKey.Clear();
        navigationButtonsByPage.Clear();
        paneLabels.Clear();

        foreach (var item in options.NavigationItems)
        {
            item.Validate();

            var label = new TextBlock
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = item.Label,
            };
            paneLabels.Add(label);

            var button = new RadioButton
            {
                Content = CreateNavigationContent(item.IconGlyph, label),
                GroupName = "RootNavigation",
                Style = (Style)FindResource("FlourishNavigationItemStyle"),
                Tag = item.Key,
            };
            button.Click += NavigationItem_Click;

            navigationItemsByKey[item.Key] = item;
            navigationButtonsByPage[item.PageType] = button;
            NavigationItemsHost.Children.Add(button);
        }
    }

    private void BuildStatusItems()
    {
        StatusItemsHost.Children.Clear();

        foreach (var item in statusService.StatusItems)
        {
            var status = new StackPanel
            {
                Margin =
                    StatusItemsHost.Children.Count > 0
                        ? new Thickness(20, 0, 0, 0)
                        : new Thickness(),
                Orientation = Orientation.Horizontal,
            };
            status.Children.Add(
                new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                    Foreground = (System.Windows.Media.Brush)FindResource("MutedTextBrush"),
                    Text = item.IconGlyph,
                }
            );
            status.Children.Add(
                new TextBlock
                {
                    Margin = new Thickness(7, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = item.Text,
                }
            );

            StatusItemsHost.Children.Add(status);
        }
    }

    private static StackPanel CreateIconTextContent(string iconGlyph, string label)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(
            new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
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

    private static Grid CreateNavigationContent(string iconGlyph, TextBlock label)
    {
        var content = new Grid();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        content.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );

        content.Children.Add(
            new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                FontSize = 16,
                Text = iconGlyph,
            }
        );

        Grid.SetColumn(label, 1);
        content.Children.Add(label);
        return content;
    }

    private void NavigateToInitialPage()
    {
        var initialKey = options.InitialNavigationPageType is null
            ? options.InitialNavigationKey ?? options.NavigationItems.FirstOrDefault()?.Key
            : options
                .NavigationItems.FirstOrDefault(item =>
                    item.PageType == options.InitialNavigationPageType
                )
                ?.Key;
        if (initialKey is null)
        {
            return;
        }

        if (
            navigationItemsByKey.TryGetValue(initialKey, out var initialItem)
            && navigationButtonsByPage.TryGetValue(initialItem.PageType, out var initialButton)
        )
        {
            initialButton.IsChecked = true;
        }

        NavigateToKey(initialKey, false);
    }

    private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
    {
        if (navigationService.CanGoBack)
        {
            navigationService.GoBack();
        }
    }

    private void TitleBar_PaneToggleRequested(object sender, RoutedEventArgs e)
    {
        isPaneOpen = !isPaneOpen;
        SetPaneWidth(isPaneOpen ? options.OpenPaneWidth : options.ClosedPaneWidth);
        PaneTitle.Visibility = isPaneOpen ? Visibility.Visible : Visibility.Collapsed;

        foreach (var label in paneLabels)
        {
            label.Visibility = isPaneOpen ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void NavigationItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string key })
        {
            NavigateToKey(key, true);
        }
    }

    private void RootFrame_Navigated(object? sender, FlourishNavigatedEventArgs e)
    {
        BackButton.IsEnabled = navigationService.CanGoBack;
        BuildToolbarItems(e.SourcePageType);
        UpdateBreadcrumb(e.SourcePageType);

        if (navigationButtonsByPage.TryGetValue(e.SourcePageType, out var navigationItem))
        {
            navigationItem.IsChecked = true;
        }
    }

    private void UpdateBreadcrumb(Type sourcePageType)
    {
        if (
            !options.IsBreadcrumbEnabled
            || options.BreadcrumbShowOption == BreadcrumbShowOption.Hidden
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        if (
            options.BreadcrumbShowOption == BreadcrumbShowOption.OnlyAvailable
            && !navigationService.CanGoBack
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        var label =
            options.NavigationItems.FirstOrDefault(item => item.PageType == sourcePageType)?.Label
            ?? sourcePageType.Name;

        BreadcrumbText.Text = $"{options.Title} / {label}";
        BreadcrumbHost.Visibility = Visibility.Visible;
    }

    private void NavigateToKey(string key, bool addToBackStack)
    {
        if (!navigationItemsByKey.TryGetValue(key, out var targetItem))
        {
            targetItem = options.NavigationItems.FirstOrDefault();
        }

        if (targetItem is null || navigationService.CurrentSourcePageType == targetItem.PageType)
        {
            return;
        }

        navigationService.Navigate(targetItem.PageType, addToBackStack: addToBackStack);

        if (!addToBackStack && navigationService.CanGoBack)
        {
            navigationService.ClearBackStack();
            BackButton.IsEnabled = false;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        MaximizeButtonIcon.Text = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
    }

    private void ToggleWindowState()
    {
        WindowState =
            WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}
