---
title: Title bar
description: Configure application identity, project selection, search, navigation, profile, and theme controls in the title bar.
---

# Title bar

Enable the title bar through [Shell configuration](shell-configuration.md), then use `ConfigureTitleBar` to provide application identity and select its controls. The visible title represents either the application or the active project; the logo opens a separate information surface for the application identity.

## Configure identity and controls

```csharp
builder
    .ConfigureShell(shell =>
        shell.UseTitleBar().UseMultiProject().UseNavigation())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .SetLogo(
                showApplicationTitle: true,
                showApplicationSubTitle: true,
                showProjectTitle: true)
            .SetApplicationTitle("Foobar")
            .SetApplicationSubTitle("Desktop workspace")
            .SetUnnamedProjectPlaceholder("Unnamed project")
            .SetSearch("Search", (_, searchText) => UpdateSearch(searchText))
            .SetBreadcrumbButton(BreadcrumbShowOption.Auto)
            .SetNavToggle()
            .SetProfile(NameOrder.FirstLast)
            .SetThemeToggle(FlourishTheme.System);
    });
```

`UseTitleBar()` is required. `SetNavToggle` is displayed only when [Navigation](navigation.md) is also enabled. `UseMultiProject` is optional and defaults to `false`.

| Method | Result |
| --- | --- |
| `SetLogo(...)` | Displays the logo button and selects which identity fields appear in its information surface. |
| `SetApplicationTitle(title)` | Sets the application title and enables the title button. |
| `SetApplicationSubTitle(subTitle)` | Sets supporting application text shown in the logo information surface. |
| `SetUnnamedProjectPlaceholder(placeholder)` | Sets the title used when project mode has no active project; the default is `Unnamed project`. |
| `SetSearch(placeholder, handler)` | Displays search and invokes the handler when the text changes. |
| `SetBreadcrumbButton(option)` | Displays back and forward navigation according to the selected behavior. |
| `SetNavToggle()` | Displays the navigation panel toggle. |
| `SetProfile(nameOrder)` | Displays the profile trigger and selects the name order. |
| `SetThemeToggle(mode)` | Displays the theme control and selects its startup fallback mode. |

Built-in tooltips and theme labels follow the locale selected through [Application data](configure-data.md). Application and project names are application-provided text and are not translated automatically.

## Application and project titles

The application identity remains stable while the active project can change during a session.

| Project mode | Title button | Title menu |
| --- | --- | --- |
| `UseMultiProject(false)` | Application title | Application title only |
| `UseMultiProject(true)` with an active project | Active project name | Registered projects and **New project** |
| `UseMultiProject(true)` without an active project | Unnamed-project placeholder | Placeholder and **New project** |

The application subtitle is not displayed directly in the title bar. It belongs to the logo information surface together with the application title and, when requested by `SetLogo`, the current project title. [Projects](projects.md) explains project metadata, selection requests, and runtime updates.

## Logo information surface

`SetLogo()` uses the built-in Flourish icon. To replace it, pass a relative URI, absolute URI, or WPF pack URI. The effective image is also assigned to the shell window icon.

```csharp
titleBar.SetLogo(
    "/Foobar;component/Assets/logo.ico",
    showApplicationTitle: true,
    showApplicationSubTitle: true,
    showProjectTitle: false);
```

The three display arguments default to `true`, `true`, and `false`. Clicking or pointing at the logo opens the information surface. Applications can add a WPF body below the identity metadata through the `TitlebarApplicationInfo` shell region:

```csharp
builder.ConfigureCustomHandler(custom =>
    custom.Add(
        FlourishRegion.TitlebarApplicationInfo,
        services => new ApplicationSummaryView()));
```

The body is application-owned. Flourish only hosts it and does not define its data or behavior; content that exceeds the window-bounded surface scrolls vertically.

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

The built-in title bar provides minimize, maximize or restore, and close commands. Maximize follows the configured resize mode, and close follows the [Window](configure-window.md) configuration. Logo, title-menu, and window commands support keyboard focus, and their flyouts close with Esc or an outside click.

## Related features

- [Projects](projects.md) manages the project identities displayed by the title bar.
- [Custom shell content](configure-custom-handler.md) adds application content to title bar regions and the logo information surface.
- [Profile](configure-profile.md) configures profile content, authentication, and persistence.
- [Navigation](navigation.md) provides the panel controlled by `SetNavToggle`.
- [Themes](configure-themes.md) explains the theme controlled by `SetThemeToggle`.
- [Window](configure-window.md) configures resize behavior and close-to-tray handling.
