using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishStatusBarBuilder(FlourishShellOptions options)
    : IFlourishStatusBarBuilder
{
    public IFlourishStatusBarBuilder SetStatusText(string text)
    {
        options.StatusText = text;
        return this;
    }

    public IFlourishStatusBarBuilder AddStatusItem(string displayText, string iconGlyph)
    {
        options.StatusItems.Add(new FlourishStatusItem(displayText, iconGlyph));
        return this;
    }

    public IFlourishStatusBarBuilder ShowLANConnectionStatus()
    {
        var text = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
            ? "已连接"
            : "未连接";

        return AddStatusItem(text, "\uE701");
    }

    public IFlourishStatusBarBuilder ShowPowerStatus()
    {
        return AddStatusItem("电源", "\uE850");
    }
}
