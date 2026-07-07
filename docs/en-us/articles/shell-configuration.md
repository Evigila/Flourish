---
title: Shell configuration
description: Configure Flourish shell feature switches and detailed shell options.
---

# Shell configuration

`ConfigureShell` controls only high-level shell feature switches. Each `Use...` method has a single `enabled` parameter and defaults to `true`.

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseTitleBar()
        .UseNavigation()
        .UseDynamicToolbar()
        .UseTips()
        .UseMotion()
        .UseMaterialEffect()
        .UseThemes()
        .UseFooter();
});
```

`ConfigureShell` has the highest priority. If a feature is not enabled there, the matching detailed configuration is still accepted during build but the shell does not display that area or behavior.

## Title bar

Enable the title bar with `UseTitleBar`, then configure its details with `ConfigureTitleBar`.

```csharp
builder.ConfigureShell(shell => shell.UseTitleBar());

builder.ConfigureTitleBar(titleBar =>
{
    titleBar
        .ShowLogo()
        .ShowTitle()
        .ShowSubTitle()
        .ShowSearch()
        .ShowBreadcrumb()
        .ShowNavToggle()
        .ShowProfile()
        .ShowThemeToggle()
        .SetTrayExit(false)
        .SetTitle("Gallery")
        .SetSubtitle("Flourish sample")
        .SetLogo("pack://application:,,,/Assets/logo.png")
        .SetSearchPlaceholder("Search images")
        .SetBreadcrumbBehavior(BreadcrumbShowOption.Auto);
});
```

`SetBreadcrumbBehavior` controls when breadcrumb navigation appears. `Always` keeps it visible, `Auto` lets Flourish decide from navigation state, and `Hidden` suppresses it.

## Navigation

Enable navigation with `UseNavigation`, then configure panel display and visible items with `ConfigureNavigation`.

```csharp
builder.ConfigureShell(shell => shell.UseNavigation());

builder.ConfigureNavigation(navigation =>
{
    navigation
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

Use `UseNavigation(false)` for applications that rely on custom navigation or a single-page shell. Use `SetDirection(NavigationPanelDirection.Right)` when your layout benefits from a right-side navigation rail.

## Dynamic Toolbar

Enable the shell surface with `UseDynamicToolbar`, then register page-specific items with `ConfigureDynamicToolbar`.

```csharp
builder.ConfigureShell(shell => shell.UseDynamicToolbar());

builder.ConfigureDynamicToolbar(toolbar =>
{
    toolbar.CreateToolbarItems<HomePage>(
        new FlourishToolbarItem("Open", "\uE8E5", "home.open"),
        new FlourishToolbarItem("Save", "\uE74E", "home.save"));
});
```

## Tips

Enable tooltips with `UseTips`, then tune tooltip behavior with `ConfigureTips`.

```csharp
builder.ConfigureShell(shell => shell.UseTips());

builder.ConfigureTips(tips =>
{
    tips.SetDelay(600).SetSpawnableMargin(5);
});
```

By default, tips appear after `800` milliseconds and keep at least `5` pixels away from the shell window bounds.

## Motion

Enable motion with `UseMotion`, then configure animation details with `ConfigureMotion`.

```csharp
builder.ConfigureShell(shell => shell.UseMotion());

builder.ConfigureMotion(motion =>
{
    motion
        .SetDuration(TimeSpan.FromMilliseconds(180))
        .SetPageTransition(FlourishPageTransition.EntranceFromBottom)
        .SetNavigationPanelTransition(FlourishNavigationPanelTransition.Resize)
        .SetHoverReveal()
        .RespectSystemReducedMotion();
});
```

Use `UseMotion(false)` when predictable, static UI is more important than motion.

## Material Effect

Enable material effects with `UseMaterialEffect`, then choose the material with `ConfigureMaterialEffect`.

```csharp
builder.ConfigureShell(shell => shell.UseMaterialEffect());
builder.ConfigureMaterialEffect(MaterialEffect.Mica);
```

> [!WARNING]
> Material effects depend on Windows desktop composition support. Use `MaterialEffect.None` for fully opaque windows or when a deployment environment should avoid platform-specific visuals.

## Themes

Enable theme support with `UseThemes`, then choose the default theme with `ConfigureThemes`.

```csharp
builder.ConfigureShell(shell => shell.UseThemes());
builder.ConfigureThemes(FlourishTheme.System);
```

When themes are enabled, Flourish stores the selected theme in application preferences.

## Font

Use `ConfigureFont` to set the shell font family and base size.

```csharp
builder.ConfigureFont("Microsoft YaHei", 14);
```

Choose a font that supports the languages your application displays.

## Window

Use `ConfigureWindow` to configure the shell window size, min/max constraints, startup location, manual position, initial state, resize mode, topmost behavior, and taskbar visibility.

```csharp
builder.ConfigureWindow(window =>
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

## Full Example

```csharp
builder
    .ConfigureShell(shell =>
    {
        shell
            .UseTitleBar()
            .UseNavigation()
            .UseDynamicToolbar()
            .UseTips()
            .UseMotion()
            .UseMaterialEffect()
            .UseThemes()
            .UseFooter();
    })
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .ShowLogo()
            .ShowTitle()
            .ShowSubTitle()
            .ShowSearch()
            .ShowBreadcrumb()
            .ShowNavToggle()
            .SetTitle("Gallery")
            .SetSubtitle("Flourish sample");
    })
    .ConfigureNavigation(navigation =>
    {
        navigation
            .SetDirection()
            .SetInitiallyOpen()
            .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
            .SetGroup("Navigation", groupId: 0, group =>
            {
                group.AddNavigableViewItem<HomePage>(isInitial: true);
            });
    })
    .ConfigureTips(tips => tips.SetDelay(600).SetSpawnableMargin(5))
    .ConfigureMotion(motion =>
    {
        motion.SetDuration().SetHoverReveal().SetNavigationPanelTransition().SetPageTransition();
    })
    .ConfigureMaterialEffect(MaterialEffect.Mica)
    .ConfigureThemes(FlourishTheme.System)
    .ConfigureFont("Microsoft YaHei")
    .ConfigureWindow(window => window.SetWindowSize().SetWindowMinSize().SetWindowPosition());
```
