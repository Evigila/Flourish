# Restore joined grid and local fragment actions

## Request and changes

- Fixed the Button scenario's status-link navigation at the reusable component level. Fragment-only Button/SplitButton targets retain their current document path/query instead of resolving against the host's base href. UniformGridButton inherits the same repair. Other destinations and disabled/busy suppression remain intact.
- Reverted the independent-card grid styling to the earlier joined container. Internal cells share the outer shape, with thin Border dividers, no separate corner radius/shadow and inset focus. Restored responsive Rectangle width without the Square cap. Square's default maximum side is now 280px; a configurable maximum applies only to Square. Row/column/narrow APIs, H1/body content and the approved color/Variant contract remain supported. This supersedes the 220px limit and independent-card statements in the earlier layout/color records; those historical files remain unchanged.
- SplitButton keeps its 3px gap and normal outer corners. The primary half's two right corners and secondary half's two left corners now use 3px. Standalone and navigation variants agree; main navigation and disclosure remain separate, and selected colors stay synchronized.
- Updated Gallery API descriptions and the active extraction/manual acceptance records. No new maintained source file was introduced, so the explanatory directory tree's path coverage is unchanged. The [regression report](../bugfix-reports/2026-10-04_fragment-links-and-joined-grid.md) records causes, evidence and limits.

## Verification

- Gallery and library-check builds: zero warnings/errors. Library checks: 96/96. Palette audit: 8/8. Mocked menu/modal DOM checks: 24/24.
- Separate temporary HTTP host: 77 component guides, eight directories, ten documentation routes, shared menus, button variants, split selection, paging, return actions and delivered thirteen-role assets passed.
- Headless Edge automated regression: saved draft count survives the scenario's status-anchor action and path/query remain on the guide. Delivered CSS sizes/joined geometry passed at 320/760/1180px; Square stayed within 280px, Rectangle exceeded that limit at wider widths, and split corner geometry matched both contexts.
- Evidence: C:/Users/Evigila/AppData/Local/Temp/flourish-grid-fragment-624a8cfe546c4959826b268465152427.cjs and matching flourish-control-http-624a8cfe546c4959826b268465152427 logs.
- Git whitespace checks passed. No Computer Use tool, dependency addition, package publication or Git commit. Only the temporary service/browser created for verification were stopped; existing VS and unrelated work were preserved.

## Manual acceptance

Rebuild/restart Gallery and refresh. Save a draft, activate its status link, inspect Square/Rectangle and narrow directory wrapping, then inspect both standalone and navigation SplitButton corners, keyboard focus and independent actions. Repeat Light/Dark and the supported browsers using [the active checklist](../blazor-manual-tests.md). Automated fixture geometry does not certify every host composition or every browser.
