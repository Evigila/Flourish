using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishFooterBuilder(FlourishShellOptions options) : IFlourishFooterBuilder
{
    public IFlourishFooterBuilder SetStatusText(string text)
    {
        options.StatusText = text;
        return this;
    }

    public IFlourishFooterBuilder AddStatusItem(string displayText, string iconGlyph)
    {
        options.StatusItems.Add(new FlourishStatusItem(displayText, iconGlyph));
        return this;
    }

    public IFlourishFooterBuilder ShowLANConnectionStatus()
    {
        var text = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
            ? "已连接"
            : "未连接";

        return AddStatusItem(text, "\uE701");
    }

    public IFlourishFooterBuilder ShowPowerStatus()
    {
        return AddStatusItem("电源", "\uE850");
    }
}
