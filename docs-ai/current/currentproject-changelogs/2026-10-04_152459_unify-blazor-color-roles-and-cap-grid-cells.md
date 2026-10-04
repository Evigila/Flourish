# Unify Blazor color roles and cap directory grid cells

## Request and authorization

Fix board positioning/spacing, make directory grids smaller and responsive, add four grid appearances, and restrict production Blazor/Gallery paint to the user's standard color roles. The user explicitly set the default Square maximum side to 220px and authorized an independent dark palette with values chosen by the implementation. This is an approved Flourish-specific exception to the portable color-generation and semantic-palette rules; common documents and other platforms remain unchanged.

## Changes

- Foundation centrally owns ten colors per mode. Added Display board, Light target preview, Dark target preview and Border. Focus uses Accent; the earlier boundary and strong-divider paints merge into Border. Hover/press uses the appropriate target preview; persistent selected items use Primary with Surface foreground. [Exact light/dark values and replacement record](../blazor-color-roles.md) cover all roles and legacy mappings.
- Filled/Danger use Primary, Outlined uses Canvas/Border, and Elevated uses Surface/shared shadow. Table header rows use Border, while sorting feedback stays inside its text button. Split navigation selection and foreground remain synchronized. Top-bar popup theme copying restores the surface preview rather than inheriting its trigger's dark-chrome preview, including native-browser fallback portals and rerenders.
- Audited the complete production Design CSS and Gallery paint resources. Removed generated mixes/shades, extra semantic red/orange/blue/purple paints and repeated literal fallbacks. Primitive/pattern/native-override audit covered 36 assets and modified 29; all palette literals now belong to the foundation. Native Framework system colors remain supported. Shadows/veils use Primary alpha; progress fill no longer derives gradient colors. Severity APIs, labels, ARIA announcements and actions are preserved.
- Removed ThemeScope and its independent guide. ApplicationLayout/Design manage the application theme. Gallery now registers 77 executable component guides. Four mode-specific Primary/Accent seed variables prevent inline startup light colors from overriding default dark-mode colors. Custom seeds remain exact rather than generating unapproved foreground/preview shades. AppearancePalette.Contrast remains available as a diagnostic utility.
- UniformGrid now separates Shape (Rectangle/Square) from Variant (Elevated default, Filled, Outlined, Danger; Outline alias). MaxCellSize defaults to 220px. Cell Item/Button inherit grid appearance unless overridden. Automatic tracks wrap by rows, keep auto width and use 16px gaps/24px block margins. Directory labels omit long descriptions while native hints and accessible names retain full identity/purpose. Existing Filled shorthand and row/column modes remain supported.
- Code boards no longer reserve an empty 80px top row. Code starts at the upper-left padding; a right-side reserve protects the absolute icon copy action. Immediately adjacent boards have 24px spacing. The [layout regression report](../bugfix-reports/2026-10-04_board-spacing-and-grid-overflow.md) records cause and limits.
- Updated Gallery API descriptions, namespace-qualified invocation snippets, three grid scenarios, display scenario and approved SVG demo paints. Updated the active extraction contract/manual checklist and refreshed the explanatory directory tree: 889 files, 132 directories and the root, 1022 nodes; no missing, extra or duplicate paths.

## Verification

- Final Gallery build, library-check executable build and Framework-only native host build: zero warnings and errors.
- Library executable: 94/94 checks passed, including independent grid shape/appearance/cell-cap configuration, inheritance/overrides, invalid dimensions, native callbacks and appearance seeds.
- Data/clipboard Node checks: 12/12; menu/modal mocked DOM: 24/24; section navigator mocked DOM: 15/15. The added menu test verifies the surface preview under dark chrome and restoration on close.
- Palette audit: 6/6 passed. It checks ten-role Light/Dark/System values, role sources/alias references, lack of private paint literals/mixes/brightness and independent subtree themes, approved static SVG paint, and 16 default foreground/background contrast pairs. The smallest checked reading pair is dark Text/Border at 4.72:1; custom seeds are not automatically certified.
- CSS parser/bundler: 21/21 passed. Evidence: C:/Users/Evigila/AppData/Local/Temp/css-bundle-dd6632b3606e44f49cff791bcf957fa9.
- All 77 displayed scenario sources compiled in an independent Razor project after removing only Gallery registration/namespace directives: zero warnings and errors. Evidence: C:/Users/Evigila/AppData/Local/Temp/flourish-copy-controls-447af5cf282647b09cd806d7bdf1e6a9.
- Temporary HTTP host verified 77 guide routes, eight directories, ten documentation routes, selected split navigation, shared menus, six Button variants, return actions, delivered JavaScript, 50px title tokens, paging controls, board/grid rules and central color values. The temporary service was stopped by its own PID. The user's VS service was not stopped.
- Git diff whitespace checks passed. No dependency was added, no package was published and no commit was created.

## Acceptance boundary

No Computer Use was performed, as required by AGENTS.md. SSR/HTTP, source and mocked DOM verification do not certify browser pixels, real pointer timing or scrollbar geometry. Rebuild/restart the VS Gallery and follow [the manual acceptance checklist](../blazor-manual-tests.md): code first-line/copy/stack positioning, directory wrapping and 220px cap at wide/narrow widths, all appearances, application Light/Dark/System, selected foreground and hover/press/focus, popup surface preview and document scrolling. WPF/WinUI, existing user-deleted skills and Colligere business resources were not changed in this task.
