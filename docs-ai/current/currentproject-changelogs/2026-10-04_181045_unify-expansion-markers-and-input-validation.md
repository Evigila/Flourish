# Compact headings, shared expansion markers and validated input examples

## Request and approved scope

The user requested a 38px collapsed page title, shared SelectBox-style triangles, right-aligned SelectBox pickers, input-height CheckBox squares and a working email-validation Field scenario. The 38px state exception and 48px checkbox size supersede prior Flourish-specific choices; portable common standards remain unchanged. This work changes Flourish only and adds no external dependencies or application-specific business behavior.

## Changes

- Optional Design keeps expanded page titles at 50px and reserves --f-type-page-compact=38px for actual compact title selectors in ApplicationShell, PageHeading, NavigationSurface, ContentSurface and legacy heading combinations. Existing scrolling thresholds remain unchanged.
- Framework exposes ExpansionIndicator with Expanded=false and optional Class. It renders the same disclosure-open counter glyph used by the customizable native SelectBox. Design supplies consistent geometry, 140ms rotation and reduced-motion handling. SplitButton, navigation, configured top-bar menus, ReferenceDropdown, MultiSelectDropdown, ServiceMenu and DataTable display menus reuse it. Native details menus rotate from their actual open attribute; custom split content and non-arrow icons remain supported.
- SelectBox's customizable native picker uses bottom/span-left positioning and a vertical-only flip fallback, keeping its physical right edge aligned when wider than its trigger. Native select binding and keyboard behavior remain intact. Unsupported browser engines retain platform-native popup geometry.
- Standard CheckBox and legacy/form/list checkbox surfaces use the shared 48px control-height token. The invisible legacy accessibility proxy remains 1px. Multiline input height is unchanged; display-menu check glyphs remain centered in their enlarged squares.
- Field's guide no longer injects an unconditional Error. The scenario uses an EditForm with Required and EmailAddress validation, actual field association, local success feedback and stale-result clearing after editing. The displayed scenario source is the compiled Razor component.
- Gallery registers ExpansionIndicator explicitly in Program navigation, the component catalog and the executable sample registry. It now covers 78 components, 542 API rows and 254 documented defaults. Active guidance, manual checks and the architecture inventory include the new files.

## Verification

- Gallery and test builds: zero warnings and errors.
- .NET regression suite: 98/98, including shared split/navigation indicator state, native menu semantics and the complete parameter/default audit.
- Node checks: 9 palette/style audits, 24 controls DOM checks and 8 RowActionMenu DOM checks passed.
- Isolated headless Edge automation: 50/38/50 title transitions; fixed document chrome; narrow headings at 760px and 320px; shared split rotation; top-bar hover/leave state; reference/multi-select reuse; wider 320px picker right-aligned to a 200px select; actual enum selection updates; 48px CheckBox and display-menu squares; DataTable details indicator opening/closing; email initial, required, invalid, corrected, successful and edited states; new guide defaults and reduced motion. No browser exceptions were observed in the main targeted run.
- HTTP checks: all 78 guides expose the five standard sections and default-value column; both generated CSS bundles contain the shared marker.
- Directory map: 896 maintained files and 132 directories; every listed path exists. Git whitespace check passed.

## Remaining manual checks

Use blazor-manual-tests.md for pointer/keyboard, zoom, long labels, narrow positioning and supported-browser checks. Native picker customization is browser-dependent; unsupported engines keep native fallback behavior. Automated checks used a temporary isolated host and did not stop the user's VS server or use Computer Use. No Git commit was created.