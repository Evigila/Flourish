---
title: Chunk
description: 使用 Chunk 和 ChunkHero 构建 Flourish 页面必需的全宽区块层级。
---

# Chunk

`Chunk` 是 Flourish 主导航内容页面的根布局单元。每个此类页面都以且仅以一个 `ChunkHero` 开头，随后放置一个或多个 `Chunk`。两种控件都会占满所在行，因此不要把两个区块并排放置。

> [!IMPORTANT]
> 所有页面内容都应放在开头的 `ChunkHero` 或后续 `Chunk` 中。不要在这个结构之外添加同级文本、卡片或依赖手工间距的区域。

> [!NOTE]
> 此页面骨架不适用于 Shell 自有的瞬时表面，例如 Profile 浮出层、Popup 或 Dialog。这些表面不是主导航内容页面，不应放入超大 `ChunkHero`；其内部内容仍需遵循 Flourish 的字体、间距、控件选择和 Button 家族规范。

## 基本用法

每个 `Chunk` 都需要简洁的 `Title` 和实际的 `Body`。`Description` 是可选的：仅当标题本身无法表达区块所需的关键信息时才添加描述。

`Body` 是默认 XAML 内容属性，因此可将一个子元素直接写在 `Chunk` 内；区块需要多个子元素时，请用布局容器组成一个内容树。

```xml
<flourish:Chunk
  Title="最近的项目"
  Description="继续上次的工作，或打开另一个项目。">
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
| `Description` | `string?` | `null` | 标题下方的可选补充说明。 |
| `Body` | `object?` | `null` | 必需的区块内容，也是默认 XAML 内容属性。 |
| `ChunkMargin` | `Thickness` | `0,32,0,0` | 提供与前一个页面区块之间的大间距。 |
| `ChunkSpacing` | `Thickness` | `0,12,0,0` | 提供各个已填充内部区域之间的间距。 |

可选区域为 `null` 或空字符串时，会连同相关间距一起完全折叠。普通页面应保留默认的 `ChunkMargin` 和 `ChunkSpacing`，使所有区块遵循统一的垂直节奏。

`Chunk` 只负责布局。请根据内容为 `Body` 选择 [Card](card.md)、[Paragraph](paragraph.md)、[Presenter](presenter.md) 或其他专用控件，不要让 `Chunk` 自身承担内容呈现职责。

## ChunkHero

`ChunkHero` 是页面唯一的头部区块。它继承 [Presenter](presenter.md) 的完整约定：每个声明都必须显式提供 `Title`、`Description`、`PresenterMode` 和 `PresenterPosition`，`Body` 与 `Presentation` 则位于各自规定的内容区域。更大的标题和强调背景使它区别于普通 `Presenter`。

```xml
<flourish:ChunkHero
  Title="欢迎使用 Flourish"
  Description="使用统一布局系统构建 WPF 应用程序。"
  PresenterMode="Split"
  PresenterPosition="Right">
  <flourish:ChunkHero.Body>
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
  </flourish:ChunkHero.Body>
  <flourish:ChunkHero.Presentation>
    <Image Source="Assets/flourish-hero.png" Stretch="Uniform" />
  </flourish:ChunkHero.Presentation>
</flourish:ChunkHero>
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 必需的页面标题，使用专用 HeaderSize 字号，必须显式声明。 |
| `Description` | `string?` | `null` | 页面标题必需的补充说明，必须显式声明。 |
| `Body` | `object?` | `null` | 与头部文案位于同一区域的辅助控件或内容，并继续作为 `ChunkHero` 的默认 XAML 内容属性。 |
| `Presentation` | `object?` | `null` | 图片、图标组、插图或其他展示内容。请通过 `ChunkHero.Presentation` 显式赋值。 |
| `PresenterMode` | `PresenterMode` | `Split` | 必须显式声明的组合模式；运行时回退值为 `Split`。 |
| `PresenterPosition` | `PresenterPosition` | `Right` | 必须显式声明的展示侧；运行时回退值为 `Right`。 |

头部 `Body` 或 `Presentation` 缺失时不会留下空占位或间距。标准 Split 会将标题、描述和主体固定在左侧，将展示内容放在右侧。在 `Overlay` 模式下，应选择能使全部叠加文案在浅色和深色主题中都保持可读的展示内容；即使 Overlay 会忽略 `PresenterPosition`，声明中也仍需显式提供它。

## 页面结构

标准页面骨架包含一个开头的 `ChunkHero`，随后是若干全宽 `Chunk`。每个普通区块仍必须提供实际的主体内容。

```xml
<ScrollViewer>
  <StackPanel>
    <flourish:ChunkHero
      Title="设计系统"
      Description="此应用程序的基础规范和可复用控件。"
      PresenterMode="Split"
      PresenterPosition="Right"
      Presentation="{StaticResource DesignSystemIllustration}" />

    <flourish:Chunk Title="基础规范">
      <flourish:Card MainText="颜色、字体、间距和动效。" />
    </flourish:Chunk>

    <flourish:Chunk Title="组件">
      <flourish:Card MainText="基于这些基础规范构建的可复用控件。" />
    </flourish:Chunk>
  </StackPanel>
</ScrollViewer>
```

不要添加第二个 `ChunkHero`，不要省略开头的头部区块，也不要把多个 `Chunk` 放在同一行。

## 相关内容

- [Presenter](presenter.md) 定义 `ChunkHero` 继承的展示模型。
- [Paragraph](paragraph.md) 将多段文本作为区块唯一的主体呈现。
- [Card](card.md) 在区块中呈现简洁信息。
- [Button](button.md) 定义区块和头部主体中使用的操作。
- [字体](../articles/configure-font.md) 说明六种字号层级。
- [Chunk API](xref:ArkheideSystem.Flourish.Controls.Chunk) 和 [ChunkHero API](xref:ArkheideSystem.Flourish.Controls.ChunkHero) 列出全部成员。
