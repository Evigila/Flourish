---
title: Button
description: 使用 Flourish Button、CardButton 与 WindowCaptionButton 表达常规操作、整卡操作和窗口标题栏操作。
---

# Button

Flourish 按钮保留 WPF `Button` 的命令、点击、键盘焦点、内容模板与自动化行为，并提供统一的主题和指针反馈。按钮的完整视觉边界都可交互。

| 控件 | 用于 |
| --- | --- |
| `Button` | 常规文字操作、图标加文字操作或仅图标操作。 |
| `CardButton` | 整个卡片都应响应点击或命令的导航、选择与调用操作。 |
| `WindowCaptionButton` | 窗口标题栏中的最小化、最大化、还原与关闭操作。 |

## Button

`Button` 同时提供可选的 `Icon` 和继承的 `Content`。可以只设置其中一项，也可以同时设置。任一属性为 `null` 或空字符串时，对应呈现区域及其间距都会完全折叠；只设置 `Icon` 时，按钮采用紧凑的仅图标布局。

```xml
<WrapPanel>
  <flourish:Button
    Variant="Filled"
    Command="{Binding SaveCommand}"
    Content="保存" />

  <flourish:Button
    Variant="Tonal"
    Command="{Binding AddCommand}"
    Content="添加项目"
    Icon="&#xE710;" />

  <flourish:Button
    Variant="Text"
    AutomationProperties.Name="刷新"
    Command="{Binding RefreshCommand}"
    Icon="&#xE72C;"
    ToolTip="刷新" />
</WrapPanel>
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Icon` | `object?` | `null` | 可选图标或其他图标内容。 |
| `Content` | `object?` | `null` | 继承的可选按钮内容。 |
| `Variant` | `ButtonVariant` | `Outlined` | 选择操作的视觉强调层级。 |

`Button.Variant` 表达操作层级，而不是尺寸或布局：

| 变体 | 使用时机 |
| --- | --- |
| `Elevated` | 需要与复杂背景分离的重要操作。 |
| `Filled` | 操作组中强调最高的主要操作。 |
| `Tonal` | 比轮廓按钮更突出、但不与主要操作竞争的辅助操作。 |
| `Outlined` | 带可见边界的中等强调操作，也是 `Button` 的默认值。 |
| `Text` | 紧凑、行内、工具栏或低强调操作。 |
| `Danger` | 删除、重置等破坏性或难以撤销的操作。 |

`Standard` 是同一枚举中供 `CardButton` 使用的默认卡片表面。常规 `Button` 应从上述六种操作变体中选择。一组操作通常只应有一个 `Filled` 按钮。

仅图标按钮应同时设置可见的 `ToolTip` 和能够说明操作意图的 `AutomationProperties.Name`。启用 `UseTips` 后，Flourish 按钮提示使用统一的 Temporary Overlay 表面；未启用时，同一提示内容使用原生 WPF Tooltip 行为。

## CardButton

`CardButton` 是具有 Card 视觉语言的按钮。它支持可选的 `Title`、`Content` 和 `Icon`，三者为空时各自的区域与间距都会折叠。`IconPosition` 可将图标放在 `Left`、`Top`、`Right` 或 `Bottom`，默认位于上方。

```xml
<flourish:CardButton
  Variant="Elevated"
  Command="{Binding OpenReportsCommand}"
  Content="查看已生成的报告与最近导出。"
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="报告" />
```

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string?` | `""` | 可选卡片标题。 |
| `Content` | `object?` | `null` | 继承的可选单段辅助内容。 |
| `Icon` | `object?` | `null` | 继承的可选单个图标。 |
| `IconPosition` | `Dock` | `Top` | 图标相对于文案的位置。 |
| `Variant` | `ButtonVariant` | `Standard` | 交互卡片的表面样式。 |

`CardButton` 支持 Card 的 `Standard`、`Elevated`、`Tonal` 和 `Filled` 视觉变体。整个卡片都应执行同一项操作时使用它；只有卡片中的局部控件需要交互时，使用 [ActionCard](card.md#actioncard)。

## WindowCaptionButton

`WindowCaptionButton` 只用于窗口标题栏。它使用标题栏专用几何呈现 `Icon`，并通过 `Command` 或 `Click` 连接实际窗口操作。关闭操作使用 `Variant="Danger"`，其他标题栏操作使用 `Text`。

```xml
<flourish:WindowCaptionButton
  Variant="Danger"
  AutomationProperties.Name="关闭"
  Command="{Binding CloseWindowCommand}"
  Icon="&#xE8BB;"
  ToolTip="关闭" />
```

## 悬停反馈与减少动态效果

按钮家族参与公共 `HoverReveal` 附加行为。优先在应用级通过[动效](../articles/configure-motion.md)配置它，并遵循操作系统的减少动态效果偏好。需要局部覆盖时，可以设置以下附加属性：

```xml
<flourish:Button
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.OverrideColor="{DynamicResource FlourishPrimarySurfaceBrush}"
  Content="预览" />
```

`Danger` 按钮默认使用危险语义的悬停颜色；局部 `OverrideColor` 会覆盖它。

## 相关内容

- [Card](card.md)区分非交互 Card、带局部操作的 ActionCard 和整卡交互。
- [Chunk](chunk.md)说明如何在页面区块中组织按钮。
- [动效](../articles/configure-motion.md)配置悬停显示与减少动态效果。
- [ButtonVariant API](xref:ArkheideSystem.Flourish.Controls.ButtonVariant)、[Button API](xref:ArkheideSystem.Flourish.Controls.Button)、[CardButton API](xref:ArkheideSystem.Flourish.Controls.CardButton) 与 [WindowCaptionButton API](xref:ArkheideSystem.Flourish.Controls.WindowCaptionButton) 列出完整成员。
