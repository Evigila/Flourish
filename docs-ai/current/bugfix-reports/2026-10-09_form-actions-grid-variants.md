# FormActions uniform-grid variant integration

## Symptoms and source evidence

The user requested that FormActions reflect the preceding UniformGridButton variant change. Child rendering already supported Elevated, FilledElevated and Danger, and the existing Gallery save/cancel compositions had migrated to FilledElevated with Elevated. The remaining defect was container presentation, not missing child variants or form command behavior.

The production Design import order loads controls.css before uniform-grid.css. The former FormActions container retained a Primary background and ink declaration. The later FormActions rule supplied Elevated child variables but used a 1px gap with a border-colored container background. This exposed a visible dividing line independently of the child's transparent interaction border. Its overflow:hidden clipped the child shadows and outward keyboard focus outline. Unlike the grouped UniformGrid, FormActions did not receive the shared occupied-cell drop-shadow.

These findings come from source inspection and complete-import CSS cascade regressions. No browser UI test established the original appearance on a physical display.

## Correction and ownership

UniformGrid and FormActions now share one default Elevated custom-property rule. Explicit child FilledElevated and Danger properties still override inherited defaults. The retired Filled and Outlined appearances remain absent; no alias or FormActions variant API was added.

FormActions uses transparent container paint and gaps, and joins the existing grouped occupied-cell drop-shadow rule. The legacy Primary container paint becomes transparent with ordinary text ink. Danger retains no individual item shadow; the shared outer elevation belongs to the action group and therefore also follows its occupied outline. Group elevation is not a new Danger item variant.

Direct FormActions child actions use the same -5px inward keyboard outline offset as UniformGrid. The existing grouped 16px corner clipping, 96px minimum child height, equal-width columns and narrow-window column adaptation remain intact. Submit transport, business callbacks, Busy and Disabled ownership are unchanged.

Gallery keeps the existing two previews and scenario actions, and adds a three-column variant preview: FilledElevated, an omitted Variant demonstrating default Elevated, and Danger. The default label is localized. Framework production guidance, Gallery API guidance and the active component usage guide now describe the supported variants, default, recommended pairing and group elevation in English, Chinese and Portuguese where applicable. The 1.1.4-preview ChangeLog includes a concise correction entry.

This record supplements [the preceding variant change](../currentproject-changelogs/2026-10-09_181019_converge-uniform-grid-action-variants.md), whose FormActions default migration did not yet address these container effects. Historical records remain unchanged.

## Verification and limits

The parent agent reports zero-warning/error Release builds for Blazor and Gallery, 429/429 Blazor checks and 7,808 Gallery checks, including 171 event examples and 195 ChangeLog assertions. The focused Node suites pass 19 checks: organization-access presentation 14 and page-body actions 5. The complete-import cascade verifies light/dark themes, all three variants, omitted defaults, Busy, Disabled, pressed and focused states, as well as FormActions background, group filter and structural geometry.

Culture integrity passes 22,169 checks, the ChangeLog validator passes 67, CSS bundle checks pass 21, and git diff --check is clean. No dependency, package version, Git tag or publication changed. No Computer Use or browser UI test was performed; actual appearance and pointer/keyboard interaction remain manual verification items.

## Manual regression checklist

1. Open FormActions in Gallery. Compare the existing two- and three-column previews with the new three-variant preview; the omitted Variant must look like Elevated.
2. Compare FilledElevated and Elevated outer group elevation, transparent dividing gaps and Danger's own fill in light and dark themes. The group should retain its shared outer shadow.
3. Hover and press all available actions. Background feedback should follow each variant without introducing borders.
4. Tab through the actions. The inset keyboard focus outline should remain visible within the clipped group corners.
5. Run the registration scenario. While submission is busy, draft/reset should remain unavailable; after completion, the existing draft/reset status callbacks should still work.
6. Resize below the existing narrow breakpoint. Three columns should adapt to two without overflow or loss of the existing grouped 96px minimum action height. Check the Forms save/cancel composition as well.
