---
title: Design principles
description: Apply Flourish page hierarchy, typography, spacing, presentation, interaction, theming, and accessibility rules consistently.
---

# Page and control design principles

Flourish treats layout and control selection as part of an application's meaning. A page should communicate hierarchy before decoration: chunks define subjects, content controls present information, and buttons communicate actions. The following rules are the default design contract for main-navigation content pages.

## Establish one page hierarchy

Every main-navigation content page has exactly one leading `ChunkHero` followed by one or more `Chunk` controls. `ChunkHero`, `Chunk`, and `Presenter` are full-width controls; each occupies its own row.

All page content belongs inside that hero or those chunks. Do not create a competing hierarchy with free-standing headings, cards, or manually spaced panels. Do not add a second hero or arrange chunks side by side.

Shell-owned transient surfaces—including the profile flyout, popups, and dialogs—are not main-navigation content pages. Do not force an oversized `ChunkHero` or the complete page skeleton into them. Their internal content still follows the typography, spacing, content-control selection, Button-family, theming, and accessibility rules in this article.

Use the three `Chunk` regions deliberately:

| Region | Rule |
| --- | --- |
| `Title` | Required. State the section's subject in a concise heading. |
| `Description` | Optional. Add only the essential context that a good title cannot cover. |
| `Body` | Required. Put the actual content control or layout tree here. `Chunk` does not present content itself. |

`ChunkHero` inherits the [Presenter](../controls/presenter.md) fields. Every hero declaration explicitly supplies the required `Title`, `Description`, `PresenterMode`, and `PresenterPosition`. `Body` supports the message in the copy region, while `Presentation` supplies the presented visual without creating another section hierarchy.

## Use typography by role

Flourish has six font-size tiers. When a control or text element does not explicitly select a tier, it uses `Standard`.

| Tier | Intended role |
| --- | --- |
| `Small` | Compact supporting text, including navigation group labels and `OutputCard` output. |
| `Standard` | Default body and control text. Use this whenever no specialized role applies. |
| `Icon` | General icon glyphs. A specialized icon control may set a local size required by its geometry. |
| `Large` | Card-title-level emphasis. |
| `ExtraLarge` | The section-title family, including `Chunk.Title`. |
| `HeaderSize` | The page title in `ChunkHero` only. |

Do not select a larger tier merely to make content more noticeable. Express hierarchy through the correct control and text role.

## Choose the content control by purpose

| Need | Control |
| --- | --- |
| One title and one paragraph | `Card` |
| One title, one paragraph, and one icon | `IconCard` |
| Several text paragraphs | `Paragraph` as the chunk's sole body |
| An image, several icons, a preview, or composed presentation | `Presenter` |
| One compact setting with a right-side local control | `ListCard` |
| Raw output, logs, progress, results, or failures | `OutputCard` |
| One action whose complete surface is clickable | `CardButton` |

One surface should communicate one subject or behavior. Split unrelated subjects into peer controls instead of building a large nested surface.

## Apply spacing and collapse consistently

Layout regions normally have explicit space between them. Use the layout control's resources and defaults rather than accumulating unrelated local margins.

- Keep the large standard separation between every pair of chunks and between `ChunkHero` and the first `Chunk`.
- Keep related ListCards closer together with `FlourishListCardPeerMargin` so they read as one group.
- Use `FlourishPresenterPeerMargin` after the first Presenter when several Presenters form one vertical stack; do not reuse Card spacing.
- Let `Paragraph` create the gap between each pair of paragraphs.
- When an optional region is empty or `null`, its presenter and associated spacing must collapse completely.
- Use one consistent row and column gap when cards wrap into a grid.

## Keep cards focused on presentation

`Card`, `IconCard`, and `ListCard` have optional `Title` and `MainText` fields, but no general-purpose `Body`. They are terminal presentation surfaces, not containers for arbitrary controls.

An ordinary `Card` presents one paragraph. `IconCard` adds one icon and permits `Left`, `Top`, `Right`, or `Bottom` icon placement; it does not accept images, icon groups, overlays, or other composed presentation content. Card and IconCard may be arranged in two or more columns when space allows.

`ListCard` has a fixed left-icon, center-copy, right-action arrangement. Keep `Title` and `MainText` concise because both are single-line with ellipsis overflow. Put one local interactive control in `ActionBody`, keep the `Standard` variant, and apply selections, toggles, and edits immediately. Do not add a separate Apply action. Stack ListCards in their own column with the compact peer gap.

`OutputCard` has no title, description, or arbitrary body. Append each message with `WriteLine`; do not replace earlier progress or results. Its compact, scrolling output viewport uses the `Small` font-size tier and should not determine the height of an adjacent ListCard column.

## Use Paragraph for continuous prose

`Paragraph` is transparent, borderless, and has no title, description, or variants. Each direct `TextBlock` is one paragraph. The control supplies a gap between paragraphs and a leading visual indentation equivalent to four standard spaces. Do not add literal spaces or per-paragraph margins.

Use `Paragraph` as a chunk's only body. If the content is a single paragraph, use `Card`; if the content needs controls or a visual composition, choose the appropriate card or Presenter instead.

## Use Presenter for rich presentation

`Presenter` separates three concerns: the required `Title` and `Description` provide copy, `Body` holds supporting controls in the copy region, and `Presentation` holds an image, icon group, illustration, or composed visual. Every declaration also names `PresenterMode` and `PresenterPosition` explicitly. The runtime fallback values remain `Split` and `Right`, but they do not replace the authoring requirement.

Standard `Split` uses a fixed horizontal composition: copy plus `Body` stay together on the left and `Presentation` occupies the right. `PresenterPosition="Left"` deliberately reverses those regions without changing their internal structure. `Overlay` fills the Presenter with `Presentation` and renders copy plus `Body` above it; position is ignored but is still declared. Consumers do not build a second Grid or use local margins to reposition these regions.

`Presentation` is the default XAML content property. Assign `Body` through an explicit `Presenter.Body` property element so direct content cannot silently move from the presentation side into the copy column. An ordinary Presenter is full width: the copy-and-body side remains transparent, with Title, Description, and Body on one left alignment; only the presentation region uses an adaptive light-neutral rounded surface, and Split presentation content is centered within it.

`ChunkHero` is the page-level Presenter specialization. It uses the same fields and modes but adds the emphasized hero background, HeaderSize title, and page-leading semantics.

## Use the button family by action

Every button's complete visual boundary is interactive.

| Control | Rule |
| --- | --- |
| `Button` | Default text action without an icon. |
| `IconButton` | Action with an icon; content may be omitted for an icon-only button. |
| `CardButton` | Whole-card action with IconCard-like presentation. |
| `WindowCaptionButton` | Window caption and title-bar actions only. |

Do not add pointer handlers to non-interactive cards to imitate a button. Give every icon-only button a visible tooltip and a meaningful `AutomationProperties.Name`.

Button variants express action hierarchy: normally use one `Filled` primary action per group, lower-emphasis variants for supporting actions, and `Danger` for destructive actions. Card variants express surface emphasis separately and never imply clickability.

## Preserve themes and accessibility

Use Flourish theme resources through `DynamicResource` for local overrides. Avoid colors that work in only one theme, and never use color, position, or an icon as the only carrier of meaning. In Overlay presentation, verify text against the brightest and darkest parts of the visual in both themes.

Keep keyboard focus, reading order, and visual order consistent. Avoid fixed heights that clip localized or enlarged text. Give `OutputCard` an accessible name when its surrounding chunk and action labels do not already identify its history.

## Consistency checklist

Before considering a page complete, verify that:

1. Exactly one `ChunkHero` leads the page, followed by full-width chunks with no peer content outside them.
2. Every `Chunk` has a concise `Title` and real `Body`; descriptions appear only when needed and empty optional regions leave no gap.
3. Every `ChunkHero` and `Presenter` explicitly declares `Title`, `Description`, `PresenterMode`, and `PresenterPosition`; standard Split keeps copy plus Body on the left and Presentation on the right.
4. Unspecified text uses `Standard`; specialized tiers follow their defined roles.
5. One paragraph uses Card, several paragraphs use Paragraph, one icon uses IconCard, and images or composed visuals use Presenter.
6. Cards have no arbitrary Body; ListCard uses one `ActionBody` control and immediate application.
7. Chunk gaps are large, ListCard peer gaps are compact, and Paragraph owns its paragraph spacing and indentation.
8. The complete interactive surface uses the correct member of the Button family.
9. Variants, theme resources, contrast, accessible names, and focus order remain semantically consistent.

## Related content

- [Chunk](../controls/chunk.md) documents the page hierarchy and hero rules.
- [Card](../controls/card.md) documents Card, IconCard, and ListCard.
- [Paragraph](../controls/paragraph.md) documents multi-paragraph text layout.
- [Presenter](../controls/presenter.md) documents Split, Overlay, and presentation content.
- [OutputCard](../controls/output-card.md) documents compact scrolling output.
- [Button](../controls/button.md) documents the action controls.
- [Typography](../articles/configure-font.md) documents the six font-size tiers and global configuration.
