using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Shell.StatusBar;

internal sealed class StatusBarBuilder(StatusBarOptions options)
    : BuilderMutationGuard,
        IStatusBarBuilder
{
    public IStatusBarBuilder SetEnabled(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsStatusBarEnabled = enabled;
        return this;
    }

    public IStatusBarBuilder AddStatusItem(
        string displayText = "OK",
        string iconGlyph = "\uE930"
    )
    {
        ThrowIfFrozen();
        options.StatusItems.Add(new StatusBarItem(displayText, iconGlyph));
        return this;
    }

    public IStatusBarBuilder SetLanStatusEnabled(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsLANConnectionStatusEnabled = enabled;
        return this;
    }

    public IStatusBarBuilder SetPowerStatusEnabled(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsPowerStatusEnabled = enabled;
        return this;
    }
}
