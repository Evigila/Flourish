# Align floating feedback and show its remaining lifetime

2026-11-09 12:53:52 · America/Sao_Paulo · UTC−03:00.

The earlier dock aligned bottom edges but left notification and save-action outer heights independent. It positioned the action panel using a horizontal shift, and an already visible notification could replace its text without replaying entrance motion. The dock now measures direct content plus shell padding/borders, preserving natural child sizes while applying a shared wide-column height. Child resize observations handle wrapping/font changes even when outer height is fixed. Short/narrow layouts retain bounded scrolling and stacked placement.

SaveChangesBar uses the original centered envelope's right boundary and transitions only width. Notification entry/exit remains vertical and is independent from action resizing. Alternating entrance animation names replay replacement content, severity or MessageKey without rebuilding Blazor DOM or moving focus. Ordinary equal-content rerenders do not replay or renew the message.

A three-pixel decorative bottom line displays the actual remaining lifetime. On start/resume its initial fraction and duration come from the timer's remaining milliseconds; hover, focus and background pauses freeze that fraction. Replacement resets it, persistent messages omit it, and zero precedes the existing exit. The pseudo-element adds no layout height or live-region text. Reduced motion removes panel transitions/entry movement and updates the clock in approximately one-second steps. The owned hidden/dismissed boundary and user-owned message state remain unchanged.

Both countdown and exit callbacks now verify timer-handle identity as well as message generation. This prevents a cleared but already queued callback from closing a later resumed timer or exit of the same message. Disposal releases the clock attributes, styles and subscriptions.

Verification: 51 targeted Node cases passed (18 floating-feedback, 24 save-action, nine header), including natural-height shrinkage, stable observer passes, replacement/reduced motion, actual pause fractions, stale countdown/exit callbacks, host-hidden/SSR reveal, narrow layout, dialogs and visual-viewport changes. CSS bundle passed 21 checks. These automated geometry/DOM/CSS checks do not claim browser visual acceptance. No dependency/public API/version change, package, commit or publication was made by this implementation task; packaging is coordinated separately.

Manual acceptance: show short and long notices beside unsaved actions and compare both outer edges; verify only the action panel's left edge moves during shrink/restore. Replace a message while visible and watch its bottom entrance replay. Pause the countdown with hover, keyboard focus and a background tab; verify it resumes from the same fraction. Check a persistent notice, narrow window, multiple messages, a Dialog and reduced motion. Confirm content remains scrollable and save/discard remain reachable.
