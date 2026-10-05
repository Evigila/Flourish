# Blazor host-contract parity verification

Date: 2026-10-05. Follow-up to the Shared consolidation and initial six-package preparation. No publication, tag, push or commit was performed.

## Cause and changes

Dependency/static-asset smoke checks passed, but integrating an established host revealed presentation contract gaps: default single-line cards replaced an explicitly approved two-line host view; header-only sort cycling left Grade and hidden-column sorting inaccessible; retired icon aliases rendered a missing-icon glyph; FormActionBar called a grid variant whose CSS did not match native child actions and could override configured column counts.

Added opt-in DataTable ShowSortControls and CardValueLines without changing other consumers' defaults. Sorting reuses the same table state and scoped preferences, includes hidden sortable columns, resets pagination and rejects busy/bulk-edit changes. Functional clamp and Design field/image geometry share the configured height. Added optional Filled icons and legacy semantic aliases using the already bundled font. FormActionBar uses its matching primitive filled UniformGrid; native POST/form associations remain untouched. Filled native actions use primary-ink instead of assuming white text.

## Verification

The final scripts/Test-Release.ps1 run completed successfully with zero build warnings/errors, all existing Core/Blazor/Culture bridge/browser/CSS suites, six package identity/dependency/static-asset checks and 67 clean-consumer checks in four registration modes. Evidence: artifacts/package-consumers/c40460ce21d94457b5f8fa3541084c7c. This is a local unpublished 1.1.0 candidate; Colligere restores it into a fresh isolated cache rather than reusing an earlier candidate DLL.

Colligere adds runtime regressions for Grade/hidden-column target/direction/default sorting, busy/bulk safeguards, card line boundaries, semantic/solid icons, native action-grid columns, search/paging/column state and scoped localization. Its final results and manual acceptance belong to the consumer's change record. Browser visual acceptance remains manual; no Computer Use or data reset was performed.
