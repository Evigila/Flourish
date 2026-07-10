---
title: Shell configuration
description: Enable Flourish shell features and understand their prerequisites.
---

# Shell configuration

`ConfigureShell` controls which Flourish features participate in the shell. It enables surfaces and behaviors; each feature article explains its detailed configuration.

```csharp
builder
    .ConfigureData(data =>
        data.SetAppCompany("Example Company").SetAppName("Foobar"))
    .ConfigureShell(shell =>
    {
        shell
            .UseTitleBar()
            .UseProfile()
            .UseNavigation()
            .UseDynamicToolbar()
            .UseTips()
            .UseMotion()
            .UseMaterialEffect()
            .UseThemes()
            .UseFooter();
    });
```

Each `Use...` method accepts an `enabled` value and defaults to `true`.

## Feature switches

| Switch | Feature | Detailed configuration |
| --- | --- | --- |
| `UseTitleBar` | Displays the built-in window title bar. | [Title bar](configure-title-bar.md) |
| `UseProfile` | Enables profile access through the title bar. | [Profile](configure-profile.md) |
| `UseNavigation` | Displays the navigation panel. | [Navigation](navigation.md) |
| `UseDynamicToolbar` | Displays page-specific toolbar content. | [Dynamic toolbar](dynamic-toolbar.md) |
| `UseTips` | Enables Flourish tooltips. | [Tooltips](configure-tips.md) |
| `UseMotion` | Enables configured transitions and animations. | [Motion](configure-motion.md) |
| `UseMaterialEffect` | Enables the selected window material. | [Material effects](configure-material-effect.md) |
| `UseThemes` | Enables theme selection, system following, and preference handling. | [Themes](configure-themes.md) |
| `UseFooter` | Displays the footer status area. | [Footer status](status-bar.md) |

## Prerequisites and priority

Feature switches take priority over detailed configuration. Detailed settings do not activate a feature; the corresponding surface or behavior remains inactive until its switch is enabled.

For example, toolbar items registered for a page are not displayed when `UseDynamicToolbar(false)` is active, and footer status items remain hidden when `UseFooter(false)` is active.

Some controls depend on more than one switch:

- The profile trigger requires both `UseTitleBar()` and `UseProfile()`.
- The title bar theme toggle requires both `UseTitleBar()` and `UseThemes()`.
- The title bar navigation toggle can be shown only when both the title bar and navigation panel are enabled.
- Theme preference storage requires the identity or explicit directory described in [Application data](configure-data.md).

## Disable a feature

Pass `false` when a feature should remain disabled while sharing a common builder setup.

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseNavigation(showNavigation)
        .UseMotion(!useStaticInterface)
        .UseMaterialEffect(false);
});
```

## Related configuration areas

These settings and extension points use their own configuration entry points:

- [Window](configure-window.md) sets size, placement, and WPF window behavior.
- [Typography](configure-font.md) sets the shell font family and base size.
- [Application data](configure-data.md) identifies preference storage.
- [Dependency injection](configure-services.md) registers application and replaceable Flourish services.
- [Custom shell content](configure-custom-handler.md) inserts application elements into enabled shell regions.
