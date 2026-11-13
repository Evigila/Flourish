# Navigation selection and document lifetime

The Workspace review found library defects in secondary selection paint and content-dependent controller lifetime. It also exposed missing composition for chart period controls and compact input actions. Corrections remain in the existing production entries, with Gallery and executable regressions; no host UI patch or compatibility renderer is introduced.

## Secondary navigation paint

SecondaryNavigationItem emitted is-active, but Foundation recognized is-selected. Foundation's normal/hover foreground used important priority, overriding NavigationSurface's separate is-active foreground. The unique selected class is now is-selected. Surface's duplicated secondary-link paint is removed; Foundation supplies direct/tree navigation selection, hover, focus and press with sufficient specificity. The palette regression calculates the actual selected/unselected cascade including important declarations and negation specificity, rather than merely checking that a rule exists.

## Content-dependent lifetime

NavigationSurface replaced its keyed main for a new DocumentKey while unkeyed SectionNavigator/BackToTop retained the same ContentId. Their DOM bindings remained attached to the removed main, preserving the preceding outline and scroll listeners.

Both controls now have independent keys containing DocumentKey and their component identity. They share the main lifetime without colliding with its key. Real HtmlRenderer rerendering checks fresh instances, cleared old entries, ignored late callbacks and no needless rebuild for an unchanged key. DOM checks additionally replace a main with the same identifier and verify current discovery, listener release and BackToTop targeting. Gallery's two simulated pages expose different outline titles/counts using actual production navigation items.

## Shared controls

LineChart.ControlsContent hosts business period/filter controls beside its own MultiSelectBox display entry. The existing toolbar aligns the control edges, spaces them and wraps when necessary. Series visibility/order remains chart-owned; disabling display options does not remove the host filter, and an empty toolbar is not rendered.

InlineActions now supports a direct actual input taking remaining width, with a naturally sized Button in Field's control slot and no extra top margin there. It remains a general compact action row, not another finite-choice wizard API. Gallery demonstrates native input/actions and a chart period choice. Catalog/parameter meanings document the actual entry; no additional component family is created.

## Verification and boundary

All 367 C# console checks pass, including genuine control rendering, navigation rerender and existing contracts. The selected palette/split-menu/inline-toolbar suites pass 28 checks; section-navigator-dom passes 26 internal checks. Current coverage is 80 components, 689 parameter rows and 308 defaults. Gallery's isolated Release build has zero warnings/errors.

Framework and Design 1.1.0 are packed locally only. Colligere restores/hash-verifies them in cache 858ebc01151f9beea0419fa7fff36580, builds cleanly and passes 130 focused cases. Its complete suite retains the three documented baseline failures and 85 database-dependent skips; full green acceptance is not claimed. The pack retains its missing-readme advisory. No dependency/version change, remote upload, running-process stop, browser automation or commit occurs.

Colligere inspection confirms direct SecondaryNavigationItem calls and no related host skin. Missing-heading reports do not establish a library H1 hiding rule: current source already has PageHeading and Compact changes size only. Consumer real-rendering tests verify the specified list headings. Retained runtime bytes and physical geometry still need user acceptance after normal startup.

## Manual acceptance

1. In Gallery and a rebuilt consumer, check direct and tree secondary selection in both themes across normal/hover/focus/press states, including keyboard focus and disabled entries.
2. Change documents with the same content ID; outline titles/counts and BackToTop should follow only the current region. Repeated same-document refresh must not recreate controllers or jump scroll unexpectedly.
3. Check chart period and display controls share a row at normal widths and wrap without overflow on narrow screens. Changing the period must not reset series visibility/order with stable keys.
4. Check Field/InlineActions input and Button, label association, validation, submit/disabled semantics and narrow layouts. Consumers must not add width/paint overrides.
