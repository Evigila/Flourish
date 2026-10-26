# Prepare Flourish 1.4.3 stable

Time: 2026-10-26 08:25 America/Sao_Paulo (UTC-03:00).

Promote the reviewed portal/access/selector/loading/Notice/Field-hover preview to stable 1.4.3, retain the six Core/Blazor package identities and Essential 1.4.0, and provide complete three-language Gallery release notes plus an empty 1.4.4-preview.

The final review identified a known hosted notification timer test race. Use the platform TimeProvider internally with the unchanged system-clock default and advance time explicitly in the replacement-expiration test, preserving cancellation and state/version assertions. No dependency or public API was added.

The final unified Prepare passed: zero build warnings/errors; Core 367, Framework 474, Culture 27, Gallery 8593, Node 198 with no skips, actual menu/browser 42/42, CSS 21/194, launcher 95, culture catalogs 24787, ChangeLog 158 and fresh candidate consumers 143. The focused notification tests passed 8/8 and stable/preview fences passed 12. All six stable package archives verified. See [release verification](../release-verification.md#flourish-143-final-preparation-2026-10-26) for evidence and limits.

This record is preparation, not a claim of public publication. Remote tag, hosted publish, public indexes and public-only restore are coordinated after the reviewed Git commit/push. Audit completion was not refreshed.
