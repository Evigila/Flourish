# Compact brand Button and header minimum

Recorded 2026-10-27 16:14:00 America/Sao_Paulo (UTC-03:00), following [information grids and header spacing](2026-10-27_155656_add-information-grid-mode-and-standard-header-spacing.md).

Logo attributes, dimensions and flex basis are consistently 24px, with no larger mobile override. The real Quiet Button retains ordinary 48px minimum height, padding, gap and feedback. Outer wing spacing remains, while fallback wing minima shrink to 82px/78px. The former 48px image plus Button and outer padding had expanded the chrome.

ShellHeader now owns a 68px minimum that is not replaced by the parent surface's auto row token. Default brand geometry fits the floor; no fixed-height/max-height cap was added, so responsive rows remain accessible. No public API, dependency type or maintained file count changed. Candidate .6 uses a new immutable version.

Producer/consumer Release builds pass with zero warnings/errors. Framework 485/485, header 7/7, native-select/CSS 31/31 and consumer focused 95/95 pass. Six packages were packed, five consumed, and local runtime checks verify eight ready responses, .6 dependencies, matching HTTP bytes and four public 24px-logo SSR responses. Gallery JSON parses/compiles; Gallery/full-solution suites were not rerun and no browser pixel measurement was made. No Computer Use or public release ran.

See [diagnosis and manual acceptance](../1_bugreports/2026-10-27_compact-brand-button-and-header-height.md), [architecture](../1_architecture.md) and [package provenance](../1_dependency.md).
