using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Flourish.WPF.Models;

namespace ArkheideSystem.Gallery.Flourish.WPF.Views;

public partial class PresenterPage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
    [
        new("Title", CKey.Controls_RequiredExplicitHeadingForThePresentation_FCE3FE80),
        new(
            "Content",
            CKey.Controls_RequiredExplicitSupportingCopyBelowTheHeading_A2E6EA83
        ),
        new(
            "Body",
            CKey.Controls_ExplicitlyHostsControlsLeftAlignedWithTheCopy_AD5BD289
        ),
        new(
            "Presentation",
            CKey.Controls_DefaultXAMLContentCenteredInTheRoundedSplitPresentationSurface_5A79E01D
        ),
        new(
            "PresenterMode",
            CKey.Controls_RequiredExplicitSplitOverlayOrTopDownComposition_2E42290A
        ),
        new(
            "PresenterPosition",
            CKey.Controls_PlacesSplitPresentationContentOnTheLeftOrRightOtherModesIgnoreIt_EF642202
        ),
    ];

    public PresenterPage()
    {
        InitializeComponent();
    }
}
