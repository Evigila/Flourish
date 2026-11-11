# Preserve DocumentFlow placement for explicitly compact headings

2026-11-09 13:48:48 · America/Sao_Paulo · UTC−03:00.

Follow-up to [the independent heading overlay](2026-11-09_134549_independent-page-heading-overlay.md): review found that Design's compact-heading position declaration tied Framework's DocumentFlow relative-position selector in specificity and loaded later. This could make an explicitly Compact DocumentFlow title sticky even though automatic overlays were excluded.

Sticky positioning now belongs only to the compact selector that excludes all .f-document-surface descendants. Compact sizing remains shared; the normal DocumentFlow position is preserved regardless of loading order. A focused CSS regression asserts that no other Design compact or breakpoint rule can reintroduce sticky positioning in DocumentFlow. Three whole-file edits also had trailing empty lines removed.

The final heading, surface and palette Node run passed 53 tests; git diff --check passed. Production source is frozen for a separate immutable local preview. No package, public release or commit was made by this follow-up. Manually compare explicit Compact inside ordinary shell and DocumentFlow: the former may stick while the latter scrolls naturally, and neither gains the automatic default-title overlay.
