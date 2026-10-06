using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Abstract;

public enum NavigationEntryKind
{
    Route,
    Command,
}

public sealed record NavigationItem(string Label, string Href, string Icon = "description", bool Exact = false, bool Disabled = false,
    IReadOnlyList<NavigationItem>? Children = null)
{
    public IReadOnlyList<NavigationItem> ChildItems => Children ?? Array.Empty<NavigationItem>();
}
public sealed record NavigationGroup(string Key, string Label, string Icon, IReadOnlyList<NavigationItem> Items, bool SecondaryNavigation = true);

public sealed record NavigationEntry(
    string Label,
    string Icon,
    NavigationEntryKind Kind,
    string? Href = null,
    string? CommandKey = null,
    IReadOnlyList<NavigationItem>? Children = null,
    bool Exact = false,
    bool Disabled = false)
{
    public IReadOnlyList<NavigationItem> SecondaryItems { get; } = Children ?? Array.Empty<NavigationItem>();
}

public sealed record TopBarMenuItem(string Label, string CommandKey, bool Disabled = false, bool Destructive = false);
public sealed record TopBarMenu(string Label, IReadOnlyList<TopBarMenuItem> Items);
public sealed record ComponentPlacement(Type ComponentType);
public sealed record AppearanceState(string Primary, string Accent, string FontFamily, ApplicationTheme Theme);
