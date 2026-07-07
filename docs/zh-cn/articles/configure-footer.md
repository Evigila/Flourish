---
title: ConfigureFooter
description: 配置 Flourish Footer 状态区域。
---

# ConfigureFooter

`ConfigureFooter` 配置内置 Footer 状态区域。Footer 只有在 [`ConfigureShell`](configure-shell.md) 启用 `UseFooter()` 后才会显示。

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

## 细节

`SetStatusText` 提供主要 Footer 文本。Footer 是持久的低优先级区域，因此文本应保持简短。

`AddStatusItem` 添加紧凑的图标加文本状态项。内置辅助方法可添加 LAN 连接状态和电源状态。

自定义 Footer 控件属于 [`ConfigureCustomHandler`](configure-custom-handler.md)，它可以向 `FooterStart` 和 `FooterEnd` 添加 WPF 内容或命令按钮。

## 相关 API

- [`ConfigureShell`](configure-shell.md) 拥有 `UseFooter` 开关。
- [`ConfigureCustomHandler`](configure-custom-handler.md) 添加自定义 Footer 内容和命令。
- [`Footer 状态`](status-bar.md) 提供面向流程的 Footer 概览。
