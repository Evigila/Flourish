---
title: CodeSpace
description: 使用 CodeSpace 以固定代码样式呈现精确文本，并提供内置复制操作。
---

# CodeSpace

`CodeSpace` 在透明圆角表面中呈现可复制的精确文本。默认显示为 72 DIP 高的紧凑“View code”表面，展开后显示代码与操作；多段正文使用 [Document](document.md)，运行时输出使用 [OutputCard](output-card.md)。

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

`CanCollapse` 默认为 `true`。设为 `false` 会移除收缩按钮，并禁止 CollapseCommand、键盘和 UI Automation 收缩；外部绑定或本地赋值仍可设置 `IsExpanded`。要始终展开，同时设置 `IsExpanded="True"` 与 `CanCollapse="False"`。

## 代码呈现

当前呈现是语法高亮的前置固定样式：默认 14 DIP 的 Large 字号层级、Normal 字形、Bold 字重、Consolas 字体和随主题变化的蓝色前景。字号会跟随全局或页面级 Large 设置变化。CodeSpace 不会解析语言，也不会为不同词法单元分别着色。不要使用子文本元素自行应用语言颜色或模拟高亮；专用高亮契约将在后续补充。

该表面与 Document 共用透明背景、带圆角且细而低对比度的边框和内边距。`CodeSpace` 本身不添加外边距；区块之间的间距由父布局负责，因此控件可以完整填充 Presenter 的 Presentation 区域而不留下内缩空白。

## 复制操作

展开后，右上角 Elevated 图标按钮调用 `ApplicationCommands.Copy`，将含前导空格和换行的完整 `Text` 复制到剪贴板。成功后 16 DIP 图标短暂变为勾；`Text` 为空时禁用。ToolTip 使用 Tip 的 Normal/Regular 字体，不继承代码区 Bold；无需另加复制按钮。

## 相关内容

- [Document](document.md)
- [OutputCard](output-card.md)
- [Chunk](chunk.md)
- [CodeSpace API](xref:ArkheideSystem.Flourish.Controls.CodeSpace)
