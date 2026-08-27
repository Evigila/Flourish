using System;

using ArkheideSystem.Flourish.Abstract;
namespace ArkheideSystem.Flourish.Navigation;

internal sealed record NavigablePageRegistration(
    string NavigationKey,
    Type PageType,
    string DisplayName,
    string IconGlyph,
    PageCacheMode CacheMode
);
