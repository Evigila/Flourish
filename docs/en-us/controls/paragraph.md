---
title: Paragraph
description: Use Paragraph to present several consistently spaced and indented text paragraphs as one Chunk body.
---

# Paragraph

`Paragraph` presents a sequence of text paragraphs without a background, border, title, description, or visual variant. Use it when a section contains substantial multi-paragraph copy; use [Card](card.md) when the copy is only one paragraph.

Use `Paragraph` as the only control in a `Chunk` body. Do not mix sibling cards, buttons, or other controls into the same paragraph composition.

## Basic usage

Add any number of `TextBlock` children directly to `Paragraph`. Each direct `TextBlock` is one paragraph.

```xml
<flourish:Chunk Title="Why Flourish">
  <flourish:Paragraph>
    <TextBlock Text="Flourish supplies a consistent visual foundation for WPF applications." />
    <TextBlock Text="Its layout controls express page hierarchy without repeating manual margins." />
    <TextBlock Text="Purpose-built content controls keep presentation and interaction responsibilities clear." />
  </flourish:Paragraph>
</flourish:Chunk>
```

`Paragraph` applies the Standard typography tier by default and enables wrapping for a `TextBlock` that does not set `TextWrapping` explicitly.

Direct `TextBlock` items are treated as text sources and rendered through a dedicated text proxy. The control preserves the source `Text` value, ordinary data-context bindings, block-level typography, and wrapping. Inline formatting, visual-tree-relative bindings, per-item layout, and input handlers are not part of this contract; use another Chunk body control when those behaviors are required.

## Spacing and indentation

Every paragraph after the first receives the standard paragraph gap. Each non-empty paragraph is rendered with exactly four ordinary spaces before its text. The spaces use that paragraph's effective font family and size, so the indent follows the current font metrics and applies only to the first wrapped line. The source `Text` value is not changed. Do not add literal leading spaces or per-item margins; the control owns both behaviors.

The first paragraph has no preceding vertical gap. Empty content creates no placeholder. `Paragraph` does not render a surface, even when `Background`, `BorderBrush`, or `BorderThickness` receives an inherited or local value.

> [!NOTE]
> `FlourishTextRole.Paragraph` styles one `FlourishTextBlock`. The `Paragraph` control described here is the multi-paragraph container. Use the control when you need automatic spacing and indentation across several text blocks.

## Related content

- [Chunk](chunk.md) defines the required section that contains a Paragraph.
- [Card](card.md) presents a title and a single paragraph on a card surface.
- [Presenter](presenter.md) combines copy, controls, and visual presentation content.
- [Typography](../articles/configure-font.md) describes the Standard font-size default.
- The [Paragraph API](xref:ArkheideSystem.Flourish.Controls.Paragraph) lists all inherited and declared members.
