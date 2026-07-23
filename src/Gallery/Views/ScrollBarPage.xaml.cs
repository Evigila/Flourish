using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ScrollBarPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Orientation", "Chooses vertical or horizontal geometry."),
        new("Minimum / Maximum", "Define the scrollable value range."),
        new("Value", "Gets or sets the current offset."),
        new("ViewportSize", "Controls thumb size relative to the range."),
    ];

    public string UsageCode { get; } =
        "<flourish:FlourishScrollBar\n"
        + "  Minimum=\"0\"\n"
        + "  Maximum=\"{Binding ScrollableHeight}\"\n"
        + "  Orientation=\"Vertical\"\n"
        + "  Value=\"{Binding VerticalOffset, Mode=TwoWay}\"\n"
        + "  ViewportSize=\"{Binding ViewportHeight}\" />";

    public ScrollBarPage()
    {
        InitializeComponent();
    }
}
