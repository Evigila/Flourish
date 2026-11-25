# Refine docked heading controls and list headers

Recorded on 2026-11-23 at 14:10:19 America/Sao_Paulo (UTC−03:00).

The workspace header dock places the original Back control between the launcher and title and aligns the original page actions before the trailing account controls. Only the page actions move to the second row on narrow screens. That row keeps end alignment. Overlay-only flex sizes now stay local to the standalone heading overlay, so the dock placeholders reserve the widths measured for the real controls.

The dock returns controls to the expanded page immediately and keeps its empty slots in place while the title slides upward. It restores the brand when the transform ends. A rapid reversal cancels the pending exit and resumes docking. Reduced-motion users get the immediate state change.

DataTable and ListView use #E4E5E7 for column headers, including the fixed action column, and #292721 for the header ink in both default themes. All 378 static and controller checks passed. All six local 1.4.11-preview.5 packages passed dependency and asset verification. Browser layout and motion remain manual acceptance checks.
