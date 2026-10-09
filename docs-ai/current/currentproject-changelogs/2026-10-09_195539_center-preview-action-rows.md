# Center direct preview action rows

Local date: 2026-10-09 19:55:39 (America/Sao_Paulo).

The user clarified that centered background boards take priority over the ordinary right-alignment rule in Button control examples and equivalent previews. Framework DisplayBoard now centers direct default action rows. InlineActions internally distinguishes omitted Alignment from an explicit value while preserving its public End default. Explicit alignment, nested page/form/dialog layouts, code/start boards and full-row sizing remain intact. No Gallery-specific CSS or new public parameter is added.

Updated production usage metadata, Gallery API guidance, three-language catalogs, 1.1.4-preview ChangeLog and focused rendering/CSS regressions. The [bug report](../bugfix-reports/2026-10-09_display-board-action-alignment.md) supersedes only the Board portion of the earlier compact-alignment diagnosis and supplies the manual checklist. Historical records remain untouched.

Verification: Framework/Gallery Release builds have zero warnings/errors; Framework 436/436; Gallery 8,275 (474 post-event, 227 ChangeLog); focused Node suites 5/5 and 16/16; CSS bundle 21; Culture catalogs 22,320; ChangeLog 91. No Computer Use testing, dependency changes, publication or Git commit. Browser visual acceptance remains manual.
