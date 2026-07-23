---
title: Presenter
description: 使用 Presenter 的 Split、TopDown 与 Overlay 模式组合文案、辅助控件和丰富展示内容。
---

# Presenter

`Presenter` 是用于高级呈现的布局控件。需要呈现图片、多个图标、插图、预览或其他组合视觉内容时使用它。Split 与 Overlay Presenter 必须全宽独占一行；只有 TopDown 模式允许多个同级 Presenter 分列。只需要标题、单段正文和单个图标时使用 [Card](card.md)。

每个 Presenter 声明都应显式提供 `Title`、`Content`、`PresenterMode` 和 `PresenterPosition`。运行时回退值是 `Split` 与 `Left`，但显式声明能够让组合方式保持清晰一致。

## 内容区域

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 展示标题。 |
| `Content` | `string?` | `null` | 标题下方的补充文案。 |
| `Body` | `object?` | `null` | 与 Title、Content 位于同一侧的辅助控件或内容。 |
| `Presentation` | `object?` | `null` | 图片、图标组、插图、预览或其他组合内容，也是默认 XAML 内容属性。 |
| `PresenterMode` | `PresenterMode` | `Split` | `Split`、`TopDown` 或 `Overlay` 组合模式。 |
| `PresenterPosition` | `PresenterPosition` | `Left` | Split 模式中的 Presentation 位置。 |

可选区域为 `null` 或空字符串时，对应区域及相关间距会完全折叠。Title、Content 与 Body 共用同一条左侧对齐线。文案和 Body 一侧保持透明；Presentation 区域使用随主题变化的浅灰色圆角背景并填满分配给它的空间，展示内容在该区域中居中。

`Presentation` 是默认 XAML 内容属性。为避免辅助控件进入展示区域，应始终通过显式的 `<flourish:Presenter.Body>` 属性元素设置 Body。

同一区块中纵向堆叠多个 Presenter 时，从第二个开始应用 `FlourishPresenterPeerMargin`。

## Split 模式

`Split` 使用横向双区域布局。`PresenterPosition` 描述 Presentation 所在侧：

| 位置 | 排列 |
| --- | --- |
| `Left` | Presentation 位于左侧，Title、Content 与 Body 位于右侧；这是默认布局。 |
| `Right` | Title、Content 与 Body 位于左侧，Presentation 位于右侧。 |

```xml
<flourish:Presenter
  Title="工作区概览"
  Content="查看活动并打开完整报告。"
  PresenterMode="Split"
  PresenterPosition="Left">
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

## TopDown 模式

`TopDown` 将 Presentation 放在上方，将 Title、Content 和 Body 放在下方并统一靠左。它适合宽幅预览、图表或需要在视觉内容之后继续说明和操作的布局。当每个展示与说明仍保持可读时，多个同级 TopDown Presenter 可以分列。

```xml
<flourish:Presenter
  Title="季度趋势"
  Content="比较最近四个季度的报告结果。"
  PresenterMode="TopDown"
  PresenterPosition="Right">
  <flourish:Presenter.Body>
    <flourish:Button
      Command="{Binding OpenTrendReportCommand}"
      Content="查看完整报告" />
  </flourish:Presenter.Body>
  <flourish:Presenter.Presentation>
    <Image Source="Assets/quarterly-trend.png" Stretch="Uniform" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

`PresenterPosition` 在 TopDown 中没有视觉效果，但仍建议显式设置，使 Presenter 的组合契约完整。

## Overlay 模式

`Overlay` 让 Presentation 跨越整个控件，并把 Title、Content 和 Body 呈现在其上方。`PresenterPosition` 在此模式中没有视觉效果。

```xml
<flourish:Presenter
  MinHeight="240"
  Title="版本亮点"
  Content="了解此版本中的变化。"
  PresenterMode="Overlay"
  PresenterPosition="Right">
  <flourish:Presenter.Presentation>
    <Image Source="Assets/release-highlights.png" Stretch="UniformToFill" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

应选择能让叠加文案在浅色和深色主题下都保持可读的展示内容。必要时，可在分配给 Presentation 的 Grid 中组合图片与对比度遮罩。

## 展示多个元素

Presentation 接受一个 WPF 内容树。请用布局容器包装多个图标或视觉元素：

```xml
<flourish:Presenter
  Title="支持的格式"
  Content="查看此导出可用的格式。"
  PresenterMode="Split"
  PresenterPosition="Right">
  <UniformGrid Columns="3">
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE8A5;" />
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE7C3;" />
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE8B7;" />
  </UniformGrid>
</flourish:Presenter>
```

## HeaderChunk

[HeaderChunk](chunk.md#headerchunk) 是页面级 Presenter 特化。它使用相同的字段与三种模式，但具有强调背景、HeaderSize 标题和页面开头语义。无论使用 Split、Overlay 还是 TopDown，HeaderChunk 都必须全宽独占一行；TopDown 分列例外只适用于普通 Presenter。它具有独立的 `Right` Split 回退值，使文案保持在左、Presentation 保持在右。HeaderChunk 与 Chunk 同级；普通 Presenter 则放在 Chunk 的 Body 中。

## 相关内容

- [Chunk](chunk.md)定义页面层级和专用 HeaderChunk。
- [Card](card.md)说明何时使用简洁文本或单图标卡片。
- [Document](document.md)呈现多段连续文本。
- [Button](button.md)定义可放在 Body 中的控件。
- [Presenter API](xref:ArkheideSystem.Flourish.Controls.Presenter)、[PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode) 和 [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) 列出全部成员。
