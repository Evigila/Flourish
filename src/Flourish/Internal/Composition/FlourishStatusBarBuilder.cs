using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishStatusBarBuilder(FlourishShellOptions options)
    : IFlourishStatusBarBuilder
{
    public IFlourishStatusBarBuilder AddStatusItem(string displayText, string iconGlyph)
    {
        options.StatusItems.Add(new FlourishStatusItem(displayText, iconGlyph));
        return this;
    }

    public IFlourishStatusBarBuilder ShowLANConnectionStatus()
    {
        options.IsLANConnectionStatusEnabled = true;
        return this;
    }

    public IFlourishStatusBarBuilder ShowPowerStatus()
    {
        options.IsPowerStatusEnabled = true;
        return this;
    }
}
