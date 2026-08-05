using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ScrollBarPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Orientation", GalleryLocaleKeys.ControlsChoosesVerticalOrHorizontalGeometry_C14694AA),
        new("Minimum / Maximum", GalleryLocaleKeys.ControlsDefineTheScrollableValueRange_7421BD50),
        new("Value", GalleryLocaleKeys.ControlsGetsOrSetsTheCurrentOffset_999EDD1B),
        new("ViewportSize", GalleryLocaleKeys.ControlsControlsThumbSizeRelativeToTheRange_397C9A2A),
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
