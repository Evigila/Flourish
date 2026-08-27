---
title: Button
description: 使用 Flourish Button、CardButton 与 WindowCaptionButton 表达常规操作、整卡操作和窗口标题栏操作。
---

# Button

Flourish 按钮保留 WPF `Button` 的命令、点击、键盘焦点、内容模板与自动化行为，并统一主题和指针反馈。完整视觉边界均可交互。

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
| `IconSize` | `double` | `14` | 默认使用 StandardIcon；紧凑操作可局部覆盖。 |
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

`IsEnabled` 为 `false` 时，所有 Button 变体都会使用统一的淡灰禁用背景、边框与前景。禁用状态会覆盖 Filled、Danger 等启用状态颜色，使不可用操作保持一致。

仅图标按钮必须设置可见 `ToolTip` 和有意义的 `AutomationProperties.Name`。启用 `ConfigureToolTips` 或 `IToolTipService` 时使用 Temporary Overlay；否则使用原生 WPF Tooltip。

## CardButton

`CardButton` 支持可选 `Title`、`Content` 和 `Icon`，空区域及间距会折叠。文本自动换行，默认最多三行；`ContentMaxLines` 接受其他正整数，溢出在末行显示省略号。`IconPosition` 可为 `Left`、`Top`、`Right` 或 `Bottom`，默认 `Top`。

CardButton 图标在中性表面使用自适应主色前景，在 Filled 表面使用主色背景上的对比前景，禁用时使用统一的弱化前景。

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

`CardButton` 支持 `Standard`、`Elevated`、`Tonal` 和 `Filled`。`IsEnabled=false` 时统一使用禁用颜色，并关闭阴影和指针反馈。整卡执行一个操作时使用它；局部控件交互使用 [ActionCard](card.md#actioncard)。

## WindowCaptionButton

`WindowCaptionButton` 只用于窗口标题栏。它使用标题栏专用几何呈现 `Icon`，并通过 `Command` 或 `Click` 连接实际窗口操作。关闭操作使用 `Variant="Danger"`，其他标题栏操作使用 `Text`。

Danger 只为启用状态的关闭操作提供警示强调；关闭操作被禁用后，会与其他禁用控件一样恢复统一的淡灰视觉。

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

交互颜色保留相互独立的默认、悬停和按下状态。`Filled`、`Tonal` 与 `Danger` 使用适合各自填充表面的反馈色；`Elevated`、`Outlined` 与 `Text` 共享同一种弱化悬停色，并共享另一种更深的按下色。

## 相关内容

- [Card](card.md)
- [Chunk](chunk.md)
- [动效](../articles/configure-motion.md)
- [ButtonVariant API](xref:ArkheideSystem.Flourish.Controls.ButtonVariant)、[Button API](xref:ArkheideSystem.Flourish.Controls.Button)、[CardButton API](xref:ArkheideSystem.Flourish.Controls.CardButton) 与 [WindowCaptionButton API](xref:ArkheideSystem.Flourish.Controls.WindowCaptionButton)
