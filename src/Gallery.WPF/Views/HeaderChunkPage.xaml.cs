using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class HeaderChunkPage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
    [
        new(
            "Title",
            CKey.Controls_NamesThePageAndUsesTheEmphasizedHeaderTitleRole_D824091B
        ),
        new("Content", CKey.Controls_AddsSupportingPageContext_DE57218F),
        new("Body", CKey.Controls_HostsControlsInTheSameRegionAsTheCopy_BD046E4D),
        new(
            "Presentation",
            CKey.Controls_HostsThePageIllustrationOrComposedVisual_4F9EE000
        ),
        new(
            "PresenterMode",
            CKey.Controls_ChoosesSplitOverlayOrTopDownComposition_C5760298
        ),
        new(
            "PresenterPosition",
            CKey.Controls_PlacesSplitPresentationContentOnTheLeftOrRight_42F1BD88
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
