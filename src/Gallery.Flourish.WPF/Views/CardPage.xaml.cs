using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Flourish.WPF.Models;

namespace ArkheideSystem.Gallery.Flourish.WPF.Views;

public partial class CardPage : Page
{
    public CardPage()
    {
        InitializeComponent();
        CardMemberGrid.ItemsSource = new MemberRow[]
        {
            new("Variant", CKey.Controls_ChoosesStandardTonalFilledOrElevated_BAFD3BC6),
            new("Title", CKey.Controls_SetsTheOptionalHeading_209DFEAA),
            new("Content", CKey.Controls_SetsOneOptionalBlockOfSupportingCopy_C07BA241),
            new("Icon", CKey.Controls_SetsOneOptionalIconGlyph_73C296CD),
            new(
                "IconPosition",
                CKey.Controls_PlacesTheIconOnTheLeftTopRightOrBottom_05E57661
            ),
            new(
                "ContentHorizontalAlignment",
                CKey.Controls_AlignsTheTitleAndCopyGroupHorizontally_645A8315
            ),
            new(
                "ContentVerticalAlignment",
                CKey.Controls_AlignsTheTitleAndCopyGroupVertically_B79A88BE
            ),
        };
    }
}
