# Correct navigation, danger and pressed colors

## Request and authorization

The user corrected five parts of the application color contract: primary navigation on dark chrome selects Accent, third-level panels use Surface, Danger retains its previous red, a held press uses a darker click color than hover, and display-controller labels remain Text. This record supersedes the affected statements in the earlier ten-role palette change record without modifying that history. The independent dark palette remains authorized. These changes apply to Flourish.Blazor and Gallery; portable common standards and other platforms remain unchanged.

## Changes

- Foundation now defines thirteen roles per mode. Danger restores the previous Light value #9D322D and Dark value #FFB4AB. Light-target click is #B5C9C4 in Light mode and #C0CEC8 in Dark mode. Dark-target click is #2A4842 in Light mode and #335141 in Dark mode. Each click value multiplies the matching preview's RGB channels by 0.9 and rounds to the nearest integer; CSS consumes fixed tokens rather than generating new paint. The [active color contract](../blazor-color-roles.md) lists all values, mappings and supersessions.
- Primary navigation's persistent selected state uses Accent with Surface foreground in both modes. Ordinary secondary/third-level selection still uses Primary with Surface. Split route/disclosure buttons retain matching selection and independent expansion. Third-level siblings share a Surface panel.
- Hover, a held native activation and persistent selection have separate rules. Dark chrome uses the dark preview/click tokens; selected controls use the click token appropriate to their selected fill. Stronger parent-hover and selected-row rules no longer mask pressed feedback. Releasing restores idle, hover or persistent selection. Keyboard focus remains Accent and disabled controls remain excluded from press rules.
- Button/Grid Danger variants and extracted destructive/error/validation aliases resolve to Danger rather than Primary. Filled, Outlined, Elevated and Quiet retain their standard fills. Shared controls, table actions, primitive controls, menu rows and pattern chrome use the appropriate click aliases. Foreground colors remain readable in both modes.
- Display-controller row labels use Text while selected, unselected, hovered or pressed. Selection remains visible on the checkbox; idle rows stay transparent. Surface popups reset preview/click context even when their triggers belong to dark chrome. Framework's existing theme-copy operation normalizes both popup preview and click aliases and restores earlier inline values when it closes.
- Gallery Foundations now displays thirteen live swatches. Updated the active extraction record, manual checklist and two affected directory-map descriptions. No public component API or dependency changed; Framework-only native styling remains available.

## Verification

- Final Gallery build using an isolated artifacts directory: zero warnings and errors.
- Palette checks: 8/8 passed, covering exact thirteen-role Light/Dark/System values, fixed click derivation, approved paint sources, interaction rule precedence, popup labels, removed local theme mechanisms, static SVG paint and 26 default foreground/background contrast pairs.
- Menu/modal mocked DOM checks: 24/24 passed. The popup theme regression copies all thirteen roles, normalizes preview/click below chrome and restores prior inline values.
- A separate temporary HTTP host verified all 77 component guides, eight directories, ten documentation routes, selected split navigation, shared menus, six button variants, return actions, paging controls and delivered CSS/JavaScript. It also confirmed thirteen actual Foundation swatches, restored Danger values, both modes' click values, primary Accent selection, third-level Surface and neutral selected display labels. Only this temporary service was stopped; the user's VS Gallery process was preserved.
- HTTP evidence: C:/Users/Evigila/AppData/Local/Temp/flourish-control-http-9b4e689de2a745f8b85ce0fdd933058f.out.log and the matching .err.log.
- Git whitespace checks passed. No package was added or published, and no commit was created.

## Acceptance boundary

No Computer Use was performed. Source, HTTP output and mocked DOM checks do not certify rendered pixels or real pointer timing. Rebuild/restart the VS Gallery, refresh its document, and follow [the manual checklist](../blazor-manual-tests.md): verify Light/Dark/System, primary and split selection, third-level Surface, Danger, hover/held press/release/outside release, display-controller labels and popup placement below dark chrome. Existing user-deleted resources and historical records were preserved.
