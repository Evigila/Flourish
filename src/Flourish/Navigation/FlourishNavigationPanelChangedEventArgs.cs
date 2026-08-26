using System;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class FlourishNavigationPanelChangedEventArgs(
    FlourishNavigationPanelState previous,
    FlourishNavigationPanelState current,
    bool animate
) : EventArgs
{
    public FlourishNavigationPanelState Previous { get; } = previous;
    public FlourishNavigationPanelState Current { get; } = current;
    public bool Animate { get; } = animate;
}
