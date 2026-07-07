---
title: ConfigureMaterialEffect
description: 配置 Flourish Shell 窗口使用的系统材质。
---

# ConfigureMaterialEffect

`ConfigureMaterialEffect` 选择 [`ConfigureShell`](configure-shell.md) 启用 `UseMaterialEffect()` 后使用的材质效果。

```csharp
builder
    .ConfigureShell(shell => shell.UseMaterialEffect())
    .ConfigureMaterialEffect(MaterialEffect.Mica);
```

## 细节

`MaterialEffect.Mica` 会在平台支持时应用 Windows Mica 材质。`MaterialEffect.None` 保持 Shell 完全不透明，并避开平台相关的合成行为。

功能开关和材质选择是分离的。即使后续调用了 `ConfigureMaterialEffect(MaterialEffect.Mica)`，`UseMaterialEffect(false)` 仍会禁用材质。

材质行为属于 Shell 窗口，而不是页面内容。页面仍可在内容框架内定义自己的 WPF 背景。

## 相关 API

- [`ConfigureWindow`](configure-window.md) 配置承载材质的窗口。
- [`ConfigureThemes`](configure-themes.md) 控制与材质一起使用的亮色和暗色资源。
