---
title: Themes
description: Choose the default Flourish theme and persist the user's selection.
---

# Themes

Themes provide light, dark, and system-following shell resources. `SetThemeToggle` configures the initial mode, enables theme handling, and displays the title bar toggle in one step.

## Configure the default theme

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetThemeToggle(FlourishTheme.System));
```

The argument supplies the fallback used when `Flourish:Preferences:Theme` is absent from Host configuration. [Application data](configure-data.md) explains the `appsettings.json` setting.

## Theme selection and persistence

`FlourishTheme.System` follows the Windows app theme. `Light` and `Dark` force a specific default until the user changes the theme.

When `SetThemeToggle` is configured, Flourish reads `Flourish:Preferences:Theme` with the full Host configuration precedence. A user selection writes only the Host's base `appsettings.json`; environment appsettings, User Secrets, environment variables, or command-line values can still take priority on a later launch. The content root must remain writable, and writing reformats the JSON and removes comments.

The toggle requires the title bar surface to be enabled with `UseTitleBar()`. If `SetThemeToggle` is not called, the control remains hidden and theme selection is not enabled.

## Related features

- [Control library](control-library.md) explains how explicit `Flourish*` controls consume the active theme resources while native WPF controls remain unchanged.
- [Application data](configure-data.md) explains Host configuration and the theme key.
- [Title bar](configure-title-bar.md) configures and displays the theme toggle.
- [Material effects](configure-material-effect.md) work with the effective light and dark resources.
