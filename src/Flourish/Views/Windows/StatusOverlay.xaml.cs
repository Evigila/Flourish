using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using ArkheideSystem.Flourish.Controls;
using UserControl = System.Windows.Controls.UserControl;
using Canvas = System.Windows.Controls.Canvas;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class StatusOverlay : UserControl
{
    public StatusOverlay()
    {
        InitializeComponent();
    }

    internal event EventHandler? DismissRequested;

    internal event EventHandler? PlacementInvalidated;

    internal bool IsOpen => Visibility == Visibility.Visible;

    internal bool ContainsKeyboardFocus => StatusFlyoutCard.IsKeyboardFocusWithin;

    internal double CardActualWidth => StatusFlyoutCard.ActualWidth;

    internal double CardActualHeight => StatusFlyoutCard.ActualHeight;

    internal double CardWidth => StatusFlyoutCard.Width;

    internal double CardMaxHeight => StatusFlyoutCard.MaxHeight;

    internal OverlayVariant Variant
    {
        get => StatusFlyoutCard.Variant;
        set => StatusFlyoutCard.Variant = value;
    }

    internal FrameworkElement? PlacementTarget
    {
        get => StatusFlyoutCard.PlacementTarget;
        set => StatusFlyoutCard.PlacementTarget = value;
    }

    internal void SetTitle(string title)
    {
        StatusFlyoutTitle.Text = title;
        AutomationProperties.SetName(StatusFlyoutTitle, title);
        AutomationProperties.SetName(StatusFlyoutCard, title);
    }

    internal void SetItems(IReadOnlyList<UIElement> desiredItems)
    {
        for (var index = 0; index < desiredItems.Count; index++)
        {
            var desired = desiredItems[index];
            var currentIndex = StatusFlyoutContentHost.Items.IndexOf(desired);
            if (currentIndex == index)
            {
                continue;
            }

            if (currentIndex >= 0)
            {
                StatusFlyoutContentHost.Items.RemoveAt(currentIndex);
            }

            StatusFlyoutContentHost.Items.Insert(index, desired);
        }

        while (StatusFlyoutContentHost.Items.Count > desiredItems.Count)
        {
            StatusFlyoutContentHost.Items.RemoveAt(
                StatusFlyoutContentHost.Items.Count - 1
            );
        }
    }

    internal void ClearItems() => StatusFlyoutContentHost.Items.Clear();

    internal void AppendItem(UIElement item) => StatusFlyoutContentHost.Items.Add(item);

    internal void Open() => Visibility = Visibility.Visible;

    internal void Close()
    {
        Visibility = Visibility.Collapsed;
        PlacementTarget = null;
    }

    internal bool FocusFallback() =>
        StatusFlyoutTitle.Focus() || StatusFlyoutCard.Focus();

    internal void SetLayout(double left, double top, double maxWidth, double maxHeight)
    {
        StatusFlyoutCard.MaxWidth = maxWidth;
        StatusFlyoutCard.MaxHeight = maxHeight;
        Canvas.SetLeft(StatusFlyoutCard, left);
        Canvas.SetTop(StatusFlyoutCard, top);
    }

    private void OverlayCanvas_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var position = e.GetPosition(StatusFlyoutCard);
        if (
            position.X >= 0
            && position.Y >= 0
            && position.X <= StatusFlyoutCard.ActualWidth
            && position.Y <= StatusFlyoutCard.ActualHeight
        )
        {
            return;
        }

        DismissRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void StatusFlyoutCard_DismissRequested(object sender, RoutedEventArgs e)
    {
        DismissRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void StatusFlyoutCard_SizeChanged(object sender, SizeChangedEventArgs e) =>
        PlacementInvalidated?.Invoke(this, EventArgs.Empty);
}
