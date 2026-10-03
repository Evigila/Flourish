# Sheet origin and reference-dropdown ARIA state

## Symptoms and evidence

In the authenticated Workspace review, closing a sheet opened from a row menu returned focus to the document instead of the stable row trigger. The transient action item had disappeared before the asynchronous sheet render captured the active element. Gallery reproduced the same interaction path.

The extracted ReferenceDropdown also inherited boolean HTML attribute serialization: an open trigger emitted an empty aria-expanded attribute, and option selection did not emit the literal ARIA state. The popup could be opened with actual pointer input and Enter, but accessibility state and the state-based chevron selector were incorrect.

## Causes and ownership

Framework owns menu/sheet focus lifetime and dropdown markup. Host asynchronous callbacks are legitimate and must not require product-specific focus repair. Capturing document.activeElement only after menu dismissal loses the originating control. ARIA expanded/selected values require explicit true/false strings rather than boolean HTML-attribute handling.

## Mitigation

- Private primitives/interaction-origin.js records a recent row trigger before the menu action dismisses its surface. A sheet first prefers a connected ordinary invoker, otherwise consumes the recent connected origin. Detached elements are ignored and the capture expires.
- ReferenceDropdown emits explicit strings for the trigger's aria-expanded and both empty/typed option aria-selected attributes. Selection and creation callback behavior is unchanged.
- Design loads foundation CSS before component and Workspace skins. Low-specificity base links allow title-bar foreground inheritance. Route headings retain the no-outline presentation while actionable keyboard focus remains visible.

The origin module is internal. Consumers continue using the public RowActionMenu and BottomSheet contracts; no host focus bridge or appearance dependency is required.

## Verification

- Authenticated Workspace and Gallery: row action opens a sheet; Escape closes it and returns focus to the connected row trigger. No source confirmation, persistence, publication or deletion was executed.
- Five mocked-DOM checks cover recent origin capture, consumption, ordinary invoker precedence, removal and asynchronous close.
- Four actual HtmlRenderer checks replay emitted event callbacks and verify expanded/collapsed state, selected options, disabled guards and encoded text. Tests do not mutate private component state or add public test hooks.
- Final console suite: 47 passed. Final Colligere Web suite: 1595 passed, 0 failed, 85 environment-gated database integration tests skipped.
- Debug and Release Blazor builds and the final Colligere AppHost build: zero warnings/errors. Independent offline Framework-only and Framework+Design NuGet consumers both restore/build with zero warnings/errors.

TRX, build logs, screenshots and independent package report remain in the task artifact directory. Current acceptance guidance is [blazor-manual-tests.md](../blazor-manual-tests.md).

## Limits and manual regression

1. Open/cancel a sheet from a row action, then repeat after rerendering/removing the row. Focus must not target a detached control.
2. Open a single lookup with pointer and Enter. Check literal expanded state, chevron direction, search focus, Escape/outside close and focus return.
3. Compare Framework-only browser appearance with explicit Design adoption. Keep busy/dirty/save policy in host callbacks.
4. Check pure hover, held-pointer intermediate visuals, screen readers and additional browsers manually. The current browser interface exposes click/key/drag but no independent hover or held-frame operation.

This UI repair does not change authentication, session validation, access decisions, business persistence or wire contracts.
