using CKey = Arkheide.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class BunchedListBoxPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new(
            "ItemsSource",
            CKey.Controls_SuppliesDataItemsAndGeneratesBunchedListBoxItemContainers_9F1AB28B
        ),
        new("SelectedItem", CKey.Controls_GetsOrSetsTheCurrentSelection_1F2CA123),
        new(
            "SelectionMode",
            CKey.Controls_ChoosesSingleMultipleOrExtendedWPFSelectionSemantics_4D921AD6
        ),
        new("Appearance", CKey.Controls_ChoosesTheStandardOrBorderlessSurface_B401F34B),
        new(
            "IsCompact",
            CKey.Controls_UsesCollapsedNavigationItemGeometryWhenTrue_A8D228F7
        ),
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
