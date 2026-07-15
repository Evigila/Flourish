---
title: 控件库
description: Flourish 控件库的简要介绍与专项文档入口。
---

# 控件库

Flourish 提供一组显式的 WPF 自定义控件，用于在应用页面、Shell 扩展区域、对话框与独立窗口中使用统一的主题和交互语言。它们不会改写 WPF 基础控件的默认外观；使用 Flourish 控件需要显式引用 `http://schemas.arkheide.system/flourish` XAML 命名空间。

控件的设计原则、属性、默认值和示例现在放在独立的[“控件”文档栏目](../controls/index.md)中：

- [Chunk 与 ChunkHero](../controls/chunk.md)：定义页面章节、页首焦点区及其标准间距。
- [Button 按钮家族](../controls/button.md)：包括 `Button`、`IconButton`、`CardButton` 和 `WindowCaptionButton`。
- [Card 卡片家族](../controls/card.md)：包括非交互式 `Card` 和具有展示区域的 `IconCard`。

其余现有控件仍可以按照原有 API 使用，专项文档会在对应控件完成重构后逐步补充。完整类型列表与成员签名参见 [Controls API](xref:ArkheideSystem.Flourish.Controls)。
