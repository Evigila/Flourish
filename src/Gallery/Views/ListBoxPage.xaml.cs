using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ListBoxPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Appearance", "Chooses the Standard or Navigation surface."),
        new("IsCompact", "Uses collapsed navigation-item geometry when true."),
        new("ItemsSource", "Supplies data items and generates FlourishListBoxItem containers."),
        new("SelectedItem", "Gets or sets the current selection."),
    ];

    public string UsageCode { get; } =
        "<flourish:FlourishListBox\n"
        + "  Appearance=\"Standard\"\n"
        + "  ItemsSource=\"{Binding Projects}\"\n"
        + "  SelectedItem=\"{Binding SelectedProject, Mode=TwoWay}\" />\n\n"
        + "// Update the selection at runtime.\n"
        + "ProjectList.SelectedItem = viewModel.ActiveProject;";

    public ListBoxPage()
    {
        InitializeComponent();
    }
}
