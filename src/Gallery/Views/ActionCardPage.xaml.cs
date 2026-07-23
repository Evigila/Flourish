using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ActionCardPage : Page
{
    public ActionCardPage()
    {
        InitializeComponent();
        ActionCardMemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Variant", "Chooses the Horizontal or Vertical fixed layout."),
            new("Title", "Sets the optional heading."),
            new("Content", "Sets one optional block of supporting copy."),
            new("Icon", "Sets one optional icon glyph."),
            new("Body", "Hosts exactly one interactive control."),
        };
    }
}
