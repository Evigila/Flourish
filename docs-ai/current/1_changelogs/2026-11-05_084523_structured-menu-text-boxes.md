# Keep structured dropdown text line boxes consistent

2026-11-05 08:45:23 · America/Sao_Paulo · UTC−03:00.

Structured dropdown titles now disable the ordinary Button text-box trim, matching the natural description line box. This corrects a component-level cause of text appearing above its icon without changing menu padding, ordinary buttons, public APIs or host styles. The existing cascade test covers both menu lines and ordinary-button preservation.

At the user's request, six local 1.4.6-preview.1 archives were built and validated for Colligere; source VersionPrefix stays 1.4.5, with no public tag/push/NuGet release. Release Blazor build passed with zero warnings/errors and the existing menu suite passed 33 checks. Colligere full Release build and 275 selected Web checks also passed. Package/source provenance is retained with the consumer local feed; no external dependency declarations changed.

See [diagnosis and manual acceptance](../1_bugreports/2026-11-05_structured-menu-title-trimming.md) and the current dependency inventory. Browser visual acceptance remains pending. No commit was created.
