---
title: Card
description: 使用 Card、ListCard 与 IconCard，在适配主题的非交互式表面上承载较长信息、紧凑配置行与视觉展示内容。
---

# Card

`Card` 是用于组织单一主题下较长说明或展示型信息的非交互式表面。它内置标题、辅助文本和 Body 区域，并会随当前 Flourish 主题调整颜色。紧凑设置或局部配置操作应改用 `ListCard`。

> [!IMPORTANT]
> 如果点击表面任意位置都会执行同一个操作，请改用 `CardButton`。不要给 `Card`、`ListCard` 或 `IconCard` 添加鼠标事件来模拟按钮行为。

## 基本用法

普通信息卡片直接设置 `Title` 和 `Text`，无需再为这两个角色手动构造文本控件。

```xml
<flourish:Card
  Title="账户状态"
  Text="工作区已完成同步。" />
```

使用 `Body` 放置细节、状态、控件或其他组合 WPF 内容树。需要多个子元素时，使用 `Grid`、`StackPanel` 或其他布局容器作为根。

```xml
<flourish:Card
  Title="存储空间"
  Text="管理与当前工作区关联的文件。">
  <flourish:Card.Body>
    <StackPanel Margin="0,12,0,0">
      <ProgressBar Maximum="100" Value="64" />
      <flourish:Button
        Margin="0,12,0,0"
        HorizontalAlignment="Left"
        Command="{Binding ReviewFilesCommand}"
        Content="查看文件" />
    </StackPanel>
  </flourish:Card.Body>
</flourish:Card>
```

### Card 属性

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Variant` | `Variant` | `Standard` | 选择具有语义的表面样式。 |
| `Title` | `string` | `""` | 卡片标题。 |
| `Text` | `string` | `""` | 与标题一同显示的可选辅助文本。 |
| `Body` | `object?` | `null` | 可选的细节、控件、状态或其他 WPF 内容树。 |
| `ContentHorizontalAlignment` | `HorizontalAlignment` | `Stretch` | 控制文字与 Body 组合的水平对齐；与垂直居中一起使用时，会将整个组合居中。 |
| `ContentVerticalAlignment` | `VerticalAlignment` | `Stretch` | 控制文字与 Body 的排列：`Top` 或 `Stretch` 保持文字在 Body 上方，`Bottom` 则将 Body 放到文字上方。 |

## 变种

`Variant` 有四种取值：

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

`Filled` 与 `Button` 使用同一套主要填充颜色。需要其他填充色时设置本地 `Background`；本地值的优先级高于变种默认值。替换时应成对使用动态背景与前景主题资源，保证两种主题下都清晰可读。

```xml
<flourish:Card
  Variant="Filled"
  Background="{DynamicResource FlourishSecondaryBrush}"
  Foreground="{DynamicResource FlourishForegroundOnSecondaryBrush}"
  Title="自定义填充表面"
  Text="此替换颜色会跟随当前主题。" />
```

## 排列文字与 Body

内置文字区由 `Title` 和 `Text` 组成。`ContentHorizontalAlignment` 与 `ContentVerticalAlignment` 负责组织文字区和 `Body`；Card 本身在页面中的位置仍由父级布局控制。

| 设置 | 排列 |
| --- | --- |
| 默认、`Top` 或 `Stretch` 垂直对齐 | 文字区在 `Body` 上方。 |
| `ContentVerticalAlignment="Bottom"` | `Body` 在文字区上方。 |
| 两个内容对齐属性都为 `Center` | 文字区和 `Body` 作为一个组合居中。 |

```xml
<flourish:Card
  MinHeight="180"
  ContentHorizontalAlignment="Center"
  ContentVerticalAlignment="Center"
  Title="居中状态"
  Text="文字区与 Body 会作为整体居中。">
  <flourish:Card.Body>
    <ProgressBar Width="160" Maximum="100" Value="64" />
  </flourish:Card.Body>
</flourish:Card>
```

## 操作输出

不要使用普通 `Card` 充当输出或结果表面。原始消息、进度、完成结果与失败信息都应追加到专用的 [OutputCard](output-card.md)。`OutputCard` 自带紧凑滚动视口，没有 `Title`、`Text` 或任意 `Body`；解释性信息应留在操作表面或所属 `Chunk` 中。

## ListCard

`ListCard` 继承自 `Card`，表示一个紧凑且独立的配置项。表面本身仍然非交互，本地输入或调用由 `Body` 中的控件处理。普通 `Card` 与 `IconCard` 应保留给较长说明或展示型内容。

它使用固定排列：可选的图标或图片 `Presenter` 始终在左侧，并在左右两侧保留刻意加宽的水平留白；`Title` 与简短的 `Text` 说明在中间纵向排列，`Body` 始终在右侧。Presenter、文字区与 Body 都纵向居中。不要用局部 Margin 压缩 Presenter 留白；Card 的内容对齐属性不会改变这些区域的位置。

`Title` 与 `Text` 都必须简洁。两者各自最多一行，溢出时显示省略号；不要依赖换行，应重新精简过长文案。`Body` 必须且只能放一个交互控件；不要在一个 ListCard 内用面板组合多个输入或操作。

```xml
<flourish:ListCard
  Title="主题"
  Text="选择应用使用的外观。">
  <flourish:ListCard.Presenter>
    <flourish:FlourishTextBlock
      AutomationProperties.Name="主题"
      Role="Icon"
      Text="&#xE790;" />
  </flourish:ListCard.Presenter>
  <flourish:ListCard.Body>
    <flourish:FlourishComboBox
      Width="160"
      ItemsSource="{Binding Themes}"
      SelectedItem="{Binding Theme, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
  </flourish:ListCard.Body>
</flourish:ListCard>
```

### ListCard 属性

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 从 Card 继承的简洁单行设置项标题；溢出使用省略号。 |
| `Text` | `string` | `""` | 从 Card 继承、显示在标题下方的简洁单行说明；溢出使用省略号。 |
| `Presenter` | `object?` | `null` | 显示在左侧的可选图标、图片或其他视觉内容。 |
| `Body` | `object?` | `null` | 显示在右侧且只能有一个的局部交互控件。 |
| `Variant` | `Variant` | `Standard` | 从 Card 继承但始终被强制转换为 `Standard`；ListCard 没有视觉变种。 |

将相关 ListCard 纵向堆叠且让每一行铺满所在列。行间使用紧凑的 `FlourishListCardPeerMargin`，让各行形成相关组合，同时仍保留独立表面。一行只承载一个独立功能。同一列中不能混用 `ListCard` 与任何其他卡片类型。每个 `Chunk` 优先采用仅含 ListCard 的单列布局。章节还需要输出时，同一 Chunk 可以增加另一列放置 `OutputCard`。将两列放入同一个自动高度 `Grid` 行，让完整 ListCard 列决定行高，并把 `OutputCard` 拉伸到布局后的高度。消息历史不会增加其期望高度；溢出内容始终留在内部滚动视口中。

`Body` 优先使用 `FlourishComboBox`、`FlourishCheckBox` 与 `Button`；选项确有需要时再使用 `FlourishTextBox` 与 `FlourishRadioButton`。选择、开关与编辑必须立即应用，ListCard 中绝不增加独立的 Apply 操作。

## IconCard

`IconCard` 与 Card 具有相同的标题、文本、Body、对齐和变种约定。它的 `Presenter` 可以承载图标、图片、插图、预览或其他任意 WPF 视觉内容；控件仍然是非交互式表面。

```xml
<flourish:IconCard
  PresenterMode="Split"
  PresenterPosition="Left"
  Title="报告"
  Text="查看生成的报告与最近导出。">
  <flourish:IconCard.Presenter>
    <TextBlock
      AutomationProperties.Name="报告"
      FontFamily="Segoe Fluent Icons"
      FontSize="{DynamicResource FlourishFontSizeStandard}"
      Text="&#xE8A5;" />
  </flourish:IconCard.Presenter>
  <flourish:IconCard.Body>
    <TextBlock Text="已有 12 份报告就绪。" />
  </flourish:IconCard.Body>
</flourish:IconCard>
```

### IconCard 属性

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Presenter` | `object?` | `null` | 图标、图片、插图、预览或其他视觉内容。 |
| `PresenterMode` | `PresenterMode` | `Split` | 选择独立展示区域或铺满卡片的叠加模式。 |
| `PresenterPosition` | `PresenterPosition` | `Left` | 在 `Split` 模式下设置 Presenter 位置；在 `Overlay` 模式下无效。 |

在 `Split` 模式下，`PresenterPosition` 始终描述 Presenter 的位置；文字区与 `Body` 一起位于对立侧。

| 位置 | Presenter 位置 | 文字区与 Body |
| --- | --- | --- |
| `Left` | 位于左侧并垂直居中。 | 位于对立侧，纵向排列。 |
| `LeftTop` | 位于左上侧。 | 位于右下侧，纵向排列。 |
| `LeftBottom` | 位于左下侧。 | 位于右上侧，纵向排列。 |
| `Top` | 位于顶部并水平居中。 | 位于下方，水平排列。 |
| `Bottom` | 位于底部并水平居中。 | 位于上方，水平排列。 |
| `Right` | 位于右侧并垂直居中。 | 位于对立侧，纵向排列。 |
| `RightTop` | 位于右上侧。 | 位于左下侧，纵向排列。 |
| `RightBottom` | 位于右下侧。 | 位于左上侧，纵向排列。 |

## 叠加展示内容

`Overlay` 模式下，`Presenter` 铺满卡片，文字区与 `Body` 显示在其上方。此时 `PresenterPosition` 不产生作用，文字与 Body 使用普通 Card 的纵向排列。请选择或组合能在两种主题下保持所有叠加内容可读的 Presenter。

```xml
<flourish:IconCard
  MinHeight="240"
  PresenterMode="Overlay"
  Title="项目预览"
  Text="展示内容会铺满整张卡片。">
  <flourish:IconCard.Presenter>
    <Image Source="Assets/project-preview.png" Stretch="UniformToFill" />
  </flourish:IconCard.Presenter>
  <flourish:IconCard.Body>
    <TextBlock Text="更新于今天" />
  </flourish:IconCard.Body>
</flourish:IconCard>
```

## 相关内容

- [理念](../conception/index.md) 定义卡片如何参与一致的页面层级。
- [Chunk](chunk.md) 说明如何将卡片放入页面章节。
- [OutputCard](output-card.md) 说明如何追加操作消息而不推高同行布局。
- [Button](button.md) 说明何时应当将信息表面改为可交互的 `CardButton`。
- [Variant API](xref:ArkheideSystem.Flourish.Controls.Variant)、[Card API](xref:ArkheideSystem.Flourish.Controls.Card)、[ListCard API](xref:ArkheideSystem.Flourish.Controls.ListCard)、[IconCard API](xref:ArkheideSystem.Flourish.Controls.IconCard)、[PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode) 与 [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) 列出完整成员。
