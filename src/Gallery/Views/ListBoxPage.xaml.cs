using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ListBoxPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Appearance", Key.Controls_ChoosesTheStandardOrBorderlessSurface_B401F34B),
        new(
            "IsCompact",
            Key.Controls_UsesCollapsedNavigationItemGeometryWhenTrue_A8D228F7
        ),
        new(
            "ItemsSource",
            Key.Controls_SuppliesDataItemsAndGeneratesFlourishListBoxItemContainers_814EAA50
        ),
        new("SelectedItem", Key.Controls_GetsOrSetsTheCurrentSelection_1F2CA123),
    ];

    public string UsageCode { get; } =
        "<flourish:ListBox\n"
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
