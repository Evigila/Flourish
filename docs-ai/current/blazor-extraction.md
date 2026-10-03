# Blazor extraction and adoption

Status: initial standalone library and Workspace-shaped Gallery implementation. Acceptance results are recorded in the accompanying change record. This is an extraction and generalization of Colligere presentation patterns, not an assertion that all existing Colligere UI has already moved. The user abandoned the previous Figma authority and approved gradual extraction directly into this repository on 2026-10-03.

## Ownership and reference graph

Gallery.Blazor -> Flourish.Blazor -> Flourish.Core. The Web library uses a Razor SDK and the installed Microsoft.AspNetCore.App shared framework; it adds no direct external PackageReference. Core's existing package references remain unchanged. WPF and WinUI projects are preserved as references, without modifying their implementation or host. The Web library does not reference either desktop assembly or any Colligere project.

Public API: Blazor.Abstract builder interfaces, immutable navigation/appearance contracts, IAppearanceService, semantic Razor components and typed table/menu metadata. Internal: builders, validated immutable options, scoped appearance implementation, field association and data processing helpers. Consumers register AddFlourish, select title/navigation/layout options and mount ApplicationShell/ApplicationLayout. They do not recreate shell grid/borders/chrome. Native ASP.NET remains the host; there is no competing Build/Run lifecycle.

IAppearanceService is scoped, not singleton. The navigation configuration is immutable and singleton because it contains host configuration rather than current-user state. Authorization-aware navigation can be supplied through ApplicationShell.NavigationGroups and ApplicationTitle, and must be composed or filtered by the host; configuration does not grant any access. JS DOM state uses element-owned maps and explicit cleanup. A single active menu is an intentional document interaction rule, not shared business state. UI settings do not write browser storage or shared files.

## Source correspondence

Paths below are relative to Colligere's src/Arkheide.Colligere.Web unless stated otherwise. The extraction preserves the documented visual/behavioral contract while replacing product names, service injection and DTOs with parameters and callbacks. Because browser and desktop APIs differ, it is not a verbatim copy of WPF or an automatic dump of every Razor page.

| Source | New library owner | Generalization |
|---|---|---|
| wwwroot/app.css + docs-ai/common/uiuxdesign.md | wwwroot/flourish.css, AppearancePalette, ThemeScope | Scoped --f-* tokens, 17px typography, approved geometry, arbitrary color derivation; remove product-dependent selectors |
| Components/Layout/MainLayout.razor/.css/.js | ApplicationShell, ApplicationLayout, public shell builders | Title bar, two rails, one vertical scroll track, responsive secondary navigation, named title-end slot; remove account/Workspace access services |
| Components/Shared/PrimaryNavigationItem.* | Shell navigation and shell.js | Accessible module labels, focus/hover tips, Escape, per-shell tooltip ownership |
| PageHeading, RecordPageHeading, WorkspacePageContent | PageHeading, PageBody, Section | Real title, parent link and actions; 1180 centered or business-fluid track; complete wrapped title, 96/24 compaction hysteresis |
| FormFields, FormActionBar, UniformGrid | FormLayout, Field, FormActions, UniformGrid | Native field skin, regular labels and approved large action cells; consumer-owned models/validation/submit |
| FilledIdentityCard | IdentityCard | Filled chrome surface and content slots, no account DTO |
| ToggleSwitch, ToggleSection, DisclosureSection | ToggleSwitch, ToggleSection, Disclosure | Distinguish persisted value changes from opening content |
| StatusNotice, PageLoading, provisioning indicator presentation | Notice, LoadingState, ProgressBar | Reuse Core severity values; no provisioning network requests |
| Native input skin + form-input-behaviors.js | TextBox, SelectBox, CheckBox and controls.js | InputBase binding/EditContext, Field error association, first-focus selection and first-invalid focus |
| BottomSheet.* | Dialog and BottomSheet | Native modal, focus containment/return, busy close guard, consumer CanClose veto; no automatic draft discard |
| RowActionMenu.* | ActionMenu/MenuAction | Callback actions, top-layer/fallback positioning, keyboard and outside close, cleanup |
| WorkspaceDataSearch/Filter/Sorter/Pager/Table + data-table-columns.js | DataTable/TableColumn/RowAction/TableText + TableData/data.js | Column-based local search, locale/typed/stable/null-last sorting, 20-page state, list/card fields, column visibility/resize; remove ApplicationPreferencesState, Workspace and business handlers |
| Original inline SVG glyphs | Icon | Functional currentColor SVGs, semantic names, no downloaded icon font |

Additional Card and CodeBlock provide Gallery-facing composition without repeating surface styles in the host. WPF contributes the API design pattern only; its 11/13px typography, window APIs and Gallery page organization are not adopted by the Web library.

## Implementation boundaries

The first extracted set targets normal pages, forms and complete local lists. It does not include a production remote query service, authenticated user shell, server/cursor pagination, virtualization, column drag reordering, spreadsheet bulk-edit draft pooling, masked input, file upload, custom searchable/multiple selects, or business-specific forms. Gallery demonstration data is explicitly labeled, scoped to a circuit, and resets when a new circuit starts; prerender and interactive execution create separate scopes. Storage behavior is deliberately not promised.

Default font is the existing Segoe UI/system stack. Noto Sans/CJK was approved for the abandoned Figma artifact; there is no Web font download or migration. ConfigureAppearance.SetFont can accept a host-supplied font stack without loading any asset. Foundation dark/system tokens cover the new library surfaces, but the Colligere application has not been converted to dark mode.

## Colligere adoption sequence

1. Reference the library and load its single stylesheet in a small consumer boundary. Keep existing framework hosting, authentication and Workspace state.
2. Substitute leaf inputs/notices/toggles/menus/dialogs, mapping models and callbacks without changing save/access semantics. Check dirty/busy, focus and validation before deleting each old implementation.
3. Replace form tracks, title/record layouts and identity surfaces. Preserve the approved action-grid and wrapped identity behavior.
4. Adapt local list pages to TableColumn/RowAction. Do not treat a remotely loaded page as all records. Preserve authorized first-open semantics and current query limits in the host.
5. Replace shell/navigation through the public builders or consumer layout slots after defining how per-user authorized navigation is supplied. Keep organization branding and route/access selection at the host boundary.
6. Migrate custom search/select/masks and advanced editing as separate complete components with their current behavioral checks, then remove redundant CSS/JS only after consumers no longer use it.

No Colligere runtime ProjectReference or wholesale UI replacement has been performed in this initial delivery. Its prior unrelated uncommitted product work is preserved. Its existing prose contract remains effective until the library is adopted and accepted; do not change that authority merely because a new package exists.

## Rendered Workspace alignment, 2026-10-03

The initial delivery is checkpointed as `1fcc922`. The follow-up used installed Edge and the existing bundled Playwright runtime, without installing packages or using Computer Use. The user logged into an isolated review window; read-only samples covered the real Workspace dashboard, product/customer lists, product/customer creation forms, Workspace information and customization, plus Account directory navigation. No business data was edited.

The library now uses primary-derived canvas, alternate/raised surfaces, foreground and borders; full-stage sticky headings; 84/52/44px section rhythm; the source primary selected treatment; secondary labels without an extra group heading; and source search/table/view/pager geometry. CSS reset specificity no longer overrides control type. Programmatic H1 focus stays intact with no input-like outline. Native inputs and keyboard controls retain visible focus. Base-select alignment, identity primary hierarchy, dark Subtle/progress roles, portal token propagation and full-width bottom-sheet dividers are corrected.

New public presentation options are `TitleBarBrand`, `TitleBarStart`, `INavigationGroupBuilder.SetSecondaryNavigation`, `PageHeading.Compact`, `Button.IconOnly`, `ActionMenu.TriggerContent` and `FactList`. Gallery exercises brand/service tracks, primary-only overview, definitions and identity facts. Its dirty-form discard now follows the saved navigation target instead of always falling back to the record list. Authentication, remote query boundaries and Colligere runtime adoption remain consumer work.

Validation: Debug Gallery/library build succeeded with no warnings or errors; 35 C# checks, 15 mocked DOM checks and 6 data DOM checks passed. Installed Edge passed 79 rendered checks across six Gallery routes and 1440/980/760/520/320px, including heading/control focus, navigation protection, menu top layer, dark Subtle, select alignment, identity type, view geometry, scrolling hysteresis and document/stage overflow. These browser results do not establish OS DPI/zoom, assistive technology or other browser compatibility; those remain in the manual checklist.

The final visible-browser review additionally found parent and child secondary routes selected together. The shell now selects only the most specific enabled match; an unlisted record falls back to its parent. This is covered by a focused C# case and the rendered detail-page check. Odd final facts span the full definition track.
