---
title: BunchedListBox
description: Present a selectable collection with one parent-owned interaction layer that moves continuously between items.
---

# BunchedListBox

`BunchedListBox` is the recommended list control when adjacent items should feel like one coordinated collection. It preserves the native WPF `ListBox` selection, keyboard, automation, data binding, scrolling, and virtualization behavior, while moving hover, pressed, and selection backgrounds out of each item container and into one parent-owned display layer.

Use `FlourishListBox` when every item deliberately needs an independent interaction surface. Use `BunchedListBox` when pointer feedback or the single-selection indicator should travel continuously from one item to the next.

## Bind a collection

Bind items and selection through the standard `ListBox` members. A data item is automatically wrapped in `BunchedListBoxItem`.

```xml
<flourish:BunchedListBox
  ItemsSource="{Binding Projects}"
  SelectedItem="{Binding SelectedProject, Mode=TwoWay}" />
```

The control accepts data templates, item container styles, `SelectedValuePath`, and other inherited collection members. It also inherits the `Appearance` and `IsCompact` presentation options from `FlourishListBox`.

## Shared interaction layer

The default template contains one non-interactive layer for hover, pressed, and selection feedback. Moving between items retargets the existing indicator instead of ending one item animation and starting another. Its geometry is calculated from the realized container, so unequal item sizes, margins, horizontal panels, right-to-left layouts, and scrolling remain supported.

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
<flourish:BunchedListBox Appearance="Navigation">
  <flourish:BunchedListBoxItem
    Content="Workspace"
    IsGroupHeader="True" />
  <flourish:BunchedListBoxItem Content="Overview" />
</flourish:BunchedListBox>
```

An explicit `FlourishListBoxItem` is treated as data and wrapped in a `BunchedListBoxItem`; use the Bunched container type when supplying a container directly.

## Customize the template

The default control template supplies `PART_InteractionViewport`, `PART_IndicatorLayer`, `PART_SelectionChrome`, `PART_HoverChrome`, `PART_PressedChrome`, and `PART_ScrollViewer`. A custom template may omit these parts and retain ordinary `ListBox` selection behavior, but coordinated interaction feedback requires the matching parts. The indicator layer must remain non-interactive and must not wrap the items presenter, so it does not interrupt hit testing, logical scrolling, or virtualization.

## Related content

- The [BunchedListBox API](xref:ArkheideSystem.Flourish.Controls.BunchedListBox) lists inherited and declared members.
- The [BunchedListBoxItem API](xref:ArkheideSystem.Flourish.Controls.BunchedListBoxItem) describes the generated container.
- [Motion](../articles/configure-motion.md) configures hover reveal and reduced-motion behavior.
- The [WPF ListBox documentation](https://learn.microsoft.com/dotnet/desktop/wpf/controls/listbox) explains the native selection model.
