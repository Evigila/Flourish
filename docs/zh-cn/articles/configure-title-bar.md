---
title: 标题栏
description: 配置 Flourish Shell 标题栏的品牌信息、搜索、导航入口和辅助操作。
---

# 标题栏

标题栏承载窗口标题、Logo、搜索框、面包屑以及导航、用户资料和主题入口。先在 [Shell 配置](shell-configuration.md)中启用 `UseTitleBar()`，再使用 `ConfigureTitleBar` 选择内容和行为。

## 最小配置

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .ShowLogo()
            .ShowTitle()
            .ShowSubTitle()
            .SetTitle("Foobar")
            .SetSubtitle("桌面工作区")
            .SetLogo("pack://application:,,,/Assets/logo.png");
    });
```

`Show...` 方法控制内置区域是否显示。自定义 WPF 内容应通过[自定义 Shell 内容](configure-custom-handler.md)插入，而不是由标题栏配置创建。

`SetLogo` 接收 pack URI 或 `ImageSource`。不使用图片时，`SetLogoFallbackText` 可以为 Logo 区域提供回退文字。

## 搜索与面包屑

```csharp
titleBar
    .ShowSearch()
    .SetSearchPlaceholder("搜索")
    .SetSearchHandler(searchText => Search(searchText))
    .ShowBreadcrumb()
    .SetBreadcrumbBehavior(BreadcrumbShowOption.Auto);
```

`SetSearchHandler` 在搜索文本变化时接收新文本。需要解析应用服务时，可以使用带 `IServiceProvider` 的重载。

`SetBreadcrumbBehavior` 控制面包屑的显示时机：`Always` 始终显示，`Auto` 根据导航状态决定，`Hidden` 隐藏面包屑。

## 功能入口

```csharp
titleBar
    .ShowNavToggle()
    .ShowProfile()
    .ShowThemeToggle();
```

这些入口还依赖相应的 Shell 功能：

- 导航切换按钮需要启用 `UseNavigation()`。
- 用户资料入口需要启用 `UseProfile()`；具体行为由[用户资料（Profile）](configure-profile.md)配置。
- 主题按钮需要启用 `UseThemes()`；默认主题由[主题](configure-themes.md)配置。

## 窗口关闭行为

`SetTrayExit(true)` 会让标题栏关闭命令把窗口隐藏到 Windows 通知区域；托盘菜单可恢复窗口或退出应用。禁用托盘退出时，关闭命令会正常关闭窗口。窗口大小、启动位置、调整大小模式和任务栏可见性由[窗口](configure-window.md)配置。
