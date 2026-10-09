# Align compact actions and standardize Appearance links

Changed the shared InlineActions default from Center to End and aligned raw action rows with the same Framework rule. Removed DisplayBoard's raw-row centering override and supplied the available row width. Explicit Start/Center choices, wrapping, growing inputs, full-row grid/action sizing and page gutters remain intact.

Colors and themes now uses production InlineActions for Apply colors and Restore default. Configure Design uses a standard Secondary link Button for See framework integration. Palette handlers, validation, submit/reset types and the framework destination are unchanged. Updated catalog/API guidance, three-language text, the active API guide and 1.1.4-preview release notes. Historical records remain unchanged.

Verification: zero-warning/error Blazor and Gallery Release builds; 429/429 Blazor checks; 7,808 Gallery checks; 5/5 focused Node checks; 22,049 culture checks; 73 ChangeLog checks; 21 CSS bundle checks; clean diff whitespace. Browser visual acceptance remains manual. See [the correction report](../bugfix-reports/2026-10-09_compact-action-alignment.md) for the checklist and verification limits.
