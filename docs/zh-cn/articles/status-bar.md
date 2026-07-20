---
title: 状态栏
description: 配置自定义状态项、后台任务指示器以及合并的系统状态入口。
---

# 状态栏

状态栏是 Shell 中用于显示活动后台任务、应用自定义状态和系统详情的紧凑区域。通过 [Shell 配置](shell-configuration.md)启用其常驻界面，再使用 `ConfigureStatusBar` 配置可选的自定义项和系统状态。

```csharp
builder
    .ConfigureShell(shell => shell.UseStatusBar())
    .ConfigureStatusBar(statusBar =>
    {
        statusBar
            .AddStatusItem("在线", "\uE774")
            .ShowLANConnectionStatus()
            .ShowPowerStatus();
    });
```

非交互式文本和图标状态使用 `AddStatusItem`；交互控件或应用定义的 WPF 内容使用[自定义 Footer 区域](configure-custom-handler.md)。

## 后台任务指示器

`IBackgroundTaskService` 会自动集成到状态栏左侧：

- 每个运行中或正在取消的任务都有独立图标
- 悬停运行图标会显示名称、可选描述、状态和进度
- 点击运行图标会打开后台任务浮层
- 等待任务共用一个不带图标和角标的纯数字计数
- 悬停或点击队列数字会打开等待列表及取消操作

Shell 从 `FlourishBackgroundTaskMetadata` 读取任务名称、描述和图标；未提供图标时使用内置任务字形。

即使省略了 `UseStatusBar()`，活动任务也会临时显示状态栏。所有活动任务结束后，状态栏恢复到配置决定的可见性。任务提交、并发上限、取消、进度和结果参见[后台任务](background-tasks.md)。

## 自定义状态项

`AddStatusItem` 添加一个包含 Small 显示文本和图标字形的紧凑非交互状态项。多个项目按注册顺序排列在系统状态图标之前。

```csharp
statusBar.AddStatusItem("在线", "\uE774");
statusBar.AddStatusItem("已同步", "\uE73E");
```

自定义项适合表示账号、工作区、同步状态或当前模式。传入的文本属于应用内容，不会自动翻译。

## 合并的网络与电源状态

`ShowLANConnectionStatus` 和 `ShowPowerStatus` 会在状态栏右侧的同一个系统状态图标中启用对应详情行。配置任一辅助方法都会显示这个图标；同时配置两者也不会产生两个独立图标。

```csharp
statusBar
    .ShowLANConnectionStatus()
    .ShowPowerStatus();
```

悬停或点击图标会打开临时 [Overlay](../controls/overlay.md)；指针同时离开图标与浮层后，它会自行关闭。网络行在浮层打开时读取当前网络可用性；电源行显示外接电源、电池供电或未知来源，并在 Windows 提供有效值时显示电池百分比。这些值是打开界面时取得的当前快照，并非持续的网络或电池监视器。

内置标签使用[应用数据](configure-data.md)中选择的语言。

## 添加自定义 Footer 内容

[自定义 Shell 内容](configure-custom-handler.md)可添加应用提供的控件和命令按钮。`FooterStart` 位于内置后台任务指示器之后；`FooterEnd` 位于自定义状态和系统状态区域之后。

```csharp
builder.ConfigureCustomHandler(custom =>
{
    custom.AddFooterCommand(
        FlourishRegion.FooterEnd,
        "同步",
        "\uE895",
        "sync.run");
});
```

自定义内容本身不会启用常驻状态栏；需要在没有活动后台任务时也保持显示，仍应调用 `UseStatusBar()`。
