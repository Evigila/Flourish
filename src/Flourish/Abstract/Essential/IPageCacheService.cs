namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>Provides runtime control over cached Flourish page instances.</summary>
public interface IPageCacheService
{
    /// <summary>Occurs after a cache mode or cached instance changes.</summary>
    event EventHandler<FlourishPageCacheChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of cache configuration and contents.</summary>
    FlourishPageCacheSnapshot Current { get; }

    /// <summary>Changes a page type's cache mode and evicts it when caching is disabled.</summary>
    void SetCacheMode(Type pageType, FlourishPageCacheMode cacheMode);

    /// <summary>Removes the cached instance for a page type.</summary>
    bool Evict(Type pageType);

    /// <summary>Removes all cached page instances.</summary>
    void Clear();

    /// <summary>Gets whether an instance of the page type is currently cached.</summary>
    bool Contains(Type pageType);
}

/// <summary>Represents current page cache configuration and cached types.</summary>
public sealed record FlourishPageCacheSnapshot(
    IReadOnlyDictionary<Type, FlourishPageCacheMode> CacheModes,
    IReadOnlyCollection<Type> CachedPageTypes,
    long Version
);

/// <summary>Provides data for <see cref="IPageCacheService.Changed" />.</summary>
public sealed class FlourishPageCacheChangedEventArgs(
    FlourishPageCacheSnapshot current,
    FlourishRuntimeChangeKind changeKind,
    Type? pageType
) : EventArgs
{
    /// <summary>Gets the current cache snapshot.</summary>
    public FlourishPageCacheSnapshot Current { get; } = current;

    /// <summary>Gets the mutation kind.</summary>
    public FlourishRuntimeChangeKind ChangeKind { get; } = changeKind;

    /// <summary>Gets the affected page type, or <see langword="null" /> for a full clear.</summary>
    public Type? PageType { get; } = pageType;
}
