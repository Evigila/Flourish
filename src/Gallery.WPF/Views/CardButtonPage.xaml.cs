using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class CardButtonPage : Page
{
    public CardButtonPage()
    {
        InitializeComponent();
        PropertiesGrid.ItemsSource = propertyRows;
    }

    private static readonly MemberRow[] propertyRows =
    [
        new("Variant", CKey.Controls_SelectsCardEmphasisAndSemanticFeedback_0A19C046),
        new("Title", CKey.Controls_SuppliesOptionalHeadingContent_BB3AF24B),
        new("Content", CKey.Controls_SuppliesOptionalSupportingContent_790A7EA4),
        new("Icon", CKey.Controls_SuppliesAnOptionalSingleIcon_B821DB91),
        new("IconPosition", CKey.Controls_PlacesTheIconAboveOrBesideTheCopy_93065A79),
        new(
            "Command",
            CKey.Controls_ConnectsActivationToApplicationOwnedBehavior_A9E29329
        ),
        new("IsEnabled", CKey.Controls_ControlsKeyboardAndPointerActivation_6A5B63B0),
    ];
}
