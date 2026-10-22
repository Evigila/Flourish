# Unify anchored popup width and edge alignment

Recorded: 2026-10-21 18:09:10, America/Sao_Paulo (UTC-03:00). Local candidate is **1.4.3-preview.portals.7**. Earlier preview packages and append-only records remain intact; no public package is published.

Anchored menus and selections now choose physical left/right alignment from the actual trigger's viewport position. Rows follow that edge and share a content-sized column. Fixed menu widths and control-sized minimums are removed in favor of max-content and a viewport cap; popup search fields fill the option-defined column. Shared positioning retains upward/downward choice, hover/action gaps and available-height scrolling. Theme copying excludes the placement variable so nested menus retain independent directions.

Native selects retain browser selection and dismissal. Their direction is synchronized before default pointer/focus/keyboard opening, and native CSS explicitly clears the UA inline-size minimum while preserving block flipping. ReferenceDropdown and SearchAutocomplete retain circuit-owned selection/focus/dismissal, reusing only placement in a manual top-layer panel that is positioned before exposure. Ordinary Buttons and modal dialogs retain their own layout. Gallery's aggregate preview change item is localized into three languages. No public component parameter or external dependency is added.

Verified against the final candidate:

- Complete Blazor solution restore and Release build: zero warnings/errors; Framework 456/456; Gallery 8529 including 474 post-event and 355 ChangeLog checks, not added again.
- Focused source/cascade/DOM Node group: 70/70; existing controls fixtures: 77/77; ActionMenu --dom-only: 28/28, with 14 physical-browser checks explicitly skipped.
- Preview identity/publication protection: 12/12. Six-package identity, dependency and required-asset verification passed; no earlier package bytes were overwritten.
- Initial Framework 455/456 was an old literal flex-start source guard. It now verifies default left, explicit right and shared alignment overriding ordinary Button center. Production did not change for that test correction. Final and earlier logs remain separately under artifacts/popup-placement-7.

See [the diagnosis, ownership limits and manual acceptance](../1_bugreports/2026-10-21_anchored-popup-placement.md). Architecture and dependency inventories are synchronized separately. The native first-open flash and physical edge rendering still require target-browser observation; source fixtures do not certify pixels. No Computer Use, normal AppHost restart, database/cloud action, commit, push or public publication occurred. Audit completion state is unchanged.
