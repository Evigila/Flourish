---
title: Navigation
description: Register and navigate between Flourish pages.
---

# Navigation

Flourish separates page registration from the visible navigation model. Register WPF pages through [Dependency injection](configure-services.md), enable the navigation surface through [Shell configuration](shell-configuration.md), then use `ConfigureNavigation` to configure panel display and visible items.

## Register pages

`AddNavigable` registers a `Page` type in dependency injection and records the page metadata used by page navigation items: display name, icon glyph, and cache mode. It does not make the page visible in the navigation panel by itself.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddNavigable<HomePage>(
        displayName: "Home",
        iconGlyph: "\uE80F",
        cacheMode: FlourishPageCacheMode.Enabled);

    services.AddNavigable<SettingsPage>(
        displayName: "Settings",
        iconGlyph: "\uE713",
        cacheMode: FlourishPageCacheMode.Enabled);
});
```

Pages must derive from `System.Windows.Controls.Page`. Flourish generates the navigation key from the simple class name by removing one trailing, case-sensitive `Page` suffix: `SettingsPage` becomes `Settings`, `ReportPagePage` becomes `ReportPage`, and `Page1` remains `Page1`. Display names do not affect keys. The display name and icon set here are reused by `AddNavigableViewItem`, so view items do not ask for those values again.

```csharp
services.AddNavigable<ReportsPage>("Reports", "\uE9D2");
services.AddNavigable<EditorPage>(
    "Editor",
    "\uE70F",
    cacheMode: FlourishPageCacheMode.Disabled);
```

Use `FlourishPageCacheMode.Enabled` for pages that should keep state while the user navigates away. Use `Disabled` for pages that should be recreated when revisited after navigating away.

## Configure groups

Use `ConfigureNavigation` to build the visible navigation model. `SetGroup` creates a scrollable group, and `AddNavigableViewItem<TPage>` places a registered page in that group.

```csharp
builder.ConfigureShell(shell =>
{
    shell.UseNavigation();
})
.ConfigureNavigation(navigation =>
{
    navigation
        .SetDirection(NavigationPanelDirection.Left)
        .SetInitiallyOpen()
        .SetPanelWidth(openWidth: 260, closedWidth: 56, maxWidth: 480, minWidth: 180)
        .SetGroup("Navigation", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<ReportsPage>();
        });

    navigation.SetGroup("Tools", groupId: 1, group =>
    {
        group.AddNavigableViewItem<EditorPage>();
    });
});
```

Group rules:

- `groupId` controls display order. Lower IDs are displayed first.
- `groupId` must be unique. Reusing a group ID throws during build.
- Group 0 may omit `displayName`. When group 0 has no name, Flourish does not reserve heading space at the top of the panel.
- Non-zero groups must provide `displayName`.
- Groups have larger spacing between them than ordinary items.

```csharp
nav.SetGroup(groupId: 0, group =>
{
    group.AddNavigableViewItem<HomePage>(isInitial: true);
});

nav.SetGroup("Admin", groupId: 10, group =>
{
    group.AddNavigableViewItem<SettingsPage>();
});
```

If the navigation panel is enabled but no groups or fixed items are configured, Flourish displays a flat fallback list built from all registered pages. `SetTitle` sets the heading displayed above that automatically generated list.

```csharp
nav.SetTitle("Navigation");
```

## Resize the panel

Use `SetPanelWidth` to configure the expanded width, collapsed width, and resize constraints for the navigation panel.

```csharp
nav.SetPanelWidth(openWidth: 260, closedWidth: 56, maxWidth: 480, minWidth: 180);
```

The default widths are `220` expanded and `56` collapsed. Set `closedWidth` to `0` to hide the collapsed panel completely; otherwise it must be at least `56`. The visible collapsed width includes the standard command surface and its margin, compact scrollbar, divider, and the shared outer gutter. The default resize range is `160` to `420`. A hover-revealed splitter shows a width preview during the drag and applies the new width when the drag completes.

Navigation content uses the Shell's horizontal gutter on the side that faces the window edge. Flourish mirrors that gutter when the panel is moved from the left to the right, keeping navigation rows aligned with built-in title bar and status bar content.

## Add command items

`AddNavigableItem` adds a button-like navigation item. It does not navigate to a page. Instead, it sends `commandKey` to the registered `ICommandParser` implementations.

```csharp
nav.SetGroup("Commands", groupId: 2, group =>
{
    group.AddNavigableItem("Hello", "\uE8F2", "demo.hello");
    group.AddNavigableItem("World", "\uE774", "demo.world");
});
```

Command items do not remain selected. After a command is invoked, the navigation panel restores the current page selection. [Command parser](command-parser.md) explains how to register and implement the handler.

## Add fixed items

Fixed items are displayed in the bottom section of the navigation panel. They are not affected by the scrollable group area, which is useful for settings, about, profile, or other persistent actions.

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.SetGroup("Navigation", groupId: 0, group =>
    {
        group.AddNavigableViewItem<HomePage>(isInitial: true);
        group.AddNavigableViewItem<ReportsPage>();
    });

    navigation.AddFixedNavigableViewItem<SettingsPage>();
    navigation.AddFixedNavigableItem("About", "\uE946", "app.about");
});
```

Fixed view items still require the page to be registered with `AddNavigable`. Fixed command items use the same `ICommandParser` path as grouped command items.

## Build one-level trees

Navigation items are flat by default. To create a one-level parent-child tree, set either `parentId` or `childId` on `AddNavigableViewItem` and `AddNavigableItem`.

```csharp
nav.SetGroup("Tree", groupId: 3, group =>
{
    group.AddNavigableViewItem<TreeParentPage>(parentId: 1);
    group.AddNavigableItem("Button1", "\uE8B7", "tree.button1", childId: 1);
    group.AddNavigableItem("Button2", "\uE8B7", "tree.button2", childId: 1);

    group.AddNavigableItem("Pages", "\uE8A5", null, parentId: 2);
    group.AddNavigableViewItem<Page1>(childId: 2);
    group.AddNavigableViewItem<Page2>(childId: 2);
});
```

Tree rules:

- `parentId` and `childId` default to 0, which means the item does not participate in the tree.
- Exactly one of `parentId` and `childId` may be non-zero.
- `parentId` must be unique inside the same group or fixed-item scope.
- A child follows the parent whose `parentId` matches its `childId`.
- Navigation trees support one visible child level.

> [!CAUTION]
> Tree IDs are scoped to the current group or fixed-item section. Reusing a `parentId` in the same scope or pointing a `childId` at a missing parent fails during build.

A page item can be a parent. Clicking it navigates to the page and toggles its children. A command item can also be a parent, but parent command items toggle children only and do not execute their `commandKey`; passing `null` is recommended.

When a page child is selected, Flourish expands and highlights its parent. Child items are hidden while the navigation panel is collapsed. Clicking a parent first expands the panel; a page parent then navigates to its page, while a command parent only toggles its children.

## Validation rules

Flourish validates the navigation model during build so invalid configuration fails early.

```csharp
nav.SetGroup("One", groupId: 1, group =>
{
    group.AddNavigableViewItem<HomePage>();
});

nav.SetGroup("Two", groupId: 2, group =>
{
    // This throws because HomePage is already displayed in group 1.
    group.AddNavigableViewItem<HomePage>();
});
```

Common validation failures include duplicate generated navigation keys, duplicate group IDs, non-zero groups without names, duplicate page display positions, unregistered view item pages, duplicate `parentId` values in the same scope, and child IDs that do not match any parent. Two pages with the same simple class name generate the same key even when their namespaces differ; `Build()` rejects them and reports the key and both full type names.

## Navigate from code

For runtime navigation, request `INavigationService` from dependency injection and pass the generated, case-sensitive string key. View models therefore do not reference WPF `Page` types.

```csharp
public sealed class HomeViewModel(INavigationService navigation)
{
    public void OpenSettings()
    {
        navigation.Navigate("Settings");
    }
}
```

If a key is unknown, `Navigate` throws an `InvalidOperationException` containing the supplied key, the generation rule, and a prompt to check spelling and casing. Renaming a Page class also changes its generated key, so update string navigation calls in the same change.
