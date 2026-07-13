---
title: Shell configuration
description: Enable Flourish shell features and understand their prerequisites.
---

# Shell configuration

`ConfigureShell` controls the main Flourish surfaces and the simple settings that belong to the shell as a whole. A method that accepts a setting configures and enables that feature in one step.

```csharp
builder
    .ConfigureShell(shell =>
    {
        shell
            .UseTitleBar()
            .UseNavigation()
            .UseDynamicToolbar()
            .UseTips(200)
            .UseMotion()
            .UseMaterialEffect(MaterialEffect.Mica)
            .UseGlobalFont("Segoe UI", 14)
            .UseStatusBar();
    });
```

`UseTitleBar`, `UseNavigation`, `UseDynamicToolbar`, `UseMotion`, and `UseStatusBar` accept an `enabled` value that defaults to `true`. The remaining methods take the setting they apply.

## Feature switches

| Switch | Feature | Detailed configuration |
| --- | --- | --- |
| `UseTitleBar` | Uses the Flourish custom title bar; when disabled, the shell uses the native Windows title bar and preserves the requested material effect. | [Title bar](configure-title-bar.md) |
| `UseNavigation` | Displays the navigation panel. | [Navigation](navigation.md) |
| `UseDynamicToolbar` | Displays page-specific toolbar content. | [Dynamic toolbar](dynamic-toolbar.md) |
| `UseTips(delay)` | Enables Flourish tooltips with the requested initial delay. | [Tooltips](configure-tips.md) |
| `UseMotion` | Enables configured transitions and animations. | [Motion](configure-motion.md) |
| `UseMaterialEffect(effect)` | Applies the selected window material. | [Material effects](configure-material-effect.md) |
| `UseGlobalFont(family, size)` | Sets the global font used by Flourish shell UI and navigated pages. | [Typography](configure-font.md) |
| `UseStatusBar` | Displays the status bar. | [Status bar](status-bar.md) |

## Prerequisites and priority

Boolean feature switches take priority over their detailed configuration. For example, toolbar items registered for a page are not displayed when `UseDynamicToolbar(false)` is active, and status items remain hidden when `UseStatusBar(false)` is active.

Background-task activity is the exception to persistent status-bar visibility: active work temporarily reveals its task indicators even when `UseStatusBar()` is omitted. The bar hides again after the active list becomes empty. See [Background tasks](background-tasks.md).

## Content body alignment

The breadcrumb, dynamic toolbar, content page, and content-region hosts share the `FlourishContentBodyMargin` dynamic resource. Its default value is `32,0,32,0`, giving every content surface the same left and right edge.

Applications that require a different gutter or full-bleed content can override the resource after adding `FlourishThemeResources`:

```xml
<Thickness x:Key="FlourishContentBodyMargin">24,0,24,0</Thickness>
```

## Shell chrome alignment

The built-in title bar content, navigation rows, and status bar use the same horizontal gutter at the left and right window edges. When the navigation panel is placed on the right, Flourish mirrors its outer gutter so navigation content remains aligned with the title bar and status bar. The collapsed navigation width includes this gutter together with its command surface and compact scrollbar. The minimize, maximize or restore, and close caption buttons remain flush with the upper-right window edge to preserve the window command hit area.

Simple shell features use configuration as their activation point:

- `UseTips(delay)` uses the built-in tooltip boundary margin.
- `UseMaterialEffect(effect)` applies the selected effect; `MaterialEffect.None` disables material composition.
- `UseGlobalFont(family, size)` uses a default base size of `14` when the size is omitted.

Title bar elements follow the same configuration-first model. [Title bar](configure-title-bar.md) explains how `SetProfile`, `SetThemeToggle`, and the other `Set...` methods both configure and display their controls. The title bar navigation toggle additionally requires `UseNavigation()` because it controls that panel. Theme preferences use the Host configuration described in [Application data](configure-data.md).

## Disable a feature

Pass `false` when a feature should remain disabled while sharing a common builder setup.

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseNavigation(showNavigation)
        .UseMotion(!useStaticInterface)
        .UseStatusBar(showStatusBar);
});
```

Omit `UseTips` or `UseGlobalFont` when their default shell behavior should remain unchanged. Use `MaterialEffect.None` when shared configuration must explicitly disable a material effect.

## Related configuration areas

These settings and extension points use their own configuration entry points:

- [Window](configure-window.md) sets size, placement, and WPF window behavior.
- [Application data](configure-data.md) explains localization and Host configuration.
- [Dependency injection](configure-services.md) registers application and replaceable Flourish services.
- [Custom shell content](configure-custom-handler.md) inserts application elements into enabled shell regions.
- [Background tasks](background-tasks.md) describes the Host-managed worker pool and its automatic status indicators.
