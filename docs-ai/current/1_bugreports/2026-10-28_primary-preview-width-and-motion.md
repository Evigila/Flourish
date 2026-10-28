# Primary-navigation preview width and reversible motion

Recorded 2026-10-28, America/Sao_Paulo (UTC-03:00), after committing the preceding shell work as `c1ae486`. This follows [action-row and preview coordinate alignment](2026-10-28_header-action-row-and-preview-box-alignment.md); original item geometry, scroll/focus/input behavior, same-DOM navigation and normal-flow reservation remain the integration boundaries.

## Requested behavior and design

The user requested a slightly narrower preview that leaves secondary navigation partly visible, with both opening and closing animation. The selected design uses a 15rem preferred minimum/18rem maximum in Design and a geometry cap when the actual secondary rail is vertical, retaining a quarter of its visible intersection with the host/viewport. Viewport/host limits and RTL remain authoritative; no-secondary and horizontal-secondary layouts use the ordinary narrower limits.

Motion must preserve the real icon anchors and must not scale glyphs or move page content. Opening and closing should be reversible during fast pointer movement, with the native manual popover retained until collapse finishes. Reduced-motion preference, unavailable animation support and forced cleanup require usable immediate fallback. No additional browser library, public component, backend request or .NET hover event is needed.

## Implemented lifecycle

Framework/Design prefer 15rem/18rem instead of the preceding Design 18rem/22rem. The controller measures the actual adjacent vertical secondary rail and caps the overlay before its last quarter; it does not derive the width from a caption or a host-specific pixel constant. A differential ResizeObserver subscription follows that secondary node. Width changes recalculate the cap on the next frame; replacement prunes/rebinds nodes without reading layout inside the document mutation callback. Horizontal and nested secondary rails do not supply the wrong cap.

The same native popover transitions its numeric inline size and caption opacity over 180ms with cubic-bezier(.2,.8,.2,1). A committed collapsed width overrides the preferred minimum before expansion; saved icon coordinates stay fixed in both directions. Collapse defers hide/geometry release until the correct width transition finishes, with a bounded timer fallback. Reentry reverses it and invalidates older frame/timer/end work. Motion preference changes settle the current operation; forced resize/blur, removal, replacement, opt-out or failure immediately releases state. No per-frame .NET callback or new request is added.

## Manual acceptance

1. Compare a business page with a vertical secondary rail before and after the upgrade. Expanded primary navigation should be modestly narrower and leave some secondary content visible; a long secondary label must not enlarge the preview.
2. Watch normal and RTL opening/closing, including route and command entries. Icons must stay at the same screen position and retain their size; the content, footer and header must not shift.
3. Leave and reenter repeatedly before the animation finishes. Expansion should reverse without a hidden-frame flash or an old completion callback dismissing a newly opened rail.
4. Test long labels, disabled descriptions, scrolling, narrow/zoomed viewports and a layout without a vertical secondary rail. Original accessible names and native links must remain usable.
5. Enable reduced motion, then exercise Tab/Escape and touch. Resize, blur/refocus, remove the surface, change routes or replace a navigation item during motion. No stale popover, style, timer, focus jump or scroll reset should remain.

## Frozen delivery and executed checks

Candidate **1.4.4-preview.shell.20261027.12** is frozen in `artifacts/dashboard-preview-motion-20261028/nuget`. The final-source twelve-project Release build passed with zero warnings/errors; Framework **487/487**, Gallery **8653/8653**, Node **79/79** (preview **37**, header **9**, native CSS **33**) and CSS bundle **21/21** passed. Preview cases cover visible secondary caps in LTR/RTL, resize/replacement, reversible transition/timer/end races, early cancellation, reduced-motion changes, focus and forced cleanup, alongside all preceding geometry/input cases.

Final review identified two task-related edge cases before freezing: secondary size changes with constant host/primary width needed a separate subscription; a partly clipped secondary rail needed its visible intersection rather than full width for the quarter reserve. Both were corrected and covered. A normal 232px rail retains 58px; a 200px embedded root exposing 124px of secondary retains 31px (preview cap169px).

Six archives pass the existing package identity/dependency/required-asset verifier. Eleven actual CSS/JS/font/DLL resources equal maintained source or Release output; header-layout.js, both fonts and previous .11 archives are unchanged. Twelve graphs retain 33 external package/version pairs. No maintained source file or public parameter was added: 714 files/121 directories, 22 projects/5 solutions and 775 parameter rows/363 defaults remain.

Build, Framework/Gallery/Node/CSS, pack and verification logs plus package-hashes.json/package-delivery.json are in the matching artifact directory. [Dependency inventory](../1_dependency.md#current-candidate-archive-and-asset-evidence) preserves exact hashes. Independent read-only review found no remaining definite defect. No Computer Use/physical geometry measurement, dependency installation, old-feed replacement, public publication, push or deployment occurred. The new source is included in the user's authorized follow-up commit; actual visual acceptance remains manual.
