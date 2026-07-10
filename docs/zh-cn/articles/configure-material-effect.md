---
title: 材质特效
description: 为 Flourish Shell 窗口选择 Windows 系统背景材质。
---

# 材质特效

材质特效改变 Shell 窗口的系统背景。`UseMaterialEffect` 会在选择材质的同时启用该效果，不需要单独的配置步骤。

## 最小配置

```csharp
builder.ConfigureShell(shell =>
    shell.UseMaterialEffect(MaterialEffect.Mica));
```

## 材质选择

`MaterialEffect.Mica` 会在平台支持时为 Shell 窗口应用 Windows Mica 材质。页面仍可在内容区域中定义自己的 WPF 背景。

使用 `MaterialEffect.None` 可以保持不透明窗口背景，并避免依赖系统材质合成：

```csharp
builder.ConfigureShell(shell =>
    shell.UseMaterialEffect(MaterialEffect.None));
```

`MaterialEffect.None` 会禁用系统材质；省略 `UseMaterialEffect` 时，材质同样不会启用。

材质效果依赖 Windows 桌面合成能力。在不支持相应效果的平台上，应用不应依赖材质来传达状态或区分内容。

## 相关功能

- [窗口](configure-window.md)配置承载材质的 Shell 窗口。
- [主题](configure-themes.md)控制与材质配合使用的亮色和暗色资源。
