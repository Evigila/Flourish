using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfPage = System.Windows.Controls.Page;
using UserControl = System.Windows.Controls.UserControl;
using Canvas = System.Windows.Controls.Canvas;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class ProfileOverlay : UserControl
{
    private Type? materializedContentType;

    public ProfileOverlay()
    {
        InitializeComponent();
    }

    internal event EventHandler? DismissRequested;

    internal event EventHandler? PlacementInvalidated;

    internal bool IsOpen => Visibility == Visibility.Visible;

    internal double CardActualWidth => ProfileCard.ActualWidth;

    internal double CardActualHeight => ProfileCard.ActualHeight;

    internal double CardWidth => ProfileCard.Width;

    internal WpfPage? ContentPage => ProfileFrame.Content as WpfPage;

    internal bool HasMaterializedContent(Type contentType) =>
        materializedContentType == contentType && ProfileFrame.Content is WpfPage;

    internal bool SetContent(WpfPage page, Type contentType)
    {
        if (!ProfileFrame.Navigate(page))
        {
            return false;
        }

        materializedContentType = contentType;
        return true;
    }

    internal void Open() => Visibility = Visibility.Visible;

    internal void Close() => Visibility = Visibility.Collapsed;

    internal void SetLayout(double left, double top, double maxWidth, double maxHeight)
    {
        ProfileCard.MaxWidth = maxWidth;
        ProfileCard.MaxHeight = maxHeight;
        Canvas.SetLeft(ProfileCard, left);
        Canvas.SetTop(ProfileCard, top);
    }

    private void OverlayCanvas_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var position = e.GetPosition(ProfileCard);
        if (
            position.X >= 0
            && position.Y >= 0
            && position.X <= ProfileCard.ActualWidth
            && position.Y <= ProfileCard.ActualHeight
        )
        {
            return;
        }

        DismissRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void ProfileCard_SizeChanged(object sender, SizeChangedEventArgs e) =>
        PlacementInvalidated?.Invoke(this, EventArgs.Empty);

    private void ProfileFrame_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
        {
            ProfileFrame.Clip = null;
            return;
        }

        var innerCornerRadius = TryFindResource("FlourishOverlayCornerRadius")
            is CornerRadius cornerRadius
                ? Math.Max(0, cornerRadius.TopLeft)
                : 0;
        ProfileFrame.Clip = new RectangleGeometry(
            new Rect(new System.Windows.Point(), e.NewSize),
            innerCornerRadius,
            innerCornerRadius
        );
    }
}
