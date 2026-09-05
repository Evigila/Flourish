using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class CodeSpacePage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
    [
        new(
            "Text",
            CKey.Controls_ContainsTheExactCodeTextDisplayedAndCopiedByTheControl_6A5A8805
        ),
        new(
            "ApplicationCommands.Copy",
            CKey.Controls_CopiesTextThroughTheBuiltInUpperRightAction_95EB53A7
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
