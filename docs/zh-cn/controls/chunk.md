---
title: Chunk
description: 使用 Chunk 与 ChunkHero 建立 Flourish 页面的章节、页首焦点区与标准间距。
---

# Chunk

`Chunk` 是 Flourish 页面的根布局单元。它像文章的章节一样，将一组相关内容组织为标题、可选简介与主体，并统一章节内外的间距。

> [!IMPORTANT]
> 页面内容应位于 `Chunk` 中。`ChunkHero` 是唯一可以与 `Chunk` 平级的内容控件，并且只应作为页面最上方的首焦区。

`Chunk` 不是卡片。它定义内容的层级与排列规则；当章节内容需要卡片表面、列表或其他布局时，将相应控件放入 `ChunkBody`。

## 基本用法

`ChunkBody` 是 `Chunk` 的默认 XAML 内容属性，因此单个子元素可以直接写在标签内。需要放置多个子元素时，使用 `StackPanel`、`Grid` 或其他容器作为该单一内容树的根。

```xml
<flourish:Chunk
  ChunkTitle="最近项目"
  ChunkDescription="继续上次工作，或打开其他项目。">
  <UniformGrid Columns="2">
    <flourish:CardButton
      Command="{Binding OpenProjectCommand}"
      Content="最后编辑于今天"
      Title="Flourish" />
    <flourish:CardButton
      Command="{Binding BrowseCommand}"
      Content="从磁盘中选择项目"
      Title="打开其他项目" />
  </UniformGrid>
</flourish:Chunk>
```

### Chunk 属性

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `ChunkTitle` | `string` | `""` | 章节标题。每个章节应设置能描述本节内容的简洁标题。 |
| `ChunkDescription` | `string?` | `null` | 标题下的可选辅助说明。 |
| `ChunkMargin` | `Thickness` | `0,32,0,0` | 建立当前章节与前一章节或页面顶部的默认分隔。 |
| `ChunkSpacing` | `Thickness` | `0,12,0,0` | 作为 `ChunkDescription` 和 `ChunkBody` 的 `Margin`，分别建立“标题 → 简介”和“前一个标头元素 → 主体”的间距。 |
| `ChunkBody` | `object?` | `null` | 章节的内容树；也是默认 XAML 内容属性。 |

通常应保留 `ChunkMargin` 和 `ChunkSpacing` 的默认值，让相邻章节共享一套垂直节奏。仅当整个页面的信息密度需要另一套明确规则时，才应成组覆盖这两个属性。

## ChunkHero

`ChunkHero` 是页面的首焦章节。它将标题、可选简介和操作区与一个展示区组合。展示区不限于图片：它可以承载 `Image`、纯色 `Border`、插图、动态预览或其他任意内容。未设置 `Presenter` 时，文字区会自动跨越全部两列，并且仍使用默认主题 Hero 背景。

`ChunkHeroBody` 也是默认 XAML 内容属性。只有主体时可以将子元素直接写在 `ChunkHero` 中；同时设置主体与展示区时，显式使用 `ChunkHero.ChunkHeroBody` 和 `ChunkHero.Presenter` 属性元素会更清晰。

```xml
<flourish:ChunkHero
  ChunkHeroDescription="在一个稳定的布局系统中构建 WPF 应用。"
  PresenterMode="Split"
  PresenterPosition="Right"
  ChunkHeroTitle="欢迎使用 Flourish">
  <flourish:ChunkHero.ChunkHeroBody>
    <StackPanel Orientation="Horizontal">
      <flourish:Button
        Variant="Filled"
        Command="{Binding CreateProjectCommand}"
        Content="创建项目" />
      <flourish:Button
        Command="{Binding OpenDocumentationCommand}"
        Content="阅读文档" />
    </StackPanel>
  </flourish:ChunkHero.ChunkHeroBody>
  <flourish:ChunkHero.Presenter>
    <Border Background="{DynamicResource FlourishPrimarySurfaceBrush}" />
  </flourish:ChunkHero.Presenter>
</flourish:ChunkHero>
```

### ChunkHero 属性

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `ChunkHeroTitle` | `string` | `""` | 首焦区标题。 |
| `ChunkHeroDescription` | `string?` | `null` | 标题下的可选简介。 |
| `ChunkHeroBody` | `object?` | `null` | 与首焦信息相关的操作或其他辅助内容。 |
| `PresenterMode` | `PresenterMode` | `Split` | 选择独立展示区域，或让展示内容在文字后方铺满首焦区。 |
| `PresenterPosition` | `PresenterPosition` | `Right` | 在 `Split` 模式下设置 `Presenter` 的位置；`ChunkHero` 仅接受 `Left` 与 `Right`。 |
| `Presenter` | `object?` | `null` | 首焦区的图片、色块或其他展示内容。 |
| `Margin` | 继承的 `Thickness` | `0,32,0,0` | 保持标准页面顶部留白；第一个普通 `Chunk` 提供后续分隔。 |

`PresenterMode` 与 `PresenterPosition` 始终描述展示内容，而不是文字区域：

| 设置 | 排列 |
| --- | --- |
| `PresenterMode="Split"`、`PresenterPosition="Right"` | `Presenter` 在右侧，文字和 `ChunkHeroBody` 在左侧；这是默认设置。 |
| `PresenterMode="Split"`、`PresenterPosition="Left"` | `Presenter` 在左侧，文字和 `ChunkHeroBody` 在右侧。 |
| `PresenterMode="Overlay"` | `Presenter` 铺满背景，文字和 `ChunkHeroBody` 叠加在其上；`PresenterPosition` 被忽略。 |

使用 `Overlay` 时，应选择能在亮色与暗色主题下都保持文字可读的展示内容。如果一张图片需要额外的对比度处理，可以先在 `Presenter` 内用 `Grid` 组合图片与半透明色层。

## 页面结构

一个典型页面由可选的一个 `ChunkHero` 和若干个 `Chunk` 组成：

```xml
<ScrollViewer>
  <StackPanel>
    <flourish:ChunkHero
      ChunkHeroTitle="设计系统"
      PresenterMode="Split"
      PresenterPosition="Right" />

    <flourish:Chunk ChunkTitle="基础" />
    <flourish:Chunk ChunkTitle="组件" />
    <flourish:Chunk ChunkTitle="实践" />
  </StackPanel>
</ScrollViewer>
```

不要在一个页面中放置多个 `ChunkHero`，也不要用平级的自由文本和手工 `Margin` 代替章节。如果页面不需要首焦区，直接从第一个 `Chunk` 开始。

## 相关内容

- [Button](button.md)说明如何在 `ChunkBody` 和 `ChunkHeroBody` 中表达操作。
- [Card](card.md)说明如何在 `ChunkBody` 内组织信息。
- [排版](../articles/configure-font.md)说明全局和页面字体配置。
- [Chunk API](xref:ArkheideSystem.Flourish.Controls.Chunk)、[ChunkHero API](xref:ArkheideSystem.Flourish.Controls.ChunkHero)、[PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode) 与 [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition)列出完整成员。
