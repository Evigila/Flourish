---
title: Card
description: Use Card, ListCard, and IconCard for longer information, compact configuration rows, and visual presentation on themed, non-interactive surfaces.
---

# Card

`Card` is a non-interactive surface for grouping one subject's longer explanatory or display-oriented information. It provides built-in title, supporting text, and body regions and adapts its colors to the active Flourish theme. Use `ListCard` instead for a compact setting or local configuration action.

> [!IMPORTANT]
> Use `CardButton` when clicking anywhere on the surface performs one action. Do not add mouse handlers to `Card`, `ListCard`, or `IconCard` to reproduce button behavior.

## Basic usage

Set `Title` and `Text` for an ordinary informational card. There is no need to construct text blocks for these roles.

```xml
<flourish:Card
  Title="Account status"
  Text="Your workspace is synchronized." />
```

Use `Body` for details, status, controls, or another composed WPF content tree. Wrap multiple children in a `Grid`, `StackPanel`, or another layout container.

```xml
<flourish:Card
  Title="Storage"
  Text="Manage files associated with this workspace.">
  <flourish:Card.Body>
    <StackPanel Margin="0,12,0,0">
      <ProgressBar Maximum="100" Value="64" />
      <flourish:Button
        Margin="0,12,0,0"
        HorizontalAlignment="Left"
        Command="{Binding ReviewFilesCommand}"
        Content="Review files" />
    </StackPanel>
  </flourish:Card.Body>
</flourish:Card>
```

### Card properties

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Variant` | `Variant` | `Standard` | Selects the semantic surface treatment. |
| `Title` | `string` | `""` | The card heading. |
| `Text` | `string` | `""` | Optional supporting text displayed with the title. |
| `Body` | `object?` | `null` | Optional details, controls, status, or another WPF content tree. |
| `ContentHorizontalAlignment` | `HorizontalAlignment` | `Stretch` | Controls horizontal alignment within the copy-and-body composition. Together with `Center` vertical alignment, it centers that composition as one unit. |
| `ContentVerticalAlignment` | `VerticalAlignment` | `Stretch` | Controls the copy/body arrangement: `Top` or `Stretch` keeps copy above Body, while `Bottom` places Body above copy. |

## Variants

`Variant` has four values:

| Variant | Use |
| --- | --- |
| `Standard` | The default surface for ordinary grouped information. |
| `Tonal` | A pale neutral gray surface for supporting information with lower emphasis. |
| `Filled` | A blue tonal surface for information that needs stronger visual emphasis. |
| `Elevated` | A raised surface for information that must remain visually separate from its background. |

```xml
<UniformGrid Columns="2">
  <flourish:Card Variant="Standard" Title="Standard" Text="Ordinary information" />
  <flourish:Card Variant="Tonal" Title="Tonal" Text="Supporting information" />
  <flourish:Card Variant="Filled" Title="Filled" Text="Emphasized information" />
  <flourish:Card Variant="Elevated" Title="Elevated" Text="Separated information" />
</UniformGrid>
```

`Filled` uses the same primary filled color family as `Button`. Set a local `Background` when another fill is required. A local value takes precedence over the variant default; use paired dynamic theme resources for background and foreground so the replacement remains readable in both themes.

```xml
<flourish:Card
  Variant="Filled"
  Background="{DynamicResource FlourishSecondaryBrush}"
  Foreground="{DynamicResource FlourishForegroundOnSecondaryBrush}"
  Title="Custom filled surface"
  Text="This replacement follows the active theme." />
```

## Arrange copy and Body

The built-in copy consists of `Title` and `Text`. `ContentHorizontalAlignment` and `ContentVerticalAlignment` organize that copy with `Body`; the parent layout still controls where the card itself is placed.

| Settings | Arrangement |
| --- | --- |
| Default, `Top`, or `Stretch` vertical alignment | Copy is above `Body`. |
| `ContentVerticalAlignment="Bottom"` | `Body` is above the copy. |
| Both content alignments set to `Center` | Copy and `Body` are centered as one composition. |

```xml
<flourish:Card
  MinHeight="180"
  ContentHorizontalAlignment="Center"
  ContentVerticalAlignment="Center"
  Title="Centered status"
  Text="Copy and body are centered together.">
  <flourish:Card.Body>
    <ProgressBar Width="160" Maximum="100" Value="64" />
  </flourish:Card.Body>
</flourish:Card>
```

## Operation output

Do not use an ordinary `Card` as an output or result surface. Append raw messages, progress, completed results, and failures to a dedicated [OutputCard](output-card.md). `OutputCard` owns its compact scrolling viewport and has no `Title`, `Text`, or arbitrary `Body`; keep explanatory information in the action surface or the containing `Chunk`.

## ListCard

`ListCard` inherits from `Card` and represents one compact, independent configuration option. The surface remains non-interactive while the local control in `Body` handles input or invocation. Reserve ordinary `Card` and `IconCard` surfaces for longer explanatory or display-oriented content.

Its arrangement is fixed: an optional icon or image `Presenter` stays on the left with deliberately generous horizontal breathing room on both sides, `Title` and the short `Text` description form a vertical copy stack in the center, and `Body` stays on the right. Presenter, copy, and Body are all vertically centered. Do not collapse the presenter spacing with local margins; Card content-alignment properties do not reposition these regions.

Keep `Title` and `Text` concise. Each is limited to one line and overflows with an ellipsis, so rewrite copy that would depend on wrapping. `Body` must contain exactly one interactive control; do not combine several inputs or actions in a panel inside one ListCard.

```xml
<flourish:ListCard
  Title="Theme"
  Text="Choose the appearance used by the application.">
  <flourish:ListCard.Presenter>
    <flourish:FlourishTextBlock
      AutomationProperties.Name="Theme"
      Role="Icon"
      Text="&#xE790;" />
  </flourish:ListCard.Presenter>
  <flourish:ListCard.Body>
    <flourish:FlourishComboBox
      Width="160"
      ItemsSource="{Binding Themes}"
      SelectedItem="{Binding Theme, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
  </flourish:ListCard.Body>
</flourish:ListCard>
```

### ListCard properties

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Inherited concise, single-line setting heading; overflow uses an ellipsis. |
| `Text` | `string` | `""` | Inherited concise, single-line description below the title; overflow uses an ellipsis. |
| `Presenter` | `object?` | `null` | Optional icon, image, or other visual displayed on the left. |
| `Body` | `object?` | `null` | Exactly one local interactive control displayed on the right. |
| `Variant` | `Variant` | `Standard` | Inherited property that is always coerced to `Standard`; ListCard has no visual variants. |

Stack related ListCards so every row fills its column, and use the compact `FlourishListCardPeerMargin` between rows so they read as one related group without merging their surfaces. Use one row for one independent function. Do not mix `ListCard` with any other card type in that same column. Prefer a single-column `Chunk` containing only ListCards. When the section also needs output, another column in the same Chunk may hold an `OutputCard`. Place both columns in the same auto-sized `Grid` row, let the complete ListCard column determine the row height, and stretch `OutputCard` to the arranged height. Its message history does not increase its desired height; overflow remains inside the scrolling viewport.

Prefer `FlourishComboBox`, `FlourishCheckBox`, and `Button` in `Body`; use `FlourishTextBox` and `FlourishRadioButton` when the option requires them. Selections, toggles, and edits must apply immediately. Never add a separate Apply action to a ListCard.

## IconCard

`IconCard` shares Card's title, text, body, alignment, and variant contract. Its `Presenter` can hold an icon, image, illustration, preview, or any other WPF visual content. It remains non-interactive.

```xml
<flourish:IconCard
  PresenterMode="Split"
  PresenterPosition="Left"
  Title="Reports"
  Text="Review generated reports and recent exports.">
  <flourish:IconCard.Presenter>
    <TextBlock
      AutomationProperties.Name="Reports"
      FontFamily="Segoe Fluent Icons"
      FontSize="{DynamicResource FlourishFontSizeStandard}"
      Text="&#xE8A5;" />
  </flourish:IconCard.Presenter>
  <flourish:IconCard.Body>
    <TextBlock Text="12 reports are ready." />
  </flourish:IconCard.Body>
</flourish:IconCard>
```

### IconCard properties

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Presenter` | `object?` | `null` | The icon, image, illustration, preview, or other visual content. |
| `PresenterMode` | `PresenterMode` | `Split` | Selects a separate presenter region or a full-card overlay. |
| `PresenterPosition` | `PresenterPosition` | `Left` | Locates `Presenter` in `Split` mode. It is ignored in `Overlay` mode. |

In `Split` mode, `PresenterPosition` always describes the presenter's location. Copy and `Body` remain together on the opposing side.

| Position | Presenter placement | Copy and Body |
| --- | --- | --- |
| `Left` | Centered along the left side. | Opposing side, arranged vertically. |
| `LeftTop` | At the upper-left side. | Opposing lower-right side, arranged vertically. |
| `LeftBottom` | At the lower-left side. | Opposing upper-right side, arranged vertically. |
| `Top` | Centered along the top. | Opposing bottom side, arranged horizontally. |
| `Bottom` | Centered along the bottom. | Opposing top side, arranged horizontally. |
| `Right` | Centered along the right side. | Opposing side, arranged vertically. |
| `RightTop` | At the upper-right side. | Opposing lower-left side, arranged vertically. |
| `RightBottom` | At the lower-right side. | Opposing upper-left side, arranged vertically. |

## Overlay presenters

In `Overlay` mode, `Presenter` fills the card while copy and `Body` are rendered above it. `PresenterPosition` has no effect, and copy/Body use the same vertical arrangement as an ordinary Card. Choose or compose a presenter that keeps all overlaid content readable in both themes.

```xml
<flourish:IconCard
  MinHeight="240"
  PresenterMode="Overlay"
  Title="Project preview"
  Text="The presenter fills the complete card.">
  <flourish:IconCard.Presenter>
    <Image Source="Assets/project-preview.png" Stretch="UniformToFill" />
  </flourish:IconCard.Presenter>
  <flourish:IconCard.Body>
    <TextBlock Text="Updated today" />
  </flourish:IconCard.Body>
</flourish:IconCard>
```

## Related content

- [Conception](../conception/index.md) defines how cards participate in a consistent page hierarchy.
- [Chunk](chunk.md) explains how to place cards in page sections.
- [OutputCard](output-card.md) explains how to append operation messages without growing a peer layout.
- [Button](button.md) explains when an information surface should instead be an interactive `CardButton`.
- The [Variant API](xref:ArkheideSystem.Flourish.Controls.Variant), [Card API](xref:ArkheideSystem.Flourish.Controls.Card), [ListCard API](xref:ArkheideSystem.Flourish.Controls.ListCard), [IconCard API](xref:ArkheideSystem.Flourish.Controls.IconCard), [PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode), and [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) list all members.
