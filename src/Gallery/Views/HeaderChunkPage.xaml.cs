using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class HeaderChunkPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", "Names the page and uses the emphasized header title role."),
        new("Content", "Adds supporting page context."),
        new("Body", "Hosts controls in the same region as the copy."),
        new("Presentation", "Hosts the page illustration or composed visual."),
        new("PresenterMode", "Chooses Split, Overlay, or TopDown composition."),
        new("PresenterPosition", "Places Split presentation content on the left or right."),
    ];

    public HeaderChunkPage()
    {
        InitializeComponent();
    }

    private void SplitRight_Click(object sender, RoutedEventArgs e)
    {
        HeaderPreview.PresenterMode = PresenterMode.Split;
        HeaderPreview.PresenterPosition = PresenterPosition.Right;
    }

    private void SplitLeft_Click(object sender, RoutedEventArgs e)
    {
        HeaderPreview.PresenterMode = PresenterMode.Split;
        HeaderPreview.PresenterPosition = PresenterPosition.Left;
    }

    private void Overlay_Click(object sender, RoutedEventArgs e) =>
        HeaderPreview.PresenterMode = PresenterMode.Overlay;

    private void TopDown_Click(object sender, RoutedEventArgs e) =>
        HeaderPreview.PresenterMode = PresenterMode.TopDown;
}
