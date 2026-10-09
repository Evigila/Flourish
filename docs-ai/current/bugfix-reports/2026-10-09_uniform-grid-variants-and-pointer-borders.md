# Uniform-grid variants and pointer borders

## Symptoms and cause

The previous Blazor UniformGridVariant exposed Elevated, Filled, Outlined and Danger. Filled and Danger assigned a visible hover-border token; the common active rule explicitly painted the standard border. FormActions supplied Filled-like defaults even when an action omitted its Variant. This conflicted with the requested three-variant contract and default Elevated behavior.

## Authorized correction

Blazor UniformGridVariant now contains only Elevated, FilledElevated and Danger. Retired Filled/Outlined enum members, class mappings, selectors and preview entries are removed without compatibility aliases. FilledElevated retains the former Filled primary color roles and adopts Elevated's shadow. Hover and press borders are transparent for every variant; keyboard focus outlines remain. FormActions' inherited default cell paint is Elevated, and primary actions explicitly choose FilledElevated.

UniformGrid, UniformGridItem and UniformGridButton share the variant mapping and default/inheritance behavior. NavigationChoices uses FilledElevated for the current ordinary grid choice and Elevated for the others; compact ordinary Button variants remain unchanged. All Gallery callers, embedded code examples, three-language descriptions and accessible preview labels use the current contract. The independent WPF variant type is outside this Blazor page task. No authentication, navigation destination, submission, validation or session behavior changed.

## Evidence

- Both Blazor and Gallery Release test graphs build with warnings as errors and zero warnings/errors.
- 427/427 Blazor checks and 7,745 Gallery rendering/language/lifecycle checks pass. Coverage includes the exact enum inventory, defaults, grid inheritance, local overrides, selected choice changes and the FilledElevated/Elevated composition in real rendering.
- 37 focused Node tests pass, using complete Framework/Design import order to check light/dark themes, ordinary/form/grid contexts, underlying Button specificity, button/link states, shadows, unavailable/busy feedback and keyboard focus outlines.
- 22,135 catalog integrity checks, 61 release-note checks and git diff --check pass. A stale structured Button Gallery test from the preceding account-action change was updated to its current selection action; the complete library suite now passes.

## Limits and manual acceptance

No Computer Use or browser UI testing was performed. On the UniformGridButton page, check rectangular and square previews for only the current three appearances. FilledElevated must show the primary fill and elevation, Elevated must remain the default, and Danger must keep its destructive color. Hover/press each variant with no border appearing; use Tab to confirm the keyboard outline. Check the primary save/secondary reset pair, busy state and narrow layout. Verify UniformGrid's appearance switch cycles through all three variants and that FormActions and wizard/choice examples retain their existing operation behavior.
