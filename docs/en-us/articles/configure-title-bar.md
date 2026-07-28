---
title: Title bar
description: Configure application identity, project selection, search, navigation, profile, and theme controls in the title bar.
---

# Title bar

Enable the title bar through [Shell configuration](shell-configuration.md), then use `ConfigTitleBar` to provide application identity and select its controls. The visible title is itself a dropdown selector: it represents the application when project mode is disabled and the active project when project mode is enabled. The logo opens a separate information surface for the application identity.

## Configure identity and controls

```csharp
builder
    .ConfigShell(shell =>
        shell.UseTitleBar().UseMultiProject().UseNavigation())
    .ConfigTitleBar(titleBar =>
    {
        titleBar
            .UseLogo(
                showApplicationTitle: true,
                showApplicationSubTitle: true,
                showProjectTitle: true)
            .InitApplicationTitle("Foobar")
            .InitApplicationSubTitle("Desktop workspace")
            .InitUnnamedProjectPlaceholder("Unnamed project")
            .UseSearch(placeholder: "Search", handler: (_, searchText) => UpdateSearch(searchText))
            .UseBreadcrumb(option: BreadcrumbShowOption.Auto)
            .UseNavigationToggle()
            .UseProfile(nameOrder: NameOrder.FirstLast)
            .UseThemeToggle(mode: FlourishTheme.System);
    });
```

`UseTitleBar()` is required. `UseNavigationToggle` is displayed only when [Navigation](navigation.md) is also enabled. `UseMultiProject` is optional and defaults to `false`.

| Method | Result |
| --- | --- |
| `UseLogo(...)` | Displays the logo button and selects which identity fields appear in its information surface. |
| `InitApplicationTitle(title)` | Sets the application title and enables the title selector. |
| `InitApplicationSubTitle(subTitle)` | Sets supporting application text shown in the logo information surface. |
| `InitUnnamedProjectPlaceholder(placeholder)` | Sets the display text for an unpersisted or missing project selection; the default is `Unnamed project`. |
| `UseSearch(placeholder, handler)` | Displays search and invokes the handler when the text changes. |
| `UseBreadcrumb(option)` | Displays back and forward navigation according to the selected behavior. |
| `UseNavigationToggle()` | Displays the navigation panel toggle. |
| `UseProfile(nameOrder)` | Displays the profile trigger and selects the name order. |
| `UseThemeToggle(mode)` | Displays the theme control, selects its startup fallback mode, and persists user selections by default. |

Built-in tooltips and theme labels follow the locale selected through [Application data](configure-data.md). Application and project names are application-provided text and are not translated automatically.

## Application title and project dropdown

The application identity remains stable while the active project can change during a session. The project-mode switch controls both the selected title and the choices exposed by the title selector.

| Project mode | Selected title | Dropdown choices |
| --- | --- | --- |
| `UseMultiProject(false)` | Application title | The application title only |
| `UseMultiProject(true)` with a persisted active project | Active project name | Every registered project and **New project** |
| `UseMultiProject(true)` with an unpersisted or missing active project | Unnamed-project placeholder | Every registered project and **New project** |

When project mode is disabled, the selector has no project-title semantics and selecting its only application-title entry performs no project operation. When project mode is enabled, selecting a project invokes `IProjectBehavior.ActivateProjectAsync`, selecting **New project** invokes `CreateProjectAsync`, and right-clicking a project exposes deletion through `DeleteProjectAsync`. [Projects](projects.md) explains lifecycle behavior, catalog persistence, and runtime updates.

The application subtitle is not displayed directly in the title bar. It belongs to the logo information surface together with the application title and, when requested by `UseLogo`, the current project title. `StoragePath == null`, rather than the placeholder text, identifies an unpersisted project.

The selected title uses the configured Large typography tier. Choices in its dropdown and built-in text in the logo information surface use Standard. See [Typography](configure-font.md).

## Logo information surface

`UseLogo()` uses the built-in Flourish icon. To replace it, pass a relative URI, absolute URI, or WPF pack URI. The effective image is also assigned to the shell window icon. The title-bar and information-surface presentations preserve the image aspect ratio, keep the complete artwork within their bounds, and leave transparent pixels unfilled.

```csharp
titleBar.UseLogo(
    "/Foobar;component/Assets/logo.ico",
    showApplicationTitle: true,
    showApplicationSubTitle: true,
    showProjectTitle: false);
```

The three display arguments default to `true`, `true`, and `false`. Clicking or pointing at the logo opens a temporary [Overlay](../controls/overlay.md). It closes after the pointer leaves both the logo and surface. Applications can add a WPF body below the identity metadata through the `TitlebarApplicationInfo` shell region:

```csharp
builder.ConfigCustomHandler(custom =>
    custom.InitRegionContent(
        FlourishRegion.TitlebarApplicationInfo,
        services => new ApplicationSummaryView()));
```

The body is application-owned. Flourish only hosts it and does not define its data or behavior; content that exceeds the window-bounded surface scrolls vertically.

## Search

`UseSearch` receives a placeholder and a handler for text changes. The handler receives the application `IServiceProvider` and current search text.

```csharp
builder.ConfigTitleBar(titleBar =>
{
    titleBar.UseSearch(placeholder: "Search", handler: (services, searchText) =>
    {
        services.GetRequiredService<SearchCoordinator>().Update(searchText);
    });
});
```

## Back and forward navigation

`UseBreadcrumb` accepts a `BreadcrumbShowOption`:

| Value | Behavior |
| --- | --- |
| `Always` | Displays the controls while the title bar is visible. |
| `Auto` | Displays the controls when the navigation service can go back or forward. |
| `Hidden` | Hides the controls. |

Omitting the argument uses `Auto`.

## Profile and theme controls

`UseProfile` displays the profile trigger and selects the order used for names and initials. [Profile](configure-profile.md) explains login behavior and custom profile pages.

`UseThemeToggle` displays the theme toggle and selects the theme used when Host configuration does not contain a saved preference. [Themes](configure-themes.md) explains system following and preference persistence.

`UseThemeToggle` and `UseProfile` persist the theme and profile name order by default. Passing `usePersistedPreference: false` makes the supplied startup value authoritative without removing an older stored value.

## Window commands

The built-in title bar provides minimize, maximize or restore, and close commands. Maximize follows the configured resize mode, and close follows the [Window](configure-window.md) configuration. Logo, title selector, and window commands support keyboard focus; the logo surface also closes with <kbd>Esc</kbd> or an outside click.

## Related features

- [Projects](projects.md) manages the persistent project catalog and title-bar lifecycle behavior.
- [Custom shell content](configure-custom-handler.md) adds application content to title bar regions and the logo information surface.
- [Profile](configure-profile.md) configures profile content, authentication, and persistence.
- [Navigation](navigation.md) provides the panel controlled by `UseNavigationToggle`.
- [Themes](configure-themes.md) explains the theme controlled by `UseThemeToggle`.
- [Window](configure-window.md) configures resize behavior and close-to-tray handling.
