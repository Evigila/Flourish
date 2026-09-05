using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class PageBodyPage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
    [
        new(
            "Children",
            CKey.Controls_ContainsThePageLeadingHeaderChunkAndSubsequentChunkElements_8F1F22B3
        ),
        new(
            "Content",
            CKey.Controls_IsOwnedInternallyByPageBodyAndMustNotBeReplacedByCallers_2C86637B
        ),
        new(
            "Scrolling",
            CKey.Controls_ProvidesTheStandardVerticalPageViewportAndContentMargin_130DA46B
        ),
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
