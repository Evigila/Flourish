using ArkheideSystem.Gallery.Models;
using System.Windows.Controls;

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
        new("Variant", "Selects visual emphasis and semantic feedback."),
        new("Content", "Supplies the visible label or custom content."),
        new("Command", "Connects activation to application-owned behavior."),
        new("IsEnabled", "Controls keyboard and pointer activation."),
        new("ToolTip", "Labels icon-only actions."),
    ];

}
