using System;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class PageCacheChangedEventArgs(
    PageCacheSnapshot current,
    CollectionChangeKind changeKind,
    Type? pageType
) : EventArgs
{
    public PageCacheSnapshot Current { get; } = current;
    public CollectionChangeKind ChangeKind { get; } = changeKind;
    public Type? PageType { get; } = pageType;
}
