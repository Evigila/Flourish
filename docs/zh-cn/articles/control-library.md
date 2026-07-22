---
title: 控件库
description: Flourish 控件库的简要介绍与专用控件文档入口。
---

# 控件库

Flourish 提供一组显式 WPF 自定义控件，用于在应用页面、Shell 扩展区域、对话框和独立窗口中建立统一的主题、布局、字体和交互语言。它们不会替换 WPF 基础控件的默认外观；通过引用 `http://schemas.arkheide.system/flourish` XAML 命名空间来选择使用 Flourish 控件。

设计指南、属性、默认值和示例位于专用的[控件文档](../controls/index.md)中：

- [PageBody](../controls/page-body.md) 提供严格的页面滚动根容器与纵向区块堆栈。
- [HeaderChunk 与 Chunk](../controls/chunk.md) 定义开头头部区块和全宽区块层级。
- [Document](../controls/document.md) 通过多个规范化 Paragraph 呈现具有统一 Large 字号、间距和缩进的正文。
- [CodeSpace](../controls/code-space.md) 呈现具有内置复制操作的精确代码文本。
- [Presenter](../controls/presenter.md) 将文案和辅助控件与图片、图标组或其他展示内容组合起来。
- [Card 家族](../controls/card.md) 包括支持可选图标的 `Card`，以及具有一个 `Body` 和 Horizontal、Vertical 结构的 `ActionCard`。
- [OutputCard](../controls/output-card.md) 将操作消息追加到使用 Small 字号的紧凑滚动视口。
- [Button 家族](../controls/button.md) 包括支持可选图标的 `Button`、`CardButton` 和 `WindowCaptionButton`。
- [DataGrid](../controls/data-grid.md)、[Overlay](../controls/overlay.md) 和 [ScrollViewer](../controls/scroll-viewer.md) 分别说明表格滚轮协作、浮层内容和滚动行为。

其他现有控件仍可通过当前 API 使用。完整类型列表和成员签名参见 [Controls API](xref:ArkheideSystem.Flourish.Controls)，页面级选择和组合规则参见[设计理念](../conception/index.md)。
