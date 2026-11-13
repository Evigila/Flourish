# Full-page shell document scroll ownership

## Symptom and earlier mitigation

The user reported that the extra body scrollbar persisted after the Busy-button repair. The full-page Gallery could therefore expose both the outer document and the shell's content scroll track, rather than confining page scrolling to its content area.

The prior [Busy-button report](2026-10-04_busy-button-scroll.md) remains historical evidence. Its contained screen-reader status, relative button positioning, board overflow removal and fixed clipboard fallback remain useful repairs, but did not establish full-page document ownership. That diagnosis alone was insufficient for this later persistent symptom.

## Source evidence and cause

ApplicationShell already owned an internal `.f-content-scroll` scroll area, but did not explicitly distinguish a full-page shell from an embedded shell. Without an html/body scroll lock and viewport-bounded root, document layout could still exceed the viewport. Fixing a conditional Busy child did not remove this independent outer scroll path. This diagnosis comes from the layout/CSS source and the user's report; browser scrollbar pixels were not measured in this task.

## Mitigation and boundary

ApplicationLayout now defaults OwnsDocument to true and passes it to ApplicationShell. The low-level ApplicationShell defaults to false so an embedded framework preview does not claim the surrounding document. A full-page shell emits `data-f-document-shell`.

Framework's functional CSS uses `html:has(.f-shell[data-f-document-shell])` and the corresponding body selector to set bounded height and hide document overflow. The marked shell is fixed to the viewport with inset zero, height 100dvh, min-height zero and hidden root overflow. Its existing content region continues to own scrolling. This behavior works without the optional Design package; theme CSS does not decide document ownership.

Embedded Gallery layouts explicitly pass OwnsDocument=false. They retain local viewport bounds and their own host scroll behavior. The selector ceases to apply when no marked shell remains. Tables and code blocks retain intentional horizontal/content overflow; this repair does not globally suppress every scrollable control.

## Verification and limits

Source inspection confirms the ownership parameter, root marker, functional CSS selectors and explicit embedded opt-out. Automated composition and DOM checks can verify those contracts and lifecycle behavior, but do not certify scrollbar pixels, dynamic viewport rendering or browser-specific `:has` support. No browser pixel verification is claimed here. Current build/test outcomes are recorded separately in append-only change records after the coordinated verification run.

## Manual regressions

- Open a long Gallery guide while idle and inspect the entire scroll range. Only the shell content should scroll; top bar and rails stay visible and html/body remain stationary.
- Repeat while Save draft is Busy, after completion and during source copying. No additional document or board scrollbar should appear.
- Check wide/narrow windows, 200% zoom, wrapped headings, keyboard/touch scrolling and reduced motion in styled and Framework-only hosts.
- Render an embedded shell with OwnsDocument=false in a scrollable page. It must remain within its host and must not lock the surrounding document.
- Leave the full-page shell and confirm the next layout can own document scrolling normally. Check that section links scroll their associated content and pending section alignment cannot run after disposal.

See [the detailed manual checklist](../blazor-manual-tests.md#document-scrolling-and-section-navigation) for heading, gutter navigation and lifecycle acceptance.
