# Bound direct PageBody actions

The user approved a library correction and local-only 1.1.4-preview.fields.3 candidate. Direct aligned InlineActions rows now account for PageBody gutter margins through a scoped width-auto rule; nested rows and public APIs remain unchanged. Gallery and real SSR/Node regressions exercise width modes, spacing and action alignment.

Blazor Release build passes with zero warnings/errors. Checks pass: library 416, Gallery 7,266, bridge 12, page-action Node five and catalog 21,543. Six candidate packages and Colligere package provenance pass verification; consumer Release build and selected regressions are recorded in the [issue report](../bugfix-reports/2026-10-08_pagebody-inline-actions-width.md). Field/Boolean corrections and prior feeds are retained. No public publication, version-props change, WPF work, commit or push is authorized by this local candidate.
