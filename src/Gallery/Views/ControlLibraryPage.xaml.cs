using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ControlLibraryPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Text / Content", "Supplies editable text, labels, or item content."),
        new("ItemsSource", "Binds collection controls to application-owned items."),
        new("SelectedItem", "Reads or updates the current selection."),
        new("IsEnabled", "Includes the control in keyboard and pointer interaction."),
    ];

    public ControlLibraryPage()
    {
        InitializeComponent();
    }
}
