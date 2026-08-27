---
title: Title bar
description: Configure application identity, project selection, search, navigation, profile, and theme controls in the title bar.
---

# Title bar

Use `ConfigureTitleBar` to enable application identity and title-bar controls. The title selects the application or active project, and the logo opens application information.

## Configure identity and controls

```csharp
builder
    .ConfigureProjects(projects => projects.SetMultiProjectEnabled())
    .ConfigureNavigation(navigation => navigation.SetEnabled())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .SetEnabled()
            .SetLogo(
                showApplicationTitle: true,
                showApplicationSubtitle: true,
                showProjectTitle: true)
            .SetApplicationTitle("Foobar")
            .SetApplicationSubtitle("Desktop workspace")
            .SetUnnamedProjectPlaceholder("Unnamed project")
            .SetSearch(placeholder: "Search", handler: (_, searchText) => UpdateSearch(searchText))
            .SetBreadcrumbMode(option: BreadcrumbShowOption.Auto)
            .SetNavigationToggle()
            .SetProfile(nameOrder: NameOrder.FirstLast)
            .SetThemeToggle(mode: ApplicationTheme.System);
    });
```

`SetEnabled()` is required on `ITitleBarBuilder`. `SetNavigationToggle` is displayed only when [Navigation](navigation.md) is also enabled. Project mode is optional and defaults to disabled; enable it through `ConfigureProjects(projects => projects.SetMultiProjectEnabled())`.

| Method | Result |
| --- | --- |
| `SetLogo(...)` | Displays the logo button and selects which identity fields appear in its information surface. |
| `SetApplicationTitle(title)` | Sets the application title and enables the title selector. |
| `SetApplicationSubtitle(subtitle)` | Sets supporting application text shown in the logo information surface. |
| `SetUnnamedProjectPlaceholder(placeholder)` | Sets the display text for an unpersisted or missing project selection; the default is `Unnamed project`. |
| `SetSearch(placeholder, handler)` | Displays search and invokes the handler when the text changes. |
| `SetBreadcrumbMode(option)` | Displays back and forward navigation according to the selected behavior. |
| `SetNavigationToggle()` | Displays the navigation panel toggle. |
| `SetProfile(nameOrder)` | Displays the profile trigger and selects the name order. |
| `SetThemeToggle(mode)` | Displays the theme control, selects its startup fallback mode, and persists user selections by default. |

Built-in tooltips and theme labels follow the locale selected through [Application data](configure-data.md). Application and project names are application-provided text and are not translated automatically.

## Application title and project dropdown

Project mode controls the selected title and its choices.

| Project mode | Selected title | Dropdown choices |
| --- | --- | --- |
| Disabled through `ConfigureProjects` | Application title | The application title only |
| Enabled with a persisted active project | Active project name | Every registered project and **New project** |
| Enabled with an unpersisted or missing active project | Unnamed-project placeholder | Every registered project and **New project** |

When project mode is disabled, the selector has no project-title semantics and selecting its only application-title entry performs no project operation. When project mode is enabled, selecting a project invokes `IProjectBehavior.ActivateProjectAsync`, selecting **New project** invokes `CreateProjectAsync`, and right-clicking a project exposes deletion through `DeleteProjectAsync`. [Projects](projects.md) explains lifecycle behavior, catalog persistence, and runtime updates.

The subtitle appears only in logo information with the application title and optional project title. `StoragePath == null`, not placeholder text, identifies an unpersisted project.

The selected title uses the configured Large typography tier. Choices in its dropdown and built-in text in the logo information surface use Standard. See [Typography](configure-font.md).

## Logo information surface

`SetLogo()` uses the built-in icon. A relative, absolute, or WPF pack URI replaces it and also sets the Shell window icon while preserving aspect ratio and transparency.

```csharp
titleBar.SetLogo(
    "/Foobar;component/Assets/logo.ico",
    showApplicationTitle: true,
    showApplicationSubtitle: true,
    showProjectTitle: false);
```

The three display arguments default to `true`, `true`, and `false`. Clicking or pointing at the logo opens a temporary [Overlay](../controls/overlay.md). It closes after the pointer leaves both the logo and surface. Applications can add a WPF body below the identity metadata through the `TitleBarApplicationInfo` shell region:

```csharp
builder.ConfigureContent(custom =>
    custom.AddRegionContent(
        ShellRegion.TitleBarApplicationInfo,
        services => new ApplicationSummaryView()));
```

The application owns this body; overflowing content scrolls vertically within the window.

## Search

`SetSearch` receives a placeholder and a handler for text changes. The handler receives the application `IServiceProvider` and current search text.

```csharp
builder.ConfigureTitleBar(titleBar =>
{
    titleBar.SetSearch(placeholder: "Search", handler: (services, searchText) =>
    {
        services.GetRequiredService<SearchCoordinator>().Update(searchText);
    });
});
```

## Back and forward navigation

`SetBreadcrumbMode` accepts a `BreadcrumbShowOption`:

| Value | Behavior |
| --- | --- |
| `Always` | Displays the controls while the title bar is visible. |
| `Auto` | Displays the controls when the navigation service can go back or forward. |
| `Hidden` | Hides the controls. |

Omitting the argument uses `Auto`.

## Profile and theme controls

`SetProfile` displays the profile trigger and selects the order used for names and initials. [Profile](configure-profile.md) explains login behavior and custom profile pages.

`SetThemeToggle` displays the theme toggle and selects the theme used when Host configuration does not contain a saved preference. [Themes](configure-themes.md) explains system following and preference persistence.

`SetThemeToggle` and `SetProfile` persist the theme and profile name order by default. Passing `usePersistedPreference: false` makes the supplied startup value authoritative without removing an older stored value.

## Window commands

The built-in title bar provides minimize, maximize or restore, and close commands. Maximize follows the configured resize mode, and close follows the [Window](configure-window.md) configuration. Logo, title selector, and window commands support keyboard focus; the logo surface also closes with <kbd>Esc</kbd> or an outside click.

## Related features

- [Projects](projects.md)
- [Custom shell content](configure-custom-handler.md)
- [Profile](configure-profile.md)
- [Navigation](navigation.md)
- [Themes](configure-themes.md)
- [Window](configure-window.md)
