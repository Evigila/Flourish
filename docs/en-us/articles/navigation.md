---
title: Navigation
description: Register and navigate between Flourish pages.
---

# Navigation

Register WPF pages through [Dependency injection](configure-services.md), then use `ConfigureNavigation` to place pages and commands in the navigation panel.

## Register pages

`AddNavigable` registers a `Page` with its display name, icon, and cache mode. Add a view item to show it in the panel.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddNavigable<HomePage>(
        displayName: "Home",
        iconGlyph: "\uE80F",
        cacheMode: PageCacheMode.Enabled);

    services.AddNavigable<SettingsPage>(
        displayName: "Settings",
        iconGlyph: "\uE713",
        cacheMode: PageCacheMode.Enabled);
});
```

Pages must derive from `System.Windows.Controls.Page`. The key removes one trailing, case-sensitive `Page` suffix from the simple class name: `SettingsPage` becomes `Settings`, `ReportPagePage` becomes `ReportPage`, and `Page1` stays unchanged. Display names do not affect keys, and view items reuse the registered name and icon.

```csharp
services.AddNavigable<ReportsPage>("Reports", "\uE9D2");
services.AddNavigable<EditorPage>(
    "Editor",
    "\uE70F",
    cacheMode: PageCacheMode.Disabled);
```

Use `PageCacheMode.Enabled` for pages that should keep state while the user navigates away. Use `Disabled` for pages that should be recreated when revisited after navigating away.

Direction, initial open state, user-adjusted open width, and the last successfully navigated route are persisted by default. Flourish restores the last route only while it remains registered. Passing `usePersistedPreference: false` to a method keeps its configured fallback and stops updating that stored value without deleting it.

```csharp
navigation
    .SetDirection(NavigationPanelDirection.Left)
    .SetInitiallyOpen()
    .SetPanelWidth(260, 64, 480, 180)
    .SetLastNavigationPersistence();
```

## Configure groups

`AddGroup` creates a scrollable group; `AddNavigableViewItem<TPage>` places a registered page in it.

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation
        .SetEnabled()
        .SetDirection(NavigationPanelDirection.Left)
        .SetInitiallyOpen()
        .SetPanelWidth(openWidth: 260, closedWidth: 64, maxWidth: 480, minWidth: 180)
        .AddGroup("Navigation", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<ReportsPage>();
        });

    navigation.AddGroup("Tools", groupId: 1, group =>
    {
        group.AddNavigableViewItem<EditorPage>();
    });
});
```

Group rules:

- `groupId` controls display order. Lower IDs are displayed first.
- `groupId` must be unique. Reusing a group ID throws during build.
- Group 0 may omit `displayName`.
- Non-zero groups must provide `displayName`.

```csharp
nav.AddGroup(groupId: 0, configureGroup: group =>
{
    group.AddNavigableViewItem<HomePage>(isInitial: true);
});

nav.AddGroup("Admin", groupId: 10, group =>
{
    group.AddNavigableViewItem<SettingsPage>();
});
```

## Resize the panel

Use `SetPanelWidth` to configure the expanded width, collapsed width, and resize constraints for the navigation panel.

```csharp
nav.SetPanelWidth(openWidth: 260, closedWidth: 64, maxWidth: 480, minWidth: 180);
```

Widths default to `250` expanded and `64` collapsed, with a resize range of `180` to `520`. Set `closedWidth` to `0` to hide the collapsed panel; other values must be at least `64`.

## Add command items

`AddNavigableItem` adds a button-like navigation item. It does not navigate to a page. Instead, it dispatches `commandKey` through `ICommandDispatcher`.

```csharp
nav.AddGroup("Commands", groupId: 2, group =>
{
    group.AddNavigableItem("Refresh", "\uE72C", "cmd_reports_refresh");
    group.AddNavigableItem("Export", "\uE898", "cmd_reports_export");
});
```

Command items do not remain selected. After a command is invoked, the navigation panel restores the current page selection. [Command dispatch](commands.md) explains how to register and implement the handler.

## Add fixed items

Fixed items appear below the scrollable groups for persistent actions.

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.AddGroup("Navigation", groupId: 0, group =>
    {
        group.AddNavigableViewItem<HomePage>(isInitial: true);
        group.AddNavigableViewItem<ReportsPage>();
    });

    navigation.AddFixedNavigableViewItem<SettingsPage>();
    navigation.AddFixedNavigableItem("Help", "\uE946", "cmd_help_open");
});
```

Fixed view items still require the page to be registered with `AddNavigable`. Fixed command items use the same command dispatch path as grouped command items.

## Build one-level trees

Navigation items are flat by default. To create a one-level parent-child tree, set either `parentId` or `childId` on `AddNavigableViewItem` and `AddNavigableItem`.

```csharp
nav.AddGroup("Tree", groupId: 3, group =>
{
    group.AddNavigableViewItem<TreeParentPage>(parentId: 1);
    group.AddNavigableItem("Button1", "\uE8B7", "cmd_tree_button1", childId: 1);
    group.AddNavigableItem("Button2", "\uE8B7", "cmd_tree_button2", childId: 1);

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

A page item can be a parent. Clicking it navigates to the page and toggles its children. A command item can also be a parent, but parent command items toggle children only and do not execute their `commandKey`; pass `null` when no command key is needed.

When a page child is selected, Flourish expands and highlights its parent. Child items are hidden while the navigation panel is collapsed. Clicking a parent first expands the panel; a page parent then navigates to its page, while a command parent only toggles its children.

## Validation rules

Flourish validates navigation during build.

```csharp
nav.AddGroup("One", groupId: 1, group =>
{
    group.AddNavigableViewItem<HomePage>();
});

nav.AddGroup("Two", groupId: 2, group =>
{
    // This throws because HomePage is already displayed in group 1.
    group.AddNavigableViewItem<HomePage>();
});
```

Validation rejects duplicate generated keys or group IDs, unnamed non-zero groups, duplicate page positions, unregistered pages, duplicate scoped `parentId` values, and unmatched `childId` values. Same-named page classes in different namespaces still generate duplicate keys.

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
