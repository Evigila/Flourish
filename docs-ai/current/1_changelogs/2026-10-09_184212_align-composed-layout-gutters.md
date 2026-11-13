# Align composed-layout page gutters

The Gallery Composed layouts destination (`/patterns`, SurfacePatterns) explicitly forced `PageBody Fluid=false`, while the other standard example pages inherit the shared layout configuration, whose default is fluid. The centered mode applies larger desktop side gutters than the fluid mode. Both page-heading padding and body margins use this selected mode, so the discrepancy affected the entire page rather than the table's internal spacing.

Removed the per-page Fluid override. The page now follows the same PageBody configuration as Examples, Forms, Records and RecordDetails. No shared CSS, table geometry, section spacing, bottom-sheet padding or business callbacks changed. Added a concise three-language entry to the 1.1.4-preview ChangeLog.

Verification: Gallery Release build completed with zero warnings and zero errors; 7,816 existing Gallery checks passed, including actual SurfacePatterns rendering across three cultures and 207 ChangeLog checks. Culture integrity passed 22,060 checks and ChangeLog validation passed 76 checks. Diff whitespace is clean. No new test family or dependency was introduced for this single-parameter correction. Browser visual verification remains manual; no Computer Use was performed.

Manual acceptance: compare `/patterns`, `/records` and `/examples` at the same wide viewport and confirm matching heading/body side gutters; repeat at a narrow viewport to check wrapping and overflow; open Create and Inspect bottom sheets and confirm their existing spacing and actions remain intact.
