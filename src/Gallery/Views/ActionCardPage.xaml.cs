using CKey = ArkheideSystem.Essential.Culture.Key;
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
                CKey.Controls_ChoosesTheHorizontalOrVerticalFixedLayout_8EF172C1
            ),
            new("Title", CKey.Controls_SetsTheOptionalHeading_209DFEAA),
            new("Content", CKey.Controls_SetsOneOptionalBlockOfSupportingCopy_C07BA241),
            new("Icon", CKey.Controls_SetsOneOptionalIconGlyph_73C296CD),
            new("Body", CKey.Controls_HostsExactlyOneInteractiveControl_DBA310D6),
        };
    }
}
