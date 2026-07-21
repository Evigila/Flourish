---
title: Presenter
description: 使用 Presenter 构建组合文案、控件和丰富展示内容的全宽 Split 或 Overlay 布局。
---

# Presenter

`Presenter` 是组合文案、辅助控件和丰富展示内容的全宽三向布局。需要呈现图片、多个图标、插图、预览或其他组合视觉内容时使用它。同一行只能放置一个 `Presenter`。

卡片只需要一个图标时使用 [IconCard](card.md#iconcard)。展示区域需要图片、图标组或自己的内容树时使用 `Presenter`。

## 内容区域

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 可选的展示标题。 |
| `Description` | `string?` | `null` | 标题下方的可选补充文案。 |
| `Body` | `object?` | `null` | 与文本处于同一文案区域的控件或辅助内容，也是默认 XAML 内容属性。 |
| `Presentation` | `object?` | `null` | 被展示的图片、图标组、插图、预览或组合内容。 |
| `PresenterMode` | `PresenterMode` | `Split` | 选择并排或叠加组合。 |
| `PresenterPosition` | `PresenterPosition` | `Right` | 在 `Split` 模式下将 `Presentation` 放在 `Left` 或 `Right`。 |

任一可选区域为空字符串或 `null` 时，都会连同相关间距一起完全折叠。`Presenter` 默认透明且无边框；仅当周围设计确实需要独立表面时，才设置继承的表面属性。

## Split 模式

`Split` 是默认模式。`PresenterPosition` 始终描述展示区域，而不是文本位置：

| 位置 | 排列 |
| --- | --- |
| `Right` | 文案和 `Body` 位于左侧，`Presentation` 位于右侧，也是默认布局。 |
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

在 `Overlay` 模式下，`Presentation` 填满控件，标题、描述和主体呈现在其上方。此时 `PresenterPosition` 没有视觉效果。

```xml
<flourish:Presenter
  MinHeight="240"
  Title="版本亮点"
  Description="了解此版本中的变化。"
  PresenterMode="Overlay">
  <flourish:Presenter.Presentation>
    <Image Source="Assets/release-highlights.png" Stretch="UniformToFill" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

应选择或组合能让叠加文本在浅色和深色主题下都保持可读的展示内容。必要时，可在分配给 `Presentation` 的 `Grid` 中组合图片与对比度遮罩。

## 展示多个元素

`Presentation` 接受一个 WPF 内容树。请用布局容器包装多个图标或视觉元素：

```xml
<flourish:Presenter Title="支持的格式">
  <flourish:Presenter.Presentation>
    <UniformGrid Columns="3">
      <flourish:FlourishTextBlock Role="Icon" Text="&#xE8A5;" />
      <flourish:FlourishTextBlock Role="Icon" Text="&#xE7C3;" />
      <flourish:FlourishTextBlock Role="Icon" Text="&#xE8B7;" />
    </UniformGrid>
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

## ChunkHero

`ChunkHero` 继承 `Presenter`，并使用包括 `Presentation` 在内的相同字段和模式。它是与 `Chunk` 同级的页面级控件，使用强调背景和专用 HeaderSize 标题，并在每个内容页面开头且仅出现一次。普通 `Presenter` 则是用于 `Chunk` 主体中的较小、透明展示布局。

## 相关内容

- [Chunk](chunk.md) 定义页面层级和专用 `ChunkHero`。
- [Card](card.md) 说明何时应使用简洁文本或单图标卡片。
- [Paragraph](paragraph.md) 呈现多段纯文本。
- [Button](button.md) 定义可放在 `Body` 中的控件。
- [Presenter API](xref:ArkheideSystem.Flourish.Controls.Presenter)、[PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode) 和 [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) 列出全部成员。
