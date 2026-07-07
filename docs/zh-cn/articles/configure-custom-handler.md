---
title: ConfigureCustomHandler
description: 将自定义 WPF 元素插入 Flourish Shell 区域。
---

# ConfigureCustomHandler

`ConfigureCustomHandler` 将应用提供的 WPF 元素插入预定义 Shell 区域。它是标题栏、导航栏、工具栏、内容区和 Footer 的统一扩展插槽 API。

```csharp
builder.ConfigureCustomHandler(custom =>
{
    custom
        .SetProfileContent(() => new Button { Content = "RC" })
        .AddTitlebarAction("同步", "\uE895", "sync.run")
        .AddFooterCommand("关于", "\uE946", "app.about");
});
```

## 细节

Custom handler 配置不会自动启用 Shell 功能。所属区域必须通过 [`ConfigureShell`](configure-shell.md) 启用。例如 Footer 内容需要 `UseFooter()`，工具栏内容需要 `UseDynamicToolbar()`，标题栏内容需要 `UseTitleBar()`。

工厂重载可以接收 `IServiceProvider`，适合自定义元素需要应用服务的场景。元素工厂必须返回尚未拥有 WPF 父级的元素。

命令辅助方法使用稳定的 command key，并通过 `ICommandParser` 路由。回调辅助方法适合很小的局部行为；command key 更容易测试和本地化。

## 相关 API

- [`ConfigureTitleBar`](configure-title-bar.md) 控制内置标题栏功能。
- [`ConfigureFooter`](configure-footer.md) 配置内置 Footer 状态文本和状态项。
- [`命令解析器`](command-parser.md) 说明 command key 处理方式。
