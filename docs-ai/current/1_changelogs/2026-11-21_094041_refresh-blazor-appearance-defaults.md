# Refresh Blazor appearance defaults

Recorded 2026-11-21 in America/Sao_Paulo (UTC-03:00).

## Changes

- Design defaults to self-hosted Noto Sans and Noto Sans SC with a generic sans-serif fallback. Source assets are 117 official variable WOFF2 web subsets, weights 100–900. Noto Sans includes normal and italic subsets. Noto Sans SC supplies Chinese coverage in normal style. The browser selects subsets by Unicode range. The optional Design package includes both OFL 1.1 notices and publishes MIT AND OFL-1.1 metadata. Framework Material Symbols continues to provide icons.
- The body, page title, compact page title, h1, h2 and h3 use 14, 42, 38, 36, 28 and 20 px respectively. Existing progressive table-card field labels use 12 px. Decorative display typography retains its own scale.
- Light remains the default runtime mode. It uses a cream canvas, silver chrome and display board, pale-yellow hover and press surfaces, deep neutral Primary and an ochre Accent. Brand-supplied seeds continue to color their Light chrome. Explicit Dark and OS-selected Dark retain the existing palette. Semantic status colors are preserved.

## Verification and delivery

- Release production build and six-package verification passed. The Design package's same-origin CSS URLs, 117 WOFF2 assets, two OFL notices and their pinned hashes were checked.
- The shared palette and typography checks passed. Existing browser fixtures cover the default root popup colors, local branded chrome inheritance and launcher operation. Manual acceptance covers font rendering across languages, thematic states and responsive typography in a consuming portal.
- Source remains based on 1.4.8. Local preview 1.4.9-preview.6 uses build properties. Replaying the appearance patch after the app launcher patch requires running scripts/Restore-NotoFontAssets.ps1 in Flourish before building. The helper verifies existing assets and acquires only missing assets from pinned sources. The browser loads fonts from the consuming portal.
