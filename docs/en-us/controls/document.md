---
title: Document
description: Present multi-paragraph prose with consistent spacing and indentation.
---

# Document

`Document` presents several paragraphs as one reading surface. Use [Card](card.md) for one paragraph.

Use `Document` as the only control in a `Chunk` body. Do not mix sibling cards, buttons, or other controls into the same prose composition.

## Basic usage

Add one `Paragraph` child for each paragraph of text. `Document` accepts only `Paragraph` items; ordinary `TextBlock` controls and unrelated elements are rejected.

```xml
<flourish:Chunk Title="Why Flourish">
  <flourish:Document>
    <flourish:Paragraph Text="Flourish supplies a consistent visual foundation for WPF applications." />
    <flourish:Paragraph Text="Its layout controls express page hierarchy without repeating manual margins." />
    <flourish:Paragraph Text="Purpose-built content controls keep presentation and interaction responsibilities clear." />
  </flourish:Document>
</flourish:Chunk>
```

`Paragraph` uses Regular weight, normal style, wrapping, the standard foreground, and the `Large` tier. Global and page `Large` settings apply.

Bindings on `Paragraph.Text` and ordinary block-level text properties remain available. Use another Chunk body control when the content needs inline control composition, per-paragraph input behavior, or non-text children.

## Spacing and indentation

`Document` inserts the standard paragraph gap before every paragraph after the first. Every non-empty paragraph begins with four ordinary spaces, equivalent to two tab stops in the design rule. The indentation affects the rendered first line without changing the `Paragraph.Text` value.

Do not add literal leading spaces or per-paragraph margins; `Document` owns both behaviors. Empty text creates no visible paragraph content.

The surface supplies its background, border, padding, and separation from Chunk text. Do not compensate with negative margins.

## Related content

- [Chunk](chunk.md)
- [Card](card.md)
- [CodeSpace](code-space.md)
- [Presenter](presenter.md)
- [Typography](../articles/configure-font.md)
- [Document API](xref:ArkheideSystem.Flourish.Controls.Document) and [Paragraph API](xref:ArkheideSystem.Flourish.Controls.Paragraph)
