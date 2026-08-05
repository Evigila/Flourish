using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class GridSplitterPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new(
            "Variant",
            GalleryLocaleKeys.ControlsChoosesTheStandardOrNavigationPaneLayoutRole_07EFD4F3
        ),
        new(
            "ResizeDirection",
            GalleryLocaleKeys.ControlsChoosesWhetherAdjacentRowsOrColumnsAreResized_4BD32627
        ),
        new(
            "ResizeBehavior",
            GalleryLocaleKeys.ControlsChoosesWhichNeighboringDefinitionsChange_E669504A
        ),
    ];

    public string UsageCode { get; } =
        "<Grid>\n"
        + "  <Grid.ColumnDefinitions>\n"
        + "    <ColumnDefinition Width=\"240\" MinWidth=\"160\" />\n"
        + "    <ColumnDefinition Width=\"Auto\" />\n"
        + "    <ColumnDefinition Width=\"*\" MinWidth=\"320\" />\n"
        + "  </Grid.ColumnDefinitions>\n"
        + "  <local:NavigationPane />\n"
        + "  <flourish:FlourishGridSplitter\n"
        + "    Grid.Column=\"1\"\n"
        + "    Variant=\"NavigationPane\" />\n"
        + "  <Frame Grid.Column=\"2\" />\n"
        + "</Grid>";

    public GridSplitterPage()
    {
        InitializeComponent();
    }
}
