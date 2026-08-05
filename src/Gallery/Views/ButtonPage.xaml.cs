using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ButtonPage : Page
{
    public ButtonPage()
    {
        InitializeComponent();
        PropertiesGrid.ItemsSource = propertyRows;
    }

    private static readonly ControlMemberRow[] propertyRows =
    [
        new("Variant", GalleryLocaleKeys.ControlsSelectsVisualEmphasisAndSemanticFeedback_00B96251),
        new("Content", GalleryLocaleKeys.ControlsSuppliesTheVisibleLabelOrCustomContent_26C0C0B7),
        new(
            "Command",
            GalleryLocaleKeys.ControlsConnectsActivationToApplicationOwnedBehavior_A9E29329
        ),
        new("IsEnabled", GalleryLocaleKeys.ControlsControlsKeyboardAndPointerActivation_6A5B63B0),
        new("ToolTip", GalleryLocaleKeys.ControlsLabelsIconOnlyActions_5AA8BA24),
    ];
}
