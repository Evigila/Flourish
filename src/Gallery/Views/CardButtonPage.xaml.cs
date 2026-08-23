using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class CardButtonPage : Page
{
    public CardButtonPage()
    {
        InitializeComponent();
        PropertiesGrid.ItemsSource = propertyRows;
    }

    private static readonly ControlMemberRow[] propertyRows =
    [
        new("Variant", Key.Controls_SelectsCardEmphasisAndSemanticFeedback_0A19C046),
        new("Title", Key.Controls_SuppliesOptionalHeadingContent_BB3AF24B),
        new("Content", Key.Controls_SuppliesOptionalSupportingContent_790A7EA4),
        new("Icon", Key.Controls_SuppliesAnOptionalSingleIcon_B821DB91),
        new("IconPosition", Key.Controls_PlacesTheIconAboveOrBesideTheCopy_93065A79),
        new(
            "Command",
            Key.Controls_ConnectsActivationToApplicationOwnedBehavior_A9E29329
        ),
        new("IsEnabled", Key.Controls_ControlsKeyboardAndPointerActivation_6A5B63B0),
    ];
}
