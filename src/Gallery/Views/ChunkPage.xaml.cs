using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class ChunkPage : Page
{
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
