using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class BunchedListBoxItemPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Content", "Sets the item label or content."),
        new("IsItemVisible", "Controls navigation-item visibility."),
        new("IsGroupHeader", "Marks the item as a navigation group heading."),
        new("IsCommandItem", "Marks the item as a command-dispatching navigation entry."),
        new("IsSelected", "Gets or sets the native WPF selection state."),
    ];

    public string UsageCode { get; } =
        "<flourish:BunchedListBox Appearance=\"Navigation\">\n"
        + "  <flourish:BunchedListBoxItem\n"
        + "    Content=\"Workspace\"\n"
        + "    IsGroupHeader=\"True\" />\n"
        + "  <flourish:BunchedListBoxItem\n"
        + "    Content=\"Refresh\"\n"
        + "    IsCommandItem=\"True\" />\n"
        + "</flourish:BunchedListBox>";

    public BunchedListBoxItemPage()
    {
        InitializeComponent();
    }
}
