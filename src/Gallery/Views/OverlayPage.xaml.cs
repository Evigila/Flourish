using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class OverlayPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Content", "Supplies the object displayed by the surface."),
        new("Variant", "Selects Temporary or Strong dismissal."),
        new("PlacementTarget", "Identifies the anchor used by Temporary hover tracking."),
        new("DismissRequested", "Asks the host to close the surface."),
    ];

    public OverlayPage()
    {
        InitializeComponent();
        TemporaryPopup.PlacementTarget = TemporaryTrigger;
        TemporaryOverlay.PlacementTarget = TemporaryTrigger;
        StrongPopup.PlacementTarget = StrongTrigger;
        StrongOverlay.PlacementTarget = StrongTrigger;
    }

    private void TemporaryTrigger_MouseEnter(object sender, MouseEventArgs e) =>
        TemporaryPopup.IsOpen = true;

    private void TemporaryTrigger_Click(object sender, RoutedEventArgs e) =>
        TemporaryPopup.IsOpen = true;

    private void StrongTrigger_Click(object sender, RoutedEventArgs e) =>
        StrongPopup.IsOpen = true;

    private void StrongPopup_Opened(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() => StrongCloseButton.Focus()));
    }

    private void StrongCloseButton_Click(object sender, RoutedEventArgs e)
    {
        StrongOverlay.RaiseEvent(
            new RoutedEventArgs(Overlay.DismissRequestedEvent, StrongOverlay)
        );
    }

    private void Overlay_DismissRequested(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(sender, TemporaryOverlay))
        {
            TemporaryPopup.IsOpen = false;
        }
        else if (ReferenceEquals(sender, StrongOverlay))
        {
            StrongPopup.IsOpen = false;
        }

        e.Handled = true;
    }

    private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || (!StrongPopup.IsOpen && !TemporaryPopup.IsOpen))
        {
            return;
        }

        StrongPopup.IsOpen = false;
        TemporaryPopup.IsOpen = false;
        e.Handled = true;
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        StrongPopup.IsOpen = false;
        TemporaryPopup.IsOpen = false;
    }
}
