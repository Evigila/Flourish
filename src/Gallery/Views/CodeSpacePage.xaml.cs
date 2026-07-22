using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class CodeSpacePage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Text", "Contains the exact code text displayed and copied by the control."),
        new("ApplicationCommands.Copy", "Copies Text through the built-in upper-right action."),
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
