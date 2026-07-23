using ArkheideSystem.Gallery.Models;
using System.Windows.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class CardButtonPage : Page
{
    public CardButtonPage()
    {
        InitializeComponent();
        PropertiesGrid.ItemsSource = propertyRows;
    }

    private static readonly ControlMemberRow[] propertyRows =
    [
        new("Variant", "Selects card emphasis and semantic feedback."),
        new("Title", "Supplies optional heading content."),
        new("Content", "Supplies optional supporting content."),
        new("Icon", "Supplies an optional single icon."),
        new("IconPosition", "Places the icon above or beside the copy."),
        new("Command", "Connects activation to application-owned behavior."),
        new("IsEnabled", "Controls keyboard and pointer activation."),
    ];
}
