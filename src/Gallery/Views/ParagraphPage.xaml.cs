using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ParagraphPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Items", "Contains TextBlock paragraphs in reading order."),
        new("ItemsSource", "Binds an application-owned paragraph collection when needed."),
        new("Margin", "Adds the standard separation from Chunk title and description copy."),
    ];

    public ParagraphPage()
    {
        InitializeComponent();
    }
}
