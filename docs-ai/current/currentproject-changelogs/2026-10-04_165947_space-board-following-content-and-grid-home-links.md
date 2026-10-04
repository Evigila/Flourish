# Space board following content and grid home links

## Changes

- Standardized DisplayBoard-to-following-content spacing at 24px in Design. This covers body paragraphs previously reset to zero top margin. Adjacent-board separation stays 24px without stacking two margins.
- Replaced Home's four-entry FactList with Rectangle UniformGrid, Columns=2 and Rows=2. UniformGridButton owns each entry's interaction and shared skin; titles, descriptions and destinations remain, with existing primary-navigation icons.
- Prevented narrow fixed-column Rectangle cells from clipping their titles/descriptions. Rectangle uses its automatic content minimum in both functional and decorative layers and grows beyond its preferred ratio when necessary. Home retains the requested two-by-two layout; Square's 280px cap and exact shape remain unchanged.
- Updated active extraction, manual acceptance and the Overview directory-map description. Historical records and unrelated user changes were preserved. The [regression report](../bugfix-reports/2026-10-04_board-following-text-and-home-grid.md) explains causes and evidence.

## Verification

- Final Gallery build: zero warnings/errors. Palette audit: 8/8 passed.
- Isolated HTTP/headless Edge check: Home plus four destination routes, real navigation, two-by-two Rectangle geometry, visible content and no horizontal page overflow at 1440/800/320px; Home's code-board-to-body gap and delivered-CSS stacked-board/body fixture both measured 24px.
- Final script/log identity: flourish-home-board-dde9d5600a2340e2997c41a17088719c.cjs and flourish-control-http-dde9d5600a2340e2997c41a17088719c under C:/Users/Evigila/AppData/Local/Temp.
- Git whitespace checks passed. No dependency addition, package publication, Computer Use tool or Git commit. Only the temporary browser/service created for verification were stopped.

## Manual acceptance

Rebuild/restart the VS Gallery and refresh the document. Inspect board-to-body and board-to-board spacing, activate each Home button by pointer/keyboard and narrow the window while checking complete labels and descriptions. Repeat both themes and supported browsers; narrow two-column grids intentionally become taller to preserve readable content. Follow [the active checklist](../blazor-manual-tests.md).
