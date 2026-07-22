---
title: Document
description: Use Document with Paragraph children to present substantial prose with standardized size, spacing, and indentation.
---

# Document

`Document` presents several text paragraphs as one reading surface. It has no title, supporting content field, or visual variant. Use it for substantial prose; use [Card](card.md) when the copy is only one paragraph.

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

`Paragraph` is a normalized Flourish text block. It defaults to Regular weight, normal style, wrapping, the standard foreground, and the defined Large typography tier. `Document` keeps every rendered paragraph at that Large size, so global and page-level Large configuration applies consistently.

Bindings on `Paragraph.Text` and ordinary block-level text properties remain available. Use another Chunk body control when the content needs inline control composition, per-paragraph input behavior, or non-text children.

## Spacing and indentation

`Document` inserts the standard paragraph gap before every paragraph after the first. Every non-empty paragraph begins with four ordinary spaces, equivalent to two tab stops in the design rule. The indentation affects the rendered first line without changing the `Paragraph.Text` value.

Do not add literal leading spaces or per-paragraph margins; `Document` owns both behaviors. Empty text creates no visible paragraph content.

The Document surface has a transparent background, a rounded thin low-contrast border, and padding suited to continuous reading. It also supplies the standard outer separation from a Chunk title or optional content. Preserve these defaults instead of compensating with local negative margins.

## Related content

- [Chunk](chunk.md) defines the section that contains a Document.
- [Card](card.md) presents a title and one paragraph on a card surface.
- [CodeSpace](code-space.md) presents exact, copyable code text in a related outlined surface.
- [Presenter](presenter.md) combines copy, controls, and visual presentation content.
- [Typography](../articles/configure-font.md) describes the Large reading tier.
- The [Document API](xref:ArkheideSystem.Flourish.Controls.Document) and [Paragraph API](xref:ArkheideSystem.Flourish.Controls.Paragraph) list all members.
