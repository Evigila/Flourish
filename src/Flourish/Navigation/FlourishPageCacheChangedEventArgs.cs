using System;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class FlourishPageCacheChangedEventArgs(
    FlourishPageCacheSnapshot current,
    FlourishRuntimeChangeKind changeKind,
    Type? pageType
) : EventArgs
{
    public FlourishPageCacheSnapshot Current { get; } = current;
    public FlourishRuntimeChangeKind ChangeKind { get; } = changeKind;
    public Type? PageType { get; } = pageType;
}
