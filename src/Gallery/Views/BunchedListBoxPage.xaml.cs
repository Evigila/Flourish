using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class BunchedListBoxPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new(
            "ItemsSource",
            GalleryLocaleKeys.ControlsSuppliesDataItemsAndGeneratesBunchedListBoxItemContainers_9F1AB28B
        ),
        new("SelectedItem", GalleryLocaleKeys.ControlsGetsOrSetsTheCurrentSelection_1F2CA123),
        new(
            "SelectionMode",
            GalleryLocaleKeys.ControlsChoosesSingleMultipleOrExtendedWPFSelectionSemantics_4D921AD6
        ),
        new("Appearance", GalleryLocaleKeys.ControlsChoosesTheStandardOrBorderlessSurface_B401F34B),
        new(
            "IsCompact",
            GalleryLocaleKeys.ControlsUsesCollapsedNavigationItemGeometryWhenTrue_A8D228F7
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
