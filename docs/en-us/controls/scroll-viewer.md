---
title: ScrollViewer
description: Host overflowing page content with smooth pixel scrolling and a slender Flourish scroll bar.
---

# ScrollViewer

`ScrollViewer` hosts content that can exceed the available viewport. It uses a slender rounded thumb and smooths mouse-wheel input without requiring a layout pass for every animation frame.

Use the Flourish XML namespace to distinguish this control from the WPF type with the same name:

```xml
<flourish:ScrollViewer
  HorizontalScrollBarVisibility="Disabled"
  VerticalScrollBarVisibility="Auto">
  <Grid>
    <!-- Page content -->
  </Grid>
</flourish:ScrollViewer>
```

## Smooth scrolling

`IsSmoothScrollingEnabled` is `true` by default. During mouse-wheel scrolling, the control advances the visible content with a render transform and synchronizes the logical offset at a lower rate. The logical offset remains authoritative for the scroll bar, keyboard navigation, thumb dragging, and programmatic scrolling.

Set `IsSmoothScrollingEnabled="False"` when immediate native pixel scrolling is required.

## Virtualized item controls

When `CanContentScroll` is `true`, `ScrollViewer` preserves WPF logical scrolling instead of treating item offsets as pixels. This keeps item-based virtualizing panels correct. Large item controls should also enable recycling on the owning control:

```xml
<ListBox
  ScrollViewer.CanContentScroll="True"
  VirtualizingPanel.IsVirtualizing="True"
  VirtualizingPanel.VirtualizationMode="Recycling" />
```

Do not wrap a virtualized item control in another `ScrollViewer`; let the item control own its scrolling viewport.

## Scroll bar appearance

The visible thumb is narrower than its transparent interaction area, so the bar keeps a light visual profile without making pointer dragging unnecessarily precise. Set `IsCompact="True"` when the viewport needs the most compact variant.

## Related features

- [Controls](index.md)
- [Chunk](chunk.md)
