---
title: Design principles
description: Apply Flourish page and control composition rules.
---

# Page and control design principles

Use hierarchy before decoration: chunks define subjects, content controls present information, and buttons express actions.

## Establish one page hierarchy

Use `PageBody` with one leading `HeaderChunk` and full-width `Chunk` sections. Other content belongs inside their bodies. Split and Overlay Presenters are full-width; only TopDown may share a row.

Transient Shell surfaces do not require `PageBody` or `HeaderChunk`, but still follow these control and accessibility rules.

Use the three `Chunk` regions deliberately:

| Region | Rule |
| --- | --- |
| `Title` | Required. State the section's subject in a concise heading. |
| `Content` | Optional. Add only essential context that a good title cannot cover. |
| `Body` | Required. Put the actual content control or layout tree here. `Chunk` does not present content itself. |

`HeaderChunk` inherits [Presenter](../controls/presenter.md). Supply `Title`, `Content`, `PresenterMode`, and `PresenterPosition`; use `Body` for controls and `Presentation` for visuals.

## Use typography by role

Unspecified text uses `Standard`; use the other five tiers only for their roles.

| Tier | Intended role |
| --- | --- |
| `Small` | Compact supporting text, including navigation group labels and `OutputCard` output. |
| `Standard` | Default body and control text. Use this whenever no specialized role applies. |
| `StandardIcon` | Ordinary icon glyphs, including button icons. Card and display icons use `LargeIcon`. |
| `Large` | Card-title-level emphasis and Document paragraphs. |
| `ExtraLarge` | The section-title family, including `Chunk.Title`. |
| `HeaderSize` | The page title in `HeaderChunk` only. |

Do not select a larger tier merely to make content more noticeable. Express hierarchy through the correct control and text role.

## Choose the content control by purpose

| Need | Control |
| --- | --- |
| One title, one paragraph, and optionally one icon | `Card` |
| Several text paragraphs | `Document` containing `Paragraph` elements |
| An image, several icons, a preview, or composed presentation | `Presenter` |
| One local control in a fixed horizontal or vertical card structure | `ActionCard` |
| Raw output, logs, progress, results, or failures | `OutputCard` |
| One action whose complete card surface is clickable | `CardButton` |

Give each surface one subject or behavior.

## Apply spacing and collapse consistently

Use layout defaults instead of unrelated local margins.

- Keep the large standard separation between every pair of chunks and between `HeaderChunk` and the first `Chunk`.
- Keep related ActionCards closer together with `FlourishActionCardPeerMargin` so they read as one group.
- Use `FlourishPresenterPeerMargin` after the first Presenter in a vertical Presenter stack.
- Let `Document` create the gap between its Paragraph elements.
- When an optional region is empty or `null`, its presenter and associated spacing collapse completely.
- Use one consistent row and column gap when cards wrap into a grid.

## Keep cards focused

`Card` presents optional `Title`, one paragraph, and one icon, with no arbitrary `Body`. Use `Presenter` for composed visuals.

`ActionCard` adds one interactive `Body`. `Horizontal` places it beside the copy; `Vertical` stacks it below. Keep `Body` to one control.

`OutputCard` has no copy regions or arbitrary body. Append with `WriteLine`, and let adjacent actions determine row height.

Use `CardButton` for a whole-surface action and `ActionCard` for one contained action.

## Use Document for continuous prose

`Document` accepts only `Paragraph` children and supplies the `Large` reading style.

`Document` owns paragraph gaps and four-space first-line indentation. Use it as a chunk's only body; choose Card for one paragraph and ActionCard or Presenter for controls or visuals.

Use `CodeSpace` for exact copyable source or commands; it supplies its own style and copy action.

## Use Presenter for rich presentation

`Presenter` separates copy, supporting `Body` controls, and rich `Presentation` content. Declare `PresenterMode` and `PresenterPosition`.

- `Split` places copy plus body on one side and the presentation surface on the other. `PresenterPosition="Left"` is the default arrangement; `Right` reverses the regions.
- `TopDown` places presentation above a left-aligned copy-and-body region and is the only ordinary Presenter mode that may share a multi-column row.
- `Overlay` fills the Presenter with presentation content and draws copy plus body above it.

Only `Presentation` supplies the neutral rounded background. Copy and `Body` remain transparent and left-aligned.

`Presentation` is Presenter's default XAML content; assign `Body` explicitly. `HeaderChunk` defaults to `Body` and is always full-width.

## Use the button family by action

Every button's complete boundary is interactive.

| Control | Rule |
| --- | --- |
| `Button` | Ordinary action. `Icon` and `Content` are both optional, so it also covers icon-only and icon-plus-text actions. |
| `CardButton` | Whole-card action with Card-like content and surface variants. |
| `WindowCaptionButton` | Window caption and title-bar actions only. |

Empty regions collapse. Do not imitate buttons with pointer handlers on cards. Give icon-only buttons a tooltip and `AutomationProperties.Name`.

Use one `Filled` primary action per group and `Danger` for destructive actions. Card variants do not imply clickability.

## Compose floating and scrollable content

`Overlay` supplies floating chrome and dismissal. Use a vertical ActionCard for icon, copy, and one action; use a custom layout when necessary.

`DataGrid` consumes mouse-wheel input only while its internal viewport can move in the requested direction. At a vertical boundary—or when no internal range exists—the wheel continues to the containing PageBody. Do not add another ScrollViewer around the grid.

## Preserve themes and accessibility

Use Flourish `DynamicResource` values. Do not rely on color, position, or icon alone, and verify Overlay text contrast in both themes.

Align focus, reading, and visual order. Avoid clipping localized or enlarged text, and name OutputCard when surrounding labels are insufficient.

## Consistency checklist

Before considering a page complete, verify that:

1. PageBody is the root, with one leading HeaderChunk followed only by full-width Chunks.
2. Every Chunk has a concise Title and real Body; Content appears only when needed and empty optional regions leave no gap.
3. Every HeaderChunk and Presenter explicitly declares Title, Content, PresenterMode, and PresenterPosition.
4. Unspecified text uses Standard; specialized tiers follow their defined roles.
5. One paragraph uses Card, several prose paragraphs use Document, exact code uses CodeSpace, and images or composed visuals use Presenter.
6. Card has no arbitrary Body; ActionCard uses one local Body control and the correct Horizontal or Vertical structure.
7. Chunk gaps are large, ActionCard peer gaps are compact, and Document owns paragraph spacing and indentation.
8. The complete interactive surface uses Button or CardButton rather than pointer handlers on a display surface.
9. DataGrid boundary scrolling, Overlay dismissal, variants, theme resources, contrast, accessible names, and focus order remain consistent.

## Related content

- [PageBody](../controls/page-body.md)
- [Chunk](../controls/chunk.md)
- [Card](../controls/card.md)
- [Document](../controls/document.md)
- [Presenter](../controls/presenter.md)
- [Button](../controls/button.md)
- [Typography](../articles/configure-font.md)
