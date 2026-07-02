using System.Windows;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishWindowPropertyBuilder
{
    IFlourishWindowPropertyBuilder SetWindowSize(double width, double height);

    IFlourishWindowPropertyBuilder SetWindowMinSize(double minWidth, double minHeight);

    IFlourishWindowPropertyBuilder SetWindowMaxSize(double maxWidth, double maxHeight);

    IFlourishWindowPropertyBuilder SetWindowPosition(WindowStartupLocation startupLocation);

    IFlourishWindowPropertyBuilder SetWindowPosition(double left, double top);

    IFlourishWindowPropertyBuilder SetWindowState(WindowState windowState);

    IFlourishWindowPropertyBuilder SetWindowResizeMode(ResizeMode resizeMode);

    IFlourishWindowPropertyBuilder UseTopmost(bool enabled = true);

    IFlourishWindowPropertyBuilder ShowInTaskbar(bool enabled = true);
}
