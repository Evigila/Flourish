---
title: Button
description: Use Button, CardButton, and WindowCaptionButton to express ordinary, card-shaped, and window-caption actions.
---

# Button

The Flourish button family keeps the command, click, keyboard, focus, and automation behavior of WPF `Button` while supplying consistent theme and pointer feedback. The complete visual boundary is interactive.

| Control | Use for |
| --- | --- |
| `Button` | Ordinary actions, with or without one icon. |
| `CardButton` | Actions whose complete card-shaped surface is invokable. |
| `WindowCaptionButton` | Minimize, maximize, restore, and close actions in a window caption. |

## Button

`Button` exposes optional `Icon` and inherited `Content` regions. Use text alone for the usual action, combine text with an icon when the icon reinforces the label, or omit `Content` for an icon-only action. A `null` or empty region and its associated spacing collapse completely.

```xml
<WrapPanel>
  <flourish:Button
    Variant="Filled"
    Command="{Binding SaveCommand}"
    Content="Save" />

  <flourish:Button
    Variant="Tonal"
    Command="{Binding AddCommand}"
    Content="Add item"
    Icon="&#xE710;" />

  <flourish:Button
    Variant="Text"
    AutomationProperties.Name="Refresh"
    Command="{Binding RefreshCommand}"
    Icon="&#xE72C;"
    ToolTip="Refresh" />
</WrapPanel>
```

`Icon` accepts an icon glyph or another WPF object. Icon-only buttons use compact square geometry. Give every icon-only action a visible `ToolTip` and an `AutomationProperties.Name` that identifies the action.

`Button.Variant` selects action emphasis, not size or layout. Its default is `Outlined`.

| `ButtonVariant` | When to use it |
| --- | --- |
| `Elevated` | An important action that needs separation from a busy background. |
| `Filled` | The highest-emphasis primary action in a screen or action group. |
| `Tonal` | A prominent supporting action that should not compete with the primary action. |
| `Outlined` | A medium-emphasis secondary action. This is the ordinary `Button` default. |
| `Text` | The lowest-emphasis inline, toolbar, or tertiary action. |
| `Danger` | A destructive or difficult-to-reverse action. |
| `Standard` | The neutral card surface used by `CardButton`; do not use it as an ordinary button treatment. |

An action group should normally have only one `Filled` button. Use `Danger` for destructive intent. Let the containing layout control determine placement rather than using `Variant` to select structural dimensions.

With `UseTips` active, Button-family hints use the Flourish temporary Overlay surface and Shell-aware placement. Without it, the same hint content uses the native WPF tooltip appearance. Tooltips attached to native WPF and third-party controls remain unchanged.

## CardButton

`CardButton` is an interactive card. Use it when invoking the complete surface is the action. When only one control inside a card should be interactive, use [ActionCard](card.md#actioncard) instead.

Like `Card`, it exposes optional `Title`, `Content`, and `Icon` regions. Each absent region and its spacing collapse completely. `IconPosition` places the icon at `Left`, `Top`, `Right`, or `Bottom`; its default is `Top`.

```xml
<flourish:CardButton
  Variant="Elevated"
  Command="{Binding OpenReportsCommand}"
  Content="Review generated reports and recent exports."
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="Reports" />
```

Use the Card-equivalent visual treatments when `CardButton` participates in a card layout:

| `ButtonVariant` | Card treatment |
| --- | --- |
| `Standard` | Ordinary neutral card. This is the `CardButton` default. |
| `Tonal` | Quiet neutral fill. |
| `Filled` | Strong primary-color emphasis. |
| `Elevated` | Visual separation through elevation. |

The control still inherits the complete `Button` interaction contract, including `Command`, `CommandParameter`, `Click`, keyboard activation, and enabled state.

## WindowCaptionButton

`WindowCaptionButton` is reserved for window captions and has dedicated caption geometry. Connect the actual window operation through `Command` or `Click`.

Set a close action to `Variant="Danger"` and use `Text` for the other caption actions.

```xml
<flourish:WindowCaptionButton
  Variant="Danger"
  AutomationProperties.Name="Close"
  Command="{Binding CloseWindowCommand}"
  Icon="&#xE8BB;"
  ToolTip="Close" />
```

## Hover feedback and reduced motion

The button family participates in the public `HoverReveal` attached behavior. Prefer application-wide configuration through [Motion](../articles/configure-motion.md), including the operating system reduced-motion preference. Set attached properties directly only for a local override:

```xml
<flourish:Button
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.OverrideColor="{DynamicResource FlourishPrimarySurfaceBrush}"
  Content="Preview" />
```

`HoverReveal.IsEnabled` defaults to `true` and inherits, `AnimationDuration` defaults to 140 milliseconds and inherits, and `OverrideColor` defaults to `null` and does not inherit. A `Danger` button supplies a danger-semantic hover color by default; a local `OverrideColor` takes precedence.

## Related content

- [Chunk](chunk.md) explains how to organize actions within page sections.
- [Card](card.md) distinguishes non-interactive cards, local ActionCard controls, and whole-card actions.
- [Motion](../articles/configure-motion.md) configures hover reveal and reduced motion.
- The [ButtonVariant API](xref:ArkheideSystem.Flourish.Controls.ButtonVariant), [Button API](xref:ArkheideSystem.Flourish.Controls.Button), [CardButton API](xref:ArkheideSystem.Flourish.Controls.CardButton), and [WindowCaptionButton API](xref:ArkheideSystem.Flourish.Controls.WindowCaptionButton) list all members.
