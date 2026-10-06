# Secondary label parity and chart heading

The configured Gallery shell and direct navigation item were both production entries but did not share label structure. This follow-up corrects long-label truncation and provides a chart-owned standard heading row with business filters and icon-only display selection. It complements the earlier [selection and lifetime repair](2026-10-06_navigation-selection-and-content-lifetime.md); that repair did not establish full parity between the two navigation entries.

## Cause and implementation

Gallery Routes uses ApplicationLayout/ApplicationShell and ConfigureNavigation entries. ApplicationShell's leaf puts text in f-navigation-label with a complete title. Direct SecondaryNavigationItem, consumed by NavigationSurface in Colligere, emitted bare label text. The existing shared ellipsis rule therefore applied only to the configured entry. The direct item now uses the same span/title and shared width-shrink constraints. A real configured-shell/direct-item rendering comparison verifies encoding, current state and complete names; the Gallery direct-item sample supplies a narrow long-label scene.

Configured tree nodes reserve icon/toggle space while a text-only direct item does not. Ellipsis responds to available width, not character count. Responsive shell/tree and tooltip composition remain supported library differences; there is no claim that every shell behavior is now identical. No host override, compatibility alias or new navigation API is added.

LineChart now accepts optional Heading and Id. With Heading, it composes the existing Section H2/anchor and places ControlsContent plus its owned MultiSelectBox in Section.Actions. Without Heading, the same controls and plot fragments retain ordinary chart composition. The chart uses remove_red_eye with Secondary trigger styling, preserving accessible localized Display text, selection validation and order/visibility ownership. The shared action group aligns right and wraps when necessary. Title remains the figure/table's accessible name and is not replaced by Heading.

Gallery's actual chart sample demonstrates the same heading/filter/icon row. The consumer does not own a second display controller or chart geometry. InlineActions and the previous document-lifetime changes remain intact.

## Verification

The final console run passes 373 contracts, including real header/filter/display rendering and configured/direct navigation comparison. Catalog coverage is 80 components, 695 parameter rows and 311 defaults. Gallery builds with zero warnings/errors in bin/NavigationParityVerification/net10.0. Framework and Design 1.1.0 are repacked locally; no publication or version/dependency change occurs.

The combined palette/menu/toolbar Node run passes 27 of 28; the navigation/chart-specific gates pass. The remaining palette assertion rejects AccessFormSurface Primary button rules in presentation.css, which this round does not edit. This style-contract conflict remains open rather than being suppressed. The section/BackToTop synthetic DOM suite passes 26 cases. These source/render/DOM tests are not physical-browser layout acceptance.

Colligere restores the local archives through hash-verified cache 1773efb58716c1a0d01468badc5597dc. Its actual package-consumer tests verify direct label/title markup, ellipsis constraints, the chart H2/filter/icon row and unchanged exact values. No sibling source reference substitutes for NuGet verification.

## Manual checks

1. Compare configured and direct long-label entries at equivalent available width. Check actual overflow ellipsis, full title, current state, encoding and both themes.
2. Inspect the chart sample and consumer at desktop/narrow widths. Heading, period and display share the standard header; controls align right and wrap without overflow.
3. Change the period and hide/reorder stable-key series. Display ownership and exact values remain chart-local. Check the localized icon tooltip/name and keyboard menu behavior.
4. Verify current section discovery after route changes. Restart existing hosts normally to load current assemblies; refresh alone does not reload server code.
