---
title: Card
description: 使用 Card 与 IconCard 在适配主题的非交互式表面上组织信息，并按需加入视觉展示内容。
---

# Card

`Card` 是用于组织相关信息的非交互式表面。它内置标题和辅助文本区域，也可以承载任意 WPF 内容，并会随当前 Flourish 主题调整颜色。

> [!IMPORTANT]
> 如果点击表面任意位置都会执行同一个操作，请改用 `CardButton`。不要给 `Card` 添加鼠标事件来模拟按钮行为。

## 基本用法

普通信息卡片直接设置 `Title` 和 `Text`，无需再为这两个角色手动构造文本控件。

```xml
<flourish:Card
  Title="账户状态"
  Text="工作区已完成同步。" />
```

`Card` 派生自 `ContentControl`，因此还可以承载一个任意内容树。需要多个子元素时，使用 `Grid`、`StackPanel` 或其他布局容器作为根。

```xml
<flourish:Card
  Title="存储空间"
  Text="管理与当前工作区关联的文件。">
  <StackPanel Margin="0,12,0,0">
    <ProgressBar Maximum="100" Value="64" />
    <flourish:Button
      Margin="0,12,0,0"
      HorizontalAlignment="Left"
      Content="查看文件" />
  </StackPanel>
</flourish:Card>
```

### Card 属性

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Variant` | `CardVariant` | `Standard` | 选择具有语义的表面样式。 |
| `Title` | `string` | `""` | 卡片标题。 |
| `Text` | `string` | `""` | 与标题一同显示的可选辅助文本。 |
| `ContentHorizontalAlignment` | `HorizontalAlignment` | `Stretch` | 控制内置 `Title` 与 `Text` 区域的水平位置。 |
| `ContentVerticalAlignment` | `VerticalAlignment` | `Stretch` | 控制内置 `Title` 与 `Text` 区域的垂直位置。 |
| `Content` | 继承的 `object?` | `null` | 可选的任意 WPF 内容树。 |
| `HorizontalContentAlignment` | 继承的 `HorizontalAlignment` | `Stretch` | 控制任意 `Content` 的水平位置，不影响 `Title` 或 `Text`。 |
| `VerticalContentAlignment` | 继承的 `VerticalAlignment` | `Stretch` | 控制任意 `Content` 的垂直位置，不影响 `Title` 或 `Text`。 |

## 变种

`CardVariant` 有四种取值：

| 变种 | 用途 |
| --- | --- |
| `Standard` | 默认表面，用于普通的分组信息。 |
| `Tonal` | 淡灰色中性色表面，用于强调程度更低的辅助信息。 |
| `Filled` | 蓝色色调表面，用于需要更强视觉强调的信息。 |
| `Elevated` | 抬升表面，用于需要和背景保持清晰层次的信息。 |

```xml
<UniformGrid Columns="2">
  <flourish:Card Variant="Standard" Title="Standard" Text="普通信息" />
  <flourish:Card Variant="Tonal" Title="Tonal" Text="辅助信息" />
  <flourish:Card Variant="Filled" Title="Filled" Text="强调信息" />
  <flourish:Card Variant="Elevated" Title="Elevated" Text="需要分隔的信息" />
</UniformGrid>
```

`Filled` 与 `Button` 使用同一套主要填充颜色。需要其他填充色时设置本地 `Background`；本地值的优先级高于变种默认值。如果替换色也需要适配亮色与暗色主题，请使用动态主题资源。

```xml
<flourish:Card
  Variant="Filled"
  Background="{DynamicResource FlourishSecondaryBrush}"
  Foreground="{DynamicResource FlourishForegroundOnSecondaryBrush}"
  Title="自定义填充表面"
  Text="此替换颜色会跟随当前主题。" />
```

## 对齐文字区域

`ContentHorizontalAlignment` 与 `ContentVerticalAlignment` 将内置标题和文本作为一个整体定位，不会改变展示内容或任意 `Content` 的位置。

```xml
<flourish:Card
  MinHeight="160"
  ContentHorizontalAlignment="Center"
  ContentVerticalAlignment="Center"
  Title="居中文字"
  Text="标题和文本始终属于同一个对齐区域。" />
```

## IconCard

`IconCard` 与 `Card` 具有相同的标题、文本、内容、对齐和变种约定，并通过 `Presenter` 承载图标、图片、插图或其他任意 WPF 内容。

```xml
<flourish:IconCard
  PresenterMode="Split"
  PresenterPosition="Left"
  Title="报告"
  Text="查看生成的报告与最近导出。">
  <flourish:IconCard.Presenter>
    <TextBlock
      FontFamily="Segoe Fluent Icons"
      FontSize="32"
      Text="&#xE8A5;" />
  </flourish:IconCard.Presenter>
</flourish:IconCard>
```

### IconCard 属性

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Presenter` | `object?` | `null` | 图标、图片、插图或其他视觉内容。 |
| `PresenterMode` | `PresenterMode` | `Split` | 选择独立展示区域或铺满卡片的叠加模式。 |
| `PresenterPosition` | `PresenterPosition` | `Left` | 在 `Split` 模式下设置 `Presenter` 的位置；在 `Overlay` 模式下无效。 |

在 `Split` 模式下，`PresenterPosition` 始终描述展示内容的位置；标题、文本和内容位于相对方向。

| 位置 | 展示内容位置 |
| --- | --- |
| `Left` | 位于左侧并垂直居中。 |
| `LeftTop` | 位于左上侧。 |
| `LeftBottom` | 位于左下侧。 |
| `Top` | 位于顶部并水平居中。 |
| `Bottom` | 位于底部并水平居中。 |
| `Right` | 位于右侧并垂直居中。 |
| `RightTop` | 位于右上侧。 |
| `RightBottom` | 位于右下侧。 |

## 叠加展示内容

在 `Overlay` 模式下，`Presenter` 铺满卡片并保持居中，标题、文本和内容显示在其上方；`PresenterPosition` 不产生作用。请选用或组合能在两种主题下保持叠加文字可读的展示内容。

```xml
<flourish:IconCard
  MinHeight="240"
  PresenterMode="Overlay"
  Title="项目预览"
  Text="展示内容会铺满整张卡片。">
  <flourish:IconCard.Presenter>
    <Image Source="Assets/project-preview.png" Stretch="UniformToFill" />
  </flourish:IconCard.Presenter>
</flourish:IconCard>
```

## 相关内容

- [Chunk](chunk.md)说明如何将卡片放入页面章节。
- [Button](button.md)说明何时应当将信息表面改为可交互的 `CardButton`。
- [CardVariant API](xref:ArkheideSystem.Flourish.Controls.CardVariant)、[Card API](xref:ArkheideSystem.Flourish.Controls.Card)、[IconCard API](xref:ArkheideSystem.Flourish.Controls.IconCard)、[PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode) 与 [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition)列出完整成员。
