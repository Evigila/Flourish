using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class TextBlockPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Text", "Sets the displayed text."),
        new("Role", "Selects a semantic FlourishTextRole and its typography resources."),
        new("TextWrapping", "Uses the native WPF wrapping behavior when content needs multiple lines."),
    ];

    public string UsageCode { get; } =
        "<flourish:FlourishTextBlock\n"
        + "  Role=\"Status\"\n"
        + "  Text=\"Synchronization completed.\"\n"
        + "  TextWrapping=\"Wrap\" />";

    public TextBlockPage()
    {
        InitializeComponent();
    }
}
