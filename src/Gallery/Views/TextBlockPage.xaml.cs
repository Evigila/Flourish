using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class TextBlockPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Text", CKey.Controls_SetsTheDisplayedText_6B2DA7F7),
        new(
            "Role",
            CKey.Controls_SelectsASemanticTextRoleAndItsTypographyResources_67B80C97
        ),
        new(
            "TextWrapping",
            CKey.Controls_UsesTheNativeWPFWrappingBehaviorWhenContentNeedsMultipleLines_BEEAB3F9
        ),
    ];

    public string UsageCode { get; } =
        "<flourish:TextBlock\n"
        + "  Role=\"Status\"\n"
        + "  Text=\"Synchronization completed.\"\n"
        + "  TextWrapping=\"Wrap\" />";

    public TextBlockPage()
    {
        InitializeComponent();
    }
}
