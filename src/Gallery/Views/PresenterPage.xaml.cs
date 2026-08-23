using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class PresenterPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", Key.Controls_RequiredExplicitHeadingForThePresentation_FCE3FE80),
        new(
            "Content",
            Key.Controls_RequiredExplicitSupportingCopyBelowTheHeading_A2E6EA83
        ),
        new(
            "Body",
            Key.Controls_ExplicitlyHostsControlsLeftAlignedWithTheCopy_AD5BD289
        ),
        new(
            "Presentation",
            Key.Controls_DefaultXAMLContentCenteredInTheRoundedSplitPresentationSurface_5A79E01D
        ),
        new(
            "PresenterMode",
            Key.Controls_RequiredExplicitSplitOverlayOrTopDownComposition_2E42290A
        ),
        new(
            "PresenterPosition",
            Key.Controls_PlacesSplitPresentationContentOnTheLeftOrRightOtherModesIgnoreIt_EF642202
        ),
    ];

    public PresenterPage()
    {
        InitializeComponent();
    }
}
