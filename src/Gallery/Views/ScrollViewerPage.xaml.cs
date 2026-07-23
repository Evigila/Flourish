using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ScrollViewerPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Content", "Hosts one scrollable content tree."),
        new("IsCompact", "Uses compact FlourishScrollBar geometry."),
        new("IsSmoothScrollingEnabled", "Enables render-only mouse-wheel interpolation."),
        new("CanContentScroll", "Switches between physical and logical scrolling."),
        new("VerticalScrollBarVisibility", "Controls the vertical scroll-bar policy."),
    ];

    public string UsageCode { get; } =
        "<flourish:ScrollViewer\n"
        + "  IsCompact=\"False\"\n"
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
