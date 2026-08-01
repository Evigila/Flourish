using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class BunchedListBoxPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("ItemsSource", "Supplies data items and generates BunchedListBoxItem containers."),
        new("SelectedItem", "Gets or sets the current selection."),
        new("SelectionMode", "Chooses single, multiple, or extended WPF selection semantics."),
        new("Appearance", "Chooses the Standard or Borderless surface."),
        new("IsCompact", "Uses collapsed navigation-item geometry when true."),
    ];

    public string UsageCode { get; } =
        "<flourish:BunchedListBox\n"
        + "  ItemsSource=\"{Binding Projects}\"\n"
        + "  SelectedItem=\"{Binding SelectedProject, Mode=TwoWay}\" />";

    public BunchedListBoxPage()
    {
        InitializeComponent();
    }
}
