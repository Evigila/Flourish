---
title: CheckBox
description: Present Boolean and optional three-state selections as compact rows or icon-led cards.
---

# CheckBox

`CheckBox` represents an independent selection. It follows the native WPF `CheckBox` state and event model while adding fixed Horizontal and Vertical layouts.

## Choose a layout

`Variant="Horizontal"` is the default and is suitable for ordinary settings. It places a circular state indicator before the content. An unchecked option displays an empty circle. A checked option replaces that circle with a larger primary-colored check mark and highlights the content and control boundary.

```xml
<flourish:CheckBox
  Content="Enable notifications"
  IsChecked="{Binding NotificationsEnabled, Mode=TwoWay}" />
```

Use `Variant="Vertical"` for a card-shaped selection that benefits from an icon. It places the icon at the upper left, the content below it, and the state indicator at the upper right. The unchecked state keeps an empty circle. When selected, the circle becomes a primary-colored check mark while the content, icon foreground, and boundary use the same highlight.

```xml
<flourish:CheckBox
  Content="Cloud workspace"
  Icon="&#xE753;"
  IsChecked="{Binding UsesCloudWorkspace, Mode=TwoWay}"
  Variant="Vertical" />
```

`Icon` accepts arbitrary content and is rendered only by the Vertical layout. Text and other foreground-aware icon content inherit the selection foreground. Artwork that supplies its own explicit colors retains those colors.

## Hover feedback

Both layouts participate in the shared `HoverReveal` behavior by default. CheckBox follows the Outlined button interaction colors: hover uses the shared subtle reveal, while pressed uses the shared deeper pressed reveal. These layers are drawn behind the icon, content, and state indicator, so checked and indeterminate highlights remain visible. When hover animation is disabled or Windows requests reduced motion, the control preserves the same feedback without animation. An inherited `HoverReveal.IsEnabled="False"` disables the animation for a subtree while retaining this static fallback.

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

- The [CheckBox API](xref:ArkheideSystem.Flourish.Controls.CheckBox) lists inherited and declared members.
- The [CheckBoxVariant API](xref:ArkheideSystem.Flourish.Controls.CheckBoxVariant) lists the fixed layouts.
- [Motion](../articles/configure-motion.md) configures hover reveal and reduced-motion behavior.
- The [WPF CheckBox documentation](https://learn.microsoft.com/dotnet/desktop/wpf/controls/checkbox) explains the native selection model.
