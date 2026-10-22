# Selection marker columns and service identity hierarchy

Date: 2026-10-22, America/Sao_Paulo (UTC-03:00). Implementation and automated verification are complete; physical browser acceptance is pending. Final consumer test evidence was checked at 2026-10-22T13:02:03Z. This follow-up uses local 1.4.3-preview.portals.9 and builds on the [.8 presentation work](2026-10-21_product-hero-media-and-access-titles.md). Earlier preview packages remain separate artifacts.

## Symptoms and diagnosis

The consumer reports a small width contraction as its native language picker first opens, and selection markers that move with label length. Source inspection found a content-sized picker whose option rows also requested percentage width/max-width, creating cyclic intrinsic-size dependencies. Flex alignment moved the marker and label together; MultiSelectBox had the same group alignment. This explains a concrete sizing dependency and alignment defect, but source inspection does not prove the unique cause of a physical first-frame paint. Browser acceptance remains manual.

The previous LogoDisplayer primary ProjectName made Colligere large and ContextName small. Removing Compact's reduced title scale in .8 enlarged the wrong semantic line for service entry. Its image also centered against both lines rather than just the service title.

## Implementation and contracts

- Native select/option ownership, keyboard handling and form values remain intact. Automatic row sizing avoids the intrinsic percentage cycle; a reserved leading checkmark column is independent of text alignment. MultiSelectBox has a fixed checkbox track and a flexible label track. Popup width includes the longest label, marker, gap and ordinary padding, within the existing viewport cap. ReferenceDropdown and SearchAutocomplete retain their current selection styling and state; no additional visual marker or selection mechanism is invented.
- Native picker alignment is synchronized before opening and written only if its physical edge changes. No delayed show, text measurement, simulated select or new sizing parameter is added. Weak/Strong lifecycle, physical popup edge, vertical flip, height constraints and native selection remain independent.
- Optional LogoDisplayer.ContextPrimary defaults false and is effective only with nonblank ContextName. It promotes context to the ordinary fluid primary scale, puts the smaller ProjectName above it, and retains one encoded semantic heading in brand-to-context reading order. With Compact, a row subgrid places the image only on the main context row; LogoSize remains independent. Ordinary Account and default Compact behavior remain available. Blank context falls back to the existing layout. Description stays after the identity.
- Gallery production samples, usage/default metadata and supported-culture descriptions demonstrate the same contract. Colligere enables it through the public parameter with its existing explicit 96px image; hosts do not take over library geometry or paint.

## Verification

- Complete Blazor solution Release build passed with zero warnings and zero errors (26.36 seconds). Framework passed 459/459; the catalog audit covered 83 components, 762 parameter rows and 353 documented defaults. Gallery passed 8,541, including its 474 post-event and 359 ChangeLog subsets.
- Final Node checks passed: access/identity 13/13; native picker, placement and palette 61/61 (15 + 20 + 26); controls mocked DOM 77/77; split-menu 5/5. Preview identity and stable-release rejection passed 12/12. These are source/simulated-DOM results and do not certify physical native picker painting.
- Six exact local .9 packages were generated after source freeze and passed Verify-PackageSet identity, internal dependency and required-asset checks. The feed is ../Colligere/artifacts/local-nuget/1.4.3-preview.portals.9; earlier candidates were not overwritten. There is no new third-party dependency or maintained source file. Inventory evidence also matches 17 packaged entries (two CSS bundles, ten other static assets and five DLLs) to final producer output and consumed cache using SHA-256; all five consumed package archives match the local feed.

Logs are under ignored artifacts/popup-identity-9: restore.log, build.log, framework-tests.log, gallery-tests.log, access-layout.log, popup-node-tests.log, controls-dom.log, split-menu-checks.log, preview-version.log, pack.log and package-verification.log. The listed final library runs passed. Consumer results and manual acceptance are recorded in [the Colligere report](C:/Users/RC_Auditoria/source/Repos/Colligere/docs-ai/current/1_bugreports/2026-10-22_selection-columns-and-service-identity.md). No Computer Use, physical browser testing, normal AppHost restart, database/cloud operation, Git commit/push or public package publication occurred. Audit state is unchanged.

## Manual acceptance

1. Open the language picker for the first time with mouse and keyboard on product, access and authenticated surfaces. Check for any first-frame contraction and compare subsequent opens.
2. Switch between short/long labels and selected values. All checkmarks stay in one column; width includes the longest label and marker without changing with the selected label.
3. Exercise left/right single- and multi-select menus, disabled rows, filtering, long/wrapped text and narrow viewports. Markers retain their track, while labels follow popup alignment. Check service and SplitButton menu alignment and vertical flips.
4. Inspect ContextPrimary on/off, blank context, logo/no-logo, description and custom heading level. Compact service identity places small brand above large service name and aligns its image with that service row. Default Account remains unchanged.
5. Check light/dark, short/narrow screens and enlarged text. Login-method changes keep the brand/navigation stable and operations reachable; access dots and product media remain independent.
