---
title: 排版
description: 配置 Flourish Shell 使用的字体系列和基础字号。
---

# 排版

使用 `ConfigureFont` 可以统一设置 Flourish Shell 框架的字体系列和基础字号。它会影响标题栏、导航栏、动态工具栏、状态栏和 Shell 对话框；应用页面仍可使用自己的 WPF 样式。

## 最小配置

```csharp
builder.ConfigureFont("Microsoft YaHei UI", 14);
```

只需更改字体系列时可以省略字号，基础字号默认为 `14`。

## 字体覆盖范围

所选字体应覆盖应用显示的全部语言。包含中文与拉丁文字的界面应选择同时提供这些字形的字体；缺少字形时，WPF 会使用字体回退，实际外观可能与 Shell 的其他文字不同。

`ConfigureFont` 只设置 Shell 框架的基础排版。页面、图表或需要特殊层级的内容可以继续通过资源字典和局部样式设置字体。

## 字号约束

字号必须是有限正数。Flourish 会从基础字号派生多个 Shell 文本尺寸，因此调整该值会同时改变不同 Shell 区域的文字比例。

## 相关功能

- [窗口](configure-window.md)定义排版需要适配的可用尺寸。
- [标题栏](configure-title-bar.md)、[导航](navigation.md)和[状态栏（Footer）](status-bar.md)会显示受全局排版影响的 Shell 文本。
- [主题](configure-themes.md)提供与文字颜色和背景相关的资源。
