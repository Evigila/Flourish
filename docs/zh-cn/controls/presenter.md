---
title: Presenter
description: 使用 Presenter 的 Split、TopDown 与 Overlay 模式组合文案、辅助控件和丰富展示内容。
---

# Presenter

`Presenter` 用于图片、图标、插图、预览等组合内容。Split 与 Overlay 必须全宽独占一行，只有 TopDown 允许同级 Presenter 分列；标题、单段正文和单个图标使用 [Card](card.md)。

每个 Presenter 声明都应显式提供 `Title`、`Content`、`PresenterMode` 和 `PresenterPosition`。运行时回退值是 `Split` 与 `Left`，但显式声明能够让组合方式保持清晰一致。

## 内容区域

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 展示标题。 |
| `Content` | `string?` | `null` | 标题下方的补充文案。 |
| `Body` | `object?` | `null` | 与 Title、Content 位于同一侧的辅助控件或内容。 |
| `Presentation` | `object?` | `null` | 图片、图标组、插图、预览或其他组合内容，也是默认 XAML 内容属性。 |
| `PresentationMinHeight` | `double` | `0` | 内置展示表面的最小高度；Presenter 默认样式提供 160 DIP 的基线。 |
| `PresenterMode` | `PresenterMode` | `Split` | `Split`、`TopDown` 或 `Overlay` 组合模式。 |
| `PresenterPosition` | `PresenterPosition` | `Left` | Split 模式中的 Presentation 位置。 |

可选区域为 `null` 或空字符串时会连同间距折叠。Title、Content 和 Body 左对齐且背景透明；Presentation 使用主题浅灰圆角背景，无需装饰 Border。同级实例可用相同 `PresentationMinHeight` 统一骨架，内容仍可增长；固定内容居中，可拉伸内容填满区域。

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

`TopDown` 将 Presentation 置顶，Title、Content 和 Body 在下方左对齐，适合宽幅预览和图表。各列仍可读时，同级 TopDown Presenter 可以分列。

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

叠加文案必须在亮色和暗色主题下可读；必要时在 Presentation 的 Grid 中加入对比度遮罩。

## 展示多个元素

Presentation 接受一个填满展示区的 WPF 内容树。纵向 `StackPanel` 填满横向并按内容高度居中；要保留自然宽度，设置 `HorizontalAlignment="Center"`。文本、横向 `StackPanel`、`WrapPanel`、`UniformGrid` 和固定尺寸内容按期望尺寸居中，不要用空白 Width/Height 模拟画布。填满区域的 `CodeSpace` 必须设置 `IsExpanded="True"`，否则保持 72 DIP 折叠高度。`HeaderChunk` 共用此布局契约；Overlay 内容会裁剪到圆角内，普通 Presentation 无需额外装饰 `Border`。

```xml
<flourish:Presenter
  Title="支持的格式"
  Content="查看此导出可用的格式。"
  PresenterMode="Split"
  PresenterPosition="Right">
  <UniformGrid
    Columns="3"
    HorizontalAlignment="Center"
    VerticalAlignment="Center">
    <flourish:TextBlock Role="Icon" Text="&#xE8A5;" />
    <flourish:TextBlock Role="Icon" Text="&#xE7C3;" />
    <flourish:TextBlock Role="Icon" Text="&#xE8B7;" />
  </UniformGrid>
</flourish:Presenter>
```

## HeaderChunk

[HeaderChunk](chunk.md#headerchunk) 使用相同字段和三种模式，并提供强调背景、HeaderSize 标题和页面开头语义。它始终全宽，TopDown 分列只适用于普通 Presenter；其 Split 回退值为 `Right`，文案在左、Presentation 在右。HeaderChunk 与 Chunk 同级，普通 Presenter 位于 Chunk Body。

## 相关内容

- [Chunk](chunk.md)
- [Card](card.md)
- [Document](document.md)
- [Button](button.md)
- [Presenter API](xref:ArkheideSystem.Flourish.Controls.Presenter)、[PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode) 和 [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition)
