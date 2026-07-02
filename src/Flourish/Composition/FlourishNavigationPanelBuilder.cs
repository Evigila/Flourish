using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishNavigationPanelBuilder(FlourishShellOptions options)
    : IFlourishNavigationPanelBuilder
{
    public IFlourishNavigationPanelBuilder SetEnabled(bool enabled = true)
    {
        options.IsNavigationPanelEnabled = enabled;
        return this;
    }

    public IFlourishNavigationPanelBuilder SetDirection(
        NavigationPanelDirection direction = NavigationPanelDirection.Left
    )
    {
        options.NavigationPanelDirection = direction;
        return this;
    }

    public IFlourishNavigationPanelBuilder SetInitiallyOpen(bool enabled = true)
    {
        options.IsNavigationPanelInitiallyOpen = enabled;
        return this;
    }

    public IFlourishNavigationPanelBuilder SetTitle(string title)
    {
        options.PaneTitle = title;
        return this;
    }
}
