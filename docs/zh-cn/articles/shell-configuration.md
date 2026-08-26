---
title: 功能配置
description: 将每个启动 Builder 对应到运行时 Service。
---

# 功能配置

Flourish 不再提供包揽全部功能的 Shell Builder。每项功能都有聚焦的启动 Builder；允许启动后修改的功能同时提供对应的运行时 Service。

| 启动入口 | 常用启动调用 | 运行时 Service |
| --- | --- | --- |
| `ConfigureAppearance` | `SetEffect`、`SetThemeColors`、`SetCornerRadius` | `IMaterialEffectService`、`IAppearanceService` |
| `ConfigureFont` | `SetFont`、`SetOverrideFont<TPage>` | `IFontService` |
| `ConfigureLayout` | `SetCenterContent`、`SetSmoothScrollingEnabled` | `IContentLayoutService`、`IScrollService` |
| `ConfigureToolTips` | `SetEnabled`、`SetSettings` | `IToolTipService` |
| `ConfigureProjects` | `SetMultiProjectEnabled` | `IProjectService` |
| `ConfigureTitleBar` | `SetEnabled`、`SetApplicationTitle`、`SetSearch`、`SetProfile` | `ITitleBarService`、`IProfileFlyoutService` |
| `ConfigureNavigation` | `SetEnabled`、`AddNavigable`、`SetPanelWidth` | `INavigationService` |
| `ConfigureContent` | `AddRegionContent`、`SetProfileContent` | `IShellRegionService` |
| `ConfigureToolbar` | `SetEnabled`、`Set<TPage>` | `IToolbarService` |
| `ConfigureMotion` | `SetEnabled` 和过渡方法 | `IMotionService` |
| `ConfigureWindow` | `SetSize`、`SetStartupLocation`、`SetTrayExit` | `IWindowService`、`IWindowCloseService`、`ITrayService` |
| `ConfigureStatusBar` | `SetEnabled`、状态项和指示器 | `IStatusBarService` |

```csharp
builder
    .ConfigureAppearance(appearance =>
        appearance.SetEffect(effect: MaterialEffect.Auto))
    .ConfigureFont(font =>
        font.SetFont("Segoe UI", 12, 14, 22, 16, 24, 32))
    .ConfigureLayout(layout =>
        layout.SetCenterContent(contentWidth: 1200).SetSmoothScrollingEnabled())
    .ConfigureToolTips(toolTips =>
        toolTips.SetEnabled().SetSettings(200, 5));
```

Builder 仅记录启动默认值。运行时 Service 操作已构建的应用；可观察状态统一通过不可变的 `Current` 快照与 `Changed` 事件公开。
