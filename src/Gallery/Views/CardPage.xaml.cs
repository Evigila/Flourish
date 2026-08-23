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
            new("Variant", Key.Controls_ChoosesStandardTonalFilledOrElevated_BAFD3BC6),
            new("Title", Key.Controls_SetsTheOptionalHeading_209DFEAA),
            new("Content", Key.Controls_SetsOneOptionalBlockOfSupportingCopy_C07BA241),
            new("Icon", Key.Controls_SetsOneOptionalIconGlyph_73C296CD),
            new(
                "IconPosition",
                Key.Controls_PlacesTheIconOnTheLeftTopRightOrBottom_05E57661
            ),
            new(
                "ContentHorizontalAlignment",
                Key.Controls_AlignsTheTitleAndCopyGroupHorizontally_645A8315
            ),
            new(
                "ContentVerticalAlignment",
                Key.Controls_AlignsTheTitleAndCopyGroupVertically_B79A88BE
            ),
        };
    }
}
