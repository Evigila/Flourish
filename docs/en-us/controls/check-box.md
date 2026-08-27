---
title: CheckBox
description: Present Boolean and optional three-state selections as compact rows or icon-led cards.
---

# CheckBox

`CheckBox` preserves native WPF state and events while adding fixed Horizontal and Vertical layouts.

## Choose a layout

`Variant="Horizontal"` is the default. It places an empty circular indicator before the content, replacing it with a primary check mark and highlighted boundary when checked.

```xml
<flourish:CheckBox
  Content="Enable notifications"
  IsChecked="{Binding NotificationsEnabled, Mode=TwoWay}" />
```

Use `Variant="Vertical"` for an icon-led card selection. It places the icon and content on the left and the state indicator on the upper right; selection applies the primary check mark and highlight.

```xml
<flourish:CheckBox
  Content="Cloud workspace"
  Icon="&#xE753;"
  IsChecked="{Binding UsesCloudWorkspace, Mode=TwoWay}"
  Variant="Vertical" />
```

`Icon` accepts arbitrary content and is rendered only by the Vertical layout. Text and other foreground-aware icon content inherit the selection foreground. Artwork that supplies its own explicit colors retains those colors.

## Hover feedback

Both layouts use shared `HoverReveal` feedback. Disabling animation or reduced motion preserves static hover and pressed states; inherited `HoverReveal.IsEnabled="False"` disables animation for a subtree.

## Three-state selections

Set `IsThreeState="True"` only when `null` represents an inherited, mixed, or unknown value. The indeterminate state replaces the check mark with a primary-colored rounded-square outline and otherwise uses the same highlighted treatment as the checked state.

```xml
<flourish:CheckBox
  Content="Use inherited setting"
  IsChecked="{Binding InheritedSetting, Mode=TwoWay}"
  IsThreeState="True" />
```

| Variant | Unchecked | Checked | Indeterminate |
| --- | --- | --- | --- |
| Horizontal | Empty circular indicator | Primary check mark and highlighted content and boundary | Primary rounded-square outline and highlighted content and boundary |
| Vertical | Upper-right empty circular indicator | Upper-right primary check mark and highlighted icon, content, and boundary | Upper-right primary rounded-square outline and highlighted icon, content, and boundary |

## Related content

- [CheckBox API](xref:ArkheideSystem.Flourish.Controls.CheckBox)
- [CheckBoxVariant API](xref:ArkheideSystem.Flourish.Controls.CheckBoxVariant)
- [Motion](../articles/configure-motion.md)
- [WPF CheckBox documentation](https://learn.microsoft.com/dotnet/desktop/wpf/controls/checkbox)
