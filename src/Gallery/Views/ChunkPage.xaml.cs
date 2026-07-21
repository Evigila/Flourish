using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ChunkPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", "Names the topic represented by a Chunk or Presenter."),
        new("Description", "Adds optional supporting context."),
        new("Body", "Hosts the required section or presenter content."),
        new("Presentation", "Hosts ChunkHero imagery, icons, or other presented content."),
        new("PresenterMode", "Chooses Split or Overlay composition."),
        new("PresenterPosition", "Places Split presentation content on the left or right."),
    ];

    public ChunkPage()
    {
        InitializeComponent();
    }

    private void PresenterRight_Click(object sender, RoutedEventArgs e)
    {
        HeroPreview.PresenterMode = PresenterMode.Split;
        HeroPreview.PresenterPosition = PresenterPosition.Right;
    }

    private void PresenterLeft_Click(object sender, RoutedEventArgs e)
    {
        HeroPreview.PresenterMode = PresenterMode.Split;
        HeroPreview.PresenterPosition = PresenterPosition.Left;
    }

    private void Overlay_Click(object sender, RoutedEventArgs e) =>
        HeroPreview.PresenterMode = PresenterMode.Overlay;
}
