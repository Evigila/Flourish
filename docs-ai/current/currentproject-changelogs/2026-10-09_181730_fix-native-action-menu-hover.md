# Fix native ActionMenu hover opening

- Enabled OpenOnHover for native ChildContent through the existing disclosure/menu controller and bound the Gallery scenario switch to both menus. Actions and ChildContent remain mutually exclusive content contracts.
- Prevented enhanced hover-mode summary clicks from closing an already hovered menu. Preserved one SSR details DOM, native click/keyboard opening, touch activation, action transport, focus ownership and teardown.
- Guarded trigger preview/pressed paint against aria-disabled and added static expanded feedback. Existing hover background declarations were retained; no browser-observed style override is claimed.
- Updated Framework/Gallery usage and parameter guidance in all three languages, corrected the active convergence note and added the concise fix to 1.1.4-preview. The prior native-hover prohibition is superseded; append-only history remains intact.
- Replaced rejection-only native-hover regressions with executable support/lifecycle checks, and added actual controller and full-import light/dark trigger cascade coverage.

Verification: Blazor/Gallery Release builds report zero warnings/errors; 428/428 Blazor checks; 7,776 Gallery checks including 171 event examples and 191 ChangeLog assertions; four focused Node suites pass 19/75/5/13 checks; 22,146 culture, 64 ChangeLog, 21 CSS bundle and 194 CSS SDK checks. The sandbox temporarily denied the first CSS SDK fixture path; its automatically approved rerun passed. Three edited JSON files and the new preview key's three translations were independently verified. See [the diagnosis and manual checklist](../bugfix-reports/2026-10-09_action-menu-native-hover.md). No Computer Use, dependency change, tag, publication or Git commit was performed.
