using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ScrollViewerPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Content", GalleryLocaleKeys.ControlsHostsOneScrollableContentTree_F9A46D80),
        new(
            "IsSmoothScrollingEnabled",
            GalleryLocaleKeys.ControlsEnablesRenderOnlyMouseWheelInterpolation_8F88D907
        ),
        new(
            "CanContentScroll",
            GalleryLocaleKeys.ControlsSwitchesBetweenPhysicalAndLogicalScrolling_577AEBFD
        ),
        new(
            "VerticalScrollBarVisibility",
            GalleryLocaleKeys.ControlsControlsTheVerticalScrollBarPolicy_87BCE4CC
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
