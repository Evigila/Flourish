---
title: Conception
description: Apply Flourish page hierarchy, surface semantics, alignment, theming, accessibility, and consistency rules when composing an interface.
---

# Page and control conception

Flourish treats layout and control selection as parts of the application's meaning. A page should communicate its hierarchy before color or decoration is considered: `Chunk` identifies a section, cards group information, and buttons identify actions. Use the following rules as the default design contract for application pages.

## Establish the page hierarchy with Chunk

Every ordinary section belongs in a `Chunk`. A page may begin with one `ChunkHero` when it needs a leading focal message; otherwise, begin with the first `Chunk`. Do not create a second hierarchy by placing free-standing headings and manually spaced panels beside chunks.

A `Chunk` always spans the available content width. Never place two chunks beside each other in the same row. When related information should appear side by side, use one full-width chunk and arrange several cards inside its body. Use separate full-width chunks only when the topics are independent enough to need separate section headings.

Use the three `Chunk` regions deliberately:

| Region | Rule |
| --- | --- |
| `ChunkTitle` | Give the section a concise, direct heading. It is the default label users scan to understand the page structure. Prefer a noun phrase or short action-oriented phrase over a sentence. |
| `ChunkDescription` | Use at most one short sentence that explains the section's purpose. Omit it when the title already makes the purpose clear. |
| `ChunkBody` | Place the section's actual information and controls here. Detailed explanations do not belong in `ChunkDescription`. |

```xml
<flourish:Chunk
  ChunkTitle="Storage"
  ChunkDescription="Review usage and manage retained files.">
  <UniformGrid Columns="2">
    <!-- Detailed information belongs in cards inside ChunkBody. -->
  </UniformGrid>
</flourish:Chunk>
```

Keep detailed content in a `Card` through its `Title`, `Text`, and `Body`. A plain `TextBlock` is appropriate when the information is continuous prose and a surface would add no meaningful grouping, but it must still live inside `ChunkBody`. Do not turn `ChunkDescription` into a paragraph or use several descriptions to avoid structuring the body.

## Assign one subject or behavior to each surface

A card should be understandable as one unit. When a section contains several independent behaviors, split them into separate cards instead of placing several unrelated controls and explanations in one large surface. This makes scanning, resizing, keyboard navigation, and future rearrangement more predictable.

Choose the surface by semantics:

| Control | Meaning |
| --- | --- |
| `Card` | A non-interactive information surface. Buttons, links, or inputs may appear in its `Body`, but the card surface itself does not invoke an action. |
| `CardButton` | One action represented by the entire card. Use it for navigation, selection, or another single invocation. Do not place independent interactive controls inside it. |
| `IconCard` | A non-interactive card whose icon, image, illustration, or preview is part of the information. `Presenter` provides visual context; it does not make the card clickable. |

Give each card exactly one `Title` and one subject. Treat `Text` as the card's Description region and place all supporting explanatory copy there. Do not add another heading or explanatory paragraph inside `Body`. Necessary field labels, option labels, list data, and result values may remain in `Body` because they label or constitute the content rather than create a second copy hierarchy.

If one card would require several subjects or headings, split it into peer cards. When an action produces changing response text, keep the action in its own card and place an adjacent `Output` card for ongoing or raw output, or a `Result` card for a completed outcome. Both cards belong in the same full-width chunk.

## Use Variant as semantic hierarchy

`Variant` communicates purpose and emphasis; it is not a shortcut for choosing a favorite color or changing dimensions. Apply the same meaning consistently throughout the application.

| Card variant | Semantic use |
| --- | --- |
| `Standard` | The default grouped-information surface. Start here. |
| `Tonal` | Quiet supporting information with lower emphasis. |
| `Filled` | Information that needs stronger emphasis through the primary tonal color. |
| `Elevated` | A surface that must remain visibly separated from a complex or similarly colored background. Use sparingly. |

Button variants describe action hierarchy separately. An action group normally has one `Filled` primary action, supporting actions use lower-emphasis variants, and destructive actions use `Danger`. Do not use a card variant to imply clickability, and do not use a button variant to solve external layout or sizing.

## Respect themes and color roles

Use Flourish theme resources through `DynamicResource` when a local override is necessary. Avoid hard-coded colors chosen for only one theme. A `Filled` card may override `Background`, but the replacement must preserve readable foreground, border, disabled, and focus states in both light and dark themes.

Color must reinforce meaning rather than carry it alone. Pair status color with text or an accessible label. For `Overlay` presenters, choose an image or composed visual that remains legible behind the overlay scrim in both themes; test the brightest and darkest regions of the image, not only its average color.

## Keep alignment responsibilities separate

The parent `Grid`, `StackPanel`, or other layout container controls where a control sits on the page. A control's content-alignment properties organize its internal copy and body. Do not use `Variant`, presenter position, or repeated local margins to perform the parent's layout work.

Cards in the same row must have the same height. When cards wrap onto several rows, use the same spacing between rows as between columns. Prefer a shared spacing resource or a layout container that enforces one consistent gap instead of unrelated per-card margins.

For `Card`, the normal arrangement places the copy above `Body`. `ContentVerticalAlignment="Bottom"` reverses that order, and setting both content alignments to `Center` centers the copy-and-body composition as one unit.

For `IconCard` in `Split` mode, `PresenterPosition` always names the presenter's location; the copy and `Body` stay together on the opposing side. `Top` and `Bottom` positions arrange the copy and body horizontally in that opposing region. The other split positions arrange them vertically. In `Overlay` mode, the presenter fills the surface, `PresenterPosition` is ignored, and the copy/body composition returns to the card-style vertical arrangement.

The same presenter-centered language applies to `ChunkHero`: `PresenterPosition="Right"` means the presenter is on the right, not that the text is on the right.

## Design for accessibility from the start

- Preserve a clear heading hierarchy. Concise, distinct `ChunkTitle` and card `Title` values make the page easier to scan visually and through assistive technology.
- Do not communicate state or action through color, position, or an icon alone. Supply visible text or an equivalent accessible name.
- Give every icon-only `IconButton` a useful `AutomationProperties.Name` and a visible tooltip. The name must describe the action, not the glyph.
- Use `CardButton` only when the complete surface is one action so its focus and invocation semantics match its visual boundary.
- Keep keyboard focus order consistent with visual and reading order. Splitting independent behaviors into separate cards usually makes this automatic.
- Verify text contrast, focus indication, disabled states, and zoomed or enlarged text in both themes. Avoid fixed heights that clip localized or wrapped copy.

## Consistency checklist

Before considering a page complete, verify that:

1. Every ordinary page section is a full-width `Chunk`, with at most one leading `ChunkHero` and no side-by-side chunks.
2. Every `ChunkTitle` is concise and every `ChunkDescription` is no more than one short purpose sentence.
3. Every card has one title and keeps all explanatory copy in its `Text` Description region.
4. Independent subjects or behaviors are split into independent cards, and changing output uses an adjacent Output or Result card.
5. Cards in the same row have the same height, and row and column gaps use the same spacing.
6. Non-interactive, whole-card action, and visual-presenter cases use `Card`, `CardButton`, and `IconCard` respectively.
7. Variants retain the same semantic meaning across pages.
8. Theme overrides work in light and dark modes and do not make color the only carrier of meaning.
9. Internal alignment, presenter position, reading order, and keyboard order agree.

## Related content

- [Chunk](../controls/chunk.md) documents the page-section controls and their properties.
- [Card](../controls/card.md) documents card variants, `Body`, and presenter arrangements.
- [Button](../controls/button.md) documents action hierarchy and specialized button controls.
