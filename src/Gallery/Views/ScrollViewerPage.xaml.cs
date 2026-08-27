using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ScrollViewerPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Content", CKey.Controls_HostsOneScrollableContentTree_F9A46D80),
        new(
            "IsSmoothScrollingEnabled",
            CKey.Controls_EnablesRenderOnlyMouseWheelInterpolation_8F88D907
        ),
        new(
            "CanContentScroll",
            CKey.Controls_SwitchesBetweenPhysicalAndLogicalScrolling_577AEBFD
        ),
        new(
            "VerticalScrollBarVisibility",
            CKey.Controls_ControlsTheVerticalScrollBarPolicy_87BCE4CC
        ),
    ];

    public string UsageCode { get; } =
        "<flourish:ScrollViewer\n"
        + "  IsSmoothScrollingEnabled=\"True\"\n"
        + "  VerticalScrollBarVisibility=\"Auto\">\n"
        + "  <StackPanel>\n"
        + "    <!-- Scrollable content -->\n"
        + "  </StackPanel>\n"
        + "</flourish:ScrollViewer>\n\n"
        + "// Move the viewport at runtime.\n"
        + "ContentViewport.ScrollToVerticalOffset(240);";

    public ScrollViewerPage()
    {
        InitializeComponent();
    }
}
