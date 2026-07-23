using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class CardPage : Page
{
    public CardPage()
    {
        InitializeComponent();
        CardMemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Variant", "Chooses Standard, Tonal, Filled, or Elevated."),
            new("Title", "Sets the optional heading."),
            new("Content", "Sets one optional block of supporting copy."),
            new("Icon", "Sets one optional icon glyph."),
            new("IconPosition", "Places the icon on the left, top, right, or bottom."),
            new("ContentHorizontalAlignment", "Aligns the title-and-copy group horizontally."),
            new("ContentVerticalAlignment", "Aligns the title-and-copy group vertically."),
        };
    }
}
