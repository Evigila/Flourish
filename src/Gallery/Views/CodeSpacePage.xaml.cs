using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class CodeSpacePage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new(
            "Text",
            GalleryLocaleKeys.ControlsContainsTheExactCodeTextDisplayedAndCopiedByTheControl_6A5A8805
        ),
        new(
            "ApplicationCommands.Copy",
            GalleryLocaleKeys.ControlsCopiesTextThroughTheBuiltInUpperRightAction_95EB53A7
        ),
    ];

    public string ExampleCode { get; } =
        "public static string Greet(string name)\n"
        + "{\n"
        + "    return $\"Hello, {name}!\";\n"
        + "}";

    public CodeSpacePage()
    {
        InitializeComponent();
    }
}
