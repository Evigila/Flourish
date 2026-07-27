---
title: Shell configuration
description: Enable Flourish Shell features and configure shared appearance options.
---

# Shell configuration

`ConfigShell` enables the main Shell surfaces and applies options shared across those surfaces. Feature-specific builders configure the content and behavior of title bar, navigation, toolbar, motion, and status features.

```csharp
builder.ConfigShell(shell =>
{
    shell
        .UseTitleBar()
        .UseMultiProject()
        .UseNavigation()
        .UseCenterContent(enabled: true, contentWidth: 1200)
        .UseDynamicToolbar()
        .UseTips(enabled: true, delay: 200)
        .UseMotion()
        .UseSmoothScroll(enabled: true)
        .UseMaterialEffect(enabled: true, effect: MaterialEffect.Mica)
        .InitGlobalFont("Segoe UI", 12, 14, 16, 16, 24, 32)
        .UseStatusBar();
});

builder.ConfigNavigation(navigation =>
    navigation.InitGroup(null, groupId: 0, group =>
        group.InitNavigableViewItem<HomePage>(isInitial: true)));
```

## Feature switches and shared options

| Shell method | Behavior | Feature guide |
| --- | --- | --- |
| `UseTitleBar` | Enables the Flourish title bar. When disabled, the Shell uses the native Windows title bar. | [Title bar](configure-title-bar.md) |
| `UseMultiProject` | Gives the title selector project semantics, exposes the persistent catalog plus **New project**, and enables the built-in project save and close workflows. | [Projects](projects.md) |
| `UseNavigation` | Enables the navigation panel. | [Navigation](navigation.md) |
| `UseCenterContent` | Limits and centers navigated page content on wide viewports. | [Content alignment](#customize-content-alignment) |
| `UseDynamicToolbar` | Enables page-specific toolbar content. | [Dynamic toolbar](dynamic-toolbar.md) |
| `UseTips` | Switches tooltips owned by Flourish controls and Shell surfaces from native WPF presentation to the Flourish presentation and sets its initial delay. | [Tooltips](configure-tips.md) |
| `UseMotion` | Enables configured transitions and animations. | [Motion](configure-motion.md) |
| `UseSmoothScroll` | Selects smooth or immediate mouse-wheel scrolling for built-in Flourish viewports. | [ScrollViewer and ScrollBar](../controls/scroll-viewer.md) |
| `UseMaterialEffect` | Selects and enables the window material; `None` disables it. | [Material effects](configure-material-effect.md) |
| `UseThemeColors` | Sets the primary, secondary, and accent colors. | [Themes](configure-themes.md) |
| `UseCornerRadius` | Sets the shared control and surface corner radius. | [Themes](configure-themes.md) |
| `InitGlobalFont` | Sets the global text family and explicit Small, Standard, Icon, Large, ExtraLarge, and HeaderSize sizes. | [Typography](configure-font.md) |
| `UseStatusBar` | Enables the persistent status bar. | [Status bar](status-bar.md) |

[Window](configure-window.md) does not require a Shell feature switch and is configured through `ConfigWindow`.

## Prerequisites and priority

Boolean feature switches take priority over detailed configuration. For example, registered toolbar items remain hidden when `UseDynamicToolbar(false)` is active, and configured status items remain hidden when `UseStatusBar(false)` is active.

Title bar elements and `UseMultiProject()` require `UseTitleBar()`. With project mode disabled, the title selector displays and lists only the application title; Flourish does not attach project-title, project-save, or project-close semantics. Project mode changes the selected title to the active project or unnamed-project placeholder, expands the choices to every project plus **New project**, and enables the built-in project lifecycle entry points. The catalog API remains available for application code in either mode.

The navigation toggle also requires `UseNavigation()` because it controls that panel. Application content added to a predefined Shell region requires the corresponding title bar, navigation, toolbar, or status surface to be enabled.

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

Use `IContentLayoutService` to change the same layout after startup:

```csharp
contentLayout.SetCenterContent(enabled: true, contentWidth: 1080);
contentLayout.Changed += OnContentLayoutChanged;
```

The change updates the active page and aligned Shell regions. Pages reached by later
navigation read the same service state.

## Disable a feature

Except for `InitGlobalFont`, every `Use...` method in the Shell family places `enabled` first. This keeps shared composition code consistent whether a feature has additional options or not. `UseCenterContent`, `UseTips`, `UseMaterialEffect`, `UseThemeColors`, and `UseCornerRadius` place their detailed settings after that switch.

```csharp
builder.ConfigShell(shell =>
{
    shell
        .UseNavigation(showNavigation)
        .UseMultiProject(useProjects)
        .UseCenterContent(enabled: useCenteredPages, contentWidth: 1200)
        .UseTips(enabled: useFlourishTips, delay: 200)
        .UseMotion(!useStaticInterface)
        .UseSmoothScroll(useSmoothScrolling)
        .UseMaterialEffect(useMaterial, MaterialEffect.Mica)
        .UseStatusBar(showStatusBar);
});

builder.ConfigNavigation(navigation =>
    navigation.InitGroup(null, groupId: 0, group =>
        group.InitNavigableViewItem<HomePage>(isInitial: true)));
```

Passing `false` to `UseTips` presents Flourish-owned tooltip content with the native WPF appearance and default behavior; tooltips attached to native WPF and third-party controls remain unchanged. `UseSmoothScroll(false)` selects immediate native mouse-wheel scrolling for built-in Flourish viewports. Pass `false` to `UseMaterialEffect`, `UseThemeColors`, or `UseCornerRadius` when shared configuration must explicitly restore the theme-defined behavior. Omit `InitGlobalFont` to retain its defaults.

## Related features

- [Window](configure-window.md) configures size, placement, and close behavior.
- [Projects](projects.md) explains project catalog persistence and replaceable lifecycle behavior.
- [Application data](configure-data.md) configures localization and Host settings.
- [Dependency injection](configure-services.md) registers application services and replaceable Flourish services.
- [Custom shell content](configure-custom-handler.md) inserts application elements into enabled Shell regions.
- [Background tasks](background-tasks.md) runs cancellable work and displays its active status.
