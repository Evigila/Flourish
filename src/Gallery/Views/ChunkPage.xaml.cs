using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ChunkPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("ChunkTitle", "Names the topic represented by the section."),
        new("ChunkDescription", "Adds optional supporting context."),
        new("ChunkBody", "Hosts the section content."),
        new("PresenterMode", "Chooses Split or Overlay composition."),
        new("PresenterPosition", "Places a Split presenter on the left or right."),
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
