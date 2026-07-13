---
title: Title bar
description: Configure the built-in title bar content, search, navigation, profile, and theme controls.
---

# Title bar

The Flourish title bar can display application identity, search, breadcrumb navigation, a navigation toggle, profile access, and theme controls. Enable the surface through [Shell configuration](shell-configuration.md), then use `ConfigureTitleBar` to configure the elements that should be visible. An element remains hidden when its `Set...` method is not called.

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
    });
```

`UseTitleBar()` is the surface prerequisite. `SetProfile` and `SetThemeToggle` activate the services needed by their controls. `SetNavToggle` is displayed only when [Navigation](navigation.md) is also enabled.

## Built-in content

Each configuration method also displays its corresponding element:

| Method | Result |
| --- | --- |
| `SetLogo()` or `SetLogo(path)` | Displays the built-in logo or an application-provided logo. |
| `SetTitle(title)` | Displays the primary title. |
| `SetSubTitle(subTitle)` | Displays supporting title text. |
| `SetSearch(placeholder, handler)` | Displays search and handles text changes; the handler receives the application service provider and search text. |
| `SetBreadcrumbButton(option)` | Displays breadcrumb navigation with the selected behavior. |
| `SetNavToggle()` | Displays the navigation panel toggle. |
| `SetProfile(nameOrder)` | Displays the profile trigger with the built-in profile behavior. |
| `SetThemeToggle(mode)` | Displays the theme toggle and selects its initial mode. |

These methods configure built-in title bar regions. [Custom shell content](configure-custom-handler.md) inserts application-provided WPF elements into the extension regions.

Built-in tooltips and theme labels follow the locale selected through [Application data](configure-data.md). Values supplied to `SetTitle`, `SetSubTitle`, and `SetSearch` are application text and are not translated automatically.

## Window commands

When the built-in title bar is enabled, its caption area provides minimize, maximize or restore, and close commands. Minimize and maximize retain the standard, non-destructive blue hover reveal and pressed feedback. Close is a destructive action, so it uses red hover and pressed feedback in both light and dark themes.

Pointer activation does not leave a focus outline on these buttons, while keyboard navigation still provides a visible focus indicator. The maximize command follows the configured resize mode, and close behavior follows the [Window](configure-window.md) configuration.

## Logo and window icon

`SetLogo()` uses the built-in Flourish application icon. To replace it, pass a relative URI, absolute URI, or WPF pack URI. Flourish removes fully transparent edge pixels before fitting the image into the title bar logo region, allowing the visible artwork to use the available space. The same effective image is assigned to the shell window icon, so the taskbar icon and title bar logo remain synchronized.

```csharp
titleBar.SetLogo("/Foobar;component/Assets/logo.ico");
```

## Breadcrumb navigation

`SetBreadcrumbButton` accepts a `BreadcrumbShowOption`. `Always` keeps breadcrumb navigation visible, `Auto` follows navigation state, and `Hidden` suppresses it. Omitting the argument uses `Auto`.

## Search

`SetSearch` receives a placeholder and a handler for search text changes. Use the service-provider overload when the handler needs application services.

```csharp
builder.ConfigureTitleBar(titleBar =>
{
    titleBar.SetSearch("Search", (services, searchText) =>
    {
        services.GetRequiredService<SearchCoordinator>().Update(searchText);
    });
});
```

## Profile and theme controls

`SetProfile` displays the trigger, enables the built-in profile behavior, and selects the order used for names and initials. [Profile](configure-profile.md) explains login behavior and custom profile pages.

`SetThemeToggle` displays the toggle, enables theme handling, and selects the theme used when Host configuration does not contain a saved preference. [Themes](configure-themes.md) explains system following and `appsettings.json` persistence.

## Related features

- [Custom shell content](configure-custom-handler.md) adds title bar actions and custom regions.
- [Profile](configure-profile.md) configures profile content, authentication, and persistence.
- [Navigation](navigation.md) provides the panel controlled by `SetNavToggle`.
- [Themes](configure-themes.md) explains the theme controlled by `SetThemeToggle`.
- [Window](configure-window.md) configures resize behavior and close-to-tray handling.
