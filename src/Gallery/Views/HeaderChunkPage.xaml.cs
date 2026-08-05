using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class HeaderChunkPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new(
            "Title",
            GalleryLocaleKeys.ControlsNamesThePageAndUsesTheEmphasizedHeaderTitleRole_D824091B
        ),
        new("Content", GalleryLocaleKeys.ControlsAddsSupportingPageContext_DE57218F),
        new("Body", GalleryLocaleKeys.ControlsHostsControlsInTheSameRegionAsTheCopy_BD046E4D),
        new(
            "Presentation",
            GalleryLocaleKeys.ControlsHostsThePageIllustrationOrComposedVisual_4F9EE000
        ),
        new(
            "PresenterMode",
            GalleryLocaleKeys.ControlsChoosesSplitOverlayOrTopDownComposition_C5760298
        ),
        new(
            "PresenterPosition",
            GalleryLocaleKeys.ControlsPlacesSplitPresentationContentOnTheLeftOrRight_42F1BD88
        ),
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
