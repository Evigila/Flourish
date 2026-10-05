# Danger feedback and popup placement

## Changes

Add Danger target preview and click roles: Light #7E2824/#712420, Dark #8C3430/#7E2F2B. Click channels are rounded 90% preview values. Button, grid actions and destructive menu options retain deep-red interaction feedback; white uses the existing Surface-light role. Idle Danger remains unchanged. Foundations exposes both tokens; palette tests cover all seventeen roles and readable foregrounds.

Anchor hover/click ActionMenu to its actual button. Shared positioning now chooses an available side, constrains height and follows scrolling/resizing. DataTable display selection uses the same manual-popover/disclosure lifecycle while preserving native checkboxes, persistent selection and summary behavior. Empty tables and old-browser portal rerenders are supported. See the [cause, implementation boundary and acceptance](../bugfix-reports/2026-10-04_popup-placement-and-board-centering.md).

Repair DisplayBoard selectors after the viewport wrapper change. Ordinary direct content and inline action groups center horizontally; full-width layouts preserve their widths and code remains upper-left.

Recommend explicit Filled primary and Outlined secondary UniformGridButton actions in form areas. All 25 Gallery FormActions/FormActionBar grid actions follow this recommendation; executable samples, copied example code and API guidance agree. Framework defaults and child-order semantics are unchanged.

Update current color/implementation notes, architecture node descriptions and manual acceptance. No maintained source path is added, so the directory map retains its file/directory counts. Historical records remain unchanged; unrelated skill deletions remain untouched.

## Verification

Blazor/Gallery builds: zero warnings/errors. Blazor 105/105, controls DOM 38/38, input-origin 5/5 and palette 9/9 checks pass. All 77 HTTP guides load their five sections/default columns and assets. Real headless Edge verifies light/dark Danger hover/held press, grid foregrounds, centered preview groups/direct buttons, upper-left code, stretched-wrapper hover anchor/entry/exit, display upward/downward placement, persistent selection, Escape/outside handling and actual form callbacks. No browser exceptions.

No external dependency, Computer Use testing, Colligere edit, user VS restart or Git commit. A focused commit is pending the user's decision.
