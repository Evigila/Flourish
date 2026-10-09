# ActionMenu native hover opening

## Symptom and verified cause

The ActionMenu Gallery scenario's three-dot native-content trigger opened only on click even when the scenario's hover switch was enabled. Source inspection identified three boundaries: the switch was bound only to the generated Actions menu; ActionMenu rejected ChildContent combined with OpenOnHover; attachDisclosureMenu initialized the shared attachMenu controller with hover disabled.

The existing .f-menu-trigger hover CSS already declared background feedback for button and summary triggers. No browser test established that this declaration was absent or overridden. The confirmed defect was missing hover opening support and scenario wiring. The style change adds unavailable-state guards and static expanded feedback; complete-import cascade checks protect both trigger shapes in light and dark themes.

## Correction and preserved boundaries

OpenOnHover now applies to generated Actions and native ChildContent after browser enhancement. The Gallery switch supplies the same value to both menus. ActionMenu passes the value to attachDisclosureMenu, which delegates it to the existing attachMenu controller. No separate menu controller, renderer or compatibility entry is added.

Native ChildContent retains one details/summary content DOM for static SSR and click/keyboard opening without JavaScript. Browser enhancement adopts the same panel into the existing native popover. Enhanced hover-mode summary activation prevents the native default toggle from closing a menu that pointer entry already opened; keyboard activation still selects the shared focus policy. Ordinary click-mode disclosure keeps its native default action.

Mouse entry does not take focus. Crossing between trigger and panel keeps the menu open; leaving both closes it. Touch entry does not activate hover. Keyboard navigation, disabled/inert boundaries, command dispatch before queued dismissal, GET/POST transport, outside dismissal, dialog focus restoration, parameter refresh and disposal keep their existing ownership.

Framework and Gallery guidance now describe both supported content modes in English, Chinese and Portuguese. The active generic-control convergence note is corrected, and 1.1.4-preview includes a concise fix entry. The native-hover restriction recorded in [the earlier native-action change](../currentproject-changelogs/2026-10-05_230030_support-native-canonical-table-actions.md) is superseded. The generated-only hover statement in [the tutorial report](2026-10-08_native-tutorial-board.md) describes its earlier audit, not the current ActionMenu contract; TutorialBoard still owns passive progress previews rather than command menus. Both historical records remain intact.

## Verification and limits

The parent agent reports Release builds with zero warnings/errors for Blazor and Gallery, 428/428 Blazor checks and 7,776 Gallery checks, including 171 event examples and 191 ChangeLog assertions. The four focused Node suites pass: ActionMenu DOM 19/19, controls DOM 75/75, split-menu checks 5/5 and organization-access presentation 13/13. These 112 checks cover the actual shared controller and complete-import style cascade rather than a replacement implementation.

Culture integrity passes 22,146 checks; the ChangeLog validator passes 64; CSS bundle checks pass 21 and CSS SDK asset checks pass 194. The first CSS SDK attempt was denied access to its sandbox temporary project path; the automatically approved rerun outside that sandbox passed. Three edited JSON files parse successfully, the preview references the new key, and all three translations are present. These are source, build and automated results; no Computer Use or physical browser verification was performed.

## Manual regression checklist

1. Open the ActionMenu scenario with hover disabled. Both the labeled Actions trigger and native three-dot trigger should open on click and support keyboard opening.
2. Enable Open on hover and close on pointer leave. Enter each trigger without clicking; its menu should open without taking focus. Cross into the panel and back, then leave both regions; opening should persist during crossing and close on departure.
3. Click an already hovered trigger. It should remain open. Use keyboard arrows and Escape to verify menu navigation and trigger focus restoration.
4. Activate an enabled native action and confirm the local status changes once before the menu closes. The unavailable action must remain unavailable.
5. Compare hover, pressed, expanded, focus and disabled feedback in light and dark themes. Switch hover mode while a menu is open and navigate away/back; no stale popup or duplicate event should remain.
6. Check touch activation and static rendering without JavaScript. Touch should require activation rather than pointer hover; the native-content details must still open by click/keyboard in static rendering.
