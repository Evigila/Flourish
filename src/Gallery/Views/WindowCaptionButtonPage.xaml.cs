using ArkheideSystem.Gallery.Models;
using System.Windows.Controls;

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
        new("Variant", "Uses Text for ordinary caption actions and Danger for close."),
        new("Icon", "Supplies the caption glyph."),
        new("Command", "Connects activation to a window-owned action."),
        new("IsEnabled", "Controls keyboard and pointer activation."),
        new("ToolTip", "Names the icon-only caption action."),
    ];
}
