---
title: Card
description: 使用 Card、IconCard、ListCard 和 OutputCard 构建职责单一、不可嵌套的内容表面。
---

# Card

卡片是 Flourish 布局系统最基础的内容表面。普通 `Card` 呈现可选的 `Title` 和一段可选的 `MainText`。任一区域为空字符串或 `null` 时，都不会产生间距。

> [!IMPORTANT]
> `Card`、`IconCard` 和 `ListCard` 都没有通用 `Body`，不能承载任意嵌套内容。需要组合式展示内容时使用 [Presenter](presenter.md)；整个表面都是操作时使用 [CardButton](button.md#cardbutton)。

## 选择卡片

| 需求 | 控件 |
| --- | --- |
| 一个标题和一段正文 | `Card` |
| 一个标题、一段正文和一个可自由定位的图标 | `IconCard` |
| 左侧图标、中间纵向文案、右侧一个操作的紧凑行 | `ListCard` |
| 调试输出、日志、进度、结果或错误 | [OutputCard](output-card.md) |

内容包含多个段落时使用 [Paragraph](paragraph.md)，不要使用 `Card`。需要图片、多个图标或任意组合视觉内容时使用 [Presenter](presenter.md)。

## Card

```xml
<flourish:Card
  Title="账户状态"
  MainText="你的工作区已同步。" />
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 可选的卡片标题。 |
| `MainText` | `string` | `""` | 标题下方可选的单段正文。 |
| `Variant` | `Variant` | `Standard` | 选择表面样式。 |
| `ContentHorizontalAlignment` | `HorizontalAlignment` | `Stretch` | 水平对齐完整文案组。 |
| `ContentVerticalAlignment` | `VerticalAlignment` | `Stretch` | 垂直对齐完整文案组。 |

`Title` 和 `MainText` 相互独立。缺少任意一项时，对应呈现器及相关间距会完全折叠。因此只包含 `MainText` 的卡片仍然有效，但没有任何文案的卡片没有可呈现的内容。

### 样式变体

| 变体 | 用途 |
| --- | --- |
| `Standard` | 普通分组信息，也是默认值。 |
| `Tonal` | 使用安静中性色填充的辅助信息。 |
| `Filled` | 需要主色强强调的信息。 |
| `Elevated` | 需要与背景形成视觉分离的信息。 |

```xml
<UniformGrid Columns="2">
  <flourish:Card Variant="Standard" Title="标准" MainText="普通信息" />
  <flourish:Card Variant="Tonal" Title="色调" MainText="辅助信息" />
  <flourish:Card Variant="Filled" Title="填充" MainText="强调信息" />
  <flourish:Card Variant="Elevated" Title="浮起" MainText="独立信息" />
</UniformGrid>
```

当可用宽度足以保持文本可读时，可在一个 `Chunk` 内将卡片排列为两列或更多列。

## IconCard

`IconCard` 在 `Card` 约定上增加且只增加一个 `Icon`。`IconPosition` 使用 WPF `Dock` 的 `Left`、`Top`、`Right` 和 `Bottom` 值，默认为 `Left`。图标与文案共同构成一个可自由对齐的卡片组合。

```xml
<flourish:IconCard
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="报告"
  MainText="查看已生成的报告和最近的导出。" />
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Icon` | `string?` | `null` | 卡片呈现的单个图标字体字形。 |
| `IconPosition` | `Dock` | `Left` | 将图标放在 `Left`、`Top`、`Right` 或 `Bottom`。 |

`Icon` 只接受一个使用已配置图标字体呈现的 Unicode 文本元素。图片、图标组和组合控件树会被拒绝；这些内容应放入 `Presenter.Presentation`。缺少图标时，图标区域及其间距会一起折叠。

`IconCard` 从 `Card` 继承 `Title`、`MainText`、`Variant` 和文案对齐属性。它没有 `Body`、`Presentation`、`PresenterMode` 或叠加布局。

## ListCard

`ListCard` 表示一个紧凑且独立的设置或局部操作。布局固定为：可选 `Icon` 位于左侧，`Title` 和 `MainText` 在中间纵向排列，`ActionBody` 位于右侧。整行垂直居中并整体靠左。

```xml
<flourish:ListCard
  Icon="&#xE790;"
  Title="主题"
  MainText="选择应用程序外观。">
  <flourish:FlourishComboBox
    Width="160"
    ItemsSource="{Binding Themes}"
    SelectedItem="{Binding Theme, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
</flourish:ListCard>
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Icon` | `string?` | `null` | 固定左侧区域中的可选单个图标字体字形。 |
| `Title` | `string` | `""` | 可选、简洁的单行设置标题。 |
| `MainText` | `string` | `""` | 可选、简洁的单行描述。 |
| `ActionBody` | `object?` | `null` | 右侧操作区中的一个局部交互控件，也是默认 XAML 内容属性。 |
| `Variant` | `Variant` | `Standard` | 始终强制为 `Standard`；`ListCard` 没有其他表面变体。 |

`Title` 和 `MainText` 都限制为单行并在溢出时显示省略号，因此应保持简短。`ActionBody` 应只包含一个按钮、下拉框、选择框、文本框、单选按钮或同类局部控件，不要放入包含多个操作的面板。变更应立即生效，不要额外添加“应用”按钮。

将相关 ListCard 堆叠在单列中，并在各行之间使用紧凑的 `FlourishListCardPeerMargin`，使它们形成一个整体。不要在该列中穿插其他卡片类型。如果这些设置会产生操作历史，可在相邻列放置 `OutputCard`。

## 相关内容

- [Chunk](chunk.md) 定义承载卡片的页面区块。
- [Paragraph](paragraph.md) 以无卡片表面的方式呈现多个段落。
- [Presenter](presenter.md) 呈现图片、图标组和组合视觉内容。
- [OutputCard](output-card.md) 呈现可滚动的操作历史。
- [Button](button.md) 说明何时应让表面具备交互性。
- [Variant API](xref:ArkheideSystem.Flourish.Controls.Variant)、[Card API](xref:ArkheideSystem.Flourish.Controls.Card)、[IconCard API](xref:ArkheideSystem.Flourish.Controls.IconCard) 和 [ListCard API](xref:ArkheideSystem.Flourish.Controls.ListCard) 列出全部成员。
