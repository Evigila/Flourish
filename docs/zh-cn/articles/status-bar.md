---
title: 状态栏（Footer）
description: 在 Flourish Shell 底部显示主要状态、自定义状态项和内置状态项。
---

# 状态栏（Footer）

状态栏是 Shell 底部持续显示的低优先级信息区域。Flourish API 使用 Footer 表示该区域。通过 [Shell 配置](shell-configuration.md)启用 `UseFooter()`，再使用 `ConfigureFooter` 配置状态内容。

```csharp
builder
    .ConfigureShell(shell => shell.UseFooter())
    .ConfigureFooter(footer =>
    {
        footer
            .SetStatusText("就绪")
            .AddStatusItem("在线", "\uE774")
            .ShowLANConnectionStatus()
            .ShowPowerStatus();
    });
```

## 主状态文本

`SetStatusText` 设置状态栏中的主文本。

```csharp
footer.SetStatusText("就绪");
```

主状态文本适合表示稳定、简短的状态。长日志和需要用户立即处理的通知应使用其他界面承载，以免小窗口中的状态栏被截断。

## 自定义状态项

`AddStatusItem` 添加带显示文本和图标字形的紧凑状态项。

```csharp
footer.AddStatusItem("在线", "\uE774");
footer.AddStatusItem("已同步", "\uE73E");
```

状态项可以表示账号、工作区、同步结果或当前模式。调用顺序决定状态项在区域中的排列顺序。

## 内置状态项

`ShowLANConnectionStatus` 添加配置执行时的局域网可用性快照，不会自动刷新。`ShowPowerStatus` 添加静态电源项，不读取实时电池或电源来源状态。

```csharp
footer.ShowLANConnectionStatus();
footer.ShowPowerStatus();
```

需要实时监视网络或电源状态时，应通过自定义状态内容提供更新逻辑。

## 添加自定义内容

`ConfigureFooter` 提供文本和内置状态项。[自定义 Shell 内容](configure-custom-handler.md)可以在状态栏起始或结束区域添加 WPF 控件或命令按钮。

```csharp
builder.ConfigureCustomHandler(custom =>
{
    custom.AddFooterCommand("同步", "\uE895", "sync.run");
});
```

命令按钮的命令键会交给 `ICommandParser`；处理方式请参阅[命令解析器](command-parser.md)。自定义内容不会启用状态栏，因此仍需在 Shell 配置中调用 `UseFooter()`。
