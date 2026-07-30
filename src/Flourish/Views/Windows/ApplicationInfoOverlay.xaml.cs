using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using WpfPanel = System.Windows.Controls.Panel;
using UserControl = System.Windows.Controls.UserControl;
using Canvas = System.Windows.Controls.Canvas;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class ApplicationInfoOverlay : UserControl
{
    public ApplicationInfoOverlay()
    {
        InitializeComponent();
    }

    internal event EventHandler? DismissRequested;

    internal event EventHandler? PlacementInvalidated;

    internal bool IsOpen => Visibility == Visibility.Visible;

    internal double CardActualWidth => TitleBarFlyoutCard.ActualWidth;

    internal double CardActualHeight => TitleBarFlyoutCard.ActualHeight;

    internal double CardWidth => TitleBarFlyoutCard.Width;

    internal FrameworkElement? PlacementTarget
    {
        get => TitleBarFlyoutCard.PlacementTarget;
        set => TitleBarFlyoutCard.PlacementTarget = value;
    }

    internal void SetState(
        FlourishTitleBarState titleState,
        FlourishProjectSnapshot projectState,
        ImageSource? logoSource
    )
    {
        ApplicationInfoLogoImage.Source = logoSource;
        ApplicationInfoLogoImage.Visibility =
            logoSource is null ? Visibility.Collapsed : Visibility.Visible;
        ApplicationInfoLogoFallback.Text = string.IsNullOrWhiteSpace(
            titleState.LogoFallbackText
        )
            ? "F"
            : titleState.LogoFallbackText[..1];
        ApplicationInfoLogoFallback.Visibility =
            logoSource is null ? Visibility.Visible : Visibility.Collapsed;

        SetTextVisibility(
            ApplicationInfoTitle,
            titleState.ApplicationTitle,
            titleState.ShowApplicationTitle
        );
        SetTextVisibility(
            ApplicationInfoSubTitle,
            titleState.ApplicationSubTitle,
            titleState.ShowApplicationSubTitle
        );
        var projectTitle = projectState.IsMultiProjectEnabled
            && projectState.ActiveProject is { } activeProject
                ? activeProject.StoragePath is null
                    ? titleState.UnnamedProjectPlaceholder
                    : activeProject.Name
                : null;
        SetTextVisibility(
            ApplicationInfoProjectTitle,
            projectTitle ?? string.Empty,
            titleState.ShowProjectTitle
        );
        ApplicationInfoBodyScrollViewer.Visibility =
            ApplicationInfoBodyHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        var identityParts = new[]
        {
            GetVisibleText(ApplicationInfoTitle),
            GetVisibleText(ApplicationInfoSubTitle),
            GetVisibleText(ApplicationInfoProjectTitle),
        }.Where(value => !string.IsNullOrWhiteSpace(value));
        AutomationProperties.SetName(TitleBarFlyoutCard, string.Join(", ", identityParts));
    }

    internal void SetBody(IReadOnlyList<FrameworkElement> elements)
    {
        SynchronizeChildren(ApplicationInfoBodyHost, elements);
        ApplicationInfoBodyScrollViewer.Visibility =
            elements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    internal void Open() => Visibility = Visibility.Visible;

    internal void Close()
    {
        Visibility = Visibility.Collapsed;
        PlacementTarget = null;
    }

    internal void FocusContent() => TitleBarFlyoutCard.Focus();

    internal void SetLayout(double left, double top, double maxWidth, double maxHeight)
    {
        TitleBarFlyoutCard.MaxWidth = maxWidth;
        TitleBarFlyoutCard.MaxHeight = maxHeight;
        Canvas.SetLeft(TitleBarFlyoutCard, left);
        Canvas.SetTop(TitleBarFlyoutCard, top);
    }

    private void OverlayCanvas_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var position = e.GetPosition(TitleBarFlyoutCard);
        if (
            position.X >= 0
            && position.Y >= 0
            && position.X <= TitleBarFlyoutCard.ActualWidth
            && position.Y <= TitleBarFlyoutCard.ActualHeight
        )
        {
            return;
        }

        DismissRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void TitleBarFlyoutCard_DismissRequested(object sender, RoutedEventArgs e)
    {
        DismissRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void TitleBarFlyoutCard_SizeChanged(object sender, SizeChangedEventArgs e) =>
        PlacementInvalidated?.Invoke(this, EventArgs.Empty);

    private static void SetTextVisibility(
        System.Windows.Controls.TextBlock element,
        string text,
        bool enabled
    )
    {
        element.Text = text;
        element.Visibility =
            enabled && !string.IsNullOrWhiteSpace(text)
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private static string? GetVisibleText(System.Windows.Controls.TextBlock element) =>
        element.Visibility == Visibility.Visible ? element.Text : null;

    private static void SynchronizeChildren(
        WpfPanel host,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        for (var index = 0; index < elements.Count; index++)
        {
            var element = elements[index];
            var currentIndex = host.Children.IndexOf(element);
            if (currentIndex == index)
            {
                continue;
            }

            if (currentIndex >= 0)
            {
                host.Children.RemoveAt(currentIndex);
            }

            host.Children.Insert(index, element);
        }

        while (host.Children.Count > elements.Count)
        {
            host.Children.RemoveAt(host.Children.Count - 1);
        }
    }
}
