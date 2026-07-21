---
title: Card
description: Use Card, IconCard, ListCard, and OutputCard as focused, non-nesting content surfaces.
---

# Card

Cards are the basic content surfaces of the Flourish layout system. An ordinary `Card` presents an optional `Title` and one optional block of `MainText`. Neither region creates spacing when it is empty or `null`.

> [!IMPORTANT]
> `Card`, `IconCard`, and `ListCard` do not have a general-purpose `Body`. They cannot host arbitrary nested content. Use [Presenter](presenter.md) for composed presentation content and [CardButton](button.md#cardbutton) when the complete surface is an action.

## Choose a card

| Need | Control |
| --- | --- |
| A title and one paragraph of text | `Card` |
| A title, one paragraph, and one freely positioned icon | `IconCard` |
| A compact row with a left icon, stacked copy, and one right-side action | `ListCard` |
| Debug output, logs, progress, results, or failures | [OutputCard](output-card.md) |

Use [Paragraph](paragraph.md) instead of `Card` when the content contains several paragraphs. Use [Presenter](presenter.md) for an image, several icons, or any composed visual.

## Card

```xml
<flourish:Card
  Title="Account status"
  MainText="Your workspace is synchronized." />
```

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Optional card heading. |
| `MainText` | `string` | `""` | Optional single paragraph below the heading. |
| `Variant` | `Variant` | `Standard` | Selects the surface treatment. |
| `ContentHorizontalAlignment` | `HorizontalAlignment` | `Stretch` | Aligns the complete copy group horizontally. |
| `ContentVerticalAlignment` | `VerticalAlignment` | `Stretch` | Aligns the complete copy group vertically. |

`Title` and `MainText` are independent. If either is absent, its presenter and associated spacing collapse completely. A card with only `MainText` is therefore valid, but a card with no copy has nothing to present.

### Variants

| Variant | Use |
| --- | --- |
| `Standard` | Ordinary grouped information. This is the default. |
| `Tonal` | Supporting information with a quiet neutral fill. |
| `Filled` | Information that needs strong primary-color emphasis. |
| `Elevated` | Information that needs visual separation from its background. |

```xml
<UniformGrid Columns="2">
  <flourish:Card Variant="Standard" Title="Standard" MainText="Ordinary information" />
  <flourish:Card Variant="Tonal" Title="Tonal" MainText="Supporting information" />
  <flourish:Card Variant="Filled" Title="Filled" MainText="Emphasized information" />
  <flourish:Card Variant="Elevated" Title="Elevated" MainText="Separated information" />
</UniformGrid>
```

Cards may be arranged in two or more columns inside one `Chunk` when the available width keeps their text readable.

## IconCard

`IconCard` adds exactly one `Icon` to the `Card` contract. `IconPosition` uses the WPF `Dock` values `Left`, `Top`, `Right`, and `Bottom`; its default is `Left`. The icon and copy remain one freely aligned card composition.

```xml
<flourish:IconCard
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="Reports"
  MainText="Review generated reports and recent exports." />
```

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Icon` | `string?` | `null` | The single icon-font glyph presented by the card. |
| `IconPosition` | `Dock` | `Left` | Places the icon at `Left`, `Top`, `Right`, or `Bottom`. |

`Icon` accepts one Unicode text element rendered with the configured icon font. Images, icon groups, and composed control trees are rejected; those belong in `Presenter.Presentation`. An absent icon collapses together with its spacing.

`IconCard` inherits `Title`, `MainText`, `Variant`, and the copy alignment properties from `Card`. It has no `Body`, `Presentation`, `PresenterMode`, or overlay layout.

## ListCard

`ListCard` represents one compact, independent setting or local action. Its layout is fixed: an optional `Icon` is on the left, `Title` and `MainText` are stacked in the center, and `ActionBody` is on the right. The row is centered vertically and left-oriented overall.

```xml
<flourish:ListCard
  Icon="&#xE790;"
  Title="Theme"
  MainText="Choose the application appearance.">
  <flourish:FlourishComboBox
    Width="160"
    ItemsSource="{Binding Themes}"
    SelectedItem="{Binding Theme, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
</flourish:ListCard>
```

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Icon` | `string?` | `null` | Optional single icon-font glyph in the fixed left region. |
| `Title` | `string` | `""` | Optional concise, single-line row heading. |
| `MainText` | `string` | `""` | Optional concise, single-line description. |
| `ActionBody` | `object?` | `null` | One local interactive control in the right action region and the default XAML content property. |
| `Variant` | `Variant` | `Standard` | Always coerced to `Standard`; `ListCard` has no alternate surface variants. |

Keep `Title` and `MainText` brief because each is restricted to one line with ellipsis overflow. `ActionBody` should contain one button, combo box, check box, text box, radio button, or comparable local control—not a panel of several actions. Changes should apply immediately; do not add a separate Apply button.

Stack related ListCards in a single column and use the compact `FlourishListCardPeerMargin` between rows so they read as one group. Do not interleave other card types in that column. An adjacent column may contain an `OutputCard` when the settings produce operation history.

## Related content

- [Chunk](chunk.md) defines the page section that contains cards.
- [Paragraph](paragraph.md) presents several paragraphs without a card surface.
- [Presenter](presenter.md) presents images, icon groups, and composed visuals.
- [OutputCard](output-card.md) presents scrolling operation history.
- [Button](button.md) explains when a surface should be interactive.
- The [Variant API](xref:ArkheideSystem.Flourish.Controls.Variant), [Card API](xref:ArkheideSystem.Flourish.Controls.Card), [IconCard API](xref:ArkheideSystem.Flourish.Controls.IconCard), and [ListCard API](xref:ArkheideSystem.Flourish.Controls.ListCard) list all members.
