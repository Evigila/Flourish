---
title: ScrollViewer and ScrollBar
description: Host overflowing content with smooth pixel scrolling and the standard Flourish scroll bar.
---

# ScrollViewer and ScrollBar

`ScrollViewer` hosts overflowing content with shared Flourish scroll bars and optional smooth mouse-wheel scrolling.

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

Applications can initialize the same policy for Flourish-owned Shell, navigation, and page scrolling surfaces during composition:

```csharp
builder.ConfigureLayout(layout =>
    layout.SetSmoothScrollingEnabled(enabled: true));
```

`SetSmoothScrollingEnabled` sets built-in Shell viewports. Use `IScrollService` at runtime:

```csharp
scrollService.SetSmoothScrollingEnabled(false);

ScrollSettings current = scrollService.Current;
scrollService.Changed += OnScrollSettingsChanged;
```

The service updates existing viewports immediately through the shared dynamic resource. An explicit `IsSmoothScrollingEnabled` value on an application-owned `ScrollViewer` has local precedence and remains appropriate when only one viewport needs different behavior.

## Nested viewports

With the default physical scrolling mode, mouse-wheel input starts at the deepest Flourish `ScrollViewer`. An inner viewport consumes the wheel while it can move in that direction. At its top or bottom boundary, outward wheel input remains available to an ancestor viewport, so a compact inner history does not trap page scrolling.

## Custom templates

Smooth pixel scrolling requires a stationary `PART_ScrollContentPresenter` with a nested `ContentPresenter` named `PART_SmoothScrollContentHost`. The control applies the per-frame transform only to this private host so the viewport clip remains fixed. If the host is absent, mouse-wheel input safely falls back to native scrolling.

For a template used with `CanContentScroll="True"`, keep the virtualizing `IScrollInfo` or `ItemsPresenter` directly connected to `PART_ScrollContentPresenter` and omit the smooth-scroll host. This preserves WPF logical scrolling and recycling.

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

All Flourish viewports use one `ScrollBar` appearance: a narrow visible thumb within a larger pointer target. There is no compact variant.

## Related features

- [Controls](index.md)
- [Chunk](chunk.md)
