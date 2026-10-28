# Align presentation footer and progress threshold

Recorded 2026-10-27 14:36:07 America/Sao_Paulo (UTC-03:00). Follow-up to [standard branding and dropdown rows](2026-10-27_142307_use-quiet-brand-and-structured-dropdown-rows.md).

Design increases the default presentation footer's responsive height baseline by half while preserving natural growth and 48px bottom inset. Framework projects the header's actual content edges into footer foreground layout; the full-width decorative watermark remains separate. The existing controller handles resize, direction, replacement and cleanup without an extra host implementation or new public API.

Presentation progress now uses the usable viewport's upper-third start line, corresponding to an incoming full-height scene occupying roughly two thirds of visible space. Explicit directory navigation selects its destination immediately until settled or interrupted, avoiding intermediate-scene selection during smooth scrolling. Existing focus, clipping, reduced-motion and footer boundaries remain intact.

The 31 progress and 6 header/footer mock checks pass. Producer 12-project and consumer 17-project Release builds pass with zero warnings/errors; eleven library and two host JS/MJS files pass syntax checks. Six immutable local .4 packages were packed, and five consumed. The restarted host returns eight ready responses and matching current HTTP resources. Full suites and Computer Use were not run. No source file, dependency type or public API was added; the .4 package is unpublished.

See [diagnosis and manual acceptance](../1_bugreports/2026-10-27_footer-alignment-and-progress-threshold.md), [architecture](../1_architecture.md) and [package provenance](../1_dependency.md). Earlier .1/.2/.3 artifacts and evidence remain separate historical identities.
