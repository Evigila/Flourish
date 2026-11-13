# Standardize interactions and vertical grid icons

## Changes

- Removed the top-bar application's brand-right divider and menu-container-right divider, which produced the two full-height lines beside Gallery's Page menu. Matching decorative brand/menu dividers were removed from ShellHeader, ServiceMenu and NavigationSurface. Popup outlines, the bar's lower border and navigation rail boundaries remain intact. Trigger feedback and weak-popup behavior are unchanged.
- Framework's built-in interactions topic now uses Rectangle UniformGrid with two columns and five passive UniformGridItem explanations. Preserved titles, descriptions and order, adding existing Material Symbols. The three related destinations use standard Underline buttons within the shared inline-actions group; the invalid-topic return is also a standard Underline button. Corrected the stale navigation description to Surface for the third-level panel.
- The shared functional f-uniform-cell-heading layout stacks the icon above the title and centers both. UniformGridItem and UniformGridButton already share this renderer, so the rule covers interactive/passive cells and both shapes without page-specific styles. H1 title/body roles, Variant, disabled/busy behavior and Square's 280px cap remain intact. This explicitly supersedes the earlier side-by-side Title/Icon description.
- Updated the active extraction record and manual checklist. No new maintained source path was introduced; the explanatory directory tree retains its existing coverage. Historical records and unrelated user changes were preserved.

## Verification

- Final Gallery build: zero warnings/errors. Palette checks: 8/8. Menu/modal mocked DOM: 24/24.
- Separate temporary HTTP/headless Edge check at 1440px and 320px: interactions renders five passive items in two columns/three rows, vertical centered icon/title geometry, readable content, three Underline links and zero brand/menu side-border widths.
- Actual top-menu hover still opens without clicking and pointer departure closes it. All three Underline links navigate to the original routes. Home's four interactive grid buttons also use centered vertical icon/title geometry without clipping.
- Evidence: C:/Users/Evigila/AppData/Local/Temp/flourish-menu-interactions-9c28cea328c04d85973629629664215c.cjs and matching flourish-control-http-9c28cea328c04d85973629629664215c.out.log/.err.log.
- Git whitespace checks passed. No Computer Use tools, new dependency, package publication or Git commit. Verification used an isolated browser/service that was closed afterward; the user's VS process was preserved.

## Manual acceptance

Rebuild/restart the VS Gallery and refresh. Check Page-menu borders, hover/press/keyboard opening and pointer departure. Check the built-in interactions grid and all three Underline destinations. Inspect both Square/Rectangle, passive/interactive and standalone cells: icon above title, description below, visible focus and complete content in both themes and supported browsers. Fixture/Edge results do not certify every consumer composition or browser.
