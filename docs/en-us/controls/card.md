---
title: Card
description: Use Card, ActionCard, and OutputCard as focused content surfaces with clear presentation and interaction boundaries.
---

# Card

Cards are the basic content surfaces of the Flourish layout system. Choose the card type by whether the surface is informational, contains one local control, or presents operation output.

| Need | Control |
| --- | --- |
| A title, one paragraph, and optionally one icon | `Card` |
| One fixed card layout with a single local interactive control | `ActionCard` |
| Debug output, logs, progress, results, or failures | [OutputCard](output-card.md) |
| A complete card surface that responds to activation | [CardButton](button.md#cardbutton) |

Use [Document](document.md) when the content contains several paragraphs. Use [Presenter](presenter.md) for an image, several icons, or another composed visual.

## Card

`Card` presents optional `Title`, `Content`, and `Icon` regions. `Content` is one paragraph, and `Icon` is one icon-font glyph. Each `null` or empty region and its associated spacing collapse completely.

```xml
<flourish:Card
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="Reports"
  Content="Review generated reports and recent exports." />
```

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string?` | `""` | Optional card heading. |
| `Content` | `string?` | `""` | Optional single paragraph of supporting text. |
| `Icon` | `string?` | `null` | Optional single icon-font glyph. |
| `IconPosition` | `Dock` | `Left` | Places the icon at `Left`, `Top`, `Right`, or `Bottom`. |
| `Variant` | `Variant` | `Standard` | Selects the surface treatment. |
| `ContentHorizontalAlignment` | `HorizontalAlignment` | `Stretch` | Aligns the card composition horizontally. |
| `ContentVerticalAlignment` | `VerticalAlignment` | `Stretch` | Aligns the card composition vertically. |

`Icon` accepts at most one Unicode text element rendered with the configured icon font. Images, icon groups, and composed control trees belong in `Presenter.Presentation`. `Card` has no `Body` and cannot host arbitrary controls.

### Variants

| Variant | Use |
| --- | --- |
| `Standard` | Ordinary grouped information. This is the default. |
| `Tonal` | Supporting information with a quiet neutral fill. |
| `Filled` | Information that needs strong primary-color emphasis. |
| `Elevated` | Information that needs visual separation from its background. |

```xml
<UniformGrid Columns="2">
  <flourish:Card Variant="Standard" Title="Standard" Content="Ordinary information" />
  <flourish:Card Variant="Tonal" Title="Tonal" Content="Supporting information" />
  <flourish:Card Variant="Filled" Title="Filled" Content="Emphasized information" />
  <flourish:Card Variant="Elevated" Title="Elevated" Content="Separated information" />
</UniformGrid>
```

Cards may be arranged in two or more columns inside one `Chunk` when the available width keeps their text readable.

## ActionCard

`ActionCard` combines optional copy and one optional icon with a single local interactive control in `Body`. It is not itself clickable. Use it when only a button, combo box, check box, text box, or comparable local control should handle interaction.

`Variant` selects one of two fixed structures:

| `ActionCardVariant` | Arrangement |
| --- | --- |
| `Horizontal` | Icon on the left, stacked `Title` and `Content` next, and `Body` on the right. The row is vertically centered. This is the default. |
| `Vertical` | Icon, `Title`, `Content`, and `Body` stack from top to bottom and align to the left. |

```xml
<flourish:ActionCard
  Icon="&#xE790;"
  Title="Theme"
  Content="Choose the application appearance.">
  <flourish:FlourishComboBox
    Width="160"
    ItemsSource="{Binding Themes}"
    SelectedItem="{Binding Theme, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
</flourish:ActionCard>
```

```xml
<flourish:ActionCard
  Variant="Vertical"
  Icon="&#xE8A5;"
  Title="Generate report"
  Content="Create a report from the current workspace.">
  <flourish:Button
    Variant="Filled"
    Command="{Binding GenerateReportCommand}"
    Content="Generate" />
</flourish:ActionCard>
```

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string?` | `""` | Optional concise heading. |
| `Content` | `string?` | `""` | Optional concise supporting text. |
| `Icon` | `string?` | `null` | Optional single icon-font glyph. |
| `Body` | `object?` | `null` | One local interactive control and the default XAML content property. |
| `Variant` | `ActionCardVariant` | `Horizontal` | Selects the fixed horizontal or vertical structure. |

As with `Card`, empty `Title`, `Content`, and `Icon` regions collapse with their spacing. Keep `Body` to one interactive control rather than a panel of unrelated actions. Stack related ActionCards with `FlourishActionCardPeerMargin` so they read as one group.

## Related content

- [Chunk](chunk.md) defines the page section that contains cards.
- [Document](document.md) presents several paragraphs in one reading surface.
- [Presenter](presenter.md) presents images, icon groups, and composed visuals.
- [OutputCard](output-card.md) presents scrolling operation history.
- [Button](button.md) explains when the complete surface should be interactive.
- The [Variant API](xref:ArkheideSystem.Flourish.Controls.Variant), [Card API](xref:ArkheideSystem.Flourish.Controls.Card), [ActionCardVariant API](xref:ArkheideSystem.Flourish.Controls.ActionCardVariant), and [ActionCard API](xref:ArkheideSystem.Flourish.Controls.ActionCard) list all members.
