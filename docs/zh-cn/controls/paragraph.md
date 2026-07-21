---
title: Paragraph
description: 使用 Paragraph 将多段文本作为一个 Chunk 主体，以统一间距和缩进呈现。
---

# Paragraph

`Paragraph` 用于呈现一系列文本段落，没有背景、边框、标题、描述或样式变体。区块包含较多的多段正文时使用它；正文只有一段时使用 [Card](card.md)。

应将 `Paragraph` 作为 `Chunk` 主体中的唯一控件。不要在同一个段落组合中混入同级卡片、按钮或其他控件。

## 基本用法

可将任意数量的 `TextBlock` 直接添加到 `Paragraph`。每个直接子 `TextBlock` 都视为一个段落。

```xml
<flourish:Chunk Title="为什么选择 Flourish">
  <flourish:Paragraph>
    <TextBlock Text="Flourish 为 WPF 应用程序提供统一的视觉基础。" />
    <TextBlock Text="它的布局控件无需重复手工边距，就能表达页面层级。" />
    <TextBlock Text="专用内容控件使呈现与交互职责保持清晰。" />
  </flourish:Paragraph>
</flourish:Chunk>
```

`Paragraph` 默认应用 Standard 字号层级，并为没有显式设置 `TextWrapping` 的 `TextBlock` 启用自动换行。

直接子 `TextBlock` 会被视为文本源，并通过专用文本代理呈现。控件会保留源 `Text` 值、常规数据上下文绑定、块级字体样式和换行设置。内联格式、依赖可视化树的相对绑定、逐项布局以及输入事件处理程序不属于该约定；需要这些行为时，请改用其他 Chunk 主体控件。

## 间距与缩进

除第一段外，每个段落之前都会应用标准段落间距。每个非空段落都会在呈现文本前准确添加四个普通空格。空格使用该段落的实际字体和字号，因此缩进宽度会跟随当前字体度量变化，并且只作用于换行后的第一行。源 `Text` 值不会被修改。不要自行添加前导空格，也不要为各项手动设置边距；这两项行为都由控件负责。

第一段之前没有垂直间距。没有内容时不会产生占位。即使 `Background`、`BorderBrush` 或 `BorderThickness` 获得继承值或本地值，`Paragraph` 也不会呈现任何表面。

> [!NOTE]
> `FlourishTextRole.Paragraph` 用于设置单个 `FlourishTextBlock` 的样式；本文所述 `Paragraph` 控件是多段落容器。需要在多个文本块之间自动应用间距与缩进时，请使用该控件。

## 相关内容

- [Chunk](chunk.md) 定义承载 Paragraph 的必需区块。
- [Card](card.md) 在卡片表面呈现标题和单段正文。
- [Presenter](presenter.md) 组合文案、控件和视觉展示内容。
- [字体](../articles/configure-font.md) 说明 Standard 默认字号。
- [Paragraph API](xref:ArkheideSystem.Flourish.Controls.Paragraph) 列出全部继承成员和声明成员。
