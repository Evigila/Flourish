---
title: Shell configuration
description: Enable Flourish Shell features and configure shared appearance options.
---

# Shell configuration

`ConfigureShell` enables the main Shell surfaces and applies options shared across those surfaces. Feature-specific builders configure the content and behavior of title bar, navigation, toolbar, motion, and status features.

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseTitleBar()
        .UseNavigation()
        .UseCenterContent(true, 1200)
        .UseDynamicToolbar()
        .UseTips(delay: 200)
        .UseMotion()
        .UseMaterialEffect(MaterialEffect.Mica)
        .UseGlobalFont("Segoe UI", 14)
        .UseStatusBar();
});

builder.ConfigureNavigation(navigation =>
    navigation.SetGroup(null, groupId: 0, group =>
        group.AddNavigableViewItem<HomePage>(isInitial: true)));
```

## Feature switches and shared options

| Shell method | Behavior | Feature guide |
| --- | --- | --- |
| `UseTitleBar` | Enables the Flourish title bar. When disabled, the Shell uses the native Windows title bar. | [Title bar](configure-title-bar.md) |
| `UseNavigation` | Enables the navigation panel. | [Navigation](navigation.md) |
| `UseCenterContent` | Limits and centers navigated page content on wide viewports. | [Content alignment](#customize-content-alignment) |
| `UseDynamicToolbar` | Enables page-specific toolbar content. | [Dynamic toolbar](dynamic-toolbar.md) |
| `UseTips` | Sets the initial delay and enables Flourish tooltips. | [Tooltips](configure-tips.md) |
| `UseMotion` | Enables configured transitions and animations. | [Motion](configure-motion.md) |
| `UseMaterialEffect` | Selects and enables the window material; `None` disables it. | [Material effects](configure-material-effect.md) |
| `UseThemeColors` | Sets the primary, secondary, and accent colors. | [Themes](configure-themes.md) |
| `UseCornerRadius` | Sets the shared control and surface corner radius. | [Themes](configure-themes.md) |
| `UseGlobalFont` | Sets the global text family and base size. | [Typography](configure-font.md) |
| `UseStatusBar` | Enables the persistent status bar. | [Status bar](status-bar.md) |

[Window](configure-window.md) does not require a Shell feature switch and is configured through `ConfigureWindow`.

## Prerequisites and priority

Boolean feature switches take priority over detailed configuration. For example, registered toolbar items remain hidden when `UseDynamicToolbar(false)` is active, and configured status items remain hidden when `UseStatusBar(false)` is active.

Title bar elements require `UseTitleBar()`. The navigation toggle also requires `UseNavigation()` because it controls that panel. Application content added to a predefined Shell region requires the corresponding title bar, navigation, toolbar, or status surface to be enabled.

Background tasks are the exception to persistent status-bar visibility. Active work temporarily shows its task indicators even when `UseStatusBar()` is omitted; the bar returns to its configured visibility after no active tasks remain. See [Background tasks](background-tasks.md).

## Customize content alignment

The breadcrumb, dynamic toolbar, content page, and content-region hosts use the `FlourishContentBodyMargin` dynamic resource. Applications can override it after adding `FlourishThemeResources`:

```xml
<Thickness x:Key="FlourishContentBodyMargin">24,0,24,0</Thickness>
```

Use `UseCenterContent(true, contentWidth)` to give navigated page content and aligned Shell regions—the content header, dynamic toolbar, breadcrumb, and content footer—a maximum width in device-independent pixels. When the available content area is wider than `contentWidth`, Flourish keeps those surfaces at that width and centers them. A narrower content area still uses all available width, and maximizing the window does not remove the configured limit.

During a `Resize` navigation-panel transition, the centered surfaces move with the changing content area while the configured width limit remains active. A surface that remains at `contentWidth` is translated without horizontally scaling its text or internal spacing. This avoids both temporary stretching and an end-of-transition layout step when the panel opens or closes.

The page's root scroll viewer remains full width. Its vertical scroll bar stays at the right edge of the content area and is not moved next to the centered content.

If `UseCenterContent` is omitted, or is called with `enabled: false`, navigated page content stretches across the available width without a maximum-width constraint.

## Disable a feature

`UseTitleBar`, `UseNavigation`, `UseDynamicToolbar`, `UseMotion`, and `UseStatusBar` accept an optional `enabled` value. `UseCenterContent` requires both `enabled` and `contentWidth`; pass `false` with the configured width when a shared builder setup must keep centered page content disabled.

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseNavigation(showNavigation)
        .UseCenterContent(useCenteredPages, 1200)
        .UseMotion(!useStaticInterface)
        .UseStatusBar(showStatusBar);
});

builder.ConfigureNavigation(navigation =>
    navigation.SetGroup(null, groupId: 0, group =>
        group.AddNavigableViewItem<HomePage>(isInitial: true)));
```

Omit `UseTips` or `UseGlobalFont` to retain their default behavior. Use `MaterialEffect.None` when shared configuration must explicitly disable the material.

## Related features

- [Window](configure-window.md) configures size, placement, and close behavior.
- [Application data](configure-data.md) configures localization and Host settings.
- [Dependency injection](configure-services.md) registers application services and replaceable Flourish services.
- [Custom shell content](configure-custom-handler.md) inserts application elements into enabled Shell regions.
- [Background tasks](background-tasks.md) runs cancellable work and displays its active status.
