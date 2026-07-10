---
title: 状态栏
description: 在 Flourish Shell 底部显示主要状态、自定义状态项和内置状态项。
---

# 状态栏

状态栏是 Shell 底部持续显示的低优先级信息区域。通过 [Shell 配置](shell-configuration.md)启用 `UseStatusBar()`，再使用 `ConfigureStatusBar` 配置状态内容。

```csharp
builder
    .ConfigureShell(shell => shell.UseStatusBar())
    .ConfigureStatusBar(statusBar =>
    {
        statusBar
            .SetStatusText("就绪")
            .AddStatusItem("在线", "\uE774")
            .ShowLANConnectionStatus()
            .ShowPowerStatus();
    });
```

## 主状态文本

`SetStatusText` 设置状态栏中的主文本。

```csharp
statusBar.SetStatusText("就绪");
```

主状态文本适合表示稳定、简短的状态。长日志和需要用户立即处理的通知应使用其他界面承载，以免小窗口中的状态栏被截断。

## 自定义状态项

`AddStatusItem` 添加带显示文本和图标字形的紧凑状态项。

```csharp
statusBar.AddStatusItem("在线", "\uE774");
statusBar.AddStatusItem("已同步", "\uE73E");
```

状态项可以表示账号、工作区、同步结果或当前模式。调用顺序决定状态项在区域中的排列顺序。

## 内置状态项

`ShowLANConnectionStatus` 添加配置执行时的局域网可用性快照，不会自动刷新。`ShowPowerStatus` 添加静态电源项，不读取实时电池或电源来源状态。

这些内置标签使用[应用数据](configure-data.md)中选择的语言。传给 `SetStatusText` 或 `AddStatusItem` 的文本属于应用内容，不会自动翻译。

```csharp
statusBar.ShowLANConnectionStatus();
statusBar.ShowPowerStatus();
```

需要实时监视网络或电源状态时，应通过自定义状态内容提供更新逻辑。

## 添加自定义内容

`ConfigureStatusBar` 提供文本和内置状态项。[自定义 Shell 内容](configure-custom-handler.md)可以在状态栏起始或结束区域添加 WPF 控件或命令按钮。

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

命令按钮的命令键会交给 `ICommandParser`；处理方式请参阅[命令解析器](command-parser.md)。自定义内容不会启用状态栏，因此仍需在 Shell 配置中调用 `UseStatusBar()`。
