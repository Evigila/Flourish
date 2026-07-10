---
title: 自定义 Shell 内容
description: 将应用提供的 WPF 元素或命令插入 Flourish Shell 的预定义区域。
---

# 自定义 Shell 内容

Flourish Shell 为标题栏、导航栏、动态工具栏、内容区和状态栏提供预定义扩展区域。使用 `ConfigureCustomHandler` 可以把应用自己的 WPF 元素或命令放入这些区域，同时保留 Shell 的布局和生命周期管理。

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseProfile().UseFooter())
    .ConfigureCustomHandler(custom =>
    {
        custom
            .SetProfileContent(() => new Button { Content = "Foobar" })
            .AddTitlebarAction("刷新", "\uE72C", "app.refresh")
            .AddFooterCommand("帮助", "\uE946", "app.help");
    });
```

## 启用所属区域

自定义内容不会自动启用所属 Shell 功能。应先在 [Shell 配置](shell-configuration.md)中启用对应区域：状态栏内容需要 `UseFooter()`，动态工具栏内容需要 `UseDynamicToolbar()`，标题栏内容需要 `UseTitleBar()`；`SetProfileContent` 还需要 `UseProfile()`。

区域的内置行为分别参见[动态工具栏](dynamic-toolbar.md)、[状态栏（Footer）](status-bar.md)和[标题栏](configure-title-bar.md)。

## 创建 WPF 内容

元素工厂应在 Shell 请求内容时创建尚未拥有 WPF 父级的元素。需要使用应用服务时，可以选择接收 `IServiceProvider` 的工厂重载，并从依赖注入容器解析所需对象。

自定义元素仍由应用负责定义内容、绑定和可访问性语义；Shell 负责把元素放入指定区域。

## 连接命令

命令辅助方法接收稳定的命令键，并通过 `ICommandParser` 路由。命令键可以在多个 Shell 区域中复用；回调辅助方法则直接执行当前区域提供的局部行为。

```csharp
custom.AddFooterCommand("帮助", "\uE946", "app.help");
```

显示文本可以本地化，命令键则应保持稳定。命令解析器的注册和处理方式参见[命令解析器](command-parser.md)。

## 相关功能

- [标题栏](configure-title-bar.md)控制内置标题栏内容和行为。
- [动态工具栏](dynamic-toolbar.md) 配置随当前页面变化的工具栏命令。
- [状态栏（Footer）](status-bar.md)配置内置状态文本和状态项。
- [依赖注入](configure-services.md) 为自定义内容和命令解析器提供应用服务。
