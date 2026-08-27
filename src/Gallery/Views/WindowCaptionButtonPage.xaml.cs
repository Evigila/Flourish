using CKey = ArkheideSystem.Essential.Culture.Key;
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
            CKey.Controls_UsesTextForOrdinaryCaptionActionsAndDangerForClose_B98A046B
        ),
        new("Icon", CKey.Controls_SuppliesTheCaptionGlyph_F1AD27EF),
        new("Command", CKey.Controls_ConnectsActivationToAWindowOwnedAction_ABB942F5),
        new("IsEnabled", CKey.Controls_ControlsKeyboardAndPointerActivation_6A5B63B0),
        new("ToolTip", CKey.Controls_NamesTheIconOnlyCaptionAction_90AA1580),
    ];
}
