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

## 搜索与面包屑

```csharp
titleBar
    .SetSearch("搜索", searchText => Search(searchText))
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
- `SetThemeToggle()` 启用主题功能并显示切换按钮。参数指定没有已保存偏好时使用的主题；主题存储要求参见[主题](configure-themes.md)。

## 相关功能

- [窗口](configure-window.md)配置尺寸、任务栏显示和托盘关闭行为。
- [自定义 Shell 内容](configure-custom-handler.md)可向标题栏预定义区域插入 WPF 内容。
- [导航](navigation.md)配置导航切换按钮所控制的导航区域。
