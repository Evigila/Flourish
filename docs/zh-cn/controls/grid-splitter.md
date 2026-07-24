---
title: GridSplitter
description: 使用细窄的 Flourish 交互高亮和连续布局更新调整相邻 Grid 区域。
---

# GridSplitter

`FlourishGridSplitter` 沿用原生 WPF `GridSplitter` 的布局模型。当用户需要调整相邻 `Grid` 行或列的相对尺寸时使用该控件。Flourish 在透明指针交互表面中提供居中的细窄高亮。

## 调整列宽

将分隔器放在两个目标定义之间的 `Auto` 列中。为内容列设置最小宽度，避免任一区域被收缩到无法使用。

```xml
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*" MinWidth="160" />
    <ColumnDefinition Width="Auto" />
    <ColumnDefinition Width="2*" MinWidth="240" />
  </Grid.ColumnDefinitions>

  <local:EditorPane />
  <flourish:FlourishGridSplitter
    Grid.Column="1"
    ResizeBehavior="PreviousAndNext"
    ResizeDirection="Columns" />
  <local:PreviewPane Grid.Column="2" />
</Grid>
```

调整行高时采用相同结构，将分隔器放入 `Auto` 行并设置 `ResizeDirection="Rows"`。

## 交互方式

拖动会连续更新受影响的 `GridLength`。鼠标悬浮、键盘焦点和拖动统一使用细窄高亮；Flourish 不会在拖动时切换为更粗的预览浮层。这样可见分隔线能与最终布局保持一致，而透明输入表面仍具有足够的指针命中范围。

最小与最大尺寸约束属于外围 `Grid`。在应用布局中显式设置 `ResizeDirection` 与 `ResizeBehavior`，使调整轴和受影响的定义保持明确。

## 布局角色

`Variant="Standard"` 是通用角色。`Variant="NavigationPane"` 仅用于 Flourish Shell 导航面板边缘，它会选择横向列调整、Shell 边缘对齐、光标和层级默认值。两个角色使用相同宽度的交互表面与细窄实时调整高亮。

应通过 Grid 列直接对齐相邻视口和分隔器。不要使用 ScrollBar 负边距掩盖空隙；负边距可能把指针命中区域移出视口、裁剪滚动条，或产生随缩放比例变化的重叠。

## 相关内容

- [ScrollViewer 与 ScrollBar](scroll-viewer.md) 说明标准视口和滚动条几何结构。
- [FlourishGridSplitter API](xref:ArkheideSystem.Flourish.Controls.FlourishGridSplitter) 列出 Flourish 特有成员。
- [WPF GridSplitter 文档](https://learn.microsoft.com/dotnet/desktop/wpf/controls/how-to-resize-rows-with-a-gridsplitter) 介绍原生 Grid 尺寸调整行为。
