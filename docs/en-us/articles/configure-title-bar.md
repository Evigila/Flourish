---
title: Title bar
description: Configure title bar identity, search, navigation, profile, and theme controls.
---

# Title bar

Enable the title bar through [Shell configuration](shell-configuration.md), then use `ConfigureTitleBar` to select its content. An element remains hidden until its `Set...` method is called.

## Configure the title bar

```csharp
builder
    .ConfigureShell(shell =>
        shell.UseTitleBar().UseNavigation())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .SetLogo()
            .SetTitle("Foobar")
            .SetSubTitle("Desktop workspace")
            .SetSearch("Search", (_, searchText) => UpdateSearch(searchText))
            .SetBreadcrumbButton(BreadcrumbShowOption.Auto)
            .SetNavToggle()
            .SetProfile(NameOrder.FirstLast)
            .SetThemeToggle(FlourishTheme.System);
    })
    .ConfigureNavigation(navigation =>
        navigation.SetGroup(null, groupId: 0, group =>
            group.AddNavigableViewItem<HomePage>(isInitial: true)));
```

`UseTitleBar()` is required. `SetNavToggle` is displayed only when [Navigation](navigation.md) is also enabled.

| Method | Result |
| --- | --- |
| `SetLogo()` or `SetLogo(path)` | Displays the built-in logo or an application-provided logo. |
| `SetTitle(title)` | Displays the primary title. |
| `SetSubTitle(subTitle)` | Displays supporting title text. |
| `SetSearch(placeholder, handler)` | Displays search and invokes the handler when the text changes. |
| `SetBreadcrumbButton(option)` | Displays back and forward navigation according to the selected behavior. |
| `SetNavToggle()` | Displays the navigation panel toggle. |
| `SetProfile(nameOrder)` | Displays the profile trigger and selects the name order. |
| `SetThemeToggle(mode)` | Displays the theme control, enables theme selection, and sets the fallback mode used when no saved preference exists. |

Built-in tooltips and theme labels follow the locale selected through [Application data](configure-data.md). Values supplied to `SetTitle`, `SetSubTitle`, and `SetSearch` are application text and are not translated automatically.

## Logo and window icon

`SetLogo()` uses the built-in Flourish icon. To replace it, pass a relative URI, absolute URI, or WPF pack URI. The effective image is also assigned to the shell window icon.

```csharp
titleBar.SetLogo("/Foobar;component/Assets/logo.ico");
```

## Search

`SetSearch` receives a placeholder and a handler for text changes. The handler receives the application `IServiceProvider` and current search text.

```csharp
builder.ConfigureTitleBar(titleBar =>
{
    titleBar.SetSearch("Search", (services, searchText) =>
    {
        services.GetRequiredService<SearchCoordinator>().Update(searchText);
    });
});
```

## Back and forward navigation

`SetBreadcrumbButton` accepts a `BreadcrumbShowOption`:

| Value | Behavior |
| --- | --- |
| `Always` | Displays the controls while the title bar is visible. |
| `Auto` | Displays the controls when the navigation service can go back or forward. |
| `Hidden` | Hides the controls. |

Omitting the argument uses `Auto`.

## Profile and theme controls

`SetProfile` displays the profile trigger and selects the order used for names and initials. [Profile](configure-profile.md) explains login behavior and custom profile pages.

`SetThemeToggle` displays the theme toggle and selects the theme used when Host configuration does not contain a saved preference. [Themes](configure-themes.md) explains system following and preference persistence.

## Window commands

The built-in title bar provides minimize, maximize or restore, and close commands. Maximize follows the configured resize mode, and close follows the [Window](configure-window.md) configuration. Keyboard navigation provides a visible focus indicator for these commands.

## Related features

- [Custom shell content](configure-custom-handler.md) adds application content to title bar regions.
- [Profile](configure-profile.md) configures profile content, authentication, and persistence.
- [Navigation](navigation.md) provides the panel controlled by `SetNavToggle`.
- [Themes](configure-themes.md) explains the theme controlled by `SetThemeToggle`.
- [Window](configure-window.md) configures resize behavior and close-to-tray handling.
