---
title: Shell 配置
description: 配置 Flourish Shell 窗口、标题栏、导航栏、动效、材质、字体和窗口行为。
---

# Shell 配置

Shell 配置通过 `ConfigureShell` 完成。它会收到一个 `IFlourishShellBuilder`，用于集中配置应用的高层视觉和窗口行为。

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

## 标题栏

`UseTitlebar` 用于启用并配置 Flourish 标题栏。标题栏可以显示 Logo、标题、副标题、搜索框、面包屑、导航开关和用户区域。

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
        .SetSubtitle("Flourish 示例")
        .SetLogo("pack://application:,,,/Assets/logo.png")
        .SetSearchPlaceholder("搜索图片")
        .SetBreadcrumbBehavior(BreadcrumbShowOption.Auto);
});
```

`SetBreadcrumbBehavior` 控制面包屑的显示时机。`Always` 始终显示，`Auto` 由 Flourish 根据导航状态决定，`Hidden` 则隐藏。

`SetTrayExit` 控制标题栏关闭行为是否走托盘流程。普通桌面窗口通常保持关闭；如果应用需要最小化到通知区域或关闭后继续运行，可以启用。标题栏关闭请求会先显示 Flourish 样式的确认窗口，只有确认后才会继续关闭或进入托盘流程。

## 导航栏

`UseNavigationPanel` 配置左侧或右侧导航区域。页面元数据通过 `AddNavigable` 注册，可见页面项、命令项、分组和底部固定项则在这里放置。

```csharp
shell.UseNavigationPanel((_, nav) =>
{
    nav
        .SetEnabled()
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

如果应用使用自定义导航或只有单页 Shell，可以使用 `SetEnabled(false)`。如果布局更适合右侧导航，可以使用 `NavigationPanelDirection.Right`。分组项位于上方可滚动区域；固定项始终显示在底部区域。

使用 `SetPanelWidth` 可以设置导航栏展开宽度、折叠宽度，以及拖拽调整时允许的最小和最大宽度。默认展开宽度为 `220`，折叠宽度为 `48`，可调整范围为 `160` 到 `420`。调整宽度的 splitter 在正常布局中不可见，仅在鼠标悬浮到边缘时显示，并使用预览模式，拖拽结束后才提交布局宽度。

## 动态工具栏区域

`UseDynamicToolbar()` 只负责启用 Shell 中的工具栏区域。具体按页面变化的工具栏项，需要通过 `ConfigureDynamicToolbar` 单独注册。

```csharp
shell.UseDynamicToolbar(enabled: true);
```

如果应用没有上下文页面命令，可以关闭它。

## Tips

`UseTips` 配置 Flourish 的提示浮层。Tips 使用 Flourish 样式，并按 Shell 区域预设生成方向：左侧导航向右显示，右侧导航向左显示，标题栏和顶部工具栏向下显示，Footer 向上显示。

```csharp
shell.UseTips((_, tips) =>
{
    tips.SetDelay(600).SetSpawnableMargin(5);
});
```

默认情况下，Tips 在悬浮 `800` 毫秒后显示，并和 Shell 窗口边界至少保持 `5` 像素距离。使用 `SetDelay` 可以调整悬浮延迟，使用 `SetSpawnableMargin` 可以调整这个边界距离。

## 动效

`UseMotion()` 会启用默认动效。接收 `IFlourishMotionBuilder` 的重载可以继续配置时长、页面过渡、导航栏过渡、Hover Reveal 和系统减少动画偏好。

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

如果应用更需要稳定、静态的界面，可以使用 `FlourishPageTransition.None` 或 `SetEnabled(false)`。

## 材质特效

`UseMaterialEffect` 用于给 Shell 窗口应用 Windows 材质效果。

> [!WARNING]
> 材质效果依赖 Windows 桌面合成能力。需要完全不透明窗口，或部署环境应避开平台相关视觉效果时，使用 `MaterialEffect.None`。

```csharp
shell.UseMaterialEffect(MaterialEffect.Mica);
```

`MaterialEffect.Mica` 是默认的 Windows 11 风格。需要完全不透明窗口，或希望避开平台相关材质行为时，可以使用 `MaterialEffect.None`。

## 全局字体

`SetGlobalFont` 设置 Shell 的字体和基础字号。

```csharp
shell.SetGlobalFont("Microsoft YaHei", 14);
```

应选择能覆盖应用显示语言的字体。对于中英混排界面，Windows 上 `Microsoft YaHei` 是稳妥选择。

## 窗口属性

`SetWindowProperty` 配置 Shell 窗口尺寸、最小/最大尺寸、启动位置、手动位置、初始状态、缩放模式、置顶行为和任务栏显示。

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

如果使用 `SetManualWindowPosition(left, top)`，通常也应把启动位置设置为 `WindowStartupLocation.Manual`。

## 完整示例

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
                .SetSubtitle("Flourish 示例");
        })
        .UseNavigationPanel((_, nav) =>
        {
            nav.SetDirection()
               .SetInitiallyOpen()
               .SetPanelWidth(openWidth: 260, closedWidth: 48, maxWidth: 480, minWidth: 180)
               .SetGroup("导航", groupId: 0, group =>
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
