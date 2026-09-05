using System;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationRoutesChangedEventArgs(
    NavigationRouteSnapshot current,
    CollectionChangeKind changeKind,
    NavigationRoute? previousRoute,
    NavigationRoute? route
) : EventArgs
{
    public NavigationRouteSnapshot Current { get; } = current;
    public CollectionChangeKind ChangeKind { get; } = changeKind;
    public NavigationRoute? PreviousRoute { get; } = previousRoute;
    public NavigationRoute? Route { get; } = route;
}
