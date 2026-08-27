---
title: Themes
description: Configure theme selection, application colors, shared corner radius, and preference persistence.
---

# Themes

`SetThemeToggle` enables system, light, and dark theme selection from the title bar.

## Configure theme selection

Enable the title bar before displaying the theme control:

```csharp
builder
    .ConfigureTitleBar(titleBar =>
        titleBar
            .SetEnabled()
            .SetThemeToggle(mode: ApplicationTheme.System));
```

Omitting the argument uses `ApplicationTheme.System`. Persistence is enabled by default, so fallback applies only when `Flourish:Preferences:Theme` is absent or invalid; pass `usePersistedPreference: false` to keep configuration authoritative. See [Application data](configure-data.md).

If `SetThemeToggle` is not called, the title bar control remains hidden and the shell initializes with the light theme. The application can still change the theme at runtime through `IThemeService`.

## Configure application colors and corner radius

Use `ConfigureAppearance` to provide primary, secondary, and accent colors and a shared corner radius:

```csharp
using System.Windows.Media;

builder.ConfigureAppearance(appearance =>
    appearance
        .SetThemeColors(enabled: true, colors: new ThemeColors(
            primary: Color.FromRgb(15, 108, 189),
            secondary: Color.FromRgb(92, 46, 145),
            accent: Color.FromRgb(216, 59, 1)))
        .SetCornerRadius(enabled: true, radius: 5));
```

All three colors must be opaque. Flourish derives semantic resources for the effective theme; pass `enabled: false` to use theme colors.

`SetCornerRadius` accepts a finite, non-negative value in device-independent pixels. A value of `0` produces square shared geometry. When the method is omitted, or when `enabled` is `false`, controls and surfaces use their theme-defined radii.

Verify application colors in both light and dark themes and preserve readable text contrast.

Use `IAppearanceService` when these values must change after startup:

```csharp
appearance.SetThemeColors(new ThemeColors(primary, secondary, accent));
appearance.SetCornerRadius(8);

// Apply both changes atomically, or restore the standard resources with null.
appearance.SetAppearance(colors: null, cornerRadius: null);
```

Runtime overrides are held in a Flourish-owned resource layer. Clearing them reveals the
standard light or dark resources and preserves application-owned resource entries.

Theme colors and corner radius use the same default persistence policy. Each complete saved group takes precedence and subsequent `IAppearanceService` changes are written back unless that method explicitly passes `usePersistedPreference: false`.

## Semantic color roles

Neutral roles define text, surfaces, interactions, disabled states, and strokes. Primary, secondary, and accent roles define the application palette; danger and warning convey status.

Controls use semantic roles instead of component colors. Shared roles cover interaction, selection, and overlays; brand and status variants keep distinct colors when needed for meaning or contrast.

When authoring a custom template, reference the closest Flourish semantic resource instead
of assigning a raw color or creating a component-specific hover brush. This keeps the
template consistent across light, dark, and runtime-customized themes.

## Theme modes and preferences

`ApplicationTheme.System` follows the Windows application theme. `Light` and `Dark` select a fixed theme until the user chooses another mode.

Flourish reads `Flourish:Preferences:Theme` with the complete Host configuration precedence. A selection made through the title bar writes the file selected by `SetAppSettingsFilePath`. Host appsettings, User Secrets, environment variables, or command-line values can take priority on a later launch.

The selected directory must be writable. Writing the preference serializes the complete JSON object again, which reformats the file and removes comments.

## Related features

- [Control library](control-library.md)
- [Title bar](configure-title-bar.md)
- [Application data](configure-data.md)
- [Runtime APIs](runtime-apis.md)
- [Material effects](configure-material-effect.md)
- [Typography](configure-font.md)
