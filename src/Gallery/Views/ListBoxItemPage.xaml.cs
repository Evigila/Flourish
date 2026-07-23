using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ListBoxItemPage : Page
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
        "<flourish:FlourishListBox Appearance=\"Navigation\">\n"
        + "  <flourish:FlourishListBoxItem\n"
        + "    Content=\"Workspace\"\n"
        + "    IsGroupHeader=\"True\" />\n"
        + "  <flourish:FlourishListBoxItem\n"
        + "    Content=\"Refresh\"\n"
        + "    IsCommandItem=\"True\" />\n"
        + "</flourish:FlourishListBox>";

    public ListBoxItemPage()
    {
        InitializeComponent();
    }
}
