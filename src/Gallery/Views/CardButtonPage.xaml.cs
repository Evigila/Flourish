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
        new("Variant", GalleryLocaleKeys.ControlsSelectsCardEmphasisAndSemanticFeedback_0A19C046),
        new("Title", GalleryLocaleKeys.ControlsSuppliesOptionalHeadingContent_BB3AF24B),
        new("Content", GalleryLocaleKeys.ControlsSuppliesOptionalSupportingContent_790A7EA4),
        new("Icon", GalleryLocaleKeys.ControlsSuppliesAnOptionalSingleIcon_B821DB91),
        new("IconPosition", GalleryLocaleKeys.ControlsPlacesTheIconAboveOrBesideTheCopy_93065A79),
        new(
            "Command",
            GalleryLocaleKeys.ControlsConnectsActivationToApplicationOwnedBehavior_A9E29329
        ),
        new("IsEnabled", GalleryLocaleKeys.ControlsControlsKeyboardAndPointerActivation_6A5B63B0),
    ];
}
