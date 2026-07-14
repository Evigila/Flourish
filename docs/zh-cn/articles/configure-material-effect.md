---
title: 材质特效
description: 选择 Flourish Shell 窗口使用的 Windows 材质。
---

# 材质特效

`UseMaterialEffect` 为 Shell 窗口选择背景材质。

```csharp
builder.ConfigureShell(shell =>
    shell.UseMaterialEffect(MaterialEffect.Mica));
```

## 选择材质

| 值 | 行为 |
| --- | --- |
| `MaterialEffect.Mica` | 在平台支持时使用 Windows Mica。 |
| `MaterialEffect.None` | 使用不带系统材质的不透明 Shell 背景。 |

`MaterialEffect.Mica` 是默认参数。Shell 不应使用系统材质时，请传入 `MaterialEffect.None` 或省略 `UseMaterialEffect`。

如果平台不支持所选材质，Shell 仍可在没有该效果的情况下使用。应用状态和内容区分不应依赖材质是否可用。

材质应用于 Shell 窗口。内置页面宿主保持透明，使 Mica 可以连续显示在内容区域中；页面仍可在设计需要时添加局部背景。

## 相关功能

- [窗口](configure-window.md)配置承载材质的窗口。
- [主题](configure-themes.md)控制与材质配合使用的亮色和暗色资源。
