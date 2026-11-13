# DisplayBoard preview action alignment

## Symptoms and cause

After compact page actions were changed to align right, the Button control preview also aligned right. Its direct raw Framework action rows occupy the board width and inherited the global End justification. Other control previews using the same rows or default InlineActions had the same cause.

The user's later instruction gives centered background boards priority for control previews. This report supersedes only the DisplayBoard portion of the earlier [compact action alignment report](2026-10-09_compact-action-alignment.md). Ordinary page actions still use the approved End default.

## Correction

Framework display-board.css now centers direct action rows in centered DisplayBoard content when no alignment was explicitly supplied. InlineActions records parameter presence through an internal data attribute; its public Alignment parameter and End default remain unchanged. Explicit Start, Center and End retain their requested behavior. Raw direct preview rows receive the same contextual centering rule.

The selector excludes code-block boards and does not reach through nested PageBody, form or dialog layouts. Start-aligned boards, wrapping, growing inputs and full-width grids/actions retain their existing sizing rules. Button, SplitButton and other previews share this library rule without per-page Gallery CSS.

Framework usage guidance, Gallery API descriptions and English/Chinese/Portuguese catalogs explain the contextual default. The 1.1.4-preview ChangeLog records the correction. Structural regressions distinguish omitted from explicit alignment and constrain the CSS selector to direct preview rows.

## Verification and limitations

- Framework and Gallery Release builds: zero warnings and zero errors.
- Framework executable checks: 436/436.
- Gallery executable checks: 8,275, including 474 post-event checks and 227 ChangeLog checks.
- Focused Node suites: page-body actions 5/5 and surface/form layout 16/16.
- CSS bundle: 21 checks. Culture catalogs: 22,320 checks (Gallery 1,552 keys, Framework 286, Culture extension 6).
- ChangeLog validation: 91 checks across four versions, including 1.1.4-preview.

No Computer Use or browser visual test was performed. Automated checks establish rendering contracts and CSS scope; pixel placement and responsive appearance remain manual acceptance items. No dependency, package version, tag or publication changed.

## Manual regression checklist

1. On Button and SplitButton, inspect control previews at wide and narrow widths. Compact direct preview rows should center and wrap without overflow.
2. Check InlineActions examples with explicit Start, Center and End. Each should retain its selected alignment inside a centered board.
3. On Colors and themes, confirm Apply colors, Restore default and See framework integration remain right-aligned in ordinary page content.
4. Check nested PageBody/form/dialog examples and full-row FormActions/UniformGrid content. Their layout and available width should remain intact; start-aligned and code previews should retain their existing placement.
