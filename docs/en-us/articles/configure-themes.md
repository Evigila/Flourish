---
title: Themes
description: Choose the default Flourish theme and persist the user's selection.
---

# Themes

Themes provide light, dark, and system-following shell resources. Enable theme support through [Shell configuration](shell-configuration.md), then use `ConfigureThemes` to choose the default used when no saved preference exists.

## Configure the default theme

```csharp
builder
    .ConfigureData(data =>
        data.SetAppCompany("Example Company").SetAppName("Foobar"))
    .ConfigureShell(shell => shell.UseThemes())
    .ConfigureThemes(FlourishTheme.System);
```

Theme preferences require either an explicit preference directory or a company name plus an application name or non-empty title. [Application data](configure-data.md) explains those storage options.

## Theme selection and persistence

`FlourishTheme.System` follows the Windows app theme. `Light` and `Dark` force a specific default until the user changes the theme.

When themes are enabled, Flourish stores the selected theme in application preferences. [Application data](configure-data.md) supplies the storage identity.

The title bar theme toggle appears only when [Title bar](configure-title-bar.md) shows the toggle and `UseThemes()` enables theme support.

## Related features

- [Application data](configure-data.md) provides the preference storage identity.
- [Title bar](configure-title-bar.md) can show the theme toggle.
- [Material effects](configure-material-effect.md) work with the effective light and dark resources.
