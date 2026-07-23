using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ToolTipPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Content", "Sets concise help content for the popup."),
        new("Placement", "Uses native WPF placement with Flourish shell-region correction."),
        new("IsOpen", "Gets or sets the popup open state."),
        new("ToolTipService", "Controls delay, duration, and host behavior through WPF attached properties."),
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
