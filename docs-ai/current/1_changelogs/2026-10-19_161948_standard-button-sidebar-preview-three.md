# Preserve the standard Button contract in Sidebar preview three

Recorded 2026-10-19 16:19:48, America/Sao_Paulo (UTC-03:00). Source version: 1.4.3-preview.portals.3. The earlier .1 and .2 local artifacts remain distinct; no package cache is overwritten and Gallery continues to aggregate these builds under 1.4.3-preview.

The .2 selected-text contrast correction addressed one role pair but left a more basic contract issue: Sidebar-specific current-page paint overrode ActiveVariant, so Primary, Danger and Elevated could not render their standard selected appearance. The final .3 candidate removes all three extra current-page paint rules. Sidebar retains only its responsive structure and two tonal surfaces. Actual standard Button instances now own selected and interaction paint entirely, and the Gallery explicitly selects Primary for the strong selected item. Its three-language ActiveVariant explanation matches this behavior. Grid and Compact retain their existing defaults.

Renderer coverage now checks Primary, Danger and Elevated reach the selected actual Button unchanged and inactive choices retain their requested variant. The palette regression replaces the retired current-page-rule assertions with checks that Sidebar contributes no choice paint, standard Primary/Secondary/Danger/Elevated cascade results remain unchanged, and the actual default Light/Dark Primary label pairs meet 4.5:1 throughout normal, hover, focus and press states. The actual node --test tests/Tests.Flourish.Blazor/palette-checks.mjs run passed all 26 checks. The coordinating task reports a successful .3 build, all six local packages and package-set verification; final renderer/Gallery and consumer verification remain with that task.

The preview-version guards and the exact package suffix remain in force. No new external dependency, public package, release tag, commit or push was created here. The earlier .1 parser defect and .2 role-pair correction remain recorded as history rather than rewritten as successful first attempts. Audit state remains unchanged.

Manual acceptance after the final consumer restore:
1. In Light and Dark modes, inspect the selected Primary choice, its icon and check during hover, keyboard focus and press; the normal Button appearance must remain recognizable and readable.
2. Compare desktop with a narrow viewport: the list sits beside one details surface, then stacks with wrapped labels and no horizontal overflow.
3. Activate both native GET choices and use back/forward and open-in-new-tab; confirm current-page state, unchanged form transport and no inactive panel affecting layout.
