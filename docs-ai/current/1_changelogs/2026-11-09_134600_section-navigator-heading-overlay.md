# Keep section anchors clear of heading overlays

Recorded 2026-11-09 13:46 America/Sao_Paulo (UTC-03).

The page-heading change leaves its normal header in document flow and displays a separate fixed title panel after the original heading has left the scrolling viewport. SectionNavigator previously accounted only for a sticky original header. Its links could therefore position section titles behind the new overlay, and its current-section boundary and visible indicator track could disagree with the area the user can actually read.

SectionNavigator now measures the visible `.f-heading-overlay-panel` owned by the current content heading. The outer overlay frame intentionally has zero height and is not used as the obstruction rectangle. Hidden overlays, other pages and nested scrolling previews do not contribute offsets. Anchor alignment, current-section selection and the indicator's available vertical track use the same obstruction. ResizeObserver includes the visual panel and the mutation observer watches its visibility contract. A panel that covers a short viewport hides the indicators until usable space returns.

The existing bounded final alignment, reduced-motion behavior, user interruption, native modified links and disposal behavior remain in place. PresentationProgress was inspected but not changed: Colligere's current presentation surfaces use DocumentFlow, where the new overlay is not activated. Explicit Compact headings and EditingGrid remain owned by their existing layout and scroll regions.

Verification: `node tests/Tests.Flourish.Blazor/section-navigator-dom.mjs` passed 31 of 31 checks. Five new DOM scenarios cover the fixed visual panel versus its zero-height wrapper, an overlay appearing during navigation, panel resizing and hiding, nested/foreign content isolation and an overlay covering a short viewport. No Computer Use, package publication or Git commit was performed. The heading controller's integrated package verification is owned by the coordinating task.

Manual acceptance: in a long page with section navigation, scroll until the title overlay appears, activate several section links and confirm their headings remain below it. Resize the window, try a short viewport and return to the top. Repeat inside a nested scrolling preview and ensure it does not affect the parent outline. Confirm manual wheel, touch and keyboard interaction interrupts a pending smooth alignment.
