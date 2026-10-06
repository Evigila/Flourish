# PageBody uniform grid overflow

Rectangular UniformGridButton rows placed directly in PageBody exceeded the content width because the grid requested 100 percent width while PageBody added horizontal content gutters. The correction belongs to Flourish Design, not to consumer styles. It preserves full-row rectangles, bounded square cells and the existing Columns and NarrowColumns API.

## Cause and affected compositions

Framework uniform-grid.css gives .f-uniform-grid width:100% and max-width:100%. Design foundation.css gives every nonheading direct PageBody child margin-inline:var(--f-content-gutter). A percentage maximum constrains the border box, not the outer margins. For a parent width W and each gutter G, the previous outer budget was W + 2G, and the right border reached W + G. With a 24px gutter the right edge exceeded the parent by 24px, not 48px.

Colligere's LicensePurchase purpose selection, CustomerEditor identity selection, CompanyEditor completion actions and NewWorkspace unlicensed entry use the actual library grid directly inside PageBody. Their component calls are valid. FormActions has the same percentage-width and margin combination. Grids inside an already bounded EditForm are a different composition and should keep filling that form.

## Correction and regression boundary

foundation.css now gives direct rectangular UniformGrid and FormActions children width:auto. Normal block sizing subtracts both existing gutters from the available row. The selectors intentionally use the direct-child combinator and the rectangle class: nested grids retain width:100%, squares retain fit-content, and explicit/narrow column tracks remain unchanged. No clipping, hidden overflow, fixed consumer width, host CSS or replacement component masks the defect.

The existing Gallery wizard at /examples/display/wizard already demonstrates direct PageBody grids with two and three columns and a narrow one-column override; no duplicate example or API was added. page-body-actions.test.mjs checks the combined gutter and sizing rules, full-row FormActions, nested sizing, square sizing and the actual Gallery composition. The existing release-run palette-checks.mjs imports these checks, so node --test tests/Tests.Flourish.Blazor/palette-checks.mjs includes them automatically. These are source/CSS contract checks, not browser geometry measurements. Fixed multi-column square fitting remains a distinct existing sizing concern, outside this rectangular form/wizard correction.

## Verification

- The two new CSS composition tests and 18 existing palette/layout checks pass.
- The Flourish console suite passes 356 of 356 checks, including Dialog results and NavigationGuard lifecycles.
- Native interaction mocks pass 75 of 75 checks; five SplitButton geometry checks also pass.
- Gallery Release builds with zero warnings and zero errors.
- Framework and Design 1.1.0 local candidates were repacked. Colligere restored the content-addressed cache 308dec277487a078392bd3cac2eb5295 and verified candidate/restored archive hashes. The restored Design bundle contains the correction. No package version or external dependency changed, and no package was published.

Only this report, its change record, the independent regression file, its one-line release-test import and the two-line CSS correction are committed. Earlier uncommitted refactors remain uncommitted at the user's explicit request. Verification describes the current working tree, not an isolated reconstruction from these narrow commits.

## Manual regression checklist

1. After a normal application restart, inspect the listed Colligere choice pages and Gallery wizard at 360px, 768px and desktop widths.
2. Expand and collapse workspace navigation; rectangles must fill the available content row without a horizontal page scrollbar or crossing its gutter.
3. Check long labels, two/three columns and the narrow one-column override. Choice, submit and cancellation behavior must remain unchanged.
4. Check a nested product/customer form grid and a square grid; the correction must not impose a rectangle width cap or change square sizing.

Aspire remains running. No Computer Use, real credentials, database changes or runtime restart were used for verification; the running instance has not automatically adopted the newly packed CSS.
