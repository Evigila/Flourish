# Split Blazor UI and integrate the source consumer

## Authorized scope

The user requested a Framework/Design/Shared/Abstract split, optional Design packages, executable Workspace-style Gallery and actual Computer Use interaction review against Colligere. The existing WPF/WinUI implementations were to be grouped, not split internally. The prior approved progress checkpoint is c3089dc, `Align Blazor UI with rendered Workspace`. This record describes subsequent uncommitted work.

## Resulting structure and public boundary

- Root Flourish.slnx groups Core and platform projects, with separate WPF, WINUI3 and Blazor sub-solutions. The Blazor sub-solution directly lists all four libraries, Core, Gallery, console checks and the native host. Desktop implementation source is unchanged.
- Canonical projects are src/Flourish.Blazor/Flourish.Blazor.{Shared,Abstract,Framework,Design}. The former monolith is removed; there is no fifth aggregator.
- Shared retains UI metadata/algorithms and existing Core enum identities. Abstract supplies configuration/builders and ITablePreferences. Framework owns Razor controls, layout Patterns, functional CSS and browser behavior. Optional Design supplies tokens, appearance service, ThemeScope and skin CSS.
- Framework references Abstract/Shared and never Design. AddFlourish and AddFlourishDesign are separate opt-ins; UseTitleBar/UseNavigation/ConfigureLayout expose composition without requiring host-built border tracks. SetColors/SetTheme/SetFont belong to the appearance builder.
- Existing external Core dependencies are retained. No new package, font or icon download was added. Namespace identities remain compatible; static roots use Arkheide.Flourish.Blazor.Framework and Arkheide.Flourish.Blazor.Design package IDs.

## Extracted presentation and integration

NavigationSurface/ContentSurface own header, primary/secondary rails, stage scroll lifetime, keyed route replacement, compact headings and top return. RecordListPage owns heading/action/status/section composition. Primitives cover shell/navigation, headings, form/action tracks, masks, lookup/selection, notices, identity, sheets, row menus, search, paging and WorkspaceDataTable.

EditingGrid renders host-supplied GridColumn/GridRow/GridCell metadata with text/decimal/date/multiline/select/masked/link editors, lock/error states and keyboard/pointer/width/measurement lifetime. GridInteractions bridges callbacks without importing a product API or persistence service. Original product and registry parsing, history, save/version/permission/dirty decisions remain in Colligere.

Colligere Web consumes Framework+Design and Presentation consumes Framework through sibling ProjectReferences. Pure presentation owners and generic browser resources moved to the library; business compositions and CSS-isolation anchors remain in the host. Authentication, sessions, access filtering, DTO adaptation, queries and transactions remain host-owned. The static-resource exception allows only the two explicit library roots.

Gallery's /workspace uses NavigationSurface, source-derived skin, RecordListPage and WorkspaceDataTable over three demonstration records. It provides list/cards, search/paging, inspect/create sheets and service navigation. Existing API/appearance demonstrations were migrated to the separate registration model. The native host references Framework only and exposes browser-native validation, masks, switches, notices, disclosure, dialogs and table behavior.

## Render and interaction corrections

Source comparison corrected native fallback visibility/icons/container behavior, Design import order, low-specificity link inheritance, role-token propagation and source geometry. Programmatic H1 focus has no field rectangle while actionable keyboard focus remains visible.

A row menu could disappear before an async sheet captured its invoker. A private expiring connected-origin capture now restores the stable row trigger; ordinary connected invokers take precedence. Legacy ReferenceDropdown boolean ARIA values became explicit strings for expansion and option selection. This changes markup/state presentation and does not change selection or creation callbacks.

## Verification

| Check | Result |
| --- | --- |
| Blazor child solution Debug and Release builds | 0 warnings, 0 errors |
| Final Colligere AppHost build | 0 warnings, 0 errors |
| Library console/HtmlRenderer suite | 47 passed |
| Browser-module checks | controls DOM 15, interaction origin 5 and table checks 6 passed; source JS suites passed |
| Complete Colligere Web suite | 1595 passed, 0 failed, 85 environment-gated database integration tests skipped; total 1680 |
| Framework-only independent NuGet consumer | Offline restore/build passed; no Design graph/DLL/static assets |
| Framework+Design independent NuGet consumer | Offline restore/build passed; both asset roots and foundation.css present |
| Package provenance | Nine restored library archives match final-feed SHA-256; no repository references/assets required |

Four version 1.1.0 Blazor packages were produced locally, plus the existing Core dependency for offline consumption checks. Two isolated SDK Web/Razor consumers use PackageReference only, with source mapping to the local final feed and existing cached packages. Both compile with zero warnings/errors. No remote package fetch or public-feed publication occurred. Independent package compilation/manifests are not a live HTTP browser claim; live source-based native/Gallery hosts were reviewed separately.

Actual browser review covered authenticated Workspace navigation, searches, row menu keyboard/outside close, lookup opening/search focus/Escape, editable grid arrow movement with Save disabled, resize drag and keyboard width changes, press-move-release cancellation, and sheet focus return. No source save/delete/publication was executed. Gallery additionally covered list/cards, search/pager synchronization, form-sheet Tab/Escape, Light/Dark roles and 375px/760px responsive geometry. Native mode verified required validation, valid sample submission, visible masked text, switch/disclosure/notice and dialog behavior without Design. Temporary viewport overrides were reset.

The final dropdown ARIA change has four real renderer callback checks. Its final authenticated browser recheck remained pending because the final Workspace document still showed its login page after the user's login reply and one page refresh. An exact-tab login handoff was requested; services were not restarted again. Earlier live lookup/focus observations precede this three-attribute-only patch. Do not treat that pending check as completed.

Pure hover and held-pointer intermediate frames are unavailable in the current browser interface. Cross-browser, screen-reader, clipboard/business persistence and those visual states remain manual acceptance in [blazor-manual-tests.md](../blazor-manual-tests.md).

## Evidence and documentation

Evidence is outside product repositories in C:/Users/Evigila/.codex/visualizations/2026/10/03/01a0ff44-995d-7090-af88-8835158b1571/:

- test-results/ui-extraction-complete.trx and web-test-complete.log.
- packages-final/ and package-consumers/REPORT.md, package-verification.json, restore/build logs.
- gallery-workspace-heading-final.jpg, gallery-workspace-final.jpg, gallery-mobile-final.jpg, gallery-dark-final.jpg, native-framework-final.jpg and workspace-form-final.jpg.
- ui-extraction-backup/manifest.json and style/resource backups preserve original extraction evidence.

Active docs: [blazor-extraction.md](../blazor-extraction.md), [blazor-render-review.md](../blazor-render-review.md), [manual acceptance](../blazor-manual-tests.md) and [focus/ARIA bug report](../bugfix-reports/2026-10-03_blazor-sheet-origin-and-dropdown-aria.md).

## Remaining integration constraints

Current Colligere development builds need the sibling Flourish checkout until its ProjectReferences are changed to an available package feed. This task prepares and verifies packages without publishing them. Extracted route skins retain host scope metadata; generic controls/patterns require no Colligere dependency. Some retained default labels are Portuguese. No virtualization, generic remote query service or universal localization is claimed.

Existing unrelated README/skill changes in Flourish and business work in Colligere were preserved. No new Git commit was created after c3089dc; committing the refactor requires the user's answer.
