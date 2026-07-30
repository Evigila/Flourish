---
title: Themes
description: Configure theme selection, application colors, shared corner radius, and preference persistence.
---

# Themes

Flourish provides system-following, light, and dark themes. `UseThemeToggle` enables theme selection, displays the title bar control, and defines the fallback mode used when Host configuration has no saved preference.

## Configure theme selection

Enable the title bar before displaying the theme control:

```csharp
builder
    .ConfigShell(shell => shell.UseTitleBar())
    .ConfigTitleBar(titleBar =>
        titleBar.UseThemeToggle(mode: FlourishTheme.System));
```

Omitting the argument uses `FlourishTheme.System`. Theme persistence is enabled by default, so the fallback applies only when `Flourish:Preferences:Theme` is absent or invalid. Pass `usePersistedPreference: false` when code must always choose the startup theme and runtime selections must not update it. [Application data](configure-data.md) explains the corresponding settings file.

If `UseThemeToggle` is not called, the title bar control remains hidden and the shell initializes with the light theme. The application can still change the theme at runtime through `IThemeService`.

## Configure application colors and corner radius

Use `ConfigShell` to provide primary, secondary, and accent colors and a shared corner radius:

```csharp
using System.Windows.Media;

builder.ConfigShell(shell =>
    shell
        .UseThemeColors(enabled: true, colors: new FlourishThemeColors(
            primary: Color.FromRgb(15, 108, 189),
            secondary: Color.FromRgb(92, 46, 145),
            accent: Color.FromRgb(216, 59, 1)))
        .UseCornerRadius(enabled: true, radius: 5));
```

All three colors must be fully opaque. Flourish derives the semantic interaction, surface, and foreground resources for the effective light or dark theme and recalculates them after a theme change. Pass `enabled: false` to restore the theme-defined colors while retaining a common builder call shape.

`UseCornerRadius` accepts a finite, non-negative value in device-independent pixels. A value of `0` produces square shared geometry. When the method is omitted, or when `enabled` is `false`, controls and surfaces use their theme-defined radii.

Verify application colors in both light and dark themes and preserve readable text contrast.

Use `IAppearanceService` when these values must change after startup:

```csharp
appearance.SetThemeColors(new FlourishThemeColors(primary, secondary, accent));
appearance.SetCornerRadius(8);

// Apply both changes atomically, or restore the standard resources with null.
appearance.SetAppearance(colors: null, cornerRadius: null);
```

Runtime overrides are held in a Flourish-owned resource layer. Clearing them reveals the
standard light or dark resources and preserves application-owned resource entries.

Theme colors and corner radius use the same default persistence policy. Each complete saved group takes precedence and subsequent `IAppearanceService` changes are written back unless that method explicitly passes `usePersistedPreference: false`.

## Semantic color roles

Flourish uses a compact subset of the Fluent color system. Neutral roles define text,
surfaces, interaction backgrounds, disabled states, and strokes. Primary, secondary, and
accent roles provide the application palette, while danger and warning roles communicate
status.

Controls consume these semantic roles instead of component-specific colors. Standard
surfaces share one hover and pressed progression, including card-shaped selection controls.
Selected content uses the shared selection roles, and presentation overlays share one scrim
and foreground pair. Brand and status variants keep separate interaction colors when their
meaning or contrast requirements differ.

When authoring a custom template, reference the closest Flourish semantic resource instead
of assigning a raw color or creating a component-specific hover brush. This keeps the
template consistent across light, dark, and runtime-customized themes.

## Theme modes and preferences

`FlourishTheme.System` follows the Windows application theme. `Light` and `Dark` select a fixed theme until the user chooses another mode.

Flourish reads `Flourish:Preferences:Theme` with the complete Host configuration precedence. A selection made through the title bar writes the file selected by `InitAppSettingsFilePath`. Host appsettings, User Secrets, environment variables, or command-line values can take priority on a later launch.

The selected directory must be writable. Writing the preference serializes the complete JSON object again, which reformats the file and removes comments.

## Related features

- [Control library](control-library.md) explains explicit Flourish controls and theme resource loading.
- [Title bar](configure-title-bar.md) configures the theme control.
- [Application data](configure-data.md) explains Host configuration and the theme preference key.
- [Runtime APIs](runtime-apis.md) describes `IThemeService` and `IAppearanceService` for live theme and appearance changes.
- [Material effects](configure-material-effect.md) configures the window material used with the active theme.
- [Typography](configure-font.md) configures the fonts used with theme resources.
