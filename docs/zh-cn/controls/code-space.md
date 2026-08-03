---
title: CodeSpace
description: 使用 CodeSpace 以固定代码样式呈现精确文本，并提供内置复制操作。
---

# CodeSpace

`CodeSpace` 在透明、圆角且带轻量描边的表面中呈现精确文本片段。它默认显示为高度 72 DIP 的紧凑“View code”表面，展开后才呈现代码与操作。源代码或命令文本可能需要由读者复制时使用它。普通多段正文使用 [Document](document.md)，运行时输出或日志历史使用 [OutputCard](output-card.md)。

## 基本用法

通过 `Text` 指定完整片段。`CodeSpace` 不是内容容器，不能承载子控件。

```xml
<flourish:Chunk Title="示例">
  <flourish:CodeSpace Text="{Binding ExampleCode}" />
</flourish:Chunk>
```

`Text` 在显示和复制时不会被插入缩进，也不会改变换行字符。建议使用绑定、资源或属性值明确表达所需空白。长行不会自动换行，并使用内置横向滚动行为。

## 展开行为

`IsExpanded` 默认为 `false`，并支持双向绑定。单击折叠表面的任意区域，或在表面具有键盘焦点时按 Enter 或 Space，都会展开完整代码。自动化客户端可以使用标准 ExpandCollapse 模式；应用命令和自定义模板可以使用 `ExpandCommand` 与 `CollapseCommand` 执行相同的状态切换。

展开后，收缩按钮显示在复制按钮左侧。它会将控件恢复为高度 72 DIP 的折叠表面，不会触发复制，也不会再次展开。需要默认展开，或需要 CodeSpace 填满 Presenter 的 Presentation 区域时，请设置 `IsExpanded="True"`。

`CanCollapse` 默认为 `true`。对于必须持续展开的代码，将其设为 `false`：收缩按钮会被移除，CollapseCommand、键盘输入和 UI Automation 也不能收缩表面。`CanCollapse` 只约束用户交互；外部绑定或本地赋值仍是状态权威，可以继续将 `IsExpanded` 设置为任一状态。Gallery 的 Usage 展示同时使用 `IsExpanded="True"` 与 `CanCollapse="False"`，使说明代码始终可见。

## 代码呈现

当前呈现是语法高亮的前置固定样式：默认 16 DIP 的 Large 字号层级、Normal 字形、Bold 字重、Consolas 字体和随主题变化的蓝色前景。字号会跟随全局或页面级 Large 设置变化。CodeSpace 不会解析语言，也不会为不同词法单元分别着色。不要使用子文本元素自行应用语言颜色或模拟高亮；专用高亮契约将在后续补充。

该表面与 Document 共用透明背景、带圆角且细而低对比度的边框和内边距。`CodeSpace` 本身不添加外边距；区块之间的间距由父布局负责，因此控件可以完整填充 Presenter 的 Presentation 区域而不留下内缩空白。

## 复制操作

展开表面右上角的图标按钮会对 `CodeSpace` 调用 `ApplicationCommands.Copy`。它使用 Elevated 变体，使操作按钮在蓝色代码文字上方仍保持清晰区分。它将完整 `Text` 值（包括前导空格和换行）复制到系统剪贴板；成功后，16 DIP 图标会短暂显示为勾，然后恢复复制图标。`Text` 为空时命令会禁用。其 ToolTip 使用共享 Tip 字体规范中的 Normal 字形和 Regular 字重，不继承代码区域的 Bold 样式。不要在控件外再添加第二个复制按钮。

## 相关内容

- [Document](document.md) 通过多个 Paragraph、自动段落间距和首行缩进呈现多段正文。
- [OutputCard](output-card.md) 在滚动视口中呈现只追加的输出和日志。
- [Chunk](chunk.md) 定义承载 CodeSpace 的区块。
- [CodeSpace API](xref:ArkheideSystem.Flourish.Controls.CodeSpace) 列出全部继承成员和声明成员。
