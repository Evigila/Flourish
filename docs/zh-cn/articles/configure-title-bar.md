---
title: 标题栏
description: 配置标题栏标识、搜索、导航、用户资料和主题控件。
---

# 标题栏

先通过 [Shell 配置](shell-configuration.md)启用标题栏，再使用 `ConfigureTitleBar` 选择其中的内容。元素只有在调用对应的 `Set...` 方法后才会显示。

## 配置标题栏

```csharp
builder
    .ConfigureShell(shell =>
        shell.UseTitleBar().UseNavigation())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .SetLogo()
            .SetTitle("Foobar")
            .SetSubTitle("桌面工作区")
            .SetSearch("搜索", (_, searchText) => UpdateSearch(searchText))
            .SetBreadcrumbButton(BreadcrumbShowOption.Auto)
            .SetNavToggle()
            .SetProfile(NameOrder.FirstLast)
            .SetThemeToggle(FlourishTheme.System);
    });
```

必须启用 `UseTitleBar()`。只有同时启用[导航](navigation.md)时，`SetNavToggle` 才会显示。

| 方法 | 结果 |
| --- | --- |
| `SetLogo()` 或 `SetLogo(path)` | 显示内置 Logo 或应用提供的 Logo。 |
| `SetTitle(title)` | 显示主标题。 |
| `SetSubTitle(subTitle)` | 显示辅助标题。 |
| `SetSearch(placeholder, handler)` | 显示搜索框，并在文本变化时调用处理程序。 |
| `SetBreadcrumbButton(option)` | 按所选行为显示后退和前进导航。 |
| `SetNavToggle()` | 显示导航栏切换按钮。 |
| `SetProfile(nameOrder)` | 显示 Profile 入口并选择名称顺序。 |
| `SetThemeToggle(mode)` | 显示主题控件、启用主题选择，并设置没有已保存首选项时使用的回退模式。 |

内置工具提示和主题文本使用[应用数据](configure-data.md)中选择的语言。`SetTitle`、`SetSubTitle` 和 `SetSearch` 接收的文本由应用提供，不会自动翻译。

## Logo 与窗口图标

`SetLogo()` 使用 Flourish 内置图标。如需替换，可传入相对 URI、绝对 URI 或 WPF pack URI。最终图像也会设置为 Shell 窗口图标。

```csharp
titleBar.SetLogo("/Foobar;component/Assets/logo.ico");
```

## 搜索

`SetSearch` 接收占位文本和文本变化处理程序。处理程序会收到应用的 `IServiceProvider` 和当前搜索文本。

```csharp
builder.ConfigureTitleBar(titleBar =>
{
    titleBar.SetSearch("搜索", (services, searchText) =>
    {
        services.GetRequiredService<SearchCoordinator>().Update(searchText);
    });
});
```

## 后退与前进导航

`SetBreadcrumbButton` 接受 `BreadcrumbShowOption`：

| 值 | 行为 |
| --- | --- |
| `Always` | 标题栏可见时显示这些控件。 |
| `Auto` | 导航服务可以后退或前进时显示这些控件。 |
| `Hidden` | 隐藏这些控件。 |

省略参数时使用 `Auto`。

## Profile 与主题入口

`SetProfile` 显示 Profile 入口，并选择名称和首字母的顺序。[用户资料（Profile）](configure-profile.md)说明登录行为与自定义页面。

`SetThemeToggle` 显示主题切换按钮，并选择 Host 配置中没有已保存偏好时使用的主题。[主题](configure-themes.md)说明跟随系统与偏好持久化。

## 窗口命令

内置标题栏提供最小化、最大化或还原以及关闭命令。最大化遵循窗口调整大小模式，关闭遵循[窗口](configure-window.md)配置。通过键盘导航到这些命令时会显示焦点指示。

## 相关功能

- [自定义 Shell 内容](configure-custom-handler.md)向标题栏区域添加应用内容。
- [用户资料（Profile）](configure-profile.md)配置 Profile 内容、认证与持久化。
- [导航](navigation.md)提供 `SetNavToggle` 控制的导航栏。
- [主题](configure-themes.md)说明 `SetThemeToggle` 控制的主题。
- [窗口](configure-window.md)配置窗口调整大小与托盘关闭行为。
