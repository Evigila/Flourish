using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class PresenterPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", GalleryLocaleKeys.ControlsRequiredExplicitHeadingForThePresentation_FCE3FE80),
        new(
            "Content",
            GalleryLocaleKeys.ControlsRequiredExplicitSupportingCopyBelowTheHeading_A2E6EA83
        ),
        new(
            "Body",
            GalleryLocaleKeys.ControlsExplicitlyHostsControlsLeftAlignedWithTheCopy_AD5BD289
        ),
        new(
            "Presentation",
            GalleryLocaleKeys.ControlsDefaultXAMLContentCenteredInTheRoundedSplitPresentationSurface_5A79E01D
        ),
        new(
            "PresenterMode",
            GalleryLocaleKeys.ControlsRequiredExplicitSplitOverlayOrTopDownComposition_2E42290A
        ),
        new(
            "PresenterPosition",
            GalleryLocaleKeys.ControlsPlacesSplitPresentationContentOnTheLeftOrRightOtherModesIgnoreIt_EF642202
        ),
    ];

    public PresenterPage()
    {
        InitializeComponent();
    }
}
