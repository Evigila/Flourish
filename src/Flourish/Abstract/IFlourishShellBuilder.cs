using System.Windows;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishShellBuilder
{
    IFlourishShellBuilder SetTitle(string title);

    IFlourishShellBuilder SetSubtitle(string subtitle);

    IFlourishShellBuilder SetLogo(string packUri);

    IFlourishShellBuilder SetFont(string fontFamily, double fontSize);

    IFlourishShellBuilder SetFontFamily(string fontFamily);

    IFlourishShellBuilder SetFontSize(double fontSize);

    IFlourishShellBuilder SetIconFontFamily(string fontFamily);

    IFlourishShellBuilder UseTitlebar(
        bool EnableSearch = true,
        bool EnableBreadcrumb = true,
        bool EnableNavToggle = true,
        bool EnableLogo = true,
        bool EnableTitle = true,
        bool EnableSubTitle = true,
        bool EnableProfile = true,
        bool EnableTrayExit = false
    );

    IFlourishShellBuilder SetSearchPlaceholder(string placeholder);

    IFlourishShellBuilder SetWindowSize(double width, double height);

    IFlourishShellBuilder SetWindowMinSize(double minWidth, double minHeight);

    IFlourishShellBuilder SetWindowMaxSize(double maxWidth, double maxHeight);

    IFlourishShellBuilder SetWindowPosition(WindowStartupLocation startupLocation);

    IFlourishShellBuilder SetWindowPosition(double left, double top);

    IFlourishShellBuilder SetWindowState(WindowState windowState);

    IFlourishShellBuilder SetWindowResizeMode(ResizeMode resizeMode);

    IFlourishShellBuilder UseMaterialEffect(MaterialEffect effect);

    IFlourishShellBuilder UseTopmost(bool enabled = true);

    IFlourishShellBuilder ShowInTaskbar(bool enabled = true);

    IFlourishShellBuilder UseNavigationPanel(
        bool enabled = true,
        NavigationPanelDirection direction = NavigationPanelDirection.Left,
        string title = "Navigation",
        bool isInitiallyOpen = true
    );

    IFlourishShellBuilder UseDynamicToolbar(bool enabled = true);

    IFlourishShellBuilder SetBreadcrumbBehavior(
        BreadcrumbShowOption behavior = BreadcrumbShowOption.Auto
    );
}
