using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ScrollBarPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Orientation", Key.Controls_ChoosesVerticalOrHorizontalGeometry_C14694AA),
        new("Minimum / Maximum", Key.Controls_DefineTheScrollableValueRange_7421BD50),
        new("Value", Key.Controls_GetsOrSetsTheCurrentOffset_999EDD1B),
        new("ViewportSize", Key.Controls_ControlsThumbSizeRelativeToTheRange_397C9A2A),
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
