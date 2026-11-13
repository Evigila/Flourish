# Board following text and home grid

## Symptoms and cause

A DisplayBoard followed by ordinary body text had no visible separation. Design had an adjacency rule only for two boards; Foundation resets paragraph top margins to zero. Therefore the Home minimal-setup paragraph and similar documentation could sit flush against a code board.

Replacing Home's four facts with the requested two-by-two Rectangle UniformGridButton also exposed narrow-window clipping. At 320px, the preferred 2:1 cell ratio and hidden overflow constrained the cell below the height required by the bilingual H1 title and 17px description. The default standalone Design cell minimum could override a Framework-only automatic minimum, so both layers needed consistent shape rules.

## Repair

- Design now supplies 24px top separation for content immediately following a board, using --f-space-6. The existing board-to-board 24px rule remains separate; no extra bottom margin doubles the stack spacing. The shared library owns the rule, without a Gallery-only paragraph fix.
- Home uses one Rectangle UniformGrid with Columns=2 and Rows=2 and four UniformGridButton links. Original titles, descriptions and routes remain intact; icons match the configured primary destinations. No wrapper or page-specific grid CSS was introduced.
- Rectangle cells use their automatic content minimum and visible internal overflow so the grid's intrinsic row sizing can grow for text. Design no longer replaces that minimum with the standalone cell height. The outer joined grid still owns clipping and its shared skin. Two-by-two remains explicit at narrow widths; 2:1 is preferred when content fits. Square remains 1:1 with its 280px maximum.

## Evidence and verification

Final Gallery build completed with zero warnings/errors. The eight palette checks passed. A separate temporary host served the Home page and all four destinations. Headless Edge automated checks verified actual two columns/two rows, correct destinations, complete cell content, no horizontal page overflow and a 24px board-to-paragraph gap at 1440, 800 and 320px. All four real Home links navigated correctly. A fixture using delivered Framework/Design CSS confirmed consecutive boards and following body text each have 24px separation without doubling.

Final regression script: C:/Users/Evigila/AppData/Local/Temp/flourish-home-board-dde9d5600a2340e2997c41a17088719c.cjs. Service logs: C:/Users/Evigila/AppData/Local/Temp/flourish-control-http-dde9d5600a2340e2997c41a17088719c.out.log and .err.log. Earlier checks detected the 320px clipping before repair; they are not acceptance evidence for the final source. The isolated browser/service were closed; the user's VS process was preserved. No Computer Use tools were used.

## Limits and manual checks

Fixture and Edge results do not certify every consumer composition or browser. Rebuild/restart Gallery and refresh. Confirm separation after code and dotted boards, keyboard navigation for the four Home entries, readable narrow-window titles/descriptions, joined grid hover/focus and both application themes. Preserving two columns with long text makes the narrow grid taller; this is intentional. No dependency or public API was added.
