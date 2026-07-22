---
title: CodeSpace
description: 使用 CodeSpace 以固定代码样式呈现精确文本，并提供内置复制操作。
---

# CodeSpace

`CodeSpace` 在透明、圆角且带轻量描边的表面中呈现精确文本片段，并在右上角提供复制按钮。源代码或命令文本可能需要由读者复制时使用它。普通多段正文使用 [Paragraph](paragraph.md)，运行时输出或日志历史使用 [OutputCard](output-card.md)。

## 基本用法

通过 `Text` 指定完整片段。`CodeSpace` 不是内容容器，不能承载子控件。

```xml
<flourish:Chunk Title="示例">
  <flourish:CodeSpace Text="{Binding ExampleCode}" />
</flourish:Chunk>
```

`Text` 在显示和复制时不会被插入缩进，也不会改变换行字符。建议使用绑定、资源或属性值明确表达所需空白。长行不会自动换行，并使用内置横向滚动行为。

## 代码呈现

当前呈现是语法高亮的前置固定样式：默认 16 DIP 的 Large 字号层级、Normal 字形、Bold 字重、Consolas 字体和随主题变化的蓝色前景。字号会跟随全局或页面级 Large 设置变化。CodeSpace 不会解析语言，也不会为不同词法单元分别着色。不要使用子文本元素自行应用语言颜色或模拟高亮；专用高亮契约将在后续补充。

该表面与 Paragraph 共用透明背景、带圆角且细而低对比度的边框、内边距和小顶部外边距。当控件位于 Chunk 标题或描述之后时，应保留此外边距。

## 复制操作

右上角图标按钮会对 `CodeSpace` 调用 `ApplicationCommands.Copy`。它将完整 `Text` 值（包括前导空格和换行）复制到系统剪贴板。`Text` 为空时命令会禁用。其 ToolTip 使用共享 Tip 字体规范中的 Normal 字形和 Regular 字重，不继承代码区域的 Bold 样式。不要在控件外再添加第二个复制按钮。

## 相关内容

- [Paragraph](paragraph.md) 通过自动段落间距和首行缩进呈现多段正文。
- [OutputCard](output-card.md) 在滚动视口中呈现只追加的输出和日志。
- [Chunk](chunk.md) 定义承载 CodeSpace 的区块。
- [CodeSpace API](xref:ArkheideSystem.Flourish.Controls.CodeSpace) 列出全部继承成员和声明成员。
