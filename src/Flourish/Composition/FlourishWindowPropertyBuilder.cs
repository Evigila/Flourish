using System.Windows;
using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishWindowPropertyBuilder(FlourishShellBuilder shellBuilder)
    : IFlourishWindowPropertyBuilder
{
    public IFlourishWindowPropertyBuilder SetWindowSize(double width, double height)
    {
        shellBuilder.SetWindowSize(width, height);
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowMinSize(double minWidth, double minHeight)
    {
        shellBuilder.SetWindowMinSize(minWidth, minHeight);
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowMaxSize(double maxWidth, double maxHeight)
    {
        shellBuilder.SetWindowMaxSize(maxWidth, maxHeight);
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowPosition(WindowStartupLocation startupLocation)
    {
        shellBuilder.SetWindowPosition(startupLocation);
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowPosition(double left, double top)
    {
        shellBuilder.SetWindowPosition(left, top);
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowState(WindowState windowState)
    {
        shellBuilder.SetWindowState(windowState);
        return this;
    }

    public IFlourishWindowPropertyBuilder SetWindowResizeMode(ResizeMode resizeMode)
    {
        shellBuilder.SetWindowResizeMode(resizeMode);
        return this;
    }

    public IFlourishWindowPropertyBuilder UseTopmost(bool enabled = true)
    {
        shellBuilder.UseTopmost(enabled);
        return this;
    }

    public IFlourishWindowPropertyBuilder ShowInTaskbar(bool enabled = true)
    {
        shellBuilder.ShowInTaskbar(enabled);
        return this;
    }
}
