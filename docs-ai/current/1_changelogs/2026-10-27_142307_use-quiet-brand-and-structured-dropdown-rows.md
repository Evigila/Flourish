# Use Quiet branding and structured dropdown rows

Recorded 2026-10-27 14:23:07 America/Sao_Paulo (UTC-03:00). Follow-up to [header alignment and hover selection](2026-10-27_103013_align-shell-headers-and-hover-selection.md).

ShellHeader now uses the real Quiet Button with Href and child content. Removed brand-specific interaction paint and full-wing stretching, retaining logo/context/navigation and content-aligned layout. Structured dropdown actions use two 17px body lines with bold primary text and regular descriptions; native and enhanced language labels explicitly use regular body weight. Ordinary Button description sizing and selected-language markers remain unchanged. No new public API or dependency is introduced.

Six immutable local `1.4.4-preview.shell.20261027.3` packages were produced and the consumer restores five. Producer 12-project and consumer 17-project Release builds pass with zero warnings/errors, and nine producer plus two consumer JS/MJS files pass syntax checks. The restarted consumer provides eight ready responses and matching package resources; public portal HTML contains the expected composition. Regression source was updated, but no functional suite or Computer Use ran. The candidate is local and unpublished.

See [diagnosis and manual acceptance](../1_bugreports/2026-10-27_standard-brand-button-and-structured-menu-rows.md), [architecture](../1_architecture.md) and [package provenance](../1_dependency.md). Earlier .1/.2 validation remains in the preceding dated records.
