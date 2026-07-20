---
title: 标题栏
description: 在标题栏中配置应用标识、项目选择、搜索、导航、用户资料和主题控件。
---

# 标题栏

先通过 [Shell 配置](shell-configuration.md)启用标题栏，再使用 `ConfigureTitleBar` 提供应用标识并选择其中的控件。标题本身就是下拉选择器：未启用项目模式时表示应用，启用后表示活动项目。Logo 会打开独立的信息视图来展示应用标识。

## 配置标识与控件

```csharp
builder
    .ConfigureShell(shell =>
        shell.UseTitleBar().UseMultiProject().UseNavigation())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .SetLogo(
                showApplicationTitle: true,
                showApplicationSubTitle: true,
                showProjectTitle: true)
            .SetApplicationTitle("Foobar")
            .SetApplicationSubTitle("桌面工作区")
            .SetUnnamedProjectPlaceholder("未命名项目")
            .SetSearch("搜索", (_, searchText) => UpdateSearch(searchText))
            .SetBreadcrumbButton(BreadcrumbShowOption.Auto)
            .SetNavToggle()
            .SetProfile(NameOrder.FirstLast)
            .SetThemeToggle(FlourishTheme.System);
    });
```

必须启用 `UseTitleBar()`。只有同时启用[导航](navigation.md)时，`SetNavToggle` 才会显示。`UseMultiProject` 是可选功能，默认值为 `false`。

| 方法 | 结果 |
| --- | --- |
| `SetLogo(...)` | 显示 Logo 按钮，并选择其信息视图中显示的标识字段。 |
| `SetApplicationTitle(title)` | 设置应用标题并启用标题选择器。 |
| `SetApplicationSubTitle(subTitle)` | 设置 Logo 信息视图中的应用辅助标题。 |
| `SetUnnamedProjectPlaceholder(placeholder)` | 设置未持久化或没有活动选择时使用的显示文本；默认值为 `Unnamed project`。 |
| `SetSearch(placeholder, handler)` | 显示搜索框，并在文本变化时调用处理程序。 |
| `SetBreadcrumbButton(option)` | 按所选行为显示后退和前进导航。 |
| `SetNavToggle()` | 显示导航栏切换按钮。 |
| `SetProfile(nameOrder)` | 显示 Profile 入口并选择名称顺序。 |
| `SetThemeToggle(mode)` | 显示主题控件并选择启动回退主题。 |

内置 Tooltip 与主题文字使用[应用数据](configure-data.md)中选择的语言。应用与项目名称由应用提供，不会自动翻译。

## 应用标题与项目下拉框

应用标识在会话期间保持稳定，活动项目则可以动态变化。项目模式开关同时控制标题选择器的当前标题和下拉选项。

| 项目模式 | 当前标题 | 下拉选项 |
| --- | --- | --- |
| `UseMultiProject(false)` | 应用标题 | 仅显示应用标题 |
| `UseMultiProject(true)` 且活动项目已持久化 | 活动项目名称 | 全部已注册项目与“新建项目” |
| `UseMultiProject(true)` 且活动项目未持久化或不存在 | 未命名项目占位文本 | 全部已注册项目与“新建项目” |

未启用项目模式时，该选择器不具有项目标题语义，选择唯一的应用标题不会执行项目操作。启用项目模式后，选择项目会调用 `IProjectBehavior.ActivateProjectAsync`；选择“新建项目”会调用 `CreateProjectAsync`；右键单击项目则通过 `DeleteProjectAsync` 执行删除。[项目](projects.md)说明生命周期行为、目录持久化与运行时更新。

应用副标题不直接显示在标题栏中。它与应用标题一起显示在 Logo 信息视图；`SetLogo` 也可以让该视图显示当前项目标题。判断项目是否未持久化时应使用 `StoragePath == null`，而不是占位文本。

当前标题使用配置的 Large 排版档位；下拉选项与 Logo 信息视图中的内置文本使用 Standard。参见[排版](configure-font.md)。

## Logo 信息视图

`SetLogo()` 使用 Flourish 内置图标。如需替换，可传入相对 URI、绝对 URI 或 WPF pack URI。最终图像也会设置为 Shell 窗口图标。标题栏与信息视图会保持图像宽高比，将完整图形限制在各自边界内，并且不会填充透明像素。

```csharp
titleBar.SetLogo(
    "/Foobar;component/Assets/logo.ico",
    showApplicationTitle: true,
    showApplicationSubTitle: true,
    showProjectTitle: false);
```

三个显示参数的默认值依次为 `true`、`true` 和 `false`。点击或指向 Logo 时会打开临时 [Overlay](../controls/overlay.md)；指针同时离开 Logo 与浮层后，它会自行关闭。应用可以通过 `TitlebarApplicationInfo` Shell 区域，在标识元数据下方加入 WPF Body：

```csharp
builder.ConfigureCustomHandler(custom =>
    custom.Add(
        FlourishRegion.TitlebarApplicationInfo,
        services => new ApplicationSummaryView()));
```

该 Body 完全由应用负责；Flourish 只提供承载区域，不定义其中的数据或行为。内容超出窗口范围内的信息视图时，该 Body 会垂直滚动。

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

内置标题栏提供最小化、最大化或还原以及关闭命令。最大化遵循窗口调整大小模式，关闭遵循[窗口](configure-window.md)配置。Logo、标题选择器与窗口命令支持键盘焦点；Logo 信息视图也可以通过 <kbd>Esc</kbd> 或点击外部关闭。

## 相关功能

- [项目](projects.md)管理持久化项目目录与标题栏生命周期行为。
- [自定义 Shell 内容](configure-custom-handler.md)向标题栏区域与 Logo 信息视图添加应用内容。
- [用户资料（Profile）](configure-profile.md)配置 Profile 内容、认证与持久化。
- [导航](navigation.md)提供 `SetNavToggle` 控制的导航栏。
- [主题](configure-themes.md)说明 `SetThemeToggle` 控制的主题。
- [窗口](configure-window.md)配置窗口调整大小与托盘关闭行为。
