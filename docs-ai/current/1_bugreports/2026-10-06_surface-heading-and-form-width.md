# Surface headings and form widths

## Symptoms and causes

Colligere's Workspace uses the production NavigationSurface and PageHeading, not a private PageHeader. Published Framework 1.1.1 and its registered scroll modules are present in the consumer. Design already reduces heading height and type size for ApplicationShell, NavigationSurface and ContentSurface, but Framework's compact title row, square parent link and hidden parent text rules matched only ApplicationShell. The supported roots therefore had different compact structure. This establishes a library defect, not proof that every reported live failure was caused by missing JavaScript.

ReferenceDropdown is also a real production control. Its outer selection surface defaults to 240px and lacked a Field-specific width rule. Field.FullWidth spans columns; it does not stretch the reference trigger. Replacing the reference with a plain SelectBox would lose search and independent reference creation.

A start-aligned DisplayBoard constrained form content to its intrinsic width. The existing full-width child list did not include Section, FormLayout or FormGroup. A customer-address board could therefore use the correct controls and still have narrow fields.

## Library corrections

The five compact structural rules now share all three supported roots and explicit compact heading mode. The production scroll controller, hysteresis, short-page guard and document lifetime are unchanged. There is no alias class, alternate header renderer or consumer scroll implementation.

ReferenceDropdown directly inside Field's control slot fills that slot. Standalone selection retains the bounded 240px default. Start-aligned DisplayBoard stretches its direct Section/FormLayout/FormGroup children; centered previews retain their alignment. DisplayBoard remains a bounded composed-content background, not a replacement page shell. No new width API or compatibility contract is introduced.

Gallery's reference sample now uses real Field/FormLayout slots, its navigation sample includes a parent link, and its board sample contains a plain start-aligned Section/form composition. All captions reuse the existing localized catalog.

## Verification

Release builds of Tests.Flourish.Blazor and Gallery.Flourish.Blazor completed with zero warnings or errors. The console suite passed 373/373 checks. The new surface-form-layout Node suite passed 15/15, and its combined run with inline-toolbar-layout passed 17/17.

The separate palette/layout run passed 21/21, including page-action gutters, both-theme roles and expanded/compact heading type sizes.

Node executes the actual shell.js and surfaces.js with simulated geometry: collapse/expand thresholds, short-page restoration, stage replacement and teardown are covered for all three roots. CSS assertions cover the complete compact selectors, field reference width and start-board form width. These checks do not substitute for browser computed-style acceptance. The generated Release bundled-css/framework.css contains the corrections.

## Release boundary

The user authorized source repair and verification but explicitly deferred publication. VersionPrefix remains 1.1.1; no package was repacked, uploaded or substituted into Colligere. These source corrections require a future newly versioned release and consumer upgrade. Do not overwrite published 1.1.1 or add a consumer skin to bridge the gap.

## Manual acceptance

1. In the navigation Gallery sample, scroll down and back up; compare parent-link geometry, title size and alignment with the configured shell in both themes.
2. Change sample documents and repeat. A short page must not oscillate between expanded and compact headings.
3. Check enabled, disabled, empty and long-label reference fields at wide and narrow widths. Search, creation, selection and focus must remain usable without overflow. A standalone reference keeps its normal bounded width.
4. Inspect the start-aligned board's form field. It fills the board; ordinary centered button previews and copyable code remain unchanged.
5. After a future package release, restore/build/restart the consumer normally and repeat these checks there. A browser refresh alone cannot replace running server assemblies.
