# Content landmark keyboard outline

## Symptom and cause

After clicking blank page content, pressing a or Shift draws a large theme-colored outline around the content viewport. ApplicationShell and the two surface patterns deliberately use tabindex=-1 on their main content landmarks so skip links can focus them. A pointer click can focus that landmark; subsequent keyboard input can make the same element match the browser's focus-visible heuristic. Design's global focus-visible selector then paints the entire landmark as though it were a control. The content outline-offset rule places that outline inside the viewport.

This is a focus presentation defect, not selected application state. Framework input-origin code does not cause it.

## Correction and boundary

Design explicitly suppresses outlines only on main.f-content-scroll, main.application-stage and main.content-stage with tabindex=-1. The targets retain their IDs, tabindex, skip links and native keyboard scrolling. Interactive controls, focusable tables and EditingGrid cells retain their focus presentation; there is no blanket suppression of tabindex=-1 or focus-visible.

## Verification and remaining acceptance

An installed Edge headless regression used real pointer and keyboard events: click blank shell content, press a and Shift, confirm focus remains on main with no outline, then press Tab and confirm the next control retains a solid focus outline. The full root solution builds without warnings/errors; 105 Blazor checks and the existing controls/input-origin/palette checks pass. All 77 guide routes load.

Manual acceptance should repeat this sequence for both surface patterns, both themes, skip-link activation and keyboard scrolling, including other supported browsers. Native Framework-only consumers keep browser-native focus rules because this correction changes Design only.
