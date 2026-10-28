# Add information-grid mode and standard header spacing

Recorded 2026-10-27 15:56:56 America/Sao_Paulo (UTC-03:00), following the [footer/progress refinement](2026-10-27_143607_align-presentation-footer-and-progress-threshold.md).

UniformGridItem adds opt-in Information=false for one semantic read-only label/value pair per cell. Labels use body typography, values the H1 token, and CopyText inherits that scale. Information cells grow naturally rather than clipping to decorative aspect/height constraints. Dialog adds opt-in DangerTitle=false without changing its accessible-name or lifecycle contract.

Card remains compatible but is explicitly Deprecated in the usage catalog, points to UniformGrid and is hidden from new API discovery through EditorBrowsable(Never). XML and Gallery guidance discourage new Card usage; old rendering and parameters remain available without an Obsolete-warning cascade. Card.razor.cs is the sole new maintained file, taking the producer tree to 705 files/121 directories.

Header branding remains the real Quiet Button and gains noninteractive outer spacing. The native select owner shares header Quiet preview/click roles, with popup/dialog roles isolated. Structured dropdown rows use 10px/16px vertical/horizontal padding, 72px minimum height and 16px icon gap; language options retain compact regular body typography. All paints and geometry remain library-owned.

Six immutable local .5 archives were packed and five consumed. Producer 12-project and consumer 17-project Release builds pass with zero warnings/errors. Framework 484/484, Gallery 8645/8645, header 6/6, native-select/CSS 31/31 and consumer focused 229/229 pass; the report retains initial assertion diagnoses separately. Eleven producer and two consumer JS/MJS syntax checks and JSON parsing pass. Fresh local runtime checks confirm eight ready responses, five .5 dependencies, six matching HTTP resources and four public management-menu responses. Whole-solution suites and browser acceptance remain unperformed; no Computer Use or public release ran.

See [implementation and manual acceptance](../1_bugreports/2026-10-27_information-grids-and-header-consistency.md), [architecture](../1_architecture.md) and [package provenance](../1_dependency.md). Earlier candidate evidence remains historical.
