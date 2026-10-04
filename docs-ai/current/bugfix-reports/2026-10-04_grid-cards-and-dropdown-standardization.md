# Occupied grids, primary cards and dropdown standardization

## Symptoms and cause

UniformGrid combined full-width fractional tracks with a Border backdrop, outside border and rectangular shadow. Empty positions inherited that sheet. Its Square percentage-in-max-track expression also caused cyclic intrinsic sizing inside fit-content. DisplayBoard explicitly forced grids to 100% width.

Card still used Surface. IdentityCard used Primary but forced its labels to Text, giving poor contrast; inline-size containment without explicit width shrank inside a centered board. Its sidebar example also omitted dt/dd grouping.

Five Gallery scenarios used raw selectors. DataSearch lacked standard hooks. Custom popup files and Surface native picker overrides competed with shared geometry.

## Corrections and compatibility

Use capped shrinkable fit-content tracks: Square 280px, Rectangle twice its 200px height cap. Available width selects auto columns rather than stretching cells; explicit/narrow layouts remain. Remove the board width override, wrapper fill/outside border/rectangular shadow. Actual cells own variant skins; Elevated silhouette shadow follows occupied surfaces. Connected composition remains.

Card uses Primary/PrimaryInk. IdentityCard labels inherit ink, width 100%/border-box fixes containment sizing and sidebar pairs are grouped.

Delete FactList and sample/nav/catalog/styles; migrate every maintained usage to existing UniformGrid. This deliberately removes a public type, with no obsolete alias. IdentityCard's definition lists remain.

Use Field+typed SelectBox in five scenarios. Standardize DataSearch and EditingGrid native hooks while preserving dense cell behavior. Shared dropdown rules own geometry; competing primitive/Surface skins are removed. No selection, create/filter, column, callback or keyboard API changed.

## Evidence

Zero-warning/error Gallery and test builds; 102 .NET checks, 27 controls DOM, 8 row-action and 9 palette audits pass. Catalog: 77 components, 541 rows, 253 defaults. Directory map: 894 maintained files/132 directories.

Headless installed Edge checks 32 shape/layout/width cases without grid overflow. Five default Square cells at 1500px availability occupy 1404px; at 1000px, 842px/three columns; 570px, 561px/two; 200px, one 200px cell. Home remains 2x2/200px height. Grid/button/identity/foundation/example pages are contained at 320px.

IdentityCard label contrast improves from 1.23/1.31 to 12.48/7.90 in Light/Dark. Card and IdentityCard role colors are confirmed live in both modes; sidebar toggle and reflow work. Five selectors have linked labels, 48px height and real changes. Native DataSearch filtering, primitive table column changes, actual selected-cell picker, ServiceMenu navigation, table paging/search, autocomplete and independent multi/reference panel widths pass. Shared panels measure 16px radius/6px padding with role-based shadow. All 77 HTTP guides retain five sections/default columns and generated CSS loads.

Temporary installed-headless-Edge evidence files (not maintained source):
- flourish-input-contract-afcbb22d3505456bab723afb0d1cea49.cjs: grid, cards, selector changes, migrations and narrow cases.
- flourish-input-contract-769f9d5b5eb545619a373add879ee29b.cjs: table/selected grid regression.
- flourish-input-contract-a63c04cb10684d649bf3c78f29318c5a.cjs: inputs/autocomplete/independent widths.
- flourish-input-contract-67388248d23249f4af229934a86e4360.cjs: shared popup geometry and confirmed Dark card roles.
- flourish-input-contract-4302a14f4857423ea2bf6b3e7bb5a277.cjs: final generated CSS, Elevated silhouette shadow, real grid-button callback and the full geometry/migration/narrow sweep.

Early diagnostic scripts referenced nonexistent sample labels/classes or an unconfigured advanced menu; corrected from source without changing product behavior. Final named runs pass.

## Acceptance and limits

Use blazor-manual-tests.md for supported engines, platform-native pickers, accessibility, zoom, long content and Framework-only presentation. The advanced menu inherits table-popup-panel rules but is not configured by this sample; no fictitious action was added. External FactList users must migrate markup. No Computer Use, dependency, business change, Colligere edit, user VS interruption or Git commit occurred.

