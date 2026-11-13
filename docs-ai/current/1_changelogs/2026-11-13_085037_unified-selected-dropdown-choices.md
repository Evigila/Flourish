# Unified selected dropdown choices

Recorded: 2026-11-13 08:50:37 -03:00 (America/Sao_Paulo)

Design now gives native SelectBox options, enhanced language/native-picker menus, selected menu items, ReferenceDropdown and MultiSelectBox choices the same inverse text/surface fill and one-pixel inset boundary. Selected pointer, pressed and keyboard states keep this paint. The compact trailing indicator remains an icon without its own border or background. Forced-colors uses system Highlight/HighlightText and an outline so selection does not depend on shadows. Form values, disabled behavior, keyboard semantics and Framework-only browser behavior are unchanged.

Local-only candidate **1.4.8-preview.13** is based on source commit **84bf6406f2750365e7b51fd7f707e355c9195ff4** plus the two reviewed Design CSS edits and their cascade regression. Default source VersionPrefix stays **1.4.7**; explicit producer version properties create all six aligned archives in ../Colligere/artifacts/flourish-preview/1.4.8-preview.13. No dependency declaration, public API, source version default, Git commit/tag/push or public NuGet publication changed.

Verification: **40/40** focused native/enhanced cascade checks; isolated Release producer build with **0 warnings/errors**; six package identities, aligned internal dependencies and required assets verified; packaged Framework/Design CSS exactly match their compiled bundles. All **583** runtime inputs were rehashed, and the **32** external closure entries match preview.12. Provenance and CSS evidence are stored beside the local archives. Browser visual acceptance remains manual; see the linked investigation.

[Diagnosis and manual acceptance](../1_bugreports/2026-11-13_085037_inconsistent-selected-dropdown-paint.md)