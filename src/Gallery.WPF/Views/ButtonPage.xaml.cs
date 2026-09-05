using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class ButtonPage : Page
{
    public ButtonPage()
    {
        InitializeComponent();
        PropertiesGrid.ItemsSource = propertyRows;
    }

    private static readonly MemberRow[] propertyRows =
    [
        new("Variant", CKey.Controls_SelectsVisualEmphasisAndSemanticFeedback_00B96251),
        new("Content", CKey.Controls_SuppliesTheVisibleLabelOrCustomContent_26C0C0B7),
        new(
            "Command",
            CKey.Controls_ConnectsActivationToApplicationOwnedBehavior_A9E29329
        ),
        new("IsEnabled", CKey.Controls_ControlsKeyboardAndPointerActivation_6A5B63B0),
        new("ToolTip", CKey.Controls_LabelsIconOnlyActions_5AA8BA24),
    ];
}
