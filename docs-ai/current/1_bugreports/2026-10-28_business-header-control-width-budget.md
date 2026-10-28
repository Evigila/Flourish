# Business header control-width budget and ordinary height

Recorded 2026-10-28, America/Sao_Paulo (UTC-03:00). Follow-up to [wide-header fitting and information insets](2026-10-28_wide-header-menu-and-information-insets.md). Local candidate **1.4.4-preview.shell.20261027.9** follows immutable .8 packages; no publication, deployment or Git commit is involved.

## Symptom and confirmed source cause

The business page's Services now shares the first row, but the overall header can remain taller than the presentation page. Comparing library source shows that NavigationSurface and ContentSurface both use auto header rows, and neither adds a business-only vertical header inset. The consuming project independently found no host ShellHeader geometry override. The defect is in the .8 width budget, which can wrap the right-hand controls despite a nominally single-row menu.

`Framework/wwwroot/patterns/header-layout.js` ignored every non-leaf element's measured width to avoid counting stretched slots and InlineActions. That also ignored the real box of a standard icon-only Button. `Design/wwwroot/controls.css` explicitly gives `.f-button-icon` a 48px width/height and `flex:0 0 48px`, with zero padding; its own glyph defaults to 26px. Recursing only into that glyph reserved 26px rather than the actual 48px target. Business EndActions contains language and POST/logout icon controls, producing a **44px combined shortfall**. `.shell-header-actions` correctly retains flex wrapping for reachable narrow-screen actions, so it wraps when this incorrectly small fitted track is allocated. Presentation text CTA controls usually include their size in text/padding and do not expose this same two-icon shortfall.

## Correction and preserved behavior

Intrinsic measurement continues to ignore stretched layout wrappers and popup/absolute/fixed layers, but now retains the bounding box of actual buttons, anchors, summary/select/input controls and the identity entry. It also retains existing CSS minimum widths and fixed-pixel nonshrinking flex bases for row-flex children. No 48px target size is hardcoded in JavaScript. This lower bound allows recovery even if an earlier narrow track already constrained the measured box.

No production CSS or height rule changes. The existing 24px logo, actual Quiet brand Button, ordinary 68px growable minimum, 8px block wing inset, .8 fitted horizontal behavior and 24px Information padding remain. Identity semantics, native POST/antiforgery, menu actions and independent footer/page content boundaries remain. Actual narrow-screen overflow still wraps; the repair does not impose fixed/max height, hide content or suppress wrapping.

The end-to-end mock uses the real structural pattern: identity200 + enhanced native-language summary48 + InlineActions gap12 + native POST form/logout48 + outer inset10 = **318px**, where .8 reserved 274px. It then reduces both control bounding boxes to 26px and confirms the CSS basis still restores 318px. With a 220px brand and 264px menu, 801px truly stacks and 802px fits, independent of previous wrapper widths. A separate complete CSS-cascade regression confirms the existing own 48px targets and flex bases for language/logout, resets document InlineActions margins, centers the native form, and derives `max(68,48+16+1)=68` for both surface types.

## Verification and delivery

- Twelve-project Release build: zero warnings/errors. Existing Framework checks: **485/485**.
- Header mock-DOM checks: **9/9**; complete native-select/CSS-cascade checks: **32/32**; CSS bundle checks: **21/21**. The new CSS fixture initially tried to read an inherited custom property as an own rule; it was corrected to inspect the defining surface token and glyph alias separately. Production code was unchanged by that assertion correction.
- Six packages pass existing package identity/dependency/required-asset verification in `artifacts/header-height-20261028/nuget`. Header JS equals maintained source; Framework/Design CSS equal actual Release bundles. Both CSS files and common/full fonts equal the previous .8 archives byte for byte. Twelve assets graphs still resolve 33 external package/version pairs.
- The existing stable-only Prepare guard is unchanged. Explicit authorized local build/pack generated a new .9 feed; .8 and earlier archives remain immutable. Pack emits the existing informational missing-package-readme notices. There was no package/tool installation, remote tag/publication, deployment or commit.

Evidence: `artifacts/header-height-20261028/` build/test/pack logs, package-hashes.json and package-delivery.json. [Current dependency identities](../1_dependency.md#current-candidate-archive-and-asset-evidence) retain all six hashes. No new maintained source files were added: the tree remains **710 files/121 directories**. No Computer Use, browser automation or physical header measurement ran; consumer integration belongs to its separate coordinated record.

## Manual acceptance

1. Compare ordinary desktop Admin Center/other business header with a presentation page at the same width. With normal identity text and controls fitting, each should use the standard single-row height, with language and logout targets remaining fully visible and clickable.
2. Resize from a previously wrapped/narrow page to desktop, switch language and navigate Standard/Expanded/Fluid pages. Confirm both the menu and right-hand controls recover a single row without clipping; ordinary 48px buttons must retain their target size.
3. At narrow widths or enlarged text, confirm genuine overflow still wraps and all actions remain reachable. Check native POST logout, language selection, identity and keyboard focus.
4. Recheck presentation/footer alignment and Information copies to confirm .8 geometry and 24px insets remain intact.
