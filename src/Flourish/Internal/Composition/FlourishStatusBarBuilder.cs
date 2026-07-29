using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishStatusBarBuilder(FlourishShellOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishStatusBarBuilder
{
    public IFlourishStatusBarBuilder AddStatusItem(
        string displayText = "OK",
        string iconGlyph = "\uE930"
    )
    {
        ThrowIfFrozen();
        options.StatusItems.Add(new FlourishStatusItem(displayText, iconGlyph));
        return this;
    }

    public IFlourishStatusBarBuilder UseLanConnectionStatus(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsLANConnectionStatusEnabled = enabled;
        return this;
    }

    public IFlourishStatusBarBuilder UsePowerStatus(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsPowerStatusEnabled = enabled;
        return this;
    }
}
