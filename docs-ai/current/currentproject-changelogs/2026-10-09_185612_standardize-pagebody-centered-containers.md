# Standardize PageBody centered containers

Added the Blazor PageBody.CenteredContainer property with Standard (default) and Expanded. Expanded halves horizontal side gutters, including their minimum, while vertical spacing remains standard. Effective centered layout is required; Fluid, FullWidth precedence and FillHeight retain their behavior. Each PageBody independently resets its centered scale.

Removed PageBody.CompactSpacing, its vertical CSS and Gallery checkbox/callers/metadata. Removed NavigationSurface.CenteredContentGutterScale and its sample control without compatibility aliases. NavigationSurface continues forwarding configured reference width. PageBody now demonstrates standard/expanded/fluid/full-width previews and provides all four selector choices. DisplayBoard supplies direct PageBody previews the available width. Updated three-language guidance, API metadata, active architecture/API notes and the 1.1.4-preview ChangeLog. Historical release descriptions and native WPF's separate older layout contract remain documented.

Both Release builds passed with zero warnings/errors. Blazor: 433/433 checks. Gallery: 8,178 checks, including 453 post-event and 219 ChangeLog checks. Page-body action Node checks: 5/5. Culture catalogs: 22,060 checks. ChangeLog validation: 85 checks. Generated CSS and diff whitespace checks passed. Browser appearance remains manual acceptance; no Computer Use, dependency/version change, Git commit, tag or publication occurred.

See [the correction report](../bugfix-reports/2026-10-09_pagebody-centered-containers.md) for cause, scope, verification limits and the manual regression checklist.
