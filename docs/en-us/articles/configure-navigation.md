---
title: ConfigureNavigation
description: Configure navigation panel display and visible navigation items.
---

# ConfigureNavigation

`ConfigureNavigation` configures the navigation panel display and the visible navigation model. The panel is displayed only when [`ConfigureShell`](configure-shell.md) enables `UseNavigation()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseNavigation())
    .ConfigureNavigation(navigation =>
    {
        navigation
            .SetDirection(NavigationPanelDirection.Left)
            .SetInitiallyOpen()
            .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
            .SetGroup("Navigation", groupId: 0, group =>
            {
                group.AddNavigableViewItem<HomePage>(isInitial: true);
                group.AddNavigableItem("Refresh", "navigation.refresh", iconGlyph: "\uE72C");
            })
            .AddFixedNavigableViewItem<SettingsPage>();
    });
```

## Details

`SetDirection`, `SetInitiallyOpen`, `SetPanelWidth`, and `SetTitle` configure panel display. They replace the older separate navigation panel API.

`SetGroup` creates scrollable groups. `groupId` controls ordering and must be unique. Group `0` can omit a name; non-zero groups require one.

`AddNavigableViewItem<TPage>` places a page registered through [`ConfigureServices`](configure-services.md). `AddNavigableItem` creates a command item and sends its command key to `ICommandParser`.

Fixed items stay in the bottom section. They are useful for settings, about, profile, or persistent utility commands.

## Related APIs

- [`ConfigureServices`](configure-services.md) registers page types with `AddNavigable`.
- [`Command parser`](command-parser.md) handles navigation command items.
- [`Navigation`](navigation.md) covers groups, fixed items, and one-level trees in more depth.
