using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class DocumentPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Items", "Contains Paragraph elements in reading order."),
        new("ItemsSource", "Binds an application-owned paragraph collection when needed."),
        new("Margin", "Adds the standard separation from Chunk title and content copy."),
    ];

    public DocumentPage()
    {
        InitializeComponent();
    }
}
