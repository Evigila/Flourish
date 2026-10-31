# Add a read-only equal-width step indicator

Recorded: 2026-10-30 23:55:13 America/Sao_Paulo (UTC-03:00).

Implemented `StepIndicator` and its `StepIndicatorItem` public contract. The full-width semantic ordered list divides available width equally, wraps labels and marks exactly the supplied current step. Null CurrentKey supports a neutral/cancelled host state. Navigation, transitions and business rules remain host-owned. Optional Design reuses semantic accent/ink, surface and border colors. Gallery supplies three-language guidance, parameters and an executable sample.

Actual CSS measurements corrected narrow-segment inline padding. A fresh build also required explicitly classifying the preceding CollectionView's selector in the bounded split-panel cascade test; production collection behavior remains unchanged. The package-consumer fixture now compiles and renders the public step API in all four consumption modes.

Release builds all twelve Blazor projects without warnings/errors. Framework 512, Gallery 8939, package consumers 151, other Node 289, controller/actual-CSS geometry 43 with zero skips, Culture 25568, changelog 197, CSS bundle 21 and CSS SDK integration 194 passed. The complete source tree is 727 files / 121 directories. The 33 external package/version pairs are unchanged.

All six packages are frozen locally as `1.4.5-preview.3` in `artifacts/step-indicator-20261030/nuget`, with 25 source/test/build hashes, source snapshots and package hashes in parent provenance. Source VersionPrefix remains 1.4.4; older previews are preserved. No Computer Use, host edit, external dependency upgrade, Git action or public publication occurred. See [the diagnostic and acceptance record](../1_bugreports/2026-10-30_equal-step-indicator.md) for retained initial failures, exact API, evidence and manual checks.
