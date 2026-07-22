---
title: Card
description: 使用 Card、ActionCard 和 OutputCard 构建职责清晰的信息表面与局部操作布局。
---

# Card

卡片是 Flourish 布局系统的基础内容表面。`Card` 呈现可选标题、单段正文和单个图标；`ActionCard` 在固定布局中增加一个局部交互区域；[OutputCard](output-card.md)用于日志与输出历史。

| 需求 | 控件 |
| --- | --- |
| 标题、单段正文或单个图标的任意组合 | `Card` |
| 文案旁边或下方需要一个局部交互控件 | `ActionCard` |
| 调试输出、日志、进度、结果或错误历史 | [OutputCard](output-card.md) |
| 整个卡片都应响应点击或命令 | [CardButton](button.md#cardbutton) |

多段连续正文使用 [Document](document.md)。图片、多个图标或组合视觉内容使用 [Presenter](presenter.md)。

## Card

`Card` 的 `Title`、`Content` 和 `Icon` 都是可选的。任一属性为 `null` 或空字符串时，对应区域及其间距会完全折叠。

```xml
<flourish:Card
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="账户状态"
  Content="你的工作区已同步。" />
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string?` | `""` | 可选卡片标题。 |
| `Content` | `string?` | `""` | 可选的单段正文。 |
| `Icon` | `string?` | `null` | 可选的单个图标字体字形。 |
| `IconPosition` | `Dock` | `Left` | 将图标放在 `Left`、`Top`、`Right` 或 `Bottom`。 |
| `Variant` | `Variant` | `Standard` | 选择卡片表面样式。 |
| `ContentHorizontalAlignment` | `HorizontalAlignment` | `Stretch` | 对齐图标与文案组合。 |
| `ContentVerticalAlignment` | `VerticalAlignment` | `Stretch` | 纵向对齐图标与文案组合。 |

`Icon` 只接受一个使用图标字体呈现的 Unicode 文本元素。图片、图标组和组合控件树应放入 `Presenter.Presentation`。`Card` 没有 `Body`，不能承载任意子控件。

### 样式变体

| 变体 | 用途 |
| --- | --- |
| `Standard` | 普通分组信息，也是默认值。 |
| `Tonal` | 使用安静中性色填充的辅助信息。 |
| `Filled` | 需要主色强强调的信息。 |
| `Elevated` | 需要与背景形成视觉分离的信息。 |

```xml
<UniformGrid Columns="2">
  <flourish:Card Variant="Standard" Title="标准" Content="普通信息" />
  <flourish:Card Variant="Tonal" Title="色调" Content="辅助信息" />
  <flourish:Card Variant="Filled" Title="填充" Content="强调信息" />
  <flourish:Card Variant="Elevated" Title="浮起" Content="独立信息" />
</UniformGrid>
```

当可用宽度足以保持文本可读时，可在一个 `Chunk` 内将 Card 排列为两列或更多列。

## ActionCard

`ActionCard` 在可选的 `Title`、`Content` 和 `Icon` 之外提供一个 `Body`。`Body` 是默认 XAML 内容属性，用于放置一个按钮、下拉框、选择框、输入框或同类局部交互控件。它不是任意组合内容区；需要复杂展示时使用 `Presenter`。

`Variant` 选择固定结构：

| `ActionCardVariant` | 布局 |
| --- | --- |
| `Horizontal` | 默认值。左侧为 Icon，中间为纵向排列的 Title 与 Content，右侧为 Body；整行垂直居中。 |
| `Vertical` | Icon、Title、Content 和 Body 从上到下排列，全部靠左。适合卡片状浮窗内容。 |

```xml
<flourish:ActionCard
  Icon="&#xE790;"
  Title="主题"
  Content="选择应用程序外观。">
  <flourish:FlourishComboBox
    Width="160"
    ItemsSource="{Binding Themes}"
    SelectedItem="{Binding Theme, Mode=TwoWay}" />
</flourish:ActionCard>

<flourish:ActionCard
  Variant="Vertical"
  Icon="&#xE8A5;"
  Title="报告"
  Content="打开完整报告并查看导出历史。">
  <flourish:Button
    Command="{Binding OpenReportCommand}"
    Content="打开报告" />
</flourish:ActionCard>
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string?` | `""` | 可选标题。 |
| `Content` | `string?` | `""` | 可选的单段辅助文案。 |
| `Icon` | `string?` | `null` | 可选的单个图标字体字形。 |
| `Body` | `object?` | `null` | 一个局部交互控件，也是默认 XAML 内容属性。 |
| `Variant` | `ActionCardVariant` | `Horizontal` | 选择横向或纵向固定结构。 |

在 `Horizontal` 模式中，标题与正文保持简洁，并在宽度不足时截断。相关 ActionCard 纵向堆叠时使用紧凑的 `FlourishActionCardPeerMargin`，使它们形成一个整体。`Vertical` 模式中的文案可以换行，常与 [Overlay](overlay.md) 组合成浮窗视图。

当交互只属于 `Body` 时使用 `ActionCard`；当整个表面应执行同一操作时使用 `CardButton`。

## 相关内容

- [Chunk](chunk.md) 定义承载卡片的页面区块。
- [Document](document.md) 呈现多个连续段落。
- [Presenter](presenter.md) 呈现图片、图标组和组合视觉内容。
- [OutputCard](output-card.md) 呈现可滚动的操作历史。
- [Button](button.md) 说明何时应让整个表面具备交互性。
- [Variant API](xref:ArkheideSystem.Flourish.Controls.Variant)、[Card API](xref:ArkheideSystem.Flourish.Controls.Card)、[ActionCardVariant API](xref:ArkheideSystem.Flourish.Controls.ActionCardVariant) 和 [ActionCard API](xref:ArkheideSystem.Flourish.Controls.ActionCard) 列出全部成员。
