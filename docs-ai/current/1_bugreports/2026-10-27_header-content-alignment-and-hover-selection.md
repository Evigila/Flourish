# Header content alignment and hover selection

Recorded 2026-10-27, America/Sao_Paulo (UTC-03:00). Base: `37a42ae`, following the public 1.4.3 source/tag `d4e8cd9`. Current local candidate: **1.4.4-preview.shell.20261027.2**. The task authorizes library changes and Colligere integration; no public release, remote tag or cloud deployment is asserted.

## Request and ownership

The consumer requires a three-column header: branding before the page's central content region, service navigation at that region's start, and actions after it. Presentation pages use narrower containers, while business pages keep their actual Standard/Expanded/Fluid geometry. All geometry, behavior and paints belong to Flourish; Colligere only supplies product names, links, button variants and opt-in interaction parameters.

## Implemented layout

ShellHeader retains its public parameters and separates Services from the brand wing. ContentSurface now injects the configured Layout.ContentWidth, matching NavigationSurface. Framework-owned `patterns/header-layout.js` measures the actual page content edge, including inner padding, navigation rails and scrollbar space. Surface lifecycle hooks and the static document initializer synchronize it and release observers on removal.

When the measured wings cannot contain the brand/actions, the header reflows to two rows. The page itself does not shrink. Expanded or Fluid pages may require this even on a desktop viewport. CSS provides a safe two-row fallback before enhancement; exact content alignment uses the library measurement. NavigationSurface row height is automatic, so the second row is not clipped by a fixed 68px track.

Design-owned header roles give Quiet actions white text and Elevated actions white surfaces with the corresponding strong brand foreground. Both menu and dialog descendants are excluded. The stable button API is unchanged, and no host color override is required. White-surface actions use the light-palette primary rather than a dark-mode canvas or low-contrast light primary alias.

## Opt-in hover selection

ActionMenu already supported OpenOnHover and Weak dismissal. SelectBox, StandaloneSelectBox and LanguagePicker now expose OpenOnHover (default false) and Strength (default Weak). Native showPicker needs user activation, so hover is implemented by the existing menu controller rather than simulating a browser gesture. See [HTMLSelectElement.showPicker](https://developer.mozilla.org/en-US/docs/Web/API/HTMLSelectElement/showPicker).

The new `Components/Internal/NativeSelectPopup.cs` is an internal component with an activatable constructor and guarded asynchronous module lifetime. It wraps the original native select and ActionMenu. Selection dispatches the original change event, preserving InputBase binding, validation and the consumer's static preference adapter. Labels, invalid focus, disabled choices, form reset, type-ahead and cleanup are synchronized. Without script/Popover support, the native control remains the fallback. Framework owns visibility/layout; Design owns colors/shape.

Scoped popup surface/text roles are separated from header button paints. Theme copying continues to retain scoped brand/font values; only a panel that remains inside the actual header uses its own surface paint roles. Detached probes retain their original theme source. Ordinary form selection defaults and palette seeds are unchanged.

## Consumer color diagnosis

The original Colligere static `culture-picker.js` disabled the currently selected language during initialization. Native option disabled styles use opacity 0.5, which makes its checked primary marker appear lighter. The interactive language control did not disable its current option. The consumer now leaves that option enabled; its existing same-value guard still prevents redundant preference writes/reloads. Truly disabled options remain disabled.

This disabled-state path is the direct code-supported explanation for the static/interactive difference. The old native picker did not call copyTheme; popup theme isolation is a separate protection and must not be cited as the old native picker's direct cause. No old final browser pixel/computed-style measurement was performed.

## Packaging and verification

The six Core/Blazor packages are produced in `artifacts/shell-20261027/nuget`. Candidate .1 was built/consumed before the final dialog exclusion; .2 uses a new immutable version rather than overwriting the old package/cache. Essential remains 1.4.0 and no external package/service was added. The changed-source snapshot remains local pending review; the stable base commit alone does not identify all candidate source bytes.

Final producer Blazor solution compilation covers 12 projects, with zero warnings/errors. The 17-project consumer also compiled with zero warnings/errors. Packaging and source/cache/resource evidence are recorded in the current dependency inventories. Eight changed producer JS/MJS files passed syntax checks; two consumer language files also passed syntax checks. Tests were updated and header-layout.test.mjs registered, but no functional/Node suite, browser automation or Computer Use was run.

The consumer was restarted through its unified project entry point. All eight API/portal readiness endpoints returned HTTP 200; the running Debug dependency manifest selects the five final .2 packages. HTTP-served Framework/Design bundles, controls.js and header-layout.js match the packaged/cache bytes; the static culture adapter matches the corrected host source. This is runtime-resource evidence, not visual acceptance. The local graph remains available for user-run manual checks.

New maintained files are header-layout.js, header-layout.test.mjs and Internal/NativeSelectPopup.cs. Architecture tree ownership and package provenance remain under the existing current documents. Local logs/artifacts are ignored; no public NuGet publication is claimed.

## Manual acceptance

1. Match service navigation to actual presentation and business content at wide, 1366px and narrow widths; include Expanded/Fluid modes, rails, scrollbar and route changes. Confirm safe two-row overflow behavior and unaltered body width.
2. Confirm header Quiet/Elevated paints in light/dark and each consumer brand; popup and dialog content must keep normal surface paints.
3. Hover across trigger/panel without a dead gap or focus theft, then test leave, outside click and Escape. Use touch, Enter/Space, arrows, Home/End and type-ahead.
4. Verify current language remains enabled/normal-colored, same-value selection is a no-op, and repeated static/interactive saves plus failure retry work.
5. Verify native form binding/required/disabled/reset semantics for opt-in controls, asynchronous disposal, no-script fallback and Framework-only behavior.

These are user-run acceptance steps, not passing test claims.

## 2026-10-27 follow-up

This report retains the initial .2 implementation and verification snapshot. The subsequent .3 refinement uses the actual Quiet brand Button, structured single-column service menu rows and regular language labels. See [the follow-up diagnosis and acceptance](2026-10-27_standard-brand-button-and-structured-menu-rows.md) for current implementation and evidence.
