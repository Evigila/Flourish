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
            Key.Controls_UsesTextForOrdinaryCaptionActionsAndDangerForClose_B98A046B
        ),
        new("Icon", Key.Controls_SuppliesTheCaptionGlyph_F1AD27EF),
        new("Command", Key.Controls_ConnectsActivationToAWindowOwnedAction_ABB942F5),
        new("IsEnabled", Key.Controls_ControlsKeyboardAndPointerActivation_6A5B63B0),
        new("ToolTip", Key.Controls_NamesTheIconOnlyCaptionAction_90AA1580),
    ];
}
