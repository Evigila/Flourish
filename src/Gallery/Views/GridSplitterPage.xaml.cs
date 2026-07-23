using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class GridSplitterPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Variant", "Chooses Standard or NavigationPane behavior."),
        new("ResizeDirection", "Chooses whether adjacent rows or columns are resized."),
        new("ResizeBehavior", "Chooses which neighboring definitions change."),
        new("ShowsPreview", "Shows a preview indicator while dragging when enabled."),
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
