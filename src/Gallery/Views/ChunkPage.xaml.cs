using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ChunkPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", GalleryLocaleKeys.ControlsNamesTheSectionSRequiredTopic_350399A3),
        new("Content", GalleryLocaleKeys.ControlsAddsOptionalSupportingContext_5790F6CE),
        new("Body", GalleryLocaleKeys.ControlsHostsTheRequiredSectionContent_90A30B71),
    ];

    public ChunkPage()
    {
        InitializeComponent();
    }
}
