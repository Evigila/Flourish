# Solid standard progress bars

Recorded: 2026-11-13 09:01:52 -03:00 (America/Sao_Paulo)

The optional Blazor Design no longer generates the white gradient highlight pseudo-element on standard ProgressBar. The obsolete f-progress-flow shimmer animation and its reduced-motion selector were removed. Both determinate and indeterminate fills keep their semantic primary color. Determinate width transitions and indeterminate 35%-segment search motion remain; reduced motion, native fallback, progress values/ARIA, ProgressRing and independent PageHeading loading are unchanged.

Local-only **1.4.8-preview.14** preserves the preview.13 selected-dropdown changes. All six aligned archives were built using explicit CLI version overrides; the default source prefix stays **1.4.7**, and existing preview archives are unchanged. The source baseline remains **84bf6406f2750365e7b51fd7f707e355c9195ff4** plus reviewed uncommitted changes. No commit, tag, push or public publication occurred.

Verification: **47/47** targeted progress, heading-loading and selection cascade checks; isolated Release producer **0 warnings/errors**; six package contracts verified; packaged Framework/Design CSS exactly match the compiled bundles. Producer provenance beside the archives records **583** runtime inputs, with only controls.css changed since preview.13, and **32** unchanged external package/consumer entries.

[Diagnosis and manual acceptance](../1_bugreports/2026-11-13_090152_remove-progress-bar-shimmer.md)