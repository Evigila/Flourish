# Compact choice indicators and option rows

Time: 2026-10-23 19:50:28, America/Sao_Paulo (UTC−03:00); UTC: 2026-10-23T22:50:28Z.
Status: implemented on source base `734124f` plus this scoped working-tree change. Physical browser appearance awaits manual acceptance. No commit, public package publication or application startup was performed by this task.

## Request and diagnosis

The user reported that selection indicators and selector items remained too large and requested smaller dimensions with rounder corners. Checkbox, native SelectBox checkmarks and MultiSelectBox reused the standard 48px form-control height; native choice rows additionally reserved 16px vertical padding, producing at least 64px rows. The standard form-control dimension incorrectly governed an inner choice marker.

## Implemented changes

- Added shared Design defaults `--f-choice-size:28px` and `--f-choice-radius:9px`; standard `--f-control-height:48px` remains independent. Framework uses matching fallbacks when Design is absent.
- Standard Checkbox, native `.form-check` inputs, MultiSelectBox checkboxes and customizable native select checkmarks use the compact size and rounder corners. Standalone Checkbox and `.form-check` labels retain their existing 48px click area.
- Native SelectBox and MultiSelectBox reserve the same 28px leading choice column with 10px separation. Native rows now have a 40px default minimum (choice size plus 12px), 6px vertical padding and 12px option corners. ReferenceDropdown, SearchAutocomplete and shared action/service options use matching compact option geometry.
- Wrapping and natural row growth, full-size triggers and ordinary Buttons, native form semantics, selected/disabled/invalid paint, keyboard focus, search, reorder and popup placement behavior remain in their existing implementations. No production JavaScript or component public parameters changed.
- Root version is `1.4.3-preview.portals.12`. Gallery retains its `1.4.3-preview` aggregate and adds an English/Chinese/Portuguese change note. No external dependency, runtime, service or maintained media asset was added.

## Verification and evidence boundaries

- Framework component checks: **460/460**, including existing reference/native choice, unavailable-state and interaction contracts.
- Focused Node native-trigger/placement/palette group: **76/76**, including independent marker/form sizing, preserved label click area, invalid/focus paint, natural multiline growth, fixed marker tracks and remeasurement of 28/36/48px markers before repeated native openings. Final log: `artifacts/compact-choices-12/node-focused.log` (ignored local output).
- Gallery: **8,565** actual checks, including **474** post-event and **371** ChangeLog checks; log: `artifacts/compact-choices-12/gallery.log` (ignored local output).
- CSS bundle checks: **21**; CSS SDK integration checks: **194**; preview identity/stable-publication-fence checks: **12**; ChangeLog validation: **142** across seven version entries.
- The first expanded Node run was **75/76**: the existing test helper omitted quoted CSS imports and interpreted complex selector arms inside `:is` as compound selectors, incorrectly reporting a service row as 52px. The helper now expands both supported import forms and evaluates ancestor/child selectors inside `:is/:not/:where`; assertions were retained and the complete final cascade passes. This was a test-fixture defect, not a reason to add a stronger production override.
- The last source adjustment only aligned the existing native `.form-check` CSS with the same tokens. The full focused Node group was repeated afterward. Component/public contracts did not change in that adjustment.
- Automated source/DOM fixtures do not establish physical native-picker rendering or touch/keyboard comfort in a real browser. Computer Use was not used. Local package creation and Colligere consumer results belong to the coordinating task's evidence.

## Manual acceptance

1. Compare standard checkboxes, table/display multi-selection and language/native select options; confirm compact 28px markers, rounder corners and consistent leading columns.
2. Click a standalone checkbox label outside its marker; confirm the full label remains clickable, required validation still works and disabled choices remain unavailable.
3. Open menus on both screen edges and with long Chinese/Portuguese labels; confirm natural multiline height, readable text, left alignment and no clipped checkmark.
4. Exercise Tab, Arrow keys, Enter/Space and Escape in native selection, reference search, autocomplete and multi-selection; confirm existing selection/dismissal/reorder behavior.
5. Check light/dark themes, selected/hover/press/invalid states and narrow screens; verify regular inputs, triggers and page Buttons retain their familiar size.
