# Preserve real header control widths and the ordinary height

Recorded 2026-10-28 11:51:24, America/Sao_Paulo (UTC-03:00).

Local candidate **1.4.4-preview.shell.20261027.9** corrects the .8 intrinsic-width budget. It had reduced each standard 48px language/logout icon Button to its 26px glyph, allocating a right-hand track 44px too narrow and causing wrapping/header growth. Measurement now retains actual control boxes, CSS minima and fixed nonshrinking row-flex bases while still ignoring stretched layout wrappers. Existing 68px growable minimum, Quiet brand/24px logo, .8 fitted menus, Information24px insets and genuine narrow-screen wrapping remain. No production CSS, public API or maintained source file was added.

Release12 projects passed with zero warnings/errors; Framework **485/485**, header **9/9**, CSS-cascade **32/32**, CSS bundle **21/21** and six-package verification passed. The end-to-end business mock includes identity, enhanced native-language selection and native POST/logout, including recovery from already-constrained boxes; complete CSS derives the shared ordinary baseline. These are source/mock checks, not measured browser geometry.

The independent frozen feed is `artifacts/header-height-20261028/nuget`; earlier archives remain intact. Header JS matches source; both CSS bundles and both fonts match .8 byte for byte. No external dependency change, Computer Use, deployment, remote publication or commit occurred. [Root cause and manual acceptance](../1_bugreports/2026-10-28_business-header-control-width-budget.md) and [archive identities](../1_dependency.md#current-candidate-archive-and-asset-evidence) retain the evidence; consumer verification is recorded separately.
