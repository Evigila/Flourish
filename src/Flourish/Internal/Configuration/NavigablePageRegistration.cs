using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Internal.Configuration;

internal sealed record NavigablePageRegistration(
    string NavigationKey,
    Type PageType,
    string DisplayName,
    string IconGlyph,
    FlourishPageCacheMode CacheMode
);
