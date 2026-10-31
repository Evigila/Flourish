# Configure reference dropdown result limits

Date: 2026-10-30 22:44:39, America/Sao_Paulo (UTC−03:00).

`ReferenceDropdown<TValue>.MaximumResults` now allows a positive bound or null for the full supplied catalog, preserving the default fifty matching items. This fixes catalogs whose later entries were unreachable by scrolling while retaining existing filtering, selection, focus and disabled behavior. Gallery documents the parameter in three languages and records it under 1.4.5-preview.

Local candidate **1.4.5-preview.1** is frozen as six aligned packages at `artifacts/reference-dropdown-20261030/nuget`, based on `7c6f4e2`. Stable source props and external dependencies remain unchanged. Release build: zero warnings/errors; Framework **501**, Gallery **8779**, package consumers **147**, changelog **191** and localization integrity **25271** checks passed. The six-archive verifier and whitespace check passed. A deeper isolated Framework test run exposed thirteen existing source-path assumptions; the conventional candidate test run passed all checks, as recorded in [the bug report](../1_bugreports/2026-10-30_reference-dropdown-result-limit.md).

This is a local preview only. No Computer Use, app restart, commit, push, remote tag, NuGet publication or Azure deployment was performed. Consumer configuration and manual UI acceptance remain host-owned; the linked report contains a manual checklist.
