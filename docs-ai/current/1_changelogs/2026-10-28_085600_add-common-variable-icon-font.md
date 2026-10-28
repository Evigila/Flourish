# Add a common variable icon font with complete dynamic fallback

Recorded 2026-10-28 08:56:00, America/Sao_Paulo (UTC-03:00).

Local candidate **1.4.4-preview.shell.20261027.7** retains the six Core/Blazor packages and Essential.Culture 1.4.0. Two disjoint font faces select a 125,404-byte common WOFF for 78 names/76 codepoints, while all other characters retain the unchanged 4,003,088-byte complete WOFF2. The common-only font requirement falls by 96.87%; the complete Icon name map, Filled mode, variation axes and Apache-2.0 attribution remain available.

Added the common manifest/font/CSS, a standard-library/Windows regeneration script and focused Node regression. Existing CSS bundling and package checks deliver the new font without another stylesheet request. No new third-party package, browser library or runtime tool requirement was introduced. Existing header/footer observers were not changed without profiling evidence.

Producer Release built with zero warnings/errors. Framework **485/485**, icon-subset **3/3**, CSS bundle **21/21**, ChangeLog **177 checks** and six-package verification passed; independent Windows default/Filled geometry and WOFF round-trip checks passed. The consumer reports 17-project Release zero warnings/errors, relevant Web **264/264** with zero skips, and five matching package caches. No browser timings or physical acceptance are claimed.

The existing Prepare route correctly rejected this preview under its stable-only guard. Explicit local build/pack delivered immutable archives to `artifacts/icon-subset-20261028/nuget`; old shell archives were preserved. No remote tag/publication, dependency installation or Git commit was performed. [Diagnosis, compatibility and manual checks](../1_bugreports/2026-10-28_common-icon-font-subset.md) and [archive identities](../1_dependency.md#current-candidate-archive-and-asset-evidence) retain the detailed evidence.
