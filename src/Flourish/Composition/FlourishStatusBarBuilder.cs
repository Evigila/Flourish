using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishStatusBarBuilder(
    FlourishShellOptions options,
    FlourishLocalizationService? localizationService = null
)
    : IFlourishStatusBarBuilder
{
    private readonly FlourishLocalizationService localizationService =
        localizationService ?? new FlourishLocalizationService(new FlourishDataOptions());

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
            ? localizationService.Get(FlourishLocaleKeys.StatusConnected)
            : localizationService.Get(FlourishLocaleKeys.StatusDisconnected);

        return AddStatusItem(text, "\uE701");
    }

    public IFlourishStatusBarBuilder ShowPowerStatus()
    {
        return AddStatusItem(
            localizationService.Get(FlourishLocaleKeys.StatusPower),
            "\uE850"
        );
    }
}
