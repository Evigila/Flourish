---
title: 标题栏
description: 配置 Flourish Shell 标题栏的品牌信息、搜索、导航入口和辅助操作。
---

# 标题栏

标题栏承载窗口标题、Logo、搜索框、面包屑以及导航、用户资料和主题入口。先在 [Shell 配置](shell-configuration.md)中启用 `UseTitleBar()`，再使用 `ConfigureTitleBar` 配置需要显示的元素。每个 `Set...` 方法会同时配置并显示对应元素；未配置的元素保持隐藏。

## 品牌信息

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .SetLogo()
            .SetTitle("Foobar")
            .SetSubTitle("桌面工作区");
    });
```

`SetLogo()` 使用 Flourish 内置应用图标。如需替换，可传入相对 URI、绝对 URI 或 WPF pack URI。Flourish 会去除图像外围的完全透明像素，使可见图案充分使用标题栏的 Logo 区域；同一图源也会用于 Shell 窗口图标，因此 Windows 任务栏与标题栏显示一致。

标题栏的内置工具提示和主题文本使用[应用数据](configure-data.md)中选择的语言。`SetTitle`、`SetSubTitle` 和 `SetSearch` 接收的文本由应用提供，不会自动翻译。

## 窗口命令

启用内置标题栏后，其窗口命令区域会提供最小化、最大化或还原以及关闭操作。最小化和最大化按钮保留标准的非破坏性蓝色悬停揭示与按下反馈。关闭属于破坏性操作，因此在亮色和暗色主题下都使用红色悬停与按下反馈。

鼠标点击不会在这些按钮上残留焦点边框，通过键盘导航获得焦点时仍会显示清晰的焦点指示。最大化命令遵循配置的窗口调整大小模式，关闭流程遵循[窗口](configure-window.md)配置。

## 搜索与面包屑

```csharp
titleBar
    .SetSearch("搜索", (_, searchText) => Search(searchText))
    .SetBreadcrumbButton(BreadcrumbShowOption.Auto);
```

`SetSearch` 在搜索文本变化时调用处理程序。需要解析应用服务时，可以使用接收 `IServiceProvider` 和搜索文本的重载。

`SetBreadcrumbButton` 控制面包屑按钮的显示时机：`Always` 始终显示，`Auto` 根据导航历史决定，`Hidden` 隐藏面包屑。省略参数时使用 `Auto`。

## 功能入口

```csharp
titleBar
    .SetNavToggle()
    .SetProfile(NameOrder.FirstLast)
    .SetThemeToggle(FlourishTheme.System);
```

- `SetNavToggle()` 显示导航切换按钮，并要求在 Shell 中启用 `UseNavigation()`。
- `SetProfile()` 启用默认 Profile 并显示入口。参数控制名称与占位首字母的顺序；自定义页面由[用户资料（Profile）](configure-profile.md)配置。
- `SetThemeToggle()` 启用主题功能并显示切换按钮。参数指定 Host 配置中没有已保存偏好时使用的主题；`appsettings.json` 持久化规则参见[主题](configure-themes.md)。

## 相关功能

- [自定义 Shell 内容](configure-custom-handler.md)可向标题栏预定义区域插入 WPF 内容。
- [用户资料（Profile）](configure-profile.md)配置 Profile 内容、身份验证与持久化。
- [导航](navigation.md)配置导航切换按钮所控制的导航区域。
- [主题](configure-themes.md)说明主题切换按钮控制的主题。
- [窗口](configure-window.md)配置窗口调整大小与托盘关闭行为。
