---
title: Themes
description: Configure theme selection, application colors, shared corner radius, and preference persistence.
---

# Themes

Flourish provides system-following, light, and dark themes. `SetThemeToggle` enables theme selection, displays the title bar control, and defines the fallback mode used when Host configuration has no saved preference.

## Configure theme selection

Enable the title bar before displaying the theme control:

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetThemeToggle(FlourishTheme.System));
```

Omitting the argument uses `FlourishTheme.System`. The fallback applies only when `Flourish:Preferences:Theme` is absent from Host configuration. [Application data](configure-data.md) explains the corresponding `appsettings.json` setting.

If `SetThemeToggle` is not called, the title bar control remains hidden and the shell initializes with the light theme. The application can still change the theme at runtime through `IThemeService`.

## Configure application colors and corner radius

Use `ConfigureShell` to provide primary, secondary, and accent colors and a shared corner radius:

```csharp
using System.Windows.Media;

builder.ConfigureShell(shell =>
    shell
        .UseThemeColors(new FlourishThemeColors(
            primary: Color.FromRgb(15, 108, 189),
            secondary: Color.FromRgb(92, 46, 145),
            accent: Color.FromRgb(216, 59, 1)))
        .UseCornerRadius(5));
```

All three colors must be fully opaque. Flourish derives the semantic interaction, surface, and foreground resources for the effective light or dark theme and recalculates them after a theme change.

`UseCornerRadius` accepts a finite, non-negative value in device-independent pixels. A value of `0` produces square shared geometry. When the method is omitted, controls and surfaces use their theme-defined radii.

Verify application colors in both light and dark themes and preserve readable text contrast.

## Theme modes and preferences

`FlourishTheme.System` follows the Windows application theme. `Light` and `Dark` select a fixed theme until the user chooses another mode.

Flourish reads `Flourish:Preferences:Theme` with the complete Host configuration precedence. A selection made through the title bar writes only the base `appsettings.json` in the Host content root. Environment-specific appsettings, User Secrets, environment variables, or command-line values can take priority on a later launch.

The content root must be writable. Writing the preference serializes the complete JSON object again, which reformats the file and removes comments.

## Related features

- [Control library](control-library.md) explains explicit Flourish controls and theme resource loading.
- [Title bar](configure-title-bar.md) configures the theme control.
- [Application data](configure-data.md) explains Host configuration and the theme preference key.
- [Runtime APIs](runtime-apis.md) describes `IThemeService` for theme changes while the application runs.
- [Material effects](configure-material-effect.md) configures the window material used with the active theme.
- [Typography](configure-font.md) configures the fonts used with theme resources.
