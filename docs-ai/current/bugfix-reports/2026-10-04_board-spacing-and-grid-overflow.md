# Board top spacing and grid overflow

## Symptoms

The user reported excessive blank space above code, insufficient spacing between consecutive boards, oversized directory cells and a directory grid that overflowed horizontally instead of wrapping.

## Source evidence and cause

The code board reserved 80px above its content for a copy action that was already absolutely positioned at the upper right. This created an unnecessary empty row. A generic board-content minimum height also applied when the content was a code block.

UniformGrid used width:100% while PageBody added left and right margins. Its border box therefore occupied the content width plus both margins. Automatic tracks also used a flexible 1fr maximum, so a small number of cells expanded far beyond their useful directory size. A shared adjoining grid skin made cells appear as one oversized panel.

This diagnosis comes from the actual CSS and the user's report. No browser pixel measurement is claimed.

## Mitigation

- Remove the copy action's whole-row top reserve. CodeBlock starts at the board's upper-left padding with zero internal code padding/minimum height. The copy action remains absolute at the upper right; a 56px code-side reserve prevents overlap. Empty copyable boards retain 80px minimum total height so the action stays inside them.
- Add 24px spacing between immediately adjacent boards. A board directly inside PageBody uses auto width so page margins do not add overflow.
- Use auto-width UniformGrid with row flow and automatic tracks capped by MaxCellSize, default 220px. Square applies 1:1 to each cell, with a default maximum side of 220px. A cell shrinks below that limit when the available width requires it.
- Separate Shape from Variant. Elevated is the default skin; Filled, Outlined and Danger are independent choices. Use individual rounded cells with 16px padding/gaps and 24px block margins. Form actions retain their existing connected-cell contract.
- Directory cells use short names and icons. The full component name and purpose remain in native title/accessibility attributes. Those attributes are passed together through AdditionalAttributes to avoid case-insensitive Title/title and captured-attribute conflicts.

## Verification

Gallery, the native consumer, the library test executable and an independent project containing all 77 copied Razor scenarios compiled with no warnings or errors. All 94 library checks, 21 CSS bundler checks and 77 HTTP guide renders passed. Delivered CSS was checked for the 220px track cap, row flow, board spacing and code/copy placement rules.

HTTP and source checks establish the composition and resource contract; they do not certify browser geometry. Explicit row-only layouts intentionally fill columns; automatic wrapping is the default mode rather than a promise for every explicitly constrained arrangement. Necessary code/table content overflow remains supported.

## Manual regression checks

- Rebuild/restart Gallery in Visual Studio and refresh the document. Confirm code begins at the upper-left padding, copy stays upper-right and consecutive boards have clear spacing.
- Resize each directory through wide, 760px and 320px windows. Square cells stay at or below 220px and wrap into rows without page-level horizontal scrolling. Check long titles and 200% zoom.
- Check all four appearances, per-cell overrides, custom MaxCellSize and explicit rows/columns. Repeat without Design and verify native behavior remains available.

See [the current manual checklist](../blazor-manual-tests.md) and [application color roles](../blazor-color-roles.md).
