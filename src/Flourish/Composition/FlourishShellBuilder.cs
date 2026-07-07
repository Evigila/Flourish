using AckSS.Flourish.Abstract;
using AckSS.Flourish.Configuration;

namespace AckSS.Flourish.Composition;

internal sealed class FlourishShellBuilder(FlourishShellOptions options) : IFlourishShellBuilder
{
    public IFlourishShellBuilder UseTitleBar(bool enabled = true)
    {
        options.IsTitlebarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseNavigation(bool enabled = true)
    {
        options.IsNavigationPanelEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseDynamicToolbar(bool enabled = true)
    {
        options.IsDynamicToolbarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseTips(bool enabled = true)
    {
        options.IsTipsEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseMotion(bool enabled = true)
    {
        options.Motion.IsEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseMaterialEffect(bool enabled = true)
    {
        options.IsMaterialEffectEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseThemes(bool enabled = true)
    {
        options.IsThemeEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseFooter(bool enabled = true)
    {
        options.IsStatusBarEnabled = enabled;
        return this;
    }
}
