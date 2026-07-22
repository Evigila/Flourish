---
title: Presenter
description: 使用 Presenter 构建组合文案、控件和丰富展示内容的全宽 Split 或 Overlay 布局。
---

# Presenter

`Presenter` 是组合文案、辅助控件和丰富展示内容的全宽三向布局。需要呈现图片、多个图标、插图、预览或其他组合视觉内容时使用它。同一行只能放置一个 `Presenter`。

卡片只需要一个图标时使用 [IconCard](card.md#iconcard)。展示区域需要图片、图标组或自己的内容树时使用 `Presenter`。

每个 Presenter 声明都必须显式提供 `Title`、`Description`、`PresenterMode` 和 `PresenterPosition`。运行时回退值仍是 `Split` 和 `Right`，但调用位置仍需写出这两个值，使预期组合方式清晰可见。

## 内容区域

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 必需的展示标题，必须显式声明。 |
| `Description` | `string?` | `null` | 标题下方必需的补充文案，必须显式声明。 |
| `Body` | `object?` | `null` | 与文本处于同一文案区域的控件或辅助内容。请通过 `Presenter.Body` 显式赋值；它不是默认 XAML 内容属性。 |
| `Presentation` | `object?` | `null` | 被展示的图片、图标组、插图、预览或组合内容，也是默认 XAML 内容属性。 |
| `PresenterMode` | `PresenterMode` | `Split` | 必须显式声明的组合模式；运行时回退值为 `Split`。 |
| `PresenterPosition` | `PresenterPosition` | `Right` | 必须显式声明的展示侧；运行时回退值为 `Right`。 |

`Body` 缺失时会连同相关间距一起完全折叠。文案与 Body 一侧保持透明；只有 `Presentation` 区域使用随主题变化的浅层中性灰背景和共享表面圆角，从而在不为 Body 增加嵌套卡片的情况下明确区分两侧。

同一区块中纵向堆叠多个 Presenter 时，从第二个开始应用 `FlourishPresenterPeerMargin`。这样可以让 Presenter 的节奏独立于 Card 间距，也无需写死局部 Margin。

## Split 模式

`Split` 是标准模式，并始终使用固定的横向双区域布局。`Title`、`Description` 和独立的 `Body` Host 在文案区域共用同一条左侧对齐线。圆角 `Presentation` 表面填满另一区域，其内容则默认在完整表面中居中。`PresenterPosition` 始终描述展示区域，而不是文本位置：

| 位置 | 排列 |
| --- | --- |
| `Right` | 文案和 `Body` 位于左侧，`Presentation` 位于右侧，也是标准布局和运行时回退值。 |
| `Left` | `Presentation` 位于左侧，文案和 `Body` 位于右侧。 |

```xml
<flourish:Presenter
  Title="工作区概览"
  Description="查看活动并打开完整报告。"
  PresenterMode="Split"
  PresenterPosition="Right">
  <flourish:Presenter.Body>
    <flourish:Button
      Command="{Binding OpenReportCommand}"
      Content="打开报告" />
  </flourish:Presenter.Body>
  <flourish:Presenter.Presentation>
    <Image Source="Assets/workspace-overview.png" Stretch="Uniform" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

`PresenterPosition` 只接受 `Left` 和 `Right`。需要将图标放在简短卡片文案的上方、下方或两侧时，请改用 `IconCard.IconPosition`。

## Overlay 模式

在 `Overlay` 模式下，圆角 `Presentation` 表面及其根内容跨越整个控件，标题、描述和主体呈现在其上方。此时 `PresenterPosition` 没有视觉效果，但作为 Presenter 契约的一部分，声明中仍需显式提供它。

```xml
<flourish:Presenter
  MinHeight="240"
  Title="版本亮点"
  Description="了解此版本中的变化。"
  PresenterMode="Overlay"
  PresenterPosition="Right">
  <flourish:Presenter.Presentation>
    <Image Source="Assets/release-highlights.png" Stretch="UniformToFill" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

应选择或组合能让叠加文本在浅色和深色主题下都保持可读的展示内容。必要时，可在分配给 `Presentation` 的 `Grid` 中组合图片与对比度遮罩。

## 展示多个元素

`Presentation` 接受一个 WPF 内容树。请用布局容器包装多个图标或视觉元素：

```xml
<flourish:Presenter
  Title="支持的格式"
  Description="查看此导出可用的格式。"
  PresenterMode="Split"
  PresenterPosition="Right">
  <UniformGrid Columns="3">
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE8A5;" />
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE7C3;" />
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE8B7;" />
  </UniformGrid>
</flourish:Presenter>
```

直接写入的 `UniformGrid` 会赋给默认 XAML 内容属性 `Presentation`。文案侧还需要辅助控件时，始终使用显式的 `<flourish:Presenter.Body>` 元素。

## ChunkHero

`ChunkHero` 继承 `Presenter`，并采用相同的显式 `Title`、`Description`、`PresenterMode` 和 `PresenterPosition` 契约，以及相同的 `Body` 和 `Presentation` 区域。它是与 `Chunk` 同级的页面级控件，使用强调背景和专用 HeaderSize 标题，并在每个内容页面开头且仅出现一次。普通 `Presenter` 则是用于 `Chunk` 主体中的较小布局，其圆角中性表面仅属于展示区域。

## 相关内容

- [Chunk](chunk.md) 定义页面层级和专用 `ChunkHero`。
- [Card](card.md) 说明何时应使用简洁文本或单图标卡片。
- [Paragraph](paragraph.md) 呈现多段纯文本。
- [Button](button.md) 定义可放在 `Body` 中的控件。
- [Presenter API](xref:ArkheideSystem.Flourish.Controls.Presenter)、[PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode) 和 [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) 列出全部成员。
