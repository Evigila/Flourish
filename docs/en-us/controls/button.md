---
title: Button
description: Use Flourish Button, IconButton, CardButton, and WindowCaptionButton to communicate action hierarchy and intent.
---

# Button

The Flourish button family consists of one general-purpose text button and three specialized derivatives. They keep the command, click event, content, keyboard, and automation behavior of WPF `Button` while supplying Flourish theme, focus, and pointer feedback.

The complete visual boundary of every button is interactive. Choose the family member by the information shown on that boundary:

| Control | Use for |
| --- | --- |
| `Button` | Ordinary actions expressed with text. |
| `IconButton` | Icon-only actions or actions that need an icon before their text. |
| `CardButton` | Navigation or selection actions where the complete card is invokable. |
| `WindowCaptionButton` | Minimize, maximize, restore, and close actions in a window caption. |

## Button

`Button.Variant` selects the button's visual presentation and emphasis, not its size or layout. Its default is `Outlined`.

| `ButtonVariant` | When to use it |
| --- | --- |
| `Elevated` | An important action that needs separation from a patterned, image-based, or similarly busy background. Use elevation sparingly. |
| `Filled` | The highest-emphasis, primary action in a screen or action group. |
| `Tonal` | A prominent supporting action that needs more emphasis than an outline without competing with a filled primary action. |
| `Outlined` | A medium-emphasis secondary action with a visible boundary. This is the default. |
| `Text` | The lowest-emphasis action, such as a compact, inline, toolbar, or tertiary action. |
| `Danger` | A destructive or difficult-to-reverse action such as delete or reset. |

```xml
<WrapPanel>
  <flourish:Button
    Variant="Filled"
    Command="{Binding SaveCommand}"
    Content="Save" />
  <flourish:Button
    Variant="Tonal"
    Command="{Binding SaveDraftCommand}"
    Content="Save draft" />
  <flourish:Button
    Variant="Outlined"
    Command="{Binding CancelCommand}"
    Content="Cancel" />
  <flourish:Button
    Variant="Text"
    Command="{Binding LearnMoreCommand}"
    Content="Learn more" />
  <flourish:Button
    Variant="Elevated"
    Command="{Binding OpenPreviewCommand}"
    Content="Open preview" />
  <flourish:Button
    Variant="Danger"
    Command="{Binding DeleteCommand}"
    Content="Delete" />
</WrapPanel>
```

An action group should normally have only one `Filled` button. Use `Danger` for destructive intent; it is the destructive semantic option in Flourish's six-variant set. Let the containing layout control external placement, and do not use `Variant` to select structural dimensions.

`Tonal` uses a palette inspired by Fluent Web's brand color ramp and tuned independently for each theme. All non-danger variants share the inherited `HoverReveal.OverrideColor`; `Danger` is the only semantic color override. Elevated shadows are drawn on a background-only template layer so text remains ClearType-rendered.

## IconButton

`IconButton` adds an `Icon` property of type `object?` with a default of `null`. It accepts a glyph string from the Flourish icon font or any visual element. Inherited `Content` is optional and becomes the text label after the icon.

```xml
<flourish:IconButton
  Variant="Text"
  AutomationProperties.Name="Refresh"
  Command="{Binding RefreshCommand}"
  Icon="&#xE72C;"
  ToolTip="Refresh" />

<flourish:IconButton
  Variant="Filled"
  Command="{Binding AddCommand}"
  Content="Add item"
  Icon="&#xE710;" />
```

When `Content` is `null`, `IconButton` uses compact `32 × 32` geometry without padding. Give every icon-only button a visible `ToolTip` and an `AutomationProperties.Name` that identifies the action. Native WPF tooltips created from strings and explicit `FlourishToolTip` instances share the Flourish Temporary Overlay surface and Shell-aware placement behavior. Button-family controls also show their tooltip while disabled.

## CardButton

`CardButton` represents an interactive card rather than an appearance variant of an ordinary button. It uses an IconCard-like arrangement and is appropriate only when invoking the complete card is the action. For a local action at the right of a compact setting row, use `ListCard.ActionBody`; for an ordinary action in another layout, use `Button` or `IconButton`.

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | The card heading. |
| `Content` | inherited `object?` | `null` | The card description or other supporting content. |
| `Icon` | `object?` | `null` | The single icon shown by the card action. |
| `IconPosition` | `Dock` | `Top` | Places the icon at `Left`, `Top`, `Right`, or `Bottom`. |

```xml
<flourish:CardButton
  Command="{Binding OpenReportsCommand}"
  Content="Review generated reports and recent exports."
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="Reports" />
```

## WindowCaptionButton

`WindowCaptionButton` is reserved for window captions. It inherits `Icon`, `Variant`, and the standard WPF button contract from `IconButton`, but uses dedicated caption geometry with a default size of `46 × 40`.

Set a close action to `Variant="Danger"` and use `Text` for the other caption actions. The control supplies presentation and button interaction only; connect the actual window operation through `Command` or `Click`.

```xml
<flourish:WindowCaptionButton
  Variant="Danger"
  AutomationProperties.Name="Close"
  Command="{Binding CloseWindowCommand}"
  Icon="&#xE8BB;"
  ToolTip="Close" />
```

## Hover feedback and reduced motion

The button family participates in the public `HoverReveal` attached behavior. Prefer application-wide configuration through [Motion](../articles/configure-motion.md), including the operating system reduced-motion preference. Set the attached properties directly only for a local override:

```xml
<flourish:Button
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.OverrideColor="{DynamicResource FlourishPrimarySurfaceBrush}"
  Content="Preview" />
```

`HoverReveal.IsEnabled` defaults to `true` and inherits, `AnimationDuration` defaults to 140 milliseconds and inherits, and `OverrideColor` defaults to `null` and does not inherit. A `Danger` button supplies a danger-semantic hover color by default; a local `OverrideColor` takes precedence.

## Related content

- [Chunk](chunk.md) explains how to organize buttons within page sections.
- [Card](card.md) distinguishes non-interactive cards, ListCard action regions, and whole-card actions.
- [Motion](../articles/configure-motion.md) configures hover reveal and reduced motion.
- The [ButtonVariant API](xref:ArkheideSystem.Flourish.Controls.ButtonVariant), [Button API](xref:ArkheideSystem.Flourish.Controls.Button), [IconButton API](xref:ArkheideSystem.Flourish.Controls.IconButton), [CardButton API](xref:ArkheideSystem.Flourish.Controls.CardButton), and [WindowCaptionButton API](xref:ArkheideSystem.Flourish.Controls.WindowCaptionButton) list all members.
