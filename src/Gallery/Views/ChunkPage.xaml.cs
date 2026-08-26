using System.Collections.Generic;

using CKey = Arkheide.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ChunkPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", CKey.Controls_NamesTheSectionSRequiredTopic_350399A3),
        new("Content", CKey.Controls_AddsOptionalSupportingContext_5790F6CE),
        new("Body", CKey.Controls_HostsTheRequiredSectionContent_90A30B71),
    ];

    public ChunkPage()
    {
        InitializeComponent();
    }
}
