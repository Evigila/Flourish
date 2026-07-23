using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class PageBodyPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Children", "Contains the page-leading HeaderChunk and subsequent Chunk elements."),
        new("Content", "Is owned internally by PageBody and must not be replaced by callers."),
        new("Scrolling", "Provides the standard vertical page viewport and content margin."),
    ];

    public string StructureCode { get; } =
        "PageBody\n"
        + "\u251c\u2500 HeaderChunk (exactly one, always first)\n"
        + "\u251c\u2500 Chunk\n"
        + "\u2514\u2500 Chunk";

    public string UsageCode { get; } =
        "<flourish:PageBody>\n"
        + "  <flourish:HeaderChunk\n"
        + "    Title=\"Page title\"\n"
        + "    Content=\"Page summary.\"\n"
        + "    PresenterMode=\"Split\"\n"
        + "    PresenterPosition=\"Right\" />\n"
        + "  <flourish:Chunk Title=\"Section\">\n"
        + "    <flourish:Card Content=\"Section content.\" />\n"
        + "  </flourish:Chunk>\n"
        + "</flourish:PageBody>";

    public PageBodyPage()
    {
        InitializeComponent();
    }
}
