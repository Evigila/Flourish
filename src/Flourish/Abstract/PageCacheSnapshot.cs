using System;
using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Represents current page cache configuration and cached types.</summary>
public sealed record PageCacheSnapshot(
    IReadOnlyDictionary<Type, PageCacheMode> CacheModes,
    IReadOnlyCollection<Type> CachedPageTypes,
    long Version
);
