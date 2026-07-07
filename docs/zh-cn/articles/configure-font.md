---
title: ConfigureFont
description: 配置 Flourish Shell 字体和基础字号。
---

# ConfigureFont

`ConfigureFont` 设置 Flourish Shell UI 使用的字体系列和基础字号。

```csharp
builder.ConfigureFont("Microsoft YaHei", 14);
```

## 细节

字体应覆盖应用显示的所有语言。中英混排的原生 WPF 应用通常选择能覆盖两类文字的 Windows 系统字体。

字号必须是有限正数。Flourish 会从基础字号派生多个 Shell 文本尺寸，因此这个值应面向重复使用的桌面工具，而不是营销式大标题。

应用页面仍然可以使用自己的 WPF 样式。`ConfigureFont` 影响的是 Flourish Shell 框架：标题栏、导航栏、工具栏、Footer 和 Shell 对话框。

## 相关 API

- [`ConfigureWindow`](configure-window.md) 控制字体需要适配的 Shell 尺寸。
- [`ConfigureTitleBar`](configure-title-bar.md)、[`ConfigureNavigation`](configure-navigation.md)、[`ConfigureFooter`](configure-footer.md) 会显示受该字体影响的 Shell 文本。
