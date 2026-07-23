---
title: Design principles
description: Apply Flourish page hierarchy, typography, spacing, presentation, interaction, theming, and accessibility rules consistently.
---

# Page and control design principles

Flourish treats layout and control selection as part of an application's meaning. A page should communicate hierarchy before decoration: chunks define subjects, content controls present information, and buttons communicate actions. These rules are the default design contract for main-navigation content pages.

## Establish one page hierarchy

Use `PageBody` as the page root. Its direct children are limited to `HeaderChunk` and `Chunk`; cards, panels, presenters, and other content belong inside a chunk body.

A standard page has one leading `HeaderChunk` followed by one or more `Chunk` controls. `PageBody` rejects a second header, a header that is not first, and every other direct child. HeaderChunk and Chunk always occupy a complete row. Split and Overlay Presenters are also full-width; only TopDown Presenter may place readable peer presentations in columns.

Shell-owned transient surfaces—including the profile flyout, popups, and dialogs—are not main-navigation content pages. They do not require a PageBody or oversized HeaderChunk. Their internal content still follows the typography, spacing, content-control, interaction, theming, and accessibility rules in this article.

Use the three `Chunk` regions deliberately:

| Region | Rule |
| --- | --- |
| `Title` | Required. State the section's subject in a concise heading. |
| `Content` | Optional. Add only essential context that a good title cannot cover. |
| `Body` | Required. Put the actual content control or layout tree here. `Chunk` does not present content itself. |

`HeaderChunk` inherits the [Presenter](../controls/presenter.md) fields. Explicitly supply `Title`, `Content`, `PresenterMode`, and `PresenterPosition`. `Body` supports the copy while `Presentation` supplies the visual without creating another section hierarchy.

## Use typography by role

Flourish has six font-size tiers. When a control or text element does not explicitly select a tier, it uses `Standard`.

| Tier | Intended role |
| --- | --- |
| `Small` | Compact supporting text, including navigation group labels and `OutputCard` output. |
| `Standard` | Default body and control text. Use this whenever no specialized role applies. |
| `Icon` | General icon glyphs. A specialized icon control may set a local size required by its geometry. |
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

One surface should communicate one subject or behavior. Split unrelated subjects into peer controls instead of building a large nested surface.

## Apply spacing and collapse consistently

Use the layout control's resources and defaults instead of accumulating unrelated local margins.

- Keep the large standard separation between every pair of chunks and between `HeaderChunk` and the first `Chunk`.
- Keep related ActionCards closer together with `FlourishActionCardPeerMargin` so they read as one group.
- Use `FlourishPresenterPeerMargin` after the first Presenter in a vertical Presenter stack.
- Let `Document` create the gap between its Paragraph elements.
- When an optional region is empty or `null`, its presenter and associated spacing collapse completely.
- Use one consistent row and column gap when cards wrap into a grid.

## Keep cards focused

`Card` has optional `Title`, `Content`, and `Icon` fields. It presents one paragraph and one icon at most, and it has no arbitrary `Body`. Images, icon groups, overlays, and composed controls belong in `Presenter`. Cards may be arranged in two or more columns when space allows.

`ActionCard` adds one local interactive `Body` to a fixed structure. `Horizontal` places the icon on the left, stacked title and content next, and body on the right. `Vertical` stacks icon, title, content, and body from top to bottom with shared left alignment. Keep `Body` to one button, combo box, check box, text box, or comparable control.

`OutputCard` has no title, content, icon, or arbitrary body. Append messages with `WriteLine`; do not replace earlier progress or results. Its compact scrolling viewport uses the Small tier and should not determine the height of an adjacent ActionCard column.

Use `CardButton` when the complete card surface is the action. It supports the same optional title, content, and icon regions and the `Standard`, `Tonal`, `Filled`, and `Elevated` card treatments. Use `ActionCard` when only a contained control should be interactive.

## Use Document for continuous prose

`Document` is a rounded, thinly outlined reading surface with no title or variants. Add one `Paragraph` child per paragraph; other item types are not accepted. Each Paragraph uses the Large tier, standard wrapping, Regular weight, and the common foreground.

`Document` supplies a gap between paragraphs and a first-line indentation of four ordinary spaces. Do not add literal leading spaces or per-paragraph margins. Use Document as a chunk's only body. If the content is one paragraph, use Card; if it needs controls or visual composition, choose ActionCard or Presenter.

Use `CodeSpace` for exact copyable source or command text. It shares Document's rounded outlined surface and Large size, uses the fixed Bold Consolas blue presentation, and provides its own upper-right copy action. Its copy tooltip follows the shared Tip typography.

## Use Presenter for rich presentation

`Presenter` separates three concerns: required `Title` and `Content` provide copy, `Body` holds supporting controls with that copy, and `Presentation` holds an image, icon group, illustration, or composed visual. Explicitly declare `PresenterMode` and `PresenterPosition`.

- `Split` places copy plus body on one side and the presentation surface on the other. `PresenterPosition="Left"` is the default arrangement; `Right` reverses the regions.
- `TopDown` places presentation above a left-aligned copy-and-body region and is the only ordinary Presenter mode that may share a multi-column row.
- `Overlay` fills the Presenter with presentation content and draws copy plus body above it.

The `Presentation` region fills its allocated space and centers its child content. Only that region receives the adaptive neutral rounded background; the copy-and-body side stays transparent. In every mode, title, content, and body keep one shared left alignment.

`Presentation` is the default XAML content property. Assign `Body` through an explicit `Presenter.Body` property element. `HeaderChunk` is the page-level Presenter specialization; unlike Presenter, its default XAML content property is `Body`. HeaderChunk never participates in multi-column layout, including when its mode is TopDown.

## Use the button family by action

Every button's complete visual boundary is interactive.

| Control | Rule |
| --- | --- |
| `Button` | Ordinary action. `Icon` and `Content` are both optional, so it also covers icon-only and icon-plus-text actions. |
| `CardButton` | Whole-card action with Card-like content and surface variants. |
| `WindowCaptionButton` | Window caption and title-bar actions only. |

Empty Button, CardButton, and Card regions collapse with their spacing. Do not add pointer handlers to a Card or ActionCard to imitate a button. Give every icon-only button a visible tooltip and a meaningful `AutomationProperties.Name`.

Button variants express action hierarchy: normally use one `Filled` primary action per group, lower-emphasis variants for supporting actions, and `Danger` for destructive actions. Card variants express surface emphasis and never imply clickability.

## Compose floating and scrollable content

`Overlay` supplies floating surface chrome and dismissal semantics. A vertical ActionCard is the standard composition for an icon, copy, and one bottom action. Custom layouts remain valid when the floating view needs a different structure, such as a profile surface with several coordinated regions.

`DataGrid` consumes mouse-wheel input only while its internal viewport can move in the requested direction. At a vertical boundary—or when no internal range exists—the wheel continues to the containing PageBody. Do not add another ScrollViewer around the grid.

## Preserve themes and accessibility

Use Flourish theme resources through `DynamicResource` for local overrides. Avoid colors that work in only one theme, and never use color, position, or an icon as the only carrier of meaning. In Overlay Presenter mode, verify text against the brightest and darkest parts of the visual in both themes.

Keep keyboard focus, reading order, and visual order consistent. Avoid fixed heights that clip localized or enlarged text. Give OutputCard an accessible name when its surrounding chunk and action labels do not already identify its history.

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

- [PageBody](../controls/page-body.md) documents the page root contract.
- [Chunk](../controls/chunk.md) documents HeaderChunk and ordinary sections.
- [Card](../controls/card.md) documents Card and ActionCard.
- [Document](../controls/document.md) documents multi-paragraph text layout.
- [Presenter](../controls/presenter.md) documents Split, TopDown, and Overlay presentation.
- [Button](../controls/button.md) documents action controls.
- [Typography](../articles/configure-font.md) documents the six font-size tiers.
