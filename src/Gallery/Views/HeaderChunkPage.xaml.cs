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
            Key.Controls_NamesThePageAndUsesTheEmphasizedHeaderTitleRole_D824091B
        ),
        new("Content", Key.Controls_AddsSupportingPageContext_DE57218F),
        new("Body", Key.Controls_HostsControlsInTheSameRegionAsTheCopy_BD046E4D),
        new(
            "Presentation",
            Key.Controls_HostsThePageIllustrationOrComposedVisual_4F9EE000
        ),
        new(
            "PresenterMode",
            Key.Controls_ChoosesSplitOverlayOrTopDownComposition_C5760298
        ),
        new(
            "PresenterPosition",
            Key.Controls_PlacesSplitPresentationContentOnTheLeftOrRight_42F1BD88
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
