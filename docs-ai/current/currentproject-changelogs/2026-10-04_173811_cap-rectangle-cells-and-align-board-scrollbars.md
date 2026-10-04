# Cap Rectangle cells and align board scrollbars

## Changes

- Added UniformGrid.MaxCellHeight=200 with positive-value validation and Rectangle-only CSS output. Square retains its independent MaxCellSize=280. Height limits apply to passive and interactive cells through the shared container. Narrow content cells grow up to the cap and scroll overflow safely rather than collapsing to half their width or truncating text. Home keeps its existing two-by-two layout. Added own/container API metadata and default tests; catalog now has 77 components, 540 rows and 253 initialized defaults.
- Removed the fixed primary navigation group's top border while preserving its spacing and lower position.
- DisplayBoard now separates an edge-aligned overflow viewport from its padded content. Direct code uses that viewport rather than an inset second scrollbar. The copy button remains outside the viewport at the upper-right, so scrolling does not move it. Design provides content padding, silver/dotted backgrounds and existing spacing.
- Added optional Design scrollbar rules: 4px thumb tracks where WebKit-style custom scrollbars are supported, transparent tracks/corners, zero track margins and no arrow buttons. Other engines retain thin native fallback behavior. Hidden navigation/menu scrollbars remain hidden; no additional palette color is introduced.
- Primitives.RowActionMenu now blocks native/aria-disabled action clicks in capture, skips disabled entries through every keyboard candidate branch and rejects disabled triggers. Disabled actions have neutral paint without hover/press feedback. The sample explicitly publishes native and ARIA disabled state. Enabled callbacks, queued closing, Escape, outside dismissal and disposal remain intact.
- Added row-action-menu-dom.mjs and updated the directory inventory to 892 maintained files and 132 directories. Active implementation notes and manual checks were updated; previous change records remain unchanged.

## Verification

- Gallery/test builds: zero warnings/errors. Console checks 98/98, palette checks 8/8, existing menu/modal mocked DOM 24/24 and new primitive row-menu DOM 8/8.
- Isolated temporary Gallery/headless Edge at 1440/800/320 confirmed Home's two-by-two grid, Rectangle cell heights no greater than 200px and no fixed-navigation top border. Narrow Home cells reach 200px rather than shrinking to 55px.
- A constrained board with overflowing code was measured flush to all four edges, zero outer padding, 24px content padding, both-axis overflow, 4px scrollbars, transparent tracks/corner, zero track margins and no native buttons. The copy target did not move when the viewport scrolled; hidden primary navigation stayed hidden.
- The real primitive menu disabled item refuses activation and hover feedback without closing; enabled actions update status and close. No page errors appeared. Final browser evidence: C:/Users/Evigila/AppData/Local/Temp/flourish-height-scrollbar-3c7961e6f9dc43e485f43950d161f12a.cjs and matching temporary HTTP logs.
- No Computer Use tools, new dependency, publication or commit. Temporary service/browser closed; VS process and unrelated pending work preserved.

## Manual acceptance

Rebuild/restart VS Gallery and reload. Compare Home and both grid shapes, configure the new container height and scroll unusually long cell contents. Check the lower fixed navigation, both board scroll axes, copy feedback, code spacing and a generic dotted board. Operate primitive-menu disabled and enabled actions by pointer/keyboard. Repeat themes, zoom and supported browser engines; use the complete blazor-manual-tests.md checklist. Commit requires the user's response.
