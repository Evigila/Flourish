# Align shell navigation and add opt-in hover selection

Recorded 2026-10-27 10:30:13 America/Sao_Paulo (UTC-03:00). Stable base: `37a42ae`; final local candidate: `1.4.4-preview.shell.20261027.2`.

ShellHeader separates brand, content-aligned Services and actions. Framework measures actual PageBody/ContentContainer bounds, preserves body width, reflows crowded wings to two rows, and owns static/interactive initialization and cleanup. ContentSurface uses the configured content width. Design supplies white Quiet and white-surface/brand-foreground Elevated chrome actions, excluding menu/dialog descendants.

SelectBox, StandaloneSelectBox and LanguagePicker add opt-in OpenOnHover/Strength parameters. An internal NativeSelectPopup reuses ActionMenu while retaining native value/change/validation semantics and native fallback. Popup surface paint stays independent from button chrome; scoped brand/font and detached preview sources remain intact. The consumer also removes its unnecessary current-language disabled state, which combined with disabled opacity caused the reported pale native marker. No palette seed or external dependency changed.

Validation: 12-project Blazor producer build and 17-project consumer build passed with zero warnings/errors; six final packages were created, source/cache/resource hashes checked, and eight producer plus two host JS/MJS files syntax-checked. The consumer graph has eight ready services and serves the new package bytes. No functional/Node suite, Computer Use or public publication ran. .1 remains a superseded local candidate; .2 does not overwrite it.

See [analysis and manual acceptance](../1_bugreports/2026-10-27_header-content-alignment-and-hover-selection.md), [architecture](../1_architecture.md) and [package provenance](../1_dependency.md).

