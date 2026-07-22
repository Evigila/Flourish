---
title: Overlay
description: 使用统一的主题表面与明确的关闭语义，展示临时悬浮详情或强浮层内容。
---

# Overlay

`Overlay` 是用于承载锚定浮动信息的主题化内容表面。`Variant` 表示浮层应随指针离开而关闭，还是应由宿主保持打开，直到用户明确执行关闭操作。

`Overlay` 负责表面样式与生命周期约定；请将它放入负责定位和打开状态的 Popup、Canvas 或其他宿主中。

## 变种

| 变种 | 行为 | 典型用途 |
| --- | --- | --- |
| `Temporary` | 设置 `PlacementTarget` 后，指针同时离开触发器与 Overlay 时，会在短暂的过渡宽限后引发 `DismissRequested`。 | Tooltip、Logo 详情和设备信息。 |
| `Strong` | 指针移动不会请求关闭；宿主通过点击外部、按 <kbd>Esc</kbd>、再次使用触发器或其他明确操作关闭表面。 | Profile 与可交互的任务视图。 |

过渡宽限允许指针跨过锚点与 Overlay 之间的间隙而不关闭表面。指针返回任一元素都会取消待处理的关闭请求。

```xml
<flourish:Overlay
  x:Name="DetailsOverlay"
  PlacementTarget="{Binding ElementName=DetailsButton}"
  Variant="Temporary"
  DismissRequested="DetailsOverlay_DismissRequested">
  <TextBlock Text="工作区详情" />
</flourish:Overlay>
```

在 `DismissRequested` 中更新外围 Popup 或 Shell 宿主拥有的打开状态。`Strong` Overlay 不会因指针移动而引发此事件。

## 在应用界面中托管 Overlay

应用页面可以将 `Overlay` 放入 WPF `Popup`。请将两者的定位目标设为可交互触发器，由 Popup 管理 `IsOpen`，并在 Overlay 请求关闭时更新该状态：

```xml
<Button x:Name="DetailsButton" Content="显示详情" Click="DetailsButton_Click" />
<Popup
  x:Name="DetailsPopup"
  AllowsTransparency="True"
  Placement="Bottom"
  StaysOpen="True">
  <flourish:Overlay
    x:Name="DetailsOverlay"
    Variant="Temporary"
    DismissRequested="DetailsOverlay_DismissRequested">
    <TextBlock Text="工作区详情" />
  </flourish:Overlay>
</Popup>
```

```csharp
DetailsPopup.PlacementTarget = DetailsButton;
DetailsOverlay.PlacementTarget = DetailsButton;

private void DetailsButton_Click(object sender, RoutedEventArgs e) =>
    DetailsPopup.IsOpen = true;

private void DetailsOverlay_DismissRequested(object sender, RoutedEventArgs e) =>
    DetailsPopup.IsOpen = false;
```

对于 `Strong` Overlay，宿主还必须提供明确的关闭方式，例如操作按钮、<kbd>Esc</kbd> 和外部点击。Popup 可以通过 `StaysOpen="False"` 提供外部点击关闭行为。

## Shell 集成

Flourish Shell 功能将 Overlay 托管在窗口范围内的浮层中，而不是应用页面的 Popup 中。Shell 负责计算锚定位置、在功能被调用时更新宿主可见性，并处理 `DismissRequested`、外部点击和 <kbd>Esc</kbd>。因此，添加 Shell 功能的代码应调用该功能的 Shell 集成点，而不是要求 `Overlay` 自行打开。

请使用 [Button](button.md)、`IconButton` 或 `CardButton` 等可交互控件作为触发器。这些控件提供点击或命令激活、键盘焦点和自动化语义。`Card`、`ListCard` 与 `IconCard` 是信息表面，不支持 Overlay 触发交互。

## Tooltip 集成

启用 `UseTips` 后，Flourish 控件使用包含单个 `Temporary` Overlay 的 `FlourishToolTip` 模板呈现自有提示。打开、延迟、Popup 定位与关闭仍由 WPF `ToolTipService` 负责，因此嵌套的 Overlay 不设置 `PlacementTarget`。

省略 `UseTips` 或在运行时禁用 `ToolTips` 功能时，Flourish 控件会使用原生 WPF Tooltip 的外观和默认行为呈现同一份提示内容。附加到原生 WPF 控件的 Tooltip 与第三方控件自有的 Tooltip 始终保留各自的模板和默认行为；Flourish 不会在应用级对其重新套用模板。

## 相关控件

- [Card](card.md)是参与页面布局的信息表面，而不是浮动内容。
- [Button](button.md)可作为常见的 Overlay 触发器。
- [ScrollViewer](scroll-viewer.md)可承载超出 Overlay 可用高度的内容。
- [Overlay API](xref:ArkheideSystem.Flourish.Controls.Overlay) 与 [OverlayVariant API](xref:ArkheideSystem.Flourish.Controls.OverlayVariant) 列出完整成员签名。
