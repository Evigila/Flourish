---
title: ConfigureDynamicToolbar
description: 配置按页面变化的动态工具栏命令。
---

# ConfigureDynamicToolbar

`ConfigureDynamicToolbar` 注册随当前页面变化的工具栏项。工具栏区域只有在 [`ConfigureShell`](configure-shell.md) 启用 `UseDynamicToolbar()` 后才会显示。

```csharp
builder
    .ConfigureShell(shell => shell.UseDynamicToolbar())
    .ConfigureDynamicToolbar(toolbar =>
    {
        toolbar.CreateToolbarItems<HomePage>(
            new FlourishToolbarItem("打开", "\uE8E5", "home.open"),
            new FlourishToolbarItem("保存", "\uE74E", "home.save"));
    });
```

## 细节

工具栏项绑定到页面类型。导航显示该页面时，Flourish 会用匹配的项替换当前工具栏内容。

每个项可以包含显示文本、图标字形和 command key。command key 会路由到 `ICommandParser`，因此工具栏动作可以复用导航项和自定义区域命令的同一套命令基础设施。

`icon: false` 重载可创建纯文本工具栏项，适合图标不足以表达语义的命令组。

## 相关 API

- [`ConfigureNavigation`](configure-navigation.md) 决定哪些已注册页面可以成为当前页面。
- [`ConfigureCustomHandler`](configure-custom-handler.md) 可在工具栏起始或结束区域添加自定义内容。
- [`动态工具栏`](dynamic-toolbar.md) 提供更完整的流程示例。
