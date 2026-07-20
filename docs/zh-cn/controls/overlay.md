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

## Tooltip 集成

`FlourishToolTip` 使用 `Temporary` Overlay 获得统一的表面外观。WPF `ToolTipService` 仍负责 Tooltip 的打开、延迟、Popup 定位与关闭，因此嵌套的 Overlay 不设置 `PlacementTarget`。

## 相关控件

- [Card](card.md)是参与页面布局的信息表面，而不是浮动内容。
- [Button](button.md)可作为常见的 Overlay 触发器。
- [ScrollViewer](scroll-viewer.md)可承载超出 Overlay 可用高度的内容。
- [Overlay API](xref:ArkheideSystem.Flourish.Controls.Overlay) 与 [OverlayVariant API](xref:ArkheideSystem.Flourish.Controls.OverlayVariant) 列出完整成员签名。
