# Grid icon regions and compact heading

## Changes

Rectangle UniformGrid MaxCellHeight defaults to 260px instead of 200px, shared by passive and interactive cells. Its 2:1 width cap is 520px; Square remains capped at 280px. Home and Framework interactions no longer specify rows or columns.

UniformGrid, UniformGridItem and UniformGridButton expose nullable IconSupport. Local configuration overrides container configuration; otherwise a nonempty icon automatically enables equal icon/copy regions. False retains continuous vertical flow. Nested automatic grids reset inheritance, host content remains encoded, and Busy preserves regions while temporarily hiding the icon. Framework owns this geometry and Design supplies its appearance. Gallery includes a working toggle and documents the real API/defaults, now 77 guides and 546 parameter rows.

Compact PageHeading places an icon-only Underline return control and 38px title in one row with a normal 72px minimum height and 12px block padding. Expanded mode retains the labeled return link above the 50px title. Accessible names survive the visual label change; long titles/actions can wrap. Explicit Compact and shell scroll-driven states share geometry.

Design excludes only layout content landmarks from the global control outline, fixing the ring after clicking blank content and pressing a or Shift without removing control focus styling. See the [cause and scope](../bugfix-reports/2026-10-04_content-landmark-focus-outline.md).

## Verification and commit scope

Root solution: 14 projects build with zero warnings/errors. Blazor: 105/105 checks, including new automatic/explicit/inherited/nested icon behavior and Busy stability. Existing controls DOM, input-origin and palette checks pass. All 77 guides load through HTTP with their five sections and API default columns.

Installed Edge headless checks actual 260px rectangle caps, automatic wrapping at 320px without document overflow, equal icon/copy regions, runtime IconSupport and shape changes, Square 280px cap, Busy icon restoration, 50px/38px/72px heading states, collapse hysteresis, explicit Compact, narrow actions, and blank-content keyboard/control focus behavior. No browser exceptions. Temporary hosts use separate dynamic ports and stop only their own processes.

Current implementation notes, Gallery parameter descriptions and manual acceptance are updated. No dependency changes, Computer Use testing, Colligere edits or user VS process restart. The requested commit also checkpoints earlier completed Gallery/design refinements and solution/project renames that were still uncommitted. Pre-existing skill-package deletions are excluded as unrelated local changes.
