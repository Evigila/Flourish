---
title: ConfigureShell
description: 启用或禁用 Flourish Shell 高层功能。
---

# ConfigureShell

`ConfigureShell` 控制高层 Shell 功能开关。每个 `Use...` 方法只接收 `enabled`，默认值为 `true`。

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseTitleBar()
        .UseNavigation()
        .UseDynamicToolbar()
        .UseProfile()
        .UseTips()
        .UseMotion()
        .UseMaterialEffect()
        .UseThemes()
        .UseFooter();
});
```

## 优先级

`ConfigureShell` 拥有最高优先级。详细配置回调可以记录选项，但对应 Shell 功能只有在这里启用后才会显示。

例如，[`ConfigureFooter`](configure-footer.md) 可以设置状态文本和 Footer 项，但没有 `UseFooter()` 时 Footer 仍然隐藏。[`ConfigureNavigation`](configure-navigation.md) 可以定义分组和宽度，但没有 `UseNavigation()` 时导航栏仍然隐藏。[`ConfigureProfile`](configure-profile.md) 可以设置默认用户和页面，但只有同时启用 `UseTitleBar()` 与 `UseProfile()` 后才会显示入口。

## 相关 API

- [`ConfigureTitleBar`](configure-title-bar.md)、[`ConfigureProfile`](configure-profile.md)、[`ConfigureNavigation`](configure-navigation.md)、[`ConfigureFooter`](configure-footer.md) 配置由 Shell 开关启用的区域。
- [`ConfigureMotion`](configure-motion.md)、[`ConfigureTips`](configure-tips.md)、[`ConfigureMaterialEffect`](configure-material-effect.md)、[`ConfigureThemes`](configure-themes.md) 配置由 Shell 开关启用的行为。
- [`Shell 配置`](shell-configuration.md) 提供完整示例。
