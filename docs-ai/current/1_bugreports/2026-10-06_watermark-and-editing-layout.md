# Empty watermark and table editing layout

## Causes and corrections

An empty personalized-dashboard page needs one large centered welcome, but ordinary EmptyState had only small status text and optional content/actions. The same entry now provides EmptyStateVariant.Standard/Watermark, defaulting to Standard. Watermark is a passive encoded Title; description/action slots and undefined enum values are rejected. Framework centers it in the remaining PageBody area with bounded wrapping, and Design owns muted theme typography at a responsive 40–88px. Catalog, defaults, three-language descriptions and executable Gallery examples follow the single contract. Four checks cover actual rendering and rejection behavior as well as layout ownership.

DataTable does not generate Atualizar lista or Concluir edição em massa: Colligere supplied those commands. The library did, however, leave the BulkEditActions fragment in an ordinary block without command placement. The existing region now aligns actions to the end, retaining an optional leading selection count. Nested InlineActions retain explicit alignment rather than being silently overridden. Gallery's two demo UGB are replaced by one ordinary localized Concluir Button, preserving the intent signal before completing the demo edit. Actual table rendering and CSS ownership are tested.

## Historical table editing layout

The last pre-adoption Colligere stylesheet at `d4c5d9a:src/Colligere.Web/Components/Pages/ProductSpreadsheet.razor.css` supplied 65vh internal scrolling, a border/radius, 74px headers, 220px ordinary columns, 280px identity columns and changed-row cues. The later canonical EditingGrid implementation had lost much of that geometry. The existing library renderer now restores these bounds/frame/header/automatic width floors and changed identity cue. Consumers retain fluid PageBody configuration, stable row/column contracts, typed business cells and persistence callbacks.

Manual resizing still permits a 100px minimum; measured automatic widths respect the initial floors. Cells remain 48px and use the current formatted-display/focused-editor engine. The historical 36px always-bordered inputs and fixed 370px readonly columns are not restored. This is deliberately a partial layout restoration, not a second old renderer, DOM adapter or claim of pixel equivalence. Keyboard, selection, paste validation, templates and atomic consumer writes remain on the current engine.

## Heading audit and release boundary

The public 1.1.1 automatic compact structure matched only f-shell; NavigationSurface's script correctly sets compact state on navigation-surface. Design already shrinks all three roots, explaining partial collapse. Explicit PageHeading.Compact in Gallery uses another selector and was already supported. Source now covers f-shell/content-surface/navigation-surface fully, including the same-row 48px icon-only parent link, from the earlier unpublished repair. No additional consumer controller or fake root class is required.

All changes are queued for a future approved newly versioned release. No version, package publication or Colligere upgrade occurred. Current 1.1.1 consumers cannot opt into Watermark and do not receive the new table/collapse CSS from a sibling source build. Colligere has removed its old Dashboard content, shortened bulk completion and restored UGB Title usage using existing public controls; the new geometry remains pending.

## Verification and manual acceptance

Release tests and Gallery builds passed with zero warnings/errors. Console checks passed 382/382; surface/toolbar/page-body Node checks passed 19/19, including automatic collapse on all three roots and scroll lifecycle. Catalog reports 80 components, 699 rows and 313 documented defaults. A future release still requires complete package preparation and fresh-cache consumer verification.

Manually check watermark centering/wrapping and both themes; batch completion with/without a leading count and narrow screens; internal table scrolling, sticky identity/header, initial and user-resized widths, changed cells, keyboard editing, tags and rejected paste; automatic compact back-icon/title layout and restoration on long/short pages. No Computer Use, running Aspire interruption, live data operation or commit occurred.
