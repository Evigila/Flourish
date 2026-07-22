---
title: Document
description: 使用 Document 组合多个规范化 Paragraph，并以统一的 Large 字号、段落间距和首行缩进呈现连续正文。
---

# Document

`Document` 用于呈现多段连续正文。它提供透明背景、圆角和细而低对比度的边框，并统一管理段落字号、间距、换行与首行缩进。正文只有一段时使用 [Card](card.md)；需要组合控件或视觉内容时使用 [Presenter](presenter.md)。

应将 Document 作为 Chunk 的唯一 Body，不要在同一正文表面中混入卡片、按钮或其他控件。

## 基本用法

Document 只接受 `Paragraph` 直接子项。每个 Paragraph 都是经过规范化的 Flourish 文本块，通过 `Text` 提供一个段落。

```xml
<flourish:Chunk Title="为什么选择 Flourish">
  <flourish:Document>
    <flourish:Paragraph Text="Flourish 为 WPF 应用程序提供统一的视觉基础。" />
    <flourish:Paragraph Text="它的布局控件无需重复手工边距，就能表达页面层级。" />
    <flourish:Paragraph Text="专用内容控件使呈现与交互职责保持清晰。" />
  </flourish:Document>
</flourish:Chunk>
```

Document 始终使用已定义的 Large 字号层级，跟随全局和页面级 Large 设置。Paragraph 默认使用 Regular 字重并自动换行。Document 会规范实际呈现字号，因此不要为单个 Paragraph 设置与正文规则冲突的本地字号。

试图把普通 `TextBlock`、按钮、卡片或其他对象作为 Document 子项会被拒绝。需要特殊交互、内联控件或任意可视化树时，应改用其他 Chunk Body 控件。

## 间距与缩进

第一段之前没有额外段落间距，后续每个 Paragraph 之前都会应用统一间距。每个非空段落在呈现时自动添加四个普通空格作为首行缩进；换行后的后续行不缩进，Paragraph 的源 `Text` 值也不会被修改。

不要自行添加前导空格或逐段 Margin。Document 已统一提供：

- 与 Chunk 标题及可选 Content 之间的外部间距；
- 圆角、细线条、低对比度的阅读表面；
- 各 Paragraph 之间的垂直间距；
- 每段首行的四空格缩进；
- Large 字号和自动换行。

没有 Paragraph 时不会产生段落占位。确实需要覆盖阅读表面时，可以在 Document 上设置 `Background`、`BorderBrush`、`BorderThickness` 与 `Padding`，但应保留轻量、低干扰的视觉层级。

## 相关内容

- [Chunk](chunk.md)定义承载 Document 的页面区块。
- [Card](card.md)在卡片表面呈现标题和单段正文。
- [CodeSpace](code-space.md)在相近的描边表面中呈现可精确复制的代码文本。
- [Presenter](presenter.md)组合文案、控件和视觉展示内容。
- [排版](../articles/configure-font.md)说明 Large 字号层级。
- [Document API](xref:ArkheideSystem.Flourish.Controls.Document) 和 [Paragraph API](xref:ArkheideSystem.Flourish.Controls.Paragraph) 列出完整成员。
