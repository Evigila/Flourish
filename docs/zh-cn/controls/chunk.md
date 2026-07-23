---
title: Chunk
description: 使用 PageBody、HeaderChunk 和 Chunk 建立 Flourish 页面必需的全宽区块层级。
---

# Chunk

`HeaderChunk` 与 `Chunk` 是 Flourish 页面布局的两个基础元素。它们应作为 [PageBody](page-body.md) 的直接子项并各自占满一行。标准内容页面以一个 `HeaderChunk` 开头，随后放置一个或多个 `Chunk`。

> [!IMPORTANT]
> 不要在 `PageBody` 下直接放置文本、卡片、面板或 Presenter。所有实际内容都应位于 HeaderChunk 或 Chunk 中；同一页面不得出现第二个 HeaderChunk。

Shell 自有的 Profile 浮出层、Popup 和 Dialog 不是导航内容页面，不需要加入完整 PageBody 或超大的 HeaderChunk。它们仍应遵循 Flourish 的字体、间距、控件选择和交互规范。

## Chunk

每个 `Chunk` 都需要简洁的 `Title` 和实际的 `Body`。`Content` 是可选补充文案：只有标题不足以覆盖关键信息时才添加。

`Body` 是默认 XAML 内容属性。需要放置多个内容元素时，应先用布局容器组成一个内容树。

```xml
<flourish:Chunk
  Title="最近的项目"
  Content="继续上次的工作，或打开另一个项目。">
  <UniformGrid Columns="2">
    <flourish:CardButton
      Command="{Binding OpenProjectCommand}"
      Content="今天最后编辑"
      Title="Flourish" />
    <flourish:CardButton
      Command="{Binding BrowseCommand}"
      Content="从磁盘选择项目"
      Title="打开另一个项目" />
  </UniformGrid>
</flourish:Chunk>
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 必需的区块标题，用于指出区块主题。 |
| `Content` | `string?` | `null` | 标题下方的可选补充说明。 |
| `Body` | `object?` | `null` | 必需的实际内容，也是默认 XAML 内容属性。 |
| `ChunkMargin` | `Thickness` | 主题默认值 | 提供与前一个页面区块之间的大间距。 |
| `ChunkSpacing` | `Thickness` | 主题默认值 | 提供各个已填充内部区域之间的间距。 |

可选区域为 `null` 或空字符串时，会连同相关间距完全折叠。`Chunk` 只负责布局；请为 `Body` 选择 [Card](card.md)、[Document](document.md)、[Presenter](presenter.md) 或其他专用内容控件。

## HeaderChunk

`HeaderChunk` 是页面开头的头部区块，也是具有更大标题与强调背景的特殊 [Presenter](presenter.md)。它采用 `Title`、`Content`、`Body`、`Presentation`、`PresenterMode` 与 `PresenterPosition` 的展示模型。

每个 HeaderChunk 声明都应显式提供 `Title`、`Content`、`PresenterMode` 和 `PresenterPosition`。与普通 Presenter 不同，HeaderChunk 的默认 XAML 内容会赋给 `Body`；展示内容应通过 `HeaderChunk.Presentation` 显式赋值。

```xml
<flourish:HeaderChunk
  Title="欢迎使用 Flourish"
  Content="使用统一布局系统构建 WPF 应用程序。"
  PresenterMode="Split"
  PresenterPosition="Right">
  <flourish:HeaderChunk.Body>
    <StackPanel Orientation="Horizontal">
      <flourish:Button
        Variant="Filled"
        Command="{Binding CreateProjectCommand}"
        Content="创建项目" />
      <flourish:Button
        Margin="12,0,0,0"
        Command="{Binding OpenDocumentationCommand}"
        Content="阅读文档" />
    </StackPanel>
  </flourish:HeaderChunk.Body>
  <flourish:HeaderChunk.Presentation>
    <Image Source="Assets/flourish-header.png" Stretch="Uniform" />
  </flourish:HeaderChunk.Presentation>
</flourish:HeaderChunk>
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 页面标题，使用专用 HeaderSize 字号。 |
| `Content` | `string?` | `null` | 标题下方的补充文案。 |
| `Body` | `object?` | `null` | 与文案位于同一区域的辅助控件，也是默认 XAML 内容属性。 |
| `Presentation` | `object?` | `null` | 图片、图标组、插图或其他展示内容。 |
| `PresenterMode` | `PresenterMode` | `Split` | `Split`、`TopDown` 或 `Overlay` 组合模式。 |
| `PresenterPosition` | `PresenterPosition` | `Right` | Split 模式中的展示侧。 |

`Split` 默认将文案与 Body 放在左侧、Presentation 放在右侧；`PresenterPosition="Left"` 会交换两侧。`TopDown` 将 Presentation 放在上方，并把 Title、Content 与 Body 放在下方靠左。`Overlay` 将文案与 Body 叠加在 Presentation 上方。HeaderChunk 在任何模式下都必须全宽独占一行，包括 TopDown；普通 Presenter 的 TopDown 分列例外不适用于 HeaderChunk。TopDown 与 Overlay 不使用 `PresenterPosition` 的视觉位置，但仍建议显式声明完整展示契约。

## 页面结构

`PageBody` 封装页面滚动和标准内容间距，并限制直接子项。页面不需要再重复声明外层 ScrollViewer 与 StackPanel。

```xml
<flourish:PageBody>
  <flourish:HeaderChunk
    Title="设计系统"
    Content="此应用程序的基础规范和可复用控件。"
    PresenterMode="Split"
    PresenterPosition="Right"
    Presentation="{StaticResource DesignSystemIllustration}" />

  <flourish:Chunk Title="基础规范">
    <flourish:Card Content="颜色、字体、间距和动效。" />
  </flourish:Chunk>

  <flourish:Chunk Title="组件">
    <flourish:Card Content="基于这些基础规范构建的可复用控件。" />
  </flourish:Chunk>
</flourish:PageBody>
```

## 相关内容

- [PageBody](page-body.md)定义页面根容器及其直接子项限制。
- [Presenter](presenter.md)定义 HeaderChunk 使用的展示模型。
- [Document](document.md)将多段文本作为区块唯一主体呈现。
- [Card](card.md)在区块中呈现简洁信息。
- [Button](button.md)定义区块和头部主体中使用的操作。
- [字体](../articles/configure-font.md)说明六种字号层级。
- [Chunk API](xref:ArkheideSystem.Flourish.Controls.Chunk) 和 [HeaderChunk API](xref:ArkheideSystem.Flourish.Controls.HeaderChunk) 列出全部成员。
