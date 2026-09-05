using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class ListBoxPage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
    [
        new("Appearance", CKey.Controls_ChoosesTheStandardOrBorderlessSurface_B401F34B),
        new(
            "IsCompact",
            CKey.Controls_UsesCollapsedNavigationItemGeometryWhenTrue_A8D228F7
        ),
        new(
            "ItemsSource",
            CKey.Controls_SuppliesDataItemsAndGeneratesListBoxItemContainers_814EAA50
        ),
        new("SelectedItem", CKey.Controls_GetsOrSetsTheCurrentSelection_1F2CA123),
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
