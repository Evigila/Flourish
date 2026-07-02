using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishShellBuilder
{
    IFlourishShellBuilder UseTitlebar(
        Action<HostBuilderContext, IFlourishTitlebarBuilder> configureTitlebar
    );

    IFlourishShellBuilder UseNavigationPanel(
        Action<HostBuilderContext, IFlourishNavigationPanelBuilder> configureNavigationPanel
    );

    IFlourishShellBuilder SetWindowProperty(
        Action<HostBuilderContext, IFlourishWindowPropertyBuilder> configureWindow
    );

    IFlourishShellBuilder SetGlobalFont(string fontFamily, double fontSize);

    IFlourishShellBuilder UseMaterialEffect(MaterialEffect effect = MaterialEffect.Mica);

    IFlourishShellBuilder UseDynamicToolbar(bool enabled = true);
}
