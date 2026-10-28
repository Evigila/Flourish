# Fit wide-page header menus and enlarge information insets

Recorded 2026-10-28 10:45:25, America/Sao_Paulo (UTC-03:00).

Local candidate **1.4.4-preview.shell.20261027.8** starts from shell .7 source `feee3ea`. ShellHeader now measures intrinsic menu controls rather than requiring the page's full center track plus wings to fit. Exact content alignment is retained when possible; fitted mode keeps ordinary wide business pages on one row by clamping only the header menu track. Actual control overflow retains the existing two-row fallback, with unchanged 68px minimum, 24px logo, standard Quiet brand Button and independent footer/page boundaries.

Information-mode UniformGrid cells alone increase internal padding from 16px to 24px. Natural row growth, one label/value pair, body/H1 type, CopyText semantics and outside section titles remain; ordinary action/form grid spacing is unchanged. No public API, maintained source file or external dependency was added.

Producer Release passed with zero warnings/errors; Framework **485/485**, header **9/9**, native-select/CSS **31/31**, CSS bundle **21/21** and six-package verification passed. Bundled CSS/header JS match delivered source/output, both icon fonts remain byte-identical to .7, and twelve assets graphs retain 33 external package/version pairs. The independent local feed is `artifacts/header-information-20261028/nuget`; old feeds are intact. No Computer Use, browser geometry measurement, deployment, remote publication or commit occurred.

[Diagnosis and manual acceptance](../1_bugreports/2026-10-28_wide-header-menu-and-information-insets.md) and [exact archive identities](../1_dependency.md#current-candidate-archive-and-asset-evidence) retain the detailed evidence. Consumer integration belongs to its separate coordinated record.
