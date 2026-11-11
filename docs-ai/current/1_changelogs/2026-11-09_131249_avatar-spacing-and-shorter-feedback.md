# Increase editable-avatar separation and shorten default feedback

2026-11-09 13:12:49 · America/Sao_Paulo · UTC−03:00.

The Framework rule for a directly contained editable Avatar followed by another Section child now reserves 36px after the row, up from 24px. Read-only and last-child avatars keep their existing spacing.

FloatingNotice's component default and static-markup JavaScript fallback now use 3500 visible milliseconds, a 30% reduction from 5000. Explicit AutoDismissMilliseconds remains an exact millisecond contract; zero remains persistent. Colligere production sources contain no explicit duration overrides, so the new default applies uniformly there. Entry and reverse-exit animations remain 180ms. Pause, replacement, progress, disposal and stale-callback behavior are unchanged.

The Gallery sample, three-language explanations, component usage catalog and component-render expectation use the new default. Node lifecycle fixtures retain an explicit 5000ms for existing timing scenarios; separate default/invalid-attribute cases verify 3500ms, while explicit 5000ms and zero are independently verified.

Verification: 29/29 targeted Node tests passed (19 floating notice, 10 avatar editor). The new spacing check constrains the selector to editable, non-last Section rows. No Computer Use was performed. Build/package verification is coordinated separately; this task does not publish, commit or change dependency/version declarations. No maintained file was added outside the documentation area.

Manual acceptance: open an editable avatar above the next form field and inspect its increased separation; show a default notice and confirm approximately 3.5 seconds of active visible time before the unchanged exit. Pause with hover/focus/background, replace the text, and verify the countdown resumes/resets correctly. Confirm a permanent notice remains visible.
