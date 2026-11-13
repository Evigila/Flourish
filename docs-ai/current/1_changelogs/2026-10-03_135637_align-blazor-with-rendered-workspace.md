# Align Blazor with the rendered Workspace

The initial extraction is checkpointed as `1fcc922` (`Extract Blazor UI library from Colligere`). The separate Colligere Figma-cancellation record is checkpointed as `74d1143`. Unrelated existing working changes were excluded from both commits.

The user then requested removal of the route-heading focus rectangle and a layout/style review against the real Workspace. Existing local Colligere Aspire resources were started; the user logged into an isolated Edge window. Read-only samples covered the existing Workspace dashboard, product/customer lists and creation screens, organization information/customization, and Account directory navigation. No business records were changed.

Flourish.Blazor now restores the source-derived light surfaces/borders, full-stage page titles, responsive section rhythm, rail geometry/selection, text secondary navigation, form typography, select alignment, facts/identity hierarchy, list search/table/pager/view geometry and dark/floating surface details. Route H1 keeps programmatic focus with no field-like outline; keyboard controls retain visible focus.

Public composition additions: shell brand/start slots, secondary-navigation configuration, explicit compact heading, icon-only button, text menu trigger and FactList. Gallery exercises these without page-owned CSS. Its dirty-form discard preserves the requested navigation destination.

Validation: Debug build 0 warnings/errors; 34 C# checks, 15 controls DOM checks and 6 data DOM checks passed. Installed Edge passed 77 real-render checks over six routes and five viewport widths. Release package generated; six static CSS/JS assets are nonempty and match final source SHA-256 values. Manual DPI/zoom, assistive technology and compatibility checks remain explicit. No external dependency was installed. This change does not migrate Colligere runtime references or auth/business logic.

Details are in ../blazor-render-review.md, ../blazor-extraction.md, ../blazor-manual-tests.md and ../bugfix-reports/2026-10-03_blazor-route-heading-focus.md. Existing append-only records were preserved.
