---
title: Themes
description: Choose the default Flourish theme and persist the user's selection.
---

# Themes

Themes provide light, dark, and system-following shell resources. `SetThemeToggle` configures the initial mode, enables theme handling, and displays the title bar toggle in one step.

## Configure the default theme

```csharp
builder
    .ConfigureData(data =>
        data.SetAppCompany("Example Company").SetAppName("Foobar"))
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetThemeToggle(FlourishTheme.System));
```

Theme preferences require either an explicit preference directory or a company name plus an application name or non-empty title. [Application data](configure-data.md) explains those storage options.

## Theme selection and persistence

`FlourishTheme.System` follows the Windows app theme. `Light` and `Dark` force a specific default until the user changes the theme.

When `SetThemeToggle` is configured, Flourish stores the selected theme in application preferences. [Application data](configure-data.md) supplies the storage identity.

The toggle requires the title bar surface to be enabled with `UseTitleBar()`. If `SetThemeToggle` is not called, the control remains hidden and theme selection is not enabled.

## Related features

- [Application data](configure-data.md) provides the preference storage identity.
- [Title bar](configure-title-bar.md) configures and displays the theme toggle.
- [Material effects](configure-material-effect.md) work with the effective light and dark resources.
