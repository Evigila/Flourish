using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class CardPage : Page
{
    public CardPage()
    {
        InitializeComponent();
        CardMemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Variant", GalleryLocaleKeys.ControlsChoosesStandardTonalFilledOrElevated_BAFD3BC6),
            new("Title", GalleryLocaleKeys.ControlsSetsTheOptionalHeading_209DFEAA),
            new("Content", GalleryLocaleKeys.ControlsSetsOneOptionalBlockOfSupportingCopy_C07BA241),
            new("Icon", GalleryLocaleKeys.ControlsSetsOneOptionalIconGlyph_73C296CD),
            new(
                "IconPosition",
                GalleryLocaleKeys.ControlsPlacesTheIconOnTheLeftTopRightOrBottom_05E57661
            ),
            new(
                "ContentHorizontalAlignment",
                GalleryLocaleKeys.ControlsAlignsTheTitleAndCopyGroupHorizontally_645A8315
            ),
            new(
                "ContentVerticalAlignment",
                GalleryLocaleKeys.ControlsAlignsTheTitleAndCopyGroupVertically_B79A88BE
            ),
        };
    }
}
