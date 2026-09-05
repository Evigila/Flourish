using System;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationPanelChangedEventArgs(
    NavigationPanelState previous,
    NavigationPanelState current,
    bool animate
) : EventArgs
{
    public NavigationPanelState Previous { get; } = previous;
    public NavigationPanelState Current { get; } = current;
    public bool Animate { get; } = animate;
}
