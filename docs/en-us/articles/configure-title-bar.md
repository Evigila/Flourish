---
title: Title bar
description: Configure application identity, project selection, search, navigation, profile, and theme controls in the title bar.
---

# Title bar

Enable the title bar through [Shell configuration](shell-configuration.md), then use `ConfigureTitleBar` to provide application identity and select its controls. The visible title is itself a dropdown selector: it represents the application when project mode is disabled and the active project when project mode is enabled. The logo opens a separate information surface for the application identity.

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
| `SetApplicationTitle(title)` | Sets the application title and enables the title selector. |
| `SetApplicationSubTitle(subTitle)` | Sets supporting application text shown in the logo information surface. |
| `SetUnnamedProjectPlaceholder(placeholder)` | Sets the display text for an unpersisted or missing project selection; the default is `Unnamed project`. |
| `SetSearch(placeholder, handler)` | Displays search and invokes the handler when the text changes. |
| `SetBreadcrumbButton(option)` | Displays back and forward navigation according to the selected behavior. |
| `SetNavToggle()` | Displays the navigation panel toggle. |
| `SetProfile(nameOrder)` | Displays the profile trigger and selects the name order. |
| `SetThemeToggle(mode)` | Displays the theme control and selects its startup fallback mode. |

Built-in tooltips and theme labels follow the locale selected through [Application data](configure-data.md). Application and project names are application-provided text and are not translated automatically.

## Application title and project dropdown

The application identity remains stable while the active project can change during a session. The project-mode switch controls both the selected title and the choices exposed by the title selector.

| Project mode | Selected title | Dropdown choices |
| --- | --- | --- |
| `UseMultiProject(false)` | Application title | The application title only |
| `UseMultiProject(true)` with a persisted active project | Active project name | Every registered project and **New project** |
| `UseMultiProject(true)` with an unpersisted or missing active project | Unnamed-project placeholder | Every registered project and **New project** |

When project mode is disabled, the selector has no project-title semantics and selecting its only application-title entry performs no project operation. When project mode is enabled, selecting a project invokes `IProjectBehavior.ActivateProjectAsync`, selecting **New project** invokes `CreateProjectAsync`, and right-clicking a project exposes deletion through `DeleteProjectAsync`. [Projects](projects.md) explains lifecycle behavior, catalog persistence, and runtime updates.

The application subtitle is not displayed directly in the title bar. It belongs to the logo information surface together with the application title and, when requested by `SetLogo`, the current project title. `StoragePath == null`, rather than the placeholder text, identifies an unpersisted project.

The selected title uses the configured Large typography tier. Choices in its dropdown and built-in text in the logo information surface use Standard. See [Typography](configure-font.md).

## Logo information surface

`SetLogo()` uses the built-in Flourish icon. To replace it, pass a relative URI, absolute URI, or WPF pack URI. The effective image is also assigned to the shell window icon. The title-bar and information-surface presentations preserve the image aspect ratio, keep the complete artwork within their bounds, and leave transparent pixels unfilled.

```csharp
titleBar.SetLogo(
    "/Foobar;component/Assets/logo.ico",
    showApplicationTitle: true,
    showApplicationSubTitle: true,
    showProjectTitle: false);
```

The three display arguments default to `true`, `true`, and `false`. Clicking or pointing at the logo opens a temporary [Overlay](../controls/overlay.md). It closes after the pointer leaves both the logo and surface. Applications can add a WPF body below the identity metadata through the `TitlebarApplicationInfo` shell region:

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

The built-in title bar provides minimize, maximize or restore, and close commands. Maximize follows the configured resize mode, and close follows the [Window](configure-window.md) configuration. Logo, title selector, and window commands support keyboard focus; the logo surface also closes with <kbd>Esc</kbd> or an outside click.

## Related features

- [Projects](projects.md) manages the persistent project catalog and title-bar lifecycle behavior.
- [Custom shell content](configure-custom-handler.md) adds application content to title bar regions and the logo information surface.
- [Profile](configure-profile.md) configures profile content, authentication, and persistence.
- [Navigation](navigation.md) provides the panel controlled by `SetNavToggle`.
- [Themes](configure-themes.md) explains the theme controlled by `SetThemeToggle`.
- [Window](configure-window.md) configures resize behavior and close-to-tray handling.
