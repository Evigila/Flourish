---
title: ConfigureThemes
description: 配置 Flourish 默认主题和主题偏好行为。
---

# ConfigureThemes

`ConfigureThemes` 选择 [`ConfigureShell`](configure-shell.md) 启用 `UseThemes()` 且尚未保存用户偏好时使用的默认主题。

```csharp
builder
    .ConfigureShell(shell => shell.UseThemes())
    .ConfigureThemes(FlourishTheme.System);
```

## 细节

`FlourishTheme.System` 跟随 Windows 应用主题。`Light` 和 `Dark` 会在用户更改主题前强制指定默认主题。

启用主题后，Flourish 会将用户选择保存到应用偏好。该存储依赖 [`ConfigureData`](configure-data.md) 提供的应用标识。

标题栏主题切换按钮由 [`ConfigureTitleBar`](configure-title-bar.md) 控制。只有主题已启用且切换按钮已显示时，它才会出现。

## 相关 API

- [`ConfigureData`](configure-data.md) 提供偏好存储标识。
- [`ConfigureTitleBar`](configure-title-bar.md) 可以显示标题栏主题切换按钮。
- [`ConfigureMaterialEffect`](configure-material-effect.md) 会配合有效的亮色和暗色资源。
