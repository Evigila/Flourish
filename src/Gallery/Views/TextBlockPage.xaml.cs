using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class TextBlockPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Text", GalleryLocaleKeys.ControlsSetsTheDisplayedText_6B2DA7F7),
        new(
            "Role",
            GalleryLocaleKeys.ControlsSelectsASemanticFlourishTextRoleAndItsTypographyResources_67B80C97
        ),
        new(
            "TextWrapping",
            GalleryLocaleKeys.ControlsUsesTheNativeWPFWrappingBehaviorWhenContentNeedsMultipleLines_BEEAB3F9
        ),
    ];

    public string UsageCode { get; } =
        "<flourish:FlourishTextBlock\n"
        + "  Role=\"Status\"\n"
        + "  Text=\"Synchronization completed.\"\n"
        + "  TextWrapping=\"Wrap\" />";

    public TextBlockPage()
    {
        InitializeComponent();
    }
}
