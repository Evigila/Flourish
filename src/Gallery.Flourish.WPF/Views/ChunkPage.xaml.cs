using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Flourish.WPF.Models;

namespace ArkheideSystem.Gallery.Flourish.WPF.Views;

public partial class ChunkPage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
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
