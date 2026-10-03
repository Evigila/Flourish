using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Abstract;

public sealed record NavigationItem(string Label, string Href, string Icon = "page", bool Exact = false, bool Disabled = false);
public sealed record NavigationGroup(string Key, string Label, string Icon, IReadOnlyList<NavigationItem> Items, bool SecondaryNavigation = true);
public sealed record AppearanceState(string Primary, string Accent, string FontFamily, ApplicationTheme Theme);