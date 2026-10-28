# Wide-page header menu fitting and information-cell insets

Recorded 2026-10-28, America/Sao_Paulo (UTC-03:00). Local candidate **1.4.4-preview.shell.20261027.8**, based on committed shell .7 `feee3ea`. No remote publication, deployment or commit is involved.

## Symptom and root cause

Expanded/Fluid business pages left too little space outside their actual content bounds for the brand and action wings. The previous controller required both wings to fit those outside gutters before choosing a single row. It therefore stacked Services even when the real brand, two menu triggers and actions could easily fit the viewport. Using the stretched center track itself as a menu requirement would retain that error: the host's aligned InlineActions legitimately occupies the available content width, while its actual menu triggers occupy much less.

Information fields inherited the ordinary cell's 16px padding. The requested increase concerns the passive label/value region, rather than editable forms or ordinary action cells.

## Implementation and compatibility

The controller now measures each region recursively from its contained controls, padding, borders and gaps, excluding stretched wrapper widths, popup panels and absolute/fixed descendants. `aligned` preserves exact page-content edges when all controls fit there. `fitted` reserves the real wing widths, then clamps only the header menu track between them; surplus alignment gutter can be returned if needed. `stacked` remains the fallback only when actual brand + menu + action requirements exceed header width. For example, 220px brand + 264px menu + 180px actions fit at 664px and stack at 663px. Navigation rails, scrollbar offsets and RTL still use the owning surface's physical content bounds.

No public component parameter was added. Services retains the same DOM/reading order, native links and menu behavior. Menu controls join the existing ResizeObserver lifecycle, allowing changed language/text widths to refresh fit even when a stretched wrapper does not resize. Updates remain frame-coalesced and idempotent, with the existing own-style mutation exclusion and disposal. Footer foreground retains the original measured page edges, independent of a fitted header menu. PageBody width is never reduced. The 68px growable header minimum, 24px logo, real 48px Quiet brand Button and 8px/10px outer wing spacing remain unchanged. Without the browser module, the existing usable two-row CSS fallback remains.

Design increases only direct Information cells to **24px padding**. Their label/value content establishes the resulting height naturally; no fixed minimum, aspect ratio or maximum is added. Section titles stay outside, each cell remains one dl/dt/dd pair, values retain 34px H1 defaults and CopyText inherits the value type/copy behavior. Ordinary action cells retain 16px padding, and FormLayout is unchanged.

## Verification and delivery

- Twelve-project producer Release build: zero warnings/errors.
- Existing Framework checks: **485/485**. Header mock-DOM checks: **9/9**; complete native-select/CSS-cascade checks: **31/31**; CSS bundle checks: **21/21**.
- Header checks exercise stretched menu wrappers, exact/fitted/stacked boundaries, actual control-width changes, RTL, page/footer preservation and subscription disposal. Information CSS checks cover Rectangle/Square/fixed-height parents, natural rows and unchanged action caps/typography.
- All six local archives pass the existing package-set, dependency and required-asset verifier. Framework/Design CSS and header module match actual Release bundle/source bytes. The common and full fonts equal .7 byte for byte; twelve assets graphs retain the same 33 external package/version pairs.
- Existing Prepare remains stable-only; no debug-handler exists. The authorized explicit preview build/pack path wrote a new independent feed, `artifacts/header-information-20261028/nuget`, without overwriting .7 or earlier packages. Pack emitted the existing missing-package-readme informational notices; package verification passed.

Evidence is under `artifacts/header-information-20261028/`, including build/check logs, six package hashes and package-delivery.json. Generated evidence and documentation remain excluded from the maintained source tree, which stays **710 files/121 directories**. See [archive and asset hashes](../1_dependency.md#current-candidate-archive-and-asset-evidence). No browser geometry or visual acceptance was measured; consumer integration is recorded separately by its owner.

## Manual acceptance

1. At ordinary desktop widths, open the business/Admin Center Expanded page and Fluid pages with navigation rails both expanded and collapsed. Brand, Services and actions should share a row whenever the controls fit, with the 68px baseline retained for ordinary one-line controls.
2. Switch Standard/Expanded/Fluid routes and languages, including long account names and zoomed text. Check menu/brand/action separation, keyboard focus and native popup opening. Narrow only until controls need the existing second row; all actions must remain reachable.
3. On a wide presentation page, check exact menu-to-content alignment when gutters suffice. Resize into fitted mode and verify the footer foreground still follows actual page edges; its watermark stays full width. Repeat with RTL if used.
4. Check information fields with short, multiline and copied identifiers: 24px inset, body label/H1 value, successful copy, natural growth and one information item per cell. Confirm adjacent ordinary FormLayout/action grids retain their prior spacing.
