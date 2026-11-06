# Timed floating feedback with reverse exit

2026-11-06 15:08:09 · America/Sao_Paulo · UTC−03:00.

FloatingNotice previously remained visible until its host removed it. It now defaults to 5000 visible milliseconds, then fades and moves downward along the entrance path in reverse. Exit uses a distinct animation name so an already completed entrance animation does not prevent playback. Reduced-motion users receive immediate dismissal. AutoDismissMilliseconds accepts zero for persistent feedback and rejects negative values.

Hover, descendant focus and a hidden document pause the remaining time. Changed visible content, severity or MessageKey begins a new occurrence; ordinary equal-content rendering preserves the deadline. MessageKey is the explicit mechanism for a repeated identical business event. Host hidden nodes remain hidden and receive a fresh lifetime when revealed. Framework uses its own wrapper state only: it neither changes the host Notice hidden flag nor clears business messages. A persistent host Error/Success value therefore does not make the same dismissed occurrence reappear on an unrelated render.

Generation and node-identity checks prevent a delayed callback from dismissing newer content. Timers, handlers and observations are released on removal. Closing notices continue sharing the bottom dock until their exit completes, then restore the original SaveChangesBar width. Frames remain fixed and zero-height; this introduces no new body clearance or document-flow region. Existing save-bar clearance remains owned by that control.

The component, controller, CSS, Gallery example, parameter meaning map, shared catalog and three-language descriptions use existing maintained paths. Source version declarations and dependencies are unchanged. This is local preparation for a later preview package; no pack, publication, tag or push occurred in this task.

Verification: Release solution build passed with zero warnings/errors. Framework passed 524 checks and Gallery passed 9135 checks; the public catalog reports 88 components, 819 parameter rows and 386 defaults. Floating lifetime/SSR/dock, SaveChangesBar and header regressions passed 44 Node cases, including 11 floating-feedback cases. Checks cover stale callbacks, child replacement, hidden pulses, content updates, pauses, reduced motion, unmount cleanup, native SSR reveal, dialog bounds and viewport changes. Automated DOM/CSS checks do not claim visual browser acceptance.

Manual acceptance: show feedback beside unsaved actions and wait five seconds; confirm downward fade and restored actions without a body shift. Repeat while hovering, focusing a nested action and switching browser tabs. Show identical feedback twice using MessageKey, reveal a host-hidden status, and inspect a narrow viewport and an open Dialog. Enable reduced motion and verify immediate disappearance. Set the delay to zero and confirm persistence.
