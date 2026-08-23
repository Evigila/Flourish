using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ChunkPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", Key.Controls_NamesTheSectionSRequiredTopic_350399A3),
        new("Content", Key.Controls_AddsOptionalSupportingContext_5790F6CE),
        new("Body", Key.Controls_HostsTheRequiredSectionContent_90A30B71),
    ];

    public ChunkPage()
    {
        InitializeComponent();
    }
}
