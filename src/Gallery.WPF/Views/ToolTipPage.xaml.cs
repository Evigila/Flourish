using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class ToolTipPage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
    [
        new("Content", CKey.Controls_SetsConciseHelpContentForThePopup_5AFEF10E),
        new(
            "Placement",
            CKey.Controls_UsesNativeWPFPlacementWithFlourishShellRegionCorrection_CC370C90
        ),
        new("IsOpen", CKey.Controls_GetsOrSetsThePopupOpenState_CEC2542B),
        new(
            "ToolTipService",
            CKey.Controls_ControlsDelayDurationAndHostBehaviorThroughWPFAttachedProperties_2CC65F82
        ),
    ];

    public string UsageCode { get; } =
        "<flourish:Button Content=\"Refresh\">\n"
        + "  <flourish:Button.ToolTip>\n"
        + "    <flourish:ToolTip\n"
        + "      Content=\"Refresh the current workspace.\" />\n"
        + "  </flourish:Button.ToolTip>\n"
        + "</flourish:Button>\n\n"
        + "<!-- With ConfigureToolTips enabled, a short string is sufficient. -->\n"
        + "<flourish:Button Content=\"Save\" ToolTip=\"Save changes.\" />";

    public ToolTipPage()
    {
        InitializeComponent();
    }
}
