using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishStatusBuilder(FlourishShellOptions options) : IFlourishStatusBuilder
{
    public IFlourishStatusBuilder SetStatusText(string text)
    {
        options.StatusText = text;
        return this;
    }

    public IFlourishStatusBuilder AddStatusItem(string displayText, string iconGlyph)
    {
        options.StatusItems.Add(new FlourishStatusItem(displayText, iconGlyph));
        return this;
    }

    public IFlourishStatusBuilder ShowLANConnectionStatus()
    {
        var text = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
            ? "已连接"
            : "未连接";

        return AddStatusItem(text, "\uE701");
    }

    public IFlourishStatusBuilder ShowPowerStatus()
    {
        return AddStatusItem("电源", "\uE850");
    }
}
