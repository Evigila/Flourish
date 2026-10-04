# Align grid occupancy, cards and dropdown surfaces

## User request

Remove UniformGrid's broad backdrop and page-width stretch, use Primary for Card, inspect IdentityCard, delete FactList and standardize every maintained dropdown.

## Result

- Grid width follows capped occupied tracks and existing separators, wraps from available width and shrinks below one cell. Both shapes, explicit rows/columns and narrow overrides remain. Wrapper paint is absent; cell variant borders/paint and Elevated silhouette shadows follow actual cells. Home remains 2x2.
- Card uses Primary/PrimaryInk. IdentityCard fills its board and inherits readable labels; its sidebar pairs are correctly grouped.
- FactList plus sample/nav/catalog/CSS are deleted. All uses migrate to existing UniformGrid items or destination buttons. This is an intentional public type removal.
- Five Gallery scenario selectors use linked Field/SelectBox. DataSearch/EditingGrid have native standard hooks; popup/row geometry is shared across menus and selection primitives, with obsolete skins removed.
- Directory tree now has 894 maintained files/132 directories. Catalog has 77 guides, 541 API rows and 253 defaults. Existing historical records are preserved.

## Verification and acceptance

Zero-warning/error builds; 102 .NET, 27 controls DOM, 8 row-action and 9 palette checks. Headless installed Edge verifies capped widths, 32 layout cases, Home, 320px containment, card themes/sidebar, real selector changes, selection/search/table callbacks, popup geometry and readable selected-cell picker. All 77 guide routes and generated CSS pass HTTP checks.

[Detailed cause/evidence](../bugfix-reports/2026-10-04_grid-cards-and-dropdown-standardization.md) and [manual acceptance](../blazor-manual-tests.md) document consumer migration and browser-native fallback limits. No Computer Use, dependency, Colligere edit, user VS interruption or Git commit.

