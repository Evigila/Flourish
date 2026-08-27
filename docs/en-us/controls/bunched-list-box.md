---
title: BunchedListBox
description: Present selectable items with one shared interaction layer.
---

# BunchedListBox

`BunchedListBox` preserves native `ListBox` behavior while moving hover, pressed, and selection feedback into one parent-owned layer.

Use `ListBox` when every item deliberately needs an independent interaction surface. Use `BunchedListBox` when pointer feedback or the single-selection indicator should travel continuously from one item to the next.

## Bind a collection

Bind items and selection through the standard `ListBox` members. A data item is automatically wrapped in `BunchedListBoxItem`.

```xml
<flourish:BunchedListBox
  ItemsSource="{Binding Projects}"
  SelectedItem="{Binding SelectedProject, Mode=TwoWay}" />
```

The control accepts data templates, item container styles, `SelectedValuePath`, and other inherited collection members. It also inherits the `Appearance` and `IsCompact` presentation options from `ListBox`.

## Shared interaction layer

The non-interactive layer retargets existing indicators as the active item changes. It follows realized containers across unequal sizes, margins, horizontal or right-to-left layouts, and scrolling.

The layer is clipped to the actual scroll viewport and sits behind item content. Keep custom `BunchedListBoxItem` backgrounds transparent if the shared feedback must remain visible.

`HoverReveal.IsEnabled`, `HoverReveal.IsMotionEnabled`, and `HoverReveal.AnimationDuration` control whether the layer moves with animation. Disabling motion preserves immediate static feedback. `HoverReveal.OverrideColor` can locally replace the hover brush.

## Selection

`SelectionMode="Single"` uses one selection indicator that moves to the realized selected container. `Multiple` and `Extended` display one shared-layer indicator for each realized selected container. An off-screen selected item is not forced into the visual tree, which preserves UI virtualization.

Keyboard selection, Ctrl/Shift selection, `ScrollIntoView`, and selection events remain native `ListBox` behavior.

```xml
<flourish:BunchedListBox
  ItemsSource="{Binding Tasks}"
  SelectionMode="Extended" />
```

## Use explicit containers

Usually the parent should generate `BunchedListBoxItem`. Declare a container directly only when a navigation entry needs inherited item-level state such as `IsGroupHeader` or `IsCommandItem`.

```xml
<flourish:BunchedListBox Appearance="Borderless">
  <flourish:BunchedListBoxItem
    Content="Workspace"
    IsGroupHeader="True" />
  <flourish:BunchedListBoxItem Content="Overview" />
</flourish:BunchedListBox>
```

An explicit `ListBoxItem` is treated as data and wrapped in a `BunchedListBoxItem`; use the Bunched container type when supplying a container directly.

## Customize the template

Shared feedback requires `PART_InteractionViewport`, `PART_IndicatorLayer`, `PART_SelectionChrome`, `PART_HoverChrome`, `PART_PressedChrome`, and `PART_ScrollViewer`. Keep the indicator layer non-interactive and outside the items presenter to preserve hit testing, scrolling, and virtualization.

## Related content

- [BunchedListBox API](xref:ArkheideSystem.Flourish.Controls.BunchedListBox)
- [BunchedListBoxItem API](xref:ArkheideSystem.Flourish.Controls.BunchedListBoxItem)
- [Motion](../articles/configure-motion.md)
- [WPF ListBox documentation](https://learn.microsoft.com/dotnet/desktop/wpf/controls/listbox)
