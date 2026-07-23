using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ParagraphPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Text", "Sets the text for exactly one paragraph."),
        new("Document parent", "Supplies paragraph spacing, indentation, border, and reading layout."),
        new("Typography", "Uses the normalized Large-size paragraph presentation."),
    ];

    public string UsageCode { get; } =
        "<flourish:Document>\n"
        + "  <flourish:Paragraph Text=\"The first paragraph.\" />\n"
        + "  <flourish:Paragraph Text=\"The second paragraph.\" />\n"
        + "</flourish:Document>";

    public ParagraphPage()
    {
        InitializeComponent();
    }
}
