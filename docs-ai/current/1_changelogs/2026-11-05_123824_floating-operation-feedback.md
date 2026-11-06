# Floating operation feedback

2026-11-05 12:38:24 · America/Sao_Paulo · UTC−03:00.

`FloatingNotice` wraps the existing Blazor Notice to display operation feedback outside the page flow. Severity, custom or localized title, body, announcement policy and host attributes apply to the original Notice element. The page controls its lifetime. Native `hidden` can reveal a statically rendered result without recreating it.

The Framework document initializer loads the docking controller on demand. Within each document or open dialog, active messages share the original save bar width. Wide areas place the messages beside Save/Discard, narrow areas stack them above the buttons. Each message can scroll when height is limited. The dialog header and footer sit outside its feedback area. Multiple messages retain their DOM reading order. Insets update on content, viewport, scroll and visibility changes. Every notice frame remains fixed at zero flow height, including while hidden. SaveChangesBar continues to reserve only its existing protection for its own wrapped controls. Floating messages do not reserve page clearance. The component does not move focus or reparent Blazor elements. Entrance movement follows the user's motion preference.

The component catalog and Gallery include the new contract and an executable example. The Framework assets and Node fixture are registered in the existing release inventory. This change adds no package dependency or service.

Manual checks: trigger each severity in a scrollable editor, with and without unsaved changes; resize until the dock stacks; reveal a hidden static notice and dismiss the host state. Open a dialog with feedback above its action row, change dialog views, and test with keyboard navigation and reduced motion.

The Release build completed with zero warnings or errors. The Framework suite passed 523 checks, Gallery passed 9,123 checks, and the dock/header/save, scoped cascade, static form and DOM-only menu regressions passed 37, 2, 4 and 28 checks respectively. An additional existing native-select browser probe could not connect to its local test browser. Physical visual and keyboard acceptance remains a user check.
