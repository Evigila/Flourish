---
title: Button
description: 使用 Flourish Button、IconButton、CardButton 与 WindowCaptionButton 表达操作层级和意图。
---

# Button

Flourish 按钮家族由一个通用文本按钮和三个专用衍生控件组成。它们保留 WPF `Button` 的命令、点击事件、内容、键盘和自动化行为，并提供 Flourish 的主题、焦点与指针反馈。

每个按钮的完整视觉边界都可交互。应根据该边界所呈现的信息选择按钮家族成员：

| 控件 | 用于 |
| --- | --- |
| `Button` | 以文字表达的常规操作。 |
| `IconButton` | 仅图标操作，或需要在文字前显示图标的操作。 |
| `CardButton` | 整个卡片都可调用的导航或选择操作。 |
| `WindowCaptionButton` | 窗口标题栏中的最小化、最大化、还原与关闭操作。 |

## Button

`Button.Variant` 选择按钮的视觉表现与强调层级，而不是按钮的尺寸或布局。默认值是 `Outlined`。

| `ButtonVariant` | 使用时机 |
| --- | --- |
| `Elevated` | 需要与纹理、图片或其他复杂背景分离的重要操作。应谨慎使用高程。 |
| `Filled` | 屏幕或操作组中强调最高的主要操作。 |
| `Tonal` | 比轮廓按钮更突出，但不与 Filled 主操作竞争的重要辅助操作。 |
| `Outlined` | 使用可见边界的中等强调次要操作。这是默认值。 |
| `Text` | 强调最低的紧凑、行内、工具栏或第三级操作。 |
| `Danger` | 删除、重置等破坏性或难以撤销的操作。 |

```xml
<WrapPanel>
  <flourish:Button
    Variant="Filled"
    Command="{Binding SaveCommand}"
    Content="保存" />
  <flourish:Button
    Variant="Tonal"
    Command="{Binding SaveDraftCommand}"
    Content="保存草稿" />
  <flourish:Button
    Variant="Outlined"
    Command="{Binding CancelCommand}"
    Content="取消" />
  <flourish:Button
    Variant="Text"
    Command="{Binding LearnMoreCommand}"
    Content="了解更多" />
  <flourish:Button
    Variant="Elevated"
    Command="{Binding OpenPreviewCommand}"
    Content="打开预览" />
  <flourish:Button
    Variant="Danger"
    Command="{Binding DeleteCommand}"
    Content="删除" />
</WrapPanel>
```

一组操作通常只应有一个 `Filled` 按钮。破坏性意图使用 `Danger`；它是 Flourish 六种变体中的破坏性语义选项。外部位置由包含按钮的布局容器控制；不要通过 `Variant` 选择结构尺寸。

`Tonal` 使用受 Fluent Web 品牌色阶启发、并针对深浅主题分别调整的配色。所有非危险变体继承同一个 `HoverReveal.OverrideColor`，只有 `Danger` 覆盖语义颜色。Elevated 阴影绘制在独立的纯背景模板层上，因此不会影响文字的 ClearType 渲染。

## IconButton

`IconButton` 增加了类型为 `object?`、默认值为 `null` 的 `Icon` 属性。它可以接受 Flourish 图标字体的字形字符串，也可以接受任意可视元素。继承的 `Content` 可选：设置后它会成为图标后的文字标签。

```xml
<flourish:IconButton
  Variant="Text"
  AutomationProperties.Name="刷新"
  Command="{Binding RefreshCommand}"
  Icon="&#xE72C;"
  ToolTip="刷新" />

<flourish:IconButton
  Variant="Filled"
  Command="{Binding AddCommand}"
  Content="添加项目"
  Icon="&#xE710;" />
```

当 `Content` 为 `null` 时，`IconButton` 使用无内边距的紧凑 `32 × 32` 布局。仅图标按钮应同时设置可见的 `ToolTip` 和可识别操作意图的 `AutomationProperties.Name`。启用 `UseTips` 后，Button 系列提示使用 Flourish Temporary Overlay 表面、Shell 感知的定位和禁用控件显示行为；未启用时，同一份提示内容使用原生 WPF Tooltip 的外观和默认行为。附加到原生 WPF 与第三方控件的 Tooltip 在两种模式下均保持不变。

## CardButton

`CardButton` 表示一块可交互的卡片，而不是普通按钮的外观变体。它采用类似 IconCard 的排列，并且只适用于调用整个卡片的操作。紧凑设置行右侧的局部操作应使用 `ListCard.ActionBody`；其他布局中的普通操作应使用 `Button` 或 `IconButton`。

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 卡片标题。 |
| `Content` | 继承的 `object?` | `null` | 卡片的描述或其他辅助内容。 |
| `Icon` | `object?` | `null` | 卡片操作显示的单个图标。 |
| `IconPosition` | `Dock` | `Top` | 将图标放在 `Left`、`Top`、`Right` 或 `Bottom`。 |

```xml
<flourish:CardButton
  Command="{Binding OpenReportsCommand}"
  Content="查看已生成的报告与最近导出。"
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="报告" />
```

## WindowCaptionButton

`WindowCaptionButton` 只用于窗口标题栏。它继承 `IconButton` 的 `Icon`、`Variant` 和标准 WPF 按钮契约，但使用标题栏专用几何，默认尺寸为 `46 × 40`。

将关闭操作设置为 `Variant="Danger"`，其他标题栏操作使用 `Text`。该控件只负责显示与按钮交互；应用仍需要通过 `Command` 或 `Click` 连接实际的窗口操作。

```xml
<flourish:WindowCaptionButton
  Variant="Danger"
  AutomationProperties.Name="关闭"
  Command="{Binding CloseWindowCommand}"
  Icon="&#xE8BB;"
  ToolTip="关闭" />
```

## 悬停反馈与减少动态效果

按钮家族参与公共 `HoverReveal` 附加行为。优先在应用级别通过[动效](../articles/configure-motion.md)配置它，并遵循操作系统的减少动态效果偏好。需要局部覆盖时，可以设置以下附加属性：

```xml
<flourish:Button
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.OverrideColor="{DynamicResource FlourishPrimarySurfaceBrush}"
  Content="预览" />
```

`HoverReveal.IsEnabled` 默认为 `true` 并向下继承，`AnimationDuration` 默认为 140 毫秒并向下继承，`OverrideColor` 默认为 `null` 且不继承。`Danger` 按钮默认使用危险语义的悬停颜色；局部 `OverrideColor` 会覆盖它。

## 相关内容

- [Chunk](chunk.md)说明如何在页面区块中组织按钮。
- [Card](card.md)区分非交互卡片、ListCard 操作区和整卡操作。
- [动效](../articles/configure-motion.md)配置悬停显示与减少动态效果。
- [ButtonVariant API](xref:ArkheideSystem.Flourish.Controls.ButtonVariant)、[Button API](xref:ArkheideSystem.Flourish.Controls.Button)、[IconButton API](xref:ArkheideSystem.Flourish.Controls.IconButton)、[CardButton API](xref:ArkheideSystem.Flourish.Controls.CardButton) 与 [WindowCaptionButton API](xref:ArkheideSystem.Flourish.Controls.WindowCaptionButton) 列出完整成员。
