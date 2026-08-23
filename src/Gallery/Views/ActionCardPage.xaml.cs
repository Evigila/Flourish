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
            new(
                "Variant",
                Key.Controls_ChoosesTheHorizontalOrVerticalFixedLayout_8EF172C1
            ),
            new("Title", Key.Controls_SetsTheOptionalHeading_209DFEAA),
            new("Content", Key.Controls_SetsOneOptionalBlockOfSupportingCopy_C07BA241),
            new("Icon", Key.Controls_SetsOneOptionalIconGlyph_73C296CD),
            new("Body", Key.Controls_HostsExactlyOneInteractiveControl_DBA310D6),
        };
    }
}
