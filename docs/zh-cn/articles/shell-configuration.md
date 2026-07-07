---
title: Shell 配置
description: 配置 Flourish Shell 功能开关和详细 Shell 选项。
---

# Shell 配置

`ConfigureShell` 只负责高层 Shell 功能开关。每个 `Use...` 方法只有一个 `enabled` 参数，默认值为 `true`。

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

`ConfigureShell` 拥有最高优先级。如果某项功能没有在这里启用，对应的详细配置仍会被构建过程接收，但 Shell 不会展示该区域或行为。

## 标题栏

通过 `UseTitleBar` 启用标题栏，再通过 `ConfigureTitleBar` 配置细节。

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
        .SetSubtitle("Flourish 示例")
        .SetLogo("pack://application:,,,/Assets/logo.png")
        .SetSearchPlaceholder("搜索图片")
        .SetBreadcrumbBehavior(BreadcrumbShowOption.Auto);
});
```

`SetBreadcrumbBehavior` 控制面包屑的显示时机。`Always` 始终显示，`Auto` 由 Flourish 根据导航状态决定，`Hidden` 则隐藏。

## 导航栏

通过 `UseNavigation` 启用导航栏，再通过 `ConfigureNavigation` 配置导航栏展示参数和可见导航项。

```csharp
builder.ConfigureShell(shell => shell.UseNavigation());

builder.ConfigureNavigation(navigation =>
{
    navigation
        .SetDirection(NavigationPanelDirection.Left)
        .SetInitiallyOpen()
        .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
        .SetGroup("导航", groupId: 0, group =>
        {
            group.AddNavigableViewItem<HomePage>(isInitial: true);
            group.AddNavigableViewItem<GalleryPage>();
            group.AddNavigableItem("刷新", "navigation.refresh", iconGlyph: "\uE72C");
        })
        .AddFixedNavigableViewItem<SettingsPage>()
        .AddFixedNavigableItem("关于", "app.about", iconGlyph: "\uE946");
});
```

如果应用使用自定义导航或只有单页 Shell，可以使用 `UseNavigation(false)`。如果布局更适合右侧导航，可以使用 `NavigationPanelDirection.Right`。

## 动态工具栏

通过 `UseDynamicToolbar` 启用 Shell 工具栏区域，再通过 `ConfigureDynamicToolbar` 注册按页面变化的工具栏项。

```csharp
builder.ConfigureShell(shell => shell.UseDynamicToolbar());

builder.ConfigureDynamicToolbar(toolbar =>
{
    toolbar.CreateToolbarItems<HomePage>(
        new FlourishToolbarItem("打开", "\uE8E5", "home.open"),
        new FlourishToolbarItem("保存", "\uE74E", "home.save"));
});
```

## Tips

通过 `UseTips` 启用提示浮层，再通过 `ConfigureTips` 调整提示行为。

```csharp
builder.ConfigureShell(shell => shell.UseTips());

builder.ConfigureTips(tips =>
{
    tips.SetDelay(600).SetSpawnableMargin(5);
});
```

默认情况下，Tips 在悬浮 `800` 毫秒后显示，并和 Shell 窗口边界至少保持 `5` 像素距离。

## 动效

通过 `UseMotion` 启用动效，再通过 `ConfigureMotion` 配置动画细节。

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

如果应用更需要稳定、静态的界面，可以使用 `UseMotion(false)`。

## 材质特效

通过 `UseMaterialEffect` 启用材质特效，再通过 `ConfigureMaterialEffect` 选择材质类型。

```csharp
builder.ConfigureShell(shell => shell.UseMaterialEffect());
builder.ConfigureMaterialEffect(MaterialEffect.Mica);
```

> [!WARNING]
> 材质效果依赖 Windows 桌面合成能力。需要完全不透明窗口，或部署环境应避开平台相关视觉效果时，使用 `MaterialEffect.None`。

## 主题

通过 `UseThemes` 启用主题支持，再通过 `ConfigureThemes` 设置默认主题。

```csharp
builder.ConfigureShell(shell => shell.UseThemes());
builder.ConfigureThemes(FlourishTheme.System);
```

启用主题后，Flourish 会把用户选择的主题保存到应用偏好中。

## 字体

通过 `ConfigureFont` 设置 Shell 字体和基础字号。

```csharp
builder.ConfigureFont("Microsoft YaHei", 14);
```

应选择能覆盖应用显示语言的字体。

## 窗口

通过 `ConfigureWindow` 配置 Shell 窗口尺寸、最小/最大尺寸、启动位置、手动位置、初始状态、缩放模式、置顶行为和任务栏显示。

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

如果使用 `SetManualWindowPosition(left, top)`，通常也应把启动位置设置为 `WindowStartupLocation.Manual`。

## 完整示例

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
            .SetSubtitle("Flourish 示例");
    })
    .ConfigureNavigation(navigation =>
    {
        navigation
            .SetDirection()
            .SetInitiallyOpen()
            .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
            .SetGroup("导航", groupId: 0, group =>
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
