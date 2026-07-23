using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ChunkPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Title", "Names the section's required topic."),
        new("Content", "Adds optional supporting context."),
        new("Body", "Hosts the required section content."),
    ];

    public ChunkPage()
    {
        InitializeComponent();
    }

}
