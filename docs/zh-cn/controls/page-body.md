---
title: PageBody
description: 使用 PageBody 作为导航页面的严格滚动根容器，并直接排列 HeaderChunk 与 Chunk。
---

# PageBody

`PageBody` 是 Flourish 导航页面的根内容容器。它封装 Flourish [ScrollViewer](scroll-viewer.md)、纵向内容堆栈和标准页面外边距，因此页面不需要重复声明外层 ScrollViewer 与 StackPanel。

`PageBody` 的直接子项只能是 [HeaderChunk](chunk.md#headerchunk) 或 [Chunk](chunk.md)。HeaderChunk 最多出现一次，并且只能位于第一个位置。标准内容页面以一个 HeaderChunk 开头，随后放置一个或多个 Chunk。

```xml
<Page
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish">
  <flourish:PageBody>
    <flourish:HeaderChunk
      Title="报告"
      Content="查看已生成的报告。"
      PresenterMode="Split"
      PresenterPosition="Right" />

    <flourish:Chunk Title="可用报告">
      <flourish:Card Content="选择一份报告以继续。" />
    </flourish:Chunk>
  </flourish:PageBody>
</Page>
```

直接声明的 XAML 内容会加入 `Children` 集合并按声明顺序纵向排列。试图加入其他控件、将 HeaderChunk 放在后续位置，或加入第二个 HeaderChunk，都会被拒绝。卡片、Presenter、Document、布局面板及普通文本应放入 Chunk 或 HeaderChunk 的 `Body`，不能成为 PageBody 的同级子项。

## 滚动与内容居中

`PageBody` 继承 Flourish `ScrollViewer` 的行为。默认禁用水平滚动，并按需显示纵向滚动条。页面有不同需求时，可以通过继承的滚动属性覆盖这些设置。

Shell 启用 `UseCenterContent` 后，会限制并居中 PageBody 内部的内容，同时让滚动视口保持全宽。因此纵向滚动条仍位于 Shell 内容区边缘；较窄视口中的页面主体会使用可用宽度。

不要设置继承的 `Content` 属性。该属性由 PageBody 用于承载内部纵向堆栈，Flourish Shell 在应用页面宽度约束时也可能包装这个堆栈。应通过 XAML 直接内容或 `Children` 添加和移除页面区块。

## 相关功能

- [Chunk](chunk.md)定义 PageBody 内的 HeaderChunk 与 Chunk 页面层级。
- [ScrollViewer](scroll-viewer.md)说明继承的滚动行为。
- [Shell 配置](../articles/shell-configuration.md#自定义内容对齐)配置页面内容的居中宽度。
- [PageBody API](xref:ArkheideSystem.Flourish.Controls.PageBody)列出完整成员。
