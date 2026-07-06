---
title: Shell configuration
description: Configure the Flourish shell window, title bar, navigation, motion, material, font, and window behavior.
---

# Shell configuration

Shell configuration is performed through `ConfigureShell`. It receives an `IFlourishShellBuilder`, which groups the high-level visual and window concerns of the application.

```csharp
builder.ConfigureShell((_, shell) =>
{
    shell
        .UseTitlebar((_, titlebar) => { })
        .UseNavigationPanel((_, nav) => { })
        .UseDynamicToolbar()
        .UseTips((_, tips) => { })
        .UseMotion((_, motion) => { })
        .UseMaterialEffect()
        .SetGlobalFont("Microsoft YaHei")
        .SetWindowProperty((_, window) => { });
});
```

## Title bar

`UseTitlebar` enables and configures the Flourish title bar. The title bar can show the logo, title, subtitle, search box, breadcrumb, navigation toggle, and profile area.

```csharp
shell.UseTitlebar((_, titlebar) =>
{
    titlebar
        .ShowLogo()
        .ShowTitle()
        .ShowSubTitle()
        .ShowSearch()
        .ShowBreadcrumb()
        .ShowNavToggle()
        .ShowProfile()
        .SetTrayExit(false)
        .SetTitle("Gallery")
        .SetSubtitle("Flourish sample")
        .SetLogo("pack://application:,,,/Assets/logo.png")
        .SetSearchPlaceholder("Search images")
        .SetBreadcrumbBehavior(BreadcrumbShowOption.Auto);
});
```

`SetBreadcrumbBehavior` controls when breadcrumb navigation appears. `Always` keeps it visible, `Auto` lets Flourish decide from navigation state, and `Hidden` suppresses it.

`SetTrayExit` controls whether title bar close behavior should follow the tray flow. Leave it disabled for ordinary desktop windows; enable it for applications that minimize or stay alive in the notification area. Title bar close requests show a Flourish-styled confirmation dialog before the close or tray action is applied.

## Navigation panel

`UseNavigationPanel` configures the left or right navigation area. Register page metadata with `AddNavigable`, then place visible page items, command items, groups, and fixed bottom items here.

```csharp
shell.UseNavigationPanel((_, nav) =>
{
    nav
        .SetEnabled()
        .SetDirection(NavigationPanelDirection.Left)
        .SetInitiallyOpen()
        .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
        .SetGroup("Navigation", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<GalleryPage>();
            group.AddNavigableItem("Refresh", "navigation.refresh", iconGlyph: "\uE72C");
        })
        .AddFixedNavigableViewItem<SettingsPage>()
        .AddFixedNavigableItem("About", "app.about", iconGlyph: "\uE946");
});
```

Use `SetEnabled(false)` for applications that rely on custom navigation or a single-page shell. Use `SetDirection(NavigationPanelDirection.Right)` when your layout benefits from a right-side navigation rail. Grouped items live in the scrollable upper area; fixed items stay visible in the bottom area.

Use `SetPanelWidth` to set the open and collapsed panel widths and constrain the preview splitter resize range. By default, the panel opens at `220`, collapses to `48`, and can be resized between `160` and `420`. The splitter is invisible in the normal layout, appears only while hovering over the resize edge, and uses preview mode so the layout is committed after dragging finishes.

## Dynamic toolbar surface

`UseDynamicToolbar()` only enables the shell surface. The page-specific items are registered separately through `ConfigureDynamicToolbar`.

```csharp
shell.UseDynamicToolbar(enabled: true);
```

Disable it when the application does not have contextual page commands.

## Tips

`UseTips` configures Flourish tooltips. Tooltips use Flourish styling and predefined shell-region placement: left navigation opens right, right navigation opens left, title bar and top toolbar controls open downward, and footer controls open upward.

```csharp
shell.UseTips((_, tips) =>
{
    tips.SetDelay(600).SetSpawnableMargin(5);
});
```

By default, tips appear after `800` milliseconds and keep at least `5` pixels away from the shell window bounds. Use `SetDelay` to tune the hover delay and `SetSpawnableMargin` to adjust that window-edge margin.

## Motion

`UseMotion()` enables default animation settings. The overload that receives `IFlourishMotionBuilder` lets you control duration, page transition, navigation panel transition, hover reveal, and reduced-motion behavior.

```csharp
shell.UseMotion((_, motion) =>
{
    motion
        .SetEnabled()
        .SetDuration(TimeSpan.FromMilliseconds(180))
        .SetPageTransition(FlourishPageTransition.EntranceFromBottom)
        .SetNavigationPanelTransition(FlourishNavigationPanelTransition.Resize)
        .SetHoverReveal()
        .RespectSystemReducedMotion();
});
```

Use `FlourishPageTransition.None` or `SetEnabled(false)` when predictable, static UI is more important than motion.

## Material effect

`UseMaterialEffect` applies a Windows material effect to the shell window.

> [!WARNING]
> Material effects depend on Windows desktop composition support. Use `MaterialEffect.None` for fully opaque windows or when a deployment environment should avoid platform-specific visuals.

```csharp
shell.UseMaterialEffect(MaterialEffect.Mica);
```

Use `MaterialEffect.Mica` for the default Windows 11 style. Use `MaterialEffect.None` when you want a fully opaque window or need to avoid platform-specific material behavior.

## Global font

`SetGlobalFont` sets the shell font family and base size.

```csharp
shell.SetGlobalFont("Microsoft YaHei", 14);
```

Choose a font that supports the languages your application displays. For mixed Chinese and English UI, `Microsoft YaHei` is a safe default on Windows.

## Window properties

`SetWindowProperty` configures the shell window size, min/max constraints, startup location, manual position, initial state, resize mode, topmost behavior, and taskbar visibility.

```csharp
shell.SetWindowProperty((_, window) =>
{
    window
        .SetWindowSize(1280, 720)
        .SetWindowMinSize(960, 540)
        .SetWindowMaxSize(1920, 1080)
        .SetWindowPosition(WindowStartupLocation.CenterScreen)
        .SetWindowState(WindowState.Normal)
        .SetWindowResizeMode(ResizeMode.CanResize)
        .UseTopmost(false)
        .ShowInTaskbar(true);
});
```

Use `SetManualWindowPosition(left, top)` when you also set `WindowStartupLocation.Manual`.

## Full example

```csharp
builder.ConfigureShell((_, shell) =>
{
    shell
        .UseTitlebar((_, titlebar) =>
        {
            titlebar
                .ShowLogo()
                .ShowTitle()
                .ShowSubTitle()
                .ShowSearch()
                .ShowBreadcrumb()
                .ShowNavToggle()
                .SetTitle("Gallery")
                .SetSubtitle("Flourish sample");
        })
        .UseNavigationPanel((_, nav) =>
        {
            nav.SetDirection()
               .SetInitiallyOpen()
               .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
               .SetGroup("Navigation", groupId: 0, group =>
               {
                   group.AddNavigableViewItem<HomePage>(isInitial: true);
               });
        })
        .UseDynamicToolbar()
        .UseTips((_, tips) =>
        {
            tips.SetDelay(600).SetSpawnableMargin(5);
        })
        .UseMotion((_, motion) =>
        {
            motion.SetDuration().SetHoverReveal().SetNavigationPanelTransition().SetPageTransition();
        })
        .UseMaterialEffect()
        .SetGlobalFont("Microsoft YaHei")
        .SetWindowProperty((_, window) =>
        {
            window.SetWindowSize().SetWindowMinSize().SetWindowPosition();
        });
});
```
