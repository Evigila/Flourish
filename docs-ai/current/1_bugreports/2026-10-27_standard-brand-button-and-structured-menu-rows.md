# Standard brand Button and structured dropdown rows

Recorded 2026-10-27, America/Sao_Paulo (UTC-03:00). Follow-up to [header alignment and hover selection](2026-10-27_header-content-alignment-and-hover-selection.md). Its .2 verification remains a historical snapshot. Current local candidate: **1.4.4-preview.shell.20261027.3**.

## Symptoms and causes

ShellHeader branding was a custom anchor with independent hover/focus/active paint and stretched layout. It resembled Quiet but did not use Button, so feedback covered the brand wing. The consumer also composed its service menu as three unframed UniformGridButton tiles rather than ordinary dropdown rows. Language options needed an explicit regular body weight across native and enhanced paths; this is separate from the consumer's previously diagnosed disabled-opacity issue.

## Implementation

ShellHeader now composes the actual Button with Href, ChildContent and Quiet. Removed brand-specific interaction paint and full-height stretching; the wing remains a layout container and the Button owns its standard interaction feedback. Logo sizes, encoded names, context, navigation, three-column content alignment and narrow-screen fallback remain supported.

Design scopes structured dropdown actions to two body-sized 17px lines: primary text at 700 and description at 400, both in the normal text role. The existing Button Icon/Text/Description contract and ActionMenu default single-column panel provide the required composition without a new API. Ordinary Button descriptions retain their compact size. Native select options and enhanced language options explicitly use regular 400 body weight; selected markers and selection behavior are retained.

Colligere supplies localized service descriptions and composes framed single-column menuitem Buttons. It does not provide replacement styling or geometry. Framework continues to own layout/interaction; Design owns typography/paint.

## Verification and limits

The 12-project Blazor solution and 17-project consumer Release builds pass with zero warnings/errors. Six immutable .3 Core/Blazor packages were packed and five restored by the consumer. Nine changed library JS/MJS files and two consumer JS/MJS files pass syntax checks. Gallery and consumer translation JSON parse successfully. Source regressions cover real Quiet Button output, structured menu roles/content, typography scopes and regular selected-language text; suites and Computer Use were not run.

See the current dependency inventory for package/cache evidence. The candidate remains local; no public package publication, remote tag or deployment was performed. Earlier .1/.2 artifacts are retained rather than overwritten.

The consumer was restarted through its unified entry point. Eight readiness responses returned HTTP 200, the Debug dependency manifest selects five .3 packages, and served Framework/Design resources match .3 cache bytes. Four public portal responses contain a real Quiet brand Button, three structured service actions and descriptions without the multi-column panel class. HTTP markup/resource checks do not establish final visual or interactive behavior.

## Manual acceptance

1. Hover the logo/title Button and the unused brand wing separately. Only the Button should react; verify focus, press and navigation.
2. In the consumer service menu, verify one column, left icons and two body-sized text lines with bold names and regular descriptions, with no selection controls.
3. Verify every language label, including the current item, uses regular body weight in native/enhanced menus, while checkmarks and save behavior still work.
4. Check light/dark brands, keyboard, touch and narrow widths; preserve content alignment and safe two-row reflow.

These are user-run steps, not completed browser acceptance.

## 2026-10-27 footer/progress follow-up

This report retains the .3 snapshot. Current .4 enlarges and aligns the presentation footer and improves manual/directory progress selection; see [the follow-up record](2026-10-27_footer-alignment-and-progress-threshold.md).
