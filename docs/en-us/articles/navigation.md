---
title: Navigation
description: Register and navigate between Flourish pages.
---

# Navigation

Flourish separates page registration from the visible navigation model. Register WPF pages with `AddNavigable` during service configuration, enable the navigation surface with `UseNavigation`, then configure panel display and visible items with `ConfigureNavigation`.

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

Pages must derive from `System.Windows.Controls.Page`. The display name and icon set here are reused by `AddNavigableViewItem`, so view items do not ask for those values again.

```csharp
services.AddNavigable<GalleryPage>("Gallery", "\uE91B");
services.AddNavigable<EditorPage>("Editor", "\uE70F", cacheMode: FlourishPageCacheMode.Disabled);
```

Use `FlourishPageCacheMode.Enabled` for pages that should keep state while the user navigates away. Use `Disabled` for pages that should be recreated on each navigation request.

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
        .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
        .SetGroup("Navigation", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<GalleryPage>();
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

If you enable the navigation panel display but do not configure any groups or fixed items, Flourish falls back to a flat legacy list built from all registered pages.

## Resize the panel

Use `SetPanelWidth` to configure the expanded width, collapsed width, and resize constraints for the navigation panel.

```csharp
nav.SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180);
```

The default widths are `220` expanded and `48` collapsed. The default resize range is `160` to `420`. Users can resize the expanded panel with a hover-revealed splitter; it uses preview mode, so Flourish updates the layout only after the drag completes.

## Add command items

`AddNavigableItem` adds a button-like navigation item. It does not navigate to a page. Instead, it sends `commandKey` to the registered `ICommandParser` implementations.

```csharp
nav.SetGroup("Commands", groupId: 2, group =>
{
    group.AddNavigableItem("Hello", "demo.hello", iconGlyph: "\uE8F2");
    group.AddNavigableItem("World", "demo.world", iconGlyph: "\uE774");
});
```

Command items keep only the hover style. After a command is invoked, the current page selection is restored and focus is cleared from the command item.

```csharp
internal sealed class AppCommandParser(IMessageService messages) : ICommandParser
{
    public bool TryParse(string commandKey)
    {
        return commandKey switch
        {
            "demo.hello" => Show("Hello"),
            "demo.world" => Show("World"),
            _ => false
        };
    }

    private bool Show(string text)
    {
        messages.Show(text, "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        return true;
    }
}
```

## Add fixed items

Fixed items are displayed in the bottom section of the navigation panel. They are not affected by the scrollable group area, which is useful for settings, about, profile, or other persistent actions.

```csharp
builder.ConfigureNavigation(navigation =>
{
    navigation.SetGroup("Navigation", groupId: 0, group =>
    {
        group.AddNavigableViewItem<HomePage>(isInitial: true);
        group.AddNavigableViewItem<GalleryPage>();
    });

    navigation.AddFixedNavigableViewItem<SettingsPage>();
    navigation.AddFixedNavigableItem("About", "app.about", iconGlyph: "\uE946");
});
```

Fixed view items still require the page to be registered with `AddNavigable`. Fixed command items use the same `ICommandParser` path as grouped command items.

## Build one-level trees

Navigation items are flat by default. To create a one-level parent-child tree, set either `parentId` or `childId` on `AddNavigableViewItem` and `AddNavigableItem`.

```csharp
nav.SetGroup("Tree", groupId: 3, group =>
{
    group.AddNavigableViewItem<TreeParentPage>(parentId: 1);
    group.AddNavigableItem("Button1", "tree.button1", childId: 1, iconGlyph: "\uE8B7");
    group.AddNavigableItem("Button2", "tree.button2", childId: 1, iconGlyph: "\uE8B7");

    group.AddNavigableItem("Pages", null, parentId: 2, iconGlyph: "\uE8A5");
    group.AddNavigableViewItem<Page1>(childId: 2);
    group.AddNavigableViewItem<Page2>(childId: 2);
});
```

Tree rules:

- `parentId` and `childId` default to 0, which means the item does not participate in the tree.
- Exactly one of `parentId` and `childId` may be non-zero.
- `parentId` must be unique inside the same group or fixed-item scope.
- A child follows the parent whose `parentId` matches its `childId`.
- Flourish currently supports one visible child level.

> [!CAUTION]
> Tree IDs are scoped to the current group or fixed-item section. Reusing a `parentId` in the same scope or pointing a `childId` at a missing parent fails during build.

A page item can be a parent. Clicking it navigates to the page and toggles its children. A command item can also be a parent, but parent command items toggle children only and do not execute their `commandKey`; passing `null` is recommended.

When a page child is selected, Flourish automatically expands its parent and highlights the parent name. When the navigation panel is collapsed, closing first hides all child items so icons stay aligned. Clicking a collapsed parent opens the whole navigation panel; page parents are selected and navigated immediately, while command parents still only reveal or toggle children.

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

Common validation failures include duplicate group IDs, non-zero groups without names, duplicate page display positions, unregistered view item pages, duplicate `parentId` values in the same scope, and child IDs that do not match any parent.

## Navigate from code

For runtime navigation, request `INavigationService` from dependency injection and navigate by page type:

```csharp
public sealed class HomeViewModel(INavigationService navigation)
{
    public void OpenSettings()
    {
        navigation.Navigate<SettingsPage>();
    }
}
```
