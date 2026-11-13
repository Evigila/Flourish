# Inset board scrollbars and misleading disabled row actions

## Symptoms and causes

Code boards scrolled an inner CodeBlock inside the board's 24px padding. Their scrollbar therefore appeared inside the board instead of at its edge. Native scrollbar arrows and opaque track/corner paint were not consistently overridden. Simply reducing margins would also remove necessary content breathing room.

The primitive row-menu sample already had a native disabled button, but its CSS applied ordinary hover feedback. The primitive menu's delegated click-close handler did not test disabled state, and a tabindex candidate branch could include disabled items; aria-disabled links/items were also unfiltered. These gaps made disabled UI misleading and could close a menu on synthetic/ARIA-disabled activation, even though the sample's native button had no action callback.

## Mitigation

DisplayBoard separates an edge-aligned overflow viewport from its padded content. Direct code overflow belongs to this viewport. Copy UI stays outside it. Optional Design standardizes transparent tracks/corners, 4px custom scrollbars, no native arrow buttons and zero track margins; native fallback remains thin. Existing hidden navigation/menu scrollbars are preserved.

Primitive row-menu CSS excludes native/ARIA-disabled entries from preview feedback. Capture-time click handling blocks their default action and propagation without closing. Keyboard candidates and trigger activation use the same disabled check. The example explicitly includes disabled and aria-disabled. Real enabled actions and dismissal remain operational.

## Evidence and regression checks

Eight new mocked DOM checks cover disabled triggers/clicks, nested icons, native/ARIA-disabled keyboard candidates, enabled closing, Escape, outside dismissal and disposal. Headless Edge used the real scenario: the disabled item kept transparent paint and refused synthetic activation without closing; an enabled action updated its status and closed normally.

A real constrained board with long multiline code overflowed both axes. Its viewport's x/y/width/height matched the board exactly; outer padding was zero and content padding was 24px. Computed scrollbar width/height were 4px, track/corner paint transparent, track margin zero and button display none. Scrolling left/top by 100px did not move the copy action. No page errors occurred.

Evidence: C:/Users/Evigila/AppData/Local/Temp/flourish-height-scrollbar-3c7961e6f9dc43e485f43950d161f12a.cjs and matching temporary HTTP logs. The service/browser were closed after verification. Other engines, themes, zoom, copy feedback and nested controls remain covered by the manual acceptance checklist rather than assumed from one browser run.
