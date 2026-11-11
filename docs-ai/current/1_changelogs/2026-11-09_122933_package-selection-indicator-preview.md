# Package selection indicators as a local candidate

2026-11-09 12:29:33 · America/Sao_Paulo · UTC−03:00.

The user authorized a local preview for Colligere consumption. Six existing Core/Blazor identities were built and packed as 1.4.8-preview.1 with explicit MSBuild overrides. Source VersionPrefix remains 1.4.7 and no external dependency declaration changed. The fresh immutable output is ../Colligere/artifacts/flourish-preview/1.4.8-preview.1.

Release production compilation and Verify-PackageSet passed. The provenance manifest records baseline 2bfb1fe9a838cb25b58b5f2bca26cf4a6f981aeb plus the current modified tree, hashes of 581 runtime build inputs, six package hashes and the production dependency closure. Build-input hashes matched before and after packaging. Packaged framework.css, controls.js and design.css were byte-for-byte verified against their current build/source outputs. Evidence: provenance.json, sha256.json and asset-verification.json in the feed; producer log artifacts/selection-preview-1.4.8.1.log.

The [implementation record](2026-11-09_111836_separate-checkbox-and-menu-indicators.md) contains the component, Gallery, style and behavior regressions and manual acceptance checklist. Consumer configuration and verification are coordinated in Colligere. No public publication, Git commit or push occurred.
