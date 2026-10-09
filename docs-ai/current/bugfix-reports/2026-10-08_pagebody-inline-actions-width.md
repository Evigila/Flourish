# PageBody action row width correction

InlineActions directly inside PageBody now uses the width remaining after the page's gutter margins. The correction is verified and included in the user-authorized local 1.1.4-preview.fields.3 candidate consumed by Colligere. Public 1.1.3 remains unchanged.

## Cause and correction

Design applies margin-inline var(--f-content-gutter) to direct PageBody content. Framework applies width 100% to InlineActions with explicit Alignment. Both rules were individually intentional, but together their total outer width exceeded the containing page. The result was a misplaced end-aligned action area, including optional company/A1 actions and completion controls.

Framework now sets width auto for `.f-page-body > .f-inline-actions[data-alignment]` only. The selector accounts for the direct child's existing margins without changing gutter calculation, alignment, nested rows or Dialog. No hidden overflow, important override, host skin, new control/API or compatibility path is added. Existing centered/fluid/full-width, CompactSpacing and framework-only composition remain supported.

Gallery's PageBody sample exercises all three alignments as direct children and a nested Section action row under its real width/spacing controls. InlineActionsSample embeds its alignment examples in a real centered compact PageBody. They reuse existing translations and local action events. Real component SSR checks cover three width modes by two spacing modes, encoding, actual Button output and direct/nested structure; Node checks verify stylesheet scope and Gallery composition. SSR/static checks do not measure browser geometry.

## Candidate and verification

The Blazor-only Release solution builds without warnings/errors. Library checks 416/416, Gallery 7,266 including 124 post-event cases, Culture bridge 12, page-action Node checks 5/5 and catalog integrity 21,543 all pass. Six local Core/Blazor packages pass identity/dependency/asset validation. Colligere's restore metadata identifies all six matching .3 packages from its approved local feed, and the packaged Framework stylesheet contains the correction. Its Release solution builds without warnings/errors; the final selected Web run passes 352 cases with five database opt-in skips, while selected Core/Application runs pass 48/83 cases. Six company transport Node checks pass.

The package set retains Field.Actions and Boolean selection conversion from prior candidates. Stable VersionPrefix remains 1.1.3 and Essential remains 1.3.0. WPF is excluded. The prior feeds are preserved; no public upload, source-reference substitution, tag, commit, push, Azure change or live restart is part of candidate authorization. Expected package README advisories do not represent compiler warnings. Full release gating and authenticated browser acceptance remain separate.

## Manual regression checks

1. After a normal restore/build/restart, verify Gallery PageBody in centered, fluid and full-width modes with both spacing modes at wide and narrow sizes. Direct Start/Center/End actions must fit the page; nested actions and Dialog must retain their original placement.
2. In Colligere, verify company completion, actual A1 record actions and Concluir remain within centered content, without horizontal scroll or a host style override.
3. Verify long labels wrap within available space and that native submit, disabled/busy behavior, existing field actions and three-state Boolean choices remain functional. Acceptance is user-operated; no Computer Use was performed.
