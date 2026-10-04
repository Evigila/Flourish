# Standardize table selectors and selected grid popup colors

## Request and causes

The user reported private/nonstandard DataTable search, page-size and display selectors, a framed total number, cramped pager total text and inverted/unreadable option labels in selected EditingGrid cells. Search and page-size selects did not carry SelectBox's shared classes, display had its own trigger skin, and native picker options did not share menu panel geometry. The count used a separate strong element with a private border. Grid selection correctly colored the closed editor but options inherited that foreground without a popup-local reset.

## Changes

- Reuse standard native f-input/f-select hooks for search and both page-size controls. Preserve bindings, searchable columns, custom positive page sizes and paging callbacks.
- Share dropdown trigger, panel and option styling through dropdown.css: 48px controls, 12px trigger corners, 16px panel corners/shadow and 10px rounded transparent-idle options with approved preview/click feedback. Display retains native details/summary, checkboxes and the shared real-state expansion marker.
- Replace the separate framed total with one localized range span. Append TableText.Total and ItemRangeFormat; the latter formats Items, first, last, Total and filtered count. Gallery uses "项1-10 / 共23" and "项0-0 / 共0". Keep Of for page totals.
- Increase the pager total's right padding to 12px. Standard page-size fields no longer use a private 40px inner control.
- Reset EditingGrid option/optgroup and enhanced picker ink/background to Text/Surface, preserving the selected closed cell's inverted ink and real edits.
- Update focused regression checks, manual acceptance and architecture node descriptions. No package, business-specific resource, public component parameter or source-file addition.

## Verification

Test and Gallery builds pass with zero warnings/errors. .NET has 101/101 passing checks, including shared selector hooks, exact populated/empty CJK range text, synchronized size changes, visibility, loading/error honesty and the component API audit (78 components, 542 rows, 254 defaults). Node checks pass: 27 control DOM, 8 row actions and 9 palette audits.

An isolated headless installed Edge host verified opening real standard native pickers, their 48px/12px controls and 16px panels, search scope (11 matching records), zero result text, top size 20 and bottom size 50, next-page counts, reset to page one, shared display hover/panel/option states and actual column hiding/restoring. Selected EditingGrid edits were exercised in both themes: closed selected ink stays Surface while each option uses Text on Surface. Dark native picker and 320px containment passed. No browser exceptions occurred. An additional bounded agent's CSS fixture passed 52 assertions across both themes and native/enhanced selected/unselected editor states. Architecture source-node count is unchanged; whitespace check passes.

## Manual acceptance and limits

Use the new DataTable/EditingGrid section in blazor-manual-tests.md for keyboard/disabled behavior, paging/filter edge states, both themes, zoom, accessibility and other browser engines. Engines without customizable selects retain OS popup geometry/positioning; the EditingGrid editor remains native. This task used builds and CLI automated checks, no Computer Use, no user VS-process interruption and no Git commit. Existing unrelated dirty work and history remain intact.