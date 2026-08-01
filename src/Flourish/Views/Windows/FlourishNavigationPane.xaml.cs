using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ListBox = ArkheideSystem.Flourish.Controls.BunchedListBox;
using UserControl = System.Windows.Controls.UserControl;
using WpfPanel = System.Windows.Controls.Panel;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishNavigationPane : UserControl
{
    private bool suppressSelection;

    public FlourishNavigationPane()
    {
        InitializeComponent();
    }

    internal event EventHandler<NavigationItemRequestedEventArgs>? ItemRequested;

    internal FrameworkElement TransitionHost => NavigationPaneTransitionHost;

    internal void SetItems(
        IReadOnlyList<FlourishNavigationItem> items,
        IReadOnlyList<FlourishNavigationItem> fixedItems
    )
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(fixedItems);

        suppressSelection = true;
        try
        {
            NavigationItemsHost.ItemsSource = null;
            FixedNavigationItemsHost.ItemsSource = null;
            NavigationItemsHost.ItemsSource = items;
            FixedNavigationItemsHost.ItemsSource = fixedItems;
        }
        finally
        {
            suppressSelection = false;
        }

        FixedNavigationItemsBorder.Visibility =
            fixedItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    internal void SetSelectedItem(FlourishNavigationItem? item)
    {
        suppressSelection = true;
        try
        {
            NavigationItemsHost.SelectedItem = item is { IsFixed: false } ? item : null;
            FixedNavigationItemsHost.SelectedItem = item is { IsFixed: true } ? item : null;
        }
        finally
        {
            suppressSelection = false;
        }
    }

    internal void SetCompact(bool compact)
    {
        NavigationItemsHost.IsCompact = compact;
        FixedNavigationItemsHost.IsCompact = compact;
    }

    internal void SetDirection(NavigationPanelDirection direction)
    {
        var isRight = direction == NavigationPanelDirection.Right;
        var flowDirection = isRight
            ? System.Windows.FlowDirection.RightToLeft
            : System.Windows.FlowDirection.LeftToRight;
        NavigationItemsHost.FlowDirection = flowDirection;
        FixedNavigationItemsHost.FlowDirection = flowDirection;
        NavigationPaneBorder.BorderThickness =
            isRight ? new Thickness(1, 0, 0, 0) : new Thickness(0, 0, 1, 0);
        NavigationPaneBorder.SetResourceReference(
            Border.PaddingProperty,
            isRight
                ? "FlourishNavigationPaneRightPadding"
                : "FlourishNavigationPaneLeftPadding"
        );
    }

    internal void SetEnabled(bool enabled)
    {
        NavigationPaneBorder.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
    }

    internal void SetRegionContent(
        FlourishRegion region,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        ArgumentNullException.ThrowIfNull(elements);
        var host = region switch
        {
            FlourishRegion.NavigationHeader => NavigationHeaderRegionHost,
            FlourishRegion.NavigationFooter => NavigationFooterRegionHost,
            _ => throw new ArgumentOutOfRangeException(
                nameof(region),
                region,
                "Unsupported navigation pane region."
            ),
        };
        SetPanelContent(host, elements);
    }

    private void NavigationItemsHost_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (
            suppressSelection
            || sender is not ListBox
            || GetNavigationItemFromInputSource(e.OriginalSource) is not { } item
        )
        {
            return;
        }

        e.Handled = true;
        ItemRequested?.Invoke(
            this,
            new NavigationItemRequestedEventArgs(item, NavigationItemRequestKind.Invoke)
        );
    }

    private void NavigationItemsHost_PreviewKeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e
    )
    {
        if (
            suppressSelection
            || sender is not ListBox listBox
            || e.Key is not (Key.Enter or Key.Space)
        )
        {
            return;
        }

        var item =
            GetNavigationItemFromInputSource(e.OriginalSource)
            ?? listBox.SelectedItem as FlourishNavigationItem;
        if (item is null)
        {
            return;
        }

        e.Handled = true;
        ItemRequested?.Invoke(
            this,
            new NavigationItemRequestedEventArgs(item, NavigationItemRequestKind.Invoke)
        );
    }

    private void NavigationItemsHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (
            suppressSelection
            || sender is not ListBox listBox
            || listBox.SelectedItem is not FlourishNavigationItem item
        )
        {
            return;
        }

        ItemRequested?.Invoke(
            this,
            new NavigationItemRequestedEventArgs(item, NavigationItemRequestKind.Selection)
        );
    }

    private static FlourishNavigationItem? GetNavigationItemFromInputSource(object source)
    {
        return
            source is DependencyObject dependencyObject
            && FindAncestor<ListBoxItem>(dependencyObject)?.DataContext
                is FlourishNavigationItem item
            ? item
            : null;
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

    private static void SetPanelContent(
        WpfPanel host,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        for (var index = 0; index < elements.Count; index++)
        {
            var element = elements[index];
            if (index < host.Children.Count && ReferenceEquals(host.Children[index], element))
            {
                continue;
            }

            var existingIndex = host.Children.IndexOf(element);
            if (existingIndex >= 0)
            {
                host.Children.RemoveAt(existingIndex);
            }

            host.Children.Insert(index, element);
        }

        while (host.Children.Count > elements.Count)
        {
            host.Children.RemoveAt(host.Children.Count - 1);
        }

        host.Visibility = elements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}

internal enum NavigationItemRequestKind
{
    Invoke,
    Selection,
}

internal sealed class NavigationItemRequestedEventArgs(
    FlourishNavigationItem item,
    NavigationItemRequestKind kind
) : EventArgs
{
    internal FlourishNavigationItem Item { get; } =
        item ?? throw new ArgumentNullException(nameof(item));

    internal NavigationItemRequestKind Kind { get; } = kind;
}
