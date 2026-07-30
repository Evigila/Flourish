---
title: CheckBox
description: Present Boolean and optional three-state selections as compact rows or icon-led cards.
---

# CheckBox

`CheckBox` represents an independent selection. It follows the native WPF `CheckBox` state and event model while adding fixed Horizontal and Vertical layouts.

## Choose a layout

`Variant="Horizontal"` is the default and is suitable for ordinary settings. It places a circular state indicator before the content. An unchecked option displays an empty circle. A checked option displays a check mark and highlights the content, indicator, and control boundary.

```xml
<flourish:CheckBox
  Content="Enable notifications"
  IsChecked="{Binding NotificationsEnabled, Mode=TwoWay}" />
```

Use `Variant="Vertical"` for a card-shaped selection that benefits from an icon. It places the icon at the upper left, the content below it, and the selected indicator at the upper right. The indicator is hidden while the option is unchecked. When selected, the content, icon foreground, indicator, and boundary use the primary highlight.

```xml
<flourish:CheckBox
  Content="Cloud workspace"
  Icon="&#xE753;"
  IsChecked="{Binding UsesCloudWorkspace, Mode=TwoWay}"
  Variant="Vertical" />
```

`Icon` accepts arbitrary content and is rendered only by the Vertical layout. Text and other foreground-aware icon content inherit the selection foreground. Artwork that supplies its own explicit colors retains those colors.

## Three-state selections

Set `IsThreeState="True"` only when `null` represents an inherited, mixed, or unknown value. The indeterminate state replaces the check mark with a horizontal line and otherwise uses the same highlighted treatment as the checked state.

```xml
<flourish:CheckBox
  Content="Use inherited setting"
  IsChecked="{Binding InheritedSetting, Mode=TwoWay}"
  IsThreeState="True" />
```

| Variant | Unchecked | Checked | Indeterminate |
| --- | --- | --- | --- |
| Horizontal | Empty circular indicator | Check-mark indicator and highlighted content and boundary | Horizontal-line indicator and highlighted content and boundary |
| Vertical | Indicator hidden | Upper-right check-mark indicator and highlighted icon, content, and boundary | Upper-right horizontal-line indicator and highlighted icon, content, and boundary |

## Related content

- The [CheckBox API](xref:ArkheideSystem.Flourish.Controls.CheckBox) lists inherited and declared members.
- The [CheckBoxVariant API](xref:ArkheideSystem.Flourish.Controls.CheckBoxVariant) lists the fixed layouts.
- The [WPF CheckBox documentation](https://learn.microsoft.com/dotnet/desktop/wpf/controls/checkbox) explains the native selection model.
