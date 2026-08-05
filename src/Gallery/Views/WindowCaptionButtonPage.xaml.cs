using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class WindowCaptionButtonPage : Page
{
    public WindowCaptionButtonPage()
    {
        InitializeComponent();
        PropertiesGrid.ItemsSource = propertyRows;
    }

    private static readonly ControlMemberRow[] propertyRows =
    [
        new(
            "Variant",
            GalleryLocaleKeys.ControlsUsesTextForOrdinaryCaptionActionsAndDangerForClose_B98A046B
        ),
        new("Icon", GalleryLocaleKeys.ControlsSuppliesTheCaptionGlyph_F1AD27EF),
        new("Command", GalleryLocaleKeys.ControlsConnectsActivationToAWindowOwnedAction_ABB942F5),
        new("IsEnabled", GalleryLocaleKeys.ControlsControlsKeyboardAndPointerActivation_6A5B63B0),
        new("ToolTip", GalleryLocaleKeys.ControlsNamesTheIconOnlyCaptionAction_90AA1580),
    ];
}
