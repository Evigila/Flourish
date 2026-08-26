using System;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class FlourishNavigationRoutesChangedEventArgs(
    FlourishNavigationRouteSnapshot current,
    FlourishRuntimeChangeKind changeKind,
    FlourishNavigationRoute? previousRoute,
    FlourishNavigationRoute? route
) : EventArgs
{
    public FlourishNavigationRouteSnapshot Current { get; } = current;
    public FlourishRuntimeChangeKind ChangeKind { get; } = changeKind;
    public FlourishNavigationRoute? PreviousRoute { get; } = previousRoute;
    public FlourishNavigationRoute? Route { get; } = route;
}
