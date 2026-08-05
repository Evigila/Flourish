using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ToolTipPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Content", GalleryLocaleKeys.ControlsSetsConciseHelpContentForThePopup_5AFEF10E),
        new(
            "Placement",
            GalleryLocaleKeys.ControlsUsesNativeWPFPlacementWithFlourishShellRegionCorrection_CC370C90
        ),
        new("IsOpen", GalleryLocaleKeys.ControlsGetsOrSetsThePopupOpenState_CEC2542B),
        new(
            "ToolTipService",
            GalleryLocaleKeys.ControlsControlsDelayDurationAndHostBehaviorThroughWPFAttachedProperties_2CC65F82
        ),
    ];

    public string UsageCode { get; } =
        "<flourish:Button Content=\"Refresh\">\n"
        + "  <flourish:Button.ToolTip>\n"
        + "    <flourish:FlourishToolTip\n"
        + "      Content=\"Refresh the current workspace.\" />\n"
        + "  </flourish:Button.ToolTip>\n"
        + "</flourish:Button>\n\n"
        + "<!-- With ConfigureTips/UseTips, a short string is sufficient. -->\n"
        + "<flourish:Button Content=\"Save\" ToolTip=\"Save changes.\" />";

    public ToolTipPage()
    {
        InitializeComponent();
    }
}
