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
                GalleryLocaleKeys.ControlsChoosesTheHorizontalOrVerticalFixedLayout_8EF172C1
            ),
            new("Title", GalleryLocaleKeys.ControlsSetsTheOptionalHeading_209DFEAA),
            new("Content", GalleryLocaleKeys.ControlsSetsOneOptionalBlockOfSupportingCopy_C07BA241),
            new("Icon", GalleryLocaleKeys.ControlsSetsOneOptionalIconGlyph_73C296CD),
            new("Body", GalleryLocaleKeys.ControlsHostsExactlyOneInteractiveControl_DBA310D6),
        };
    }
}
