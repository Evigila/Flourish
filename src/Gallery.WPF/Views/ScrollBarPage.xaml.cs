using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class ScrollBarPage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
    [
        new("Orientation", CKey.Controls_ChoosesVerticalOrHorizontalGeometry_C14694AA),
        new("Minimum / Maximum", CKey.Controls_DefineTheScrollableValueRange_7421BD50),
        new("Value", CKey.Controls_GetsOrSetsTheCurrentOffset_999EDD1B),
        new("ViewportSize", CKey.Controls_ControlsThumbSizeRelativeToTheRange_397C9A2A),
    ];

    public string UsageCode { get; } =
        "<flourish:ScrollBar\n"
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
