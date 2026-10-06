# Native canonical table actions

The primary Components.DataTable now supports Account's native record-opening boundary directly. Capabilities are added to its existing renderer, ActionMenu, TableColumn and scoped preference service rather than introducing another table/menu family. This is not the complete merger of the separate advanced Primitives.DataTable.

## Changes

DataTable adds RowActionsContent, RowOpenAvailable and PreferenceKey. The native slot uses the same ActionMenu in Table and Cards, replaces automatic Open and rejects a competing nonempty Actions collection. A template alone never creates row opening. Availability guards the visual marker, generated Open action and callback. Stable keys and the existing ITablePreferences retain named sort state independently and discard stale columns; existing search, paging, view, display and resize remain owned by the standard table.

ActionMenu adds native ChildContent without a second renderer/controller. Its static details/summary is usable without a popover and retains one content DOM. The panel grows in normal flow before enhancement so the last table row/card cannot clip actions. After controller readiness, the same panel uses attachDisclosureMenu and the existing top layer; removal of the static dropdown-surface class prevents its minimum-width geometry leaking into viewport positioning. Disabled is inert, and native content rejects structured Actions and OpenOnHover combinations. Host native transport, menuitem content, authorization and busy/submission locks remain explicit boundaries.

The packaged DataTable module captures non-control double-click opening synchronously to preserve genuine native link/form activation. It activates exactly one eligible data-record-open entry, resolves a real submitter for requestSubmit, stops the duplicate Blazor callback and fails closed for unavailable, disabled, busy, ambiguous or missing entries. Listener teardown remains standard. The host no longer needs a visual directory controller.

The DataTable guide retains its structured-action example and adds native GET, safe local simulated POST and an unavailable row. POST is disabled during static SSR and after readiness records only local intent; it contacts no authentication or billing backend. Parameter descriptions document the new contracts. The specialized ActionMenu ChildContent description is ordered before the generic fallback, avoiding an unreachable switch arm.

Design's existing compact PageHeading rules now also recognize ContentSurface's same scroll-state attribute. This preserves its existing shell helper and hysteresis while consumers use direct PageHeading instead of a nested compatibility heading's doubled gutters. No host heading skin or another scroll controller is added.

## Verification

- Complete final scripts/Test-Release.ps1: exit 0; Release build zero warnings/errors; Core 367, Blazor 269, Culture bridge 12 passed.
- Ten new C# component checks cover Table/Cards native slots, unavailable callback refusal, explicit opening, conflicts, sort preference isolation/stale keys, disabled native menus, branch transitions, delayed import/disposal and unsupported hover. Existing native CSS cascade checks explicitly cover the new branch without dropping strict unknown-selector detection.
- All ten configured JavaScript files pass: 53 Node-runner checks and existing mock suites, including controls-DOM 46/46 with eight new native-activation cases. CSS bundle 21 and SDK integration 194 pass.
- Six local 1.1.0 candidates and four fresh isolated NuGet-only consumers pass; consumer evidence: artifacts/package-consumers/94244e241b8f4867a9daa3793b54b429, 95 checks. Expected README package advisories are separate from compilation.
- Catalog: 98 components, 741 parameter rows and 326 documented defaults.
- Nine Development HTTP checks pass six actual Gallery routes and the served Framework/Design CSS plus DataTable module. The task-owned Gallery process is stopped.
- Colligere's verified local candidate cache is artifacts/nuget/c19c3ebd9982ec1b691e73a7a0d418ff. Full Release solution and final Web test build are warning/error-free. Focused suite: 391 passed, zero failed, two unconfigured PostgreSQL integration cases skipped.
- No Computer Use, Debug build, user-instance restart, live database acceptance, dependency change, new commit, push or public NuGet publication. Essential and desktop sources remain unchanged. Initial compilation/cascade issues were corrected and the complete gate rerun rather than bypassed.

## Manual acceptance

Use the primary DataTable guide and migrated Account directories in Light/Dark, narrow widths and 200 percent zoom. Exercise search, hidden-column filtering, sort/default order, paging, page size, display, resizing and Table/Cards without changing native transport. Owned/invited named sort/search state must remain independent.

Use the native menu by pointer and keyboard, check viewport-contained enhanced placement and last-row/card access without JavaScript, and verify unavailable/disabled rows cannot activate. Confirm native GET/new-tab, actual host specialized POST/new-tab and double-click open each activate only once; interactive controls must not trigger row opening. Gallery POST remains only a fictitious local intent. Reconnect, navigation and delayed import must not leave duplicate listeners or revive disposed controls. Physical geometry and real browser popup behavior are manual acceptance.
