# Blazor extraction and current adoption

Status as of 2026-10-03: the former single Blazor library has been split into Framework, Design, Shared and Abstract. Colligere now consumes the extracted components through local project references. This is gradual presentation extraction: business pages, API calls, authorization, session state and draft persistence remain in Colligere. The earlier Figma proposal was abandoned in favor of reusable code and an executable Gallery.

This active document describes the current source. Per-run build, package and browser evidence belongs in append-only `currentproject-changelogs/` records; acceptance beyond those observations is tracked in [blazor-manual-tests.md](blazor-manual-tests.md).

## Projects, packages and dependency direction

All four projects are under `src/Flourish.Blazor/` in full-name subdirectories, target .NET 10 and are configured as separately packable NuGet packages at version prefix 1.1.0. A configured package is not evidence of public-feed publication.

| Project / package | Responsibilities | Direct project dependencies |
| --- | --- | --- |
| `Flourish.Blazor.Shared` / `Arkheide.Flourish.Blazor.Shared` | Immutable navigation/appearance metadata; control, table, selection and grid data contracts; local filter/sort/pagination and mask helpers | `Flourish.Core` |
| `Flourish.Blazor.Abstract` / `Arkheide.Flourish.Blazor.Abstract` | Shell/navigation/layout builders, optional appearance interfaces, `ITablePreferences` and `TableSortPreference` | Shared |
| `Flourish.Blazor.Framework` / `Arkheide.Flourish.Blazor.Framework` | Razor components, Patterns and Primitives; runtime registration; browser interaction JS and functional CSS | Abstract, Shared |
| `Flourish.Blazor.Design` / `Arkheide.Flourish.Blazor.Design` | Optional foundation and skin CSS, `AppearancePalette`, `ThemePalette`, scoped appearance runtime and `ThemeScope` | Framework, Abstract, Shared |

Framework does not reference Design. None of the four Blazor projects references WPF, WinUI or Colligere. Shared preserves existing Core `ApplicationTheme` and `NotificationSeverity` identities; it does not register Core hosting or desktop services. Core's existing `Microsoft.Extensions.Configuration.Json`, `Microsoft.Extensions.Hosting.Abstractions` and `Microsoft.Extensions.Logging.Abstractions` references remain transitive dependencies. The split adds no direct external PackageReference to the four Blazor projects. Framework and Design use the Razor SDK and `Microsoft.AspNetCore.App` shared framework.

Public namespaces remain `ArkheideSystem.Flourish.Blazor`, `.Abstract`, `.Components`, `.Components.Primitives` and `.Components.Patterns`. Assembly names are `Flourish.Blazor.*`; static asset paths use package IDs rather than assembly names.

The root `Flourish.slnx` contains platform projects and their Galleries. `src/Flourish.Blazor/Flourish.Blazor.slnx` lists the four Blazor projects, Core, Gallery and the console/native fixtures directly. Sub-solutions appear as files in the root's Solutions folder; they are not nested project entries.

## Native Framework and optional Design

A native consumer references Framework and registers `builder.Services.AddFlourishFramework(...)`. `ApplicationLayout` adds the required Framework stylesheet through `HeadContent`; a low-level custom layout that renders `ApplicationShell` directly must load:

```html
<link rel="stylesheet" href="_content/Arkheide.Flourish.Blazor.Framework/framework.css" />
```

`AddFlourish` freezes singleton startup shell options and registers scoped default `ITablePreferences` with `TryAddScoped`. It does not register or resolve `IAppearanceService`. Hosts retain normal ASP.NET `Build`/`Run`, routing and authentication. A consumer can replace table preference storage through the public interface.

Functional CSS is required for hidden/accessibility content, scroll ownership, grids, popup placement, resize hit areas and responsive navigation. `framework.css` includes extracted functional layout CSS and `primitives/native.css`. The native fallback preserves visible mask input text, system-colored popup surfaces, painted relative-size SVGs, disclosure/switch state and named container queries. Native layouts retain structural constraints; they do not promise the fixed density of the Design skin. Removing both stylesheets is not a supported equivalent of native mode.

Design is a separate explicit opt-in:

1. Reference Design in addition to Framework.
2. Call `AddFlourishDesign(appearance => ...)` separately. `IAppearanceBuilder.SetColors`, `SetTheme` and `SetFont` configure startup appearance; the old `IApplicationBuilder.ConfigureAppearance` API is no longer present.
3. Load `_content/Arkheide.Flourish.Blazor.Design/design.css` after `framework.css`.
4. Use `ApplicationLayout` to apply the registered Design theme automatically. Use `<ThemeScope>...</ThemeScope>` only for an intentionally independent nested subtree.

A no-argument ThemeScope reads the registered scoped appearance service, subscribes to changes and emits `--f-*` tokens plus light/dark/system classes. Optional `Primary`, `Accent` and `Theme` parameters override that scope. Design's `theme-aliases.css` maps the public tokens to the extracted Primitives and NavigationSurface/ContentSurface role variables, including color-scheme. Host inline role variables retain CSS precedence. `native-overrides.css` restores decorative mask overlays and switch appearance only when Design is loaded.

## Program-configured application shell

`AddFlourishFramework` now exposes `ConfigureTopBar`, `ConfigureNavigation`, `ConfigureLayout` and `SetCommandParser<TParser>`. Top-bar configuration owns the application name, local logo path, command menus and optional left/center/right component types. Navigation explicitly distinguishes route entries, command buttons, secondary route entries and bottom-fixed route/command entries. A primary route always has its own target; the framework no longer infers it from the first secondary item.

Configured component regions store only component types. `DynamicComponent` creates each instance in the current Blazor scope, so singleton startup options never capture a component or scoped service. The configured Core `ICommandParser` is also created per Blazor scope and registers handlers into a scope-owned dispatcher. Two server circuits therefore do not share mutable command handlers or their dependencies.

`ApplicationLayout` owns the default `ApplicationShell`, emits the Framework stylesheet, and consumes the optional `IThemeProvider` registered by Design. This preserves the dependency direction: Framework never references Design. The host retains its standard Router and selects `ApplicationLayout` as the default; custom layouts and the low-level runtime override parameters remain available for advanced applications.

Appearance state is scoped to a user circuit/client host. Startup navigation/layout options are immutable configuration, not current-user authorization. No new font or icon download is installed; `SetFont` selects a host-provided CSS stack.

## Public component layers

| Layer | Current API and role | Consumer boundary |
| --- | --- | --- |
| `.Components` | `ApplicationShell`/`ApplicationLayout`, `PageHeading`/`PageBody`/`Section`, forms and InputBase controls, notices/progress, facts/identity, local `DataTable`, `ActionMenu`, `Dialog`/`BottomSheet` | Models, validation, event callbacks and navigation data remain host-supplied |
| `.Components.Primitives` | Extracted presentation components including `ShellHeader`, `ServiceMenu`, `PrimaryNavigationItem`, headings/page contents, forms/action grids, identity/facts, mask/select controls, `DataTable`, `RowActionMenu`, `NavigationGuard`, `InteractionBoundary` and `EditingGrid` | No Colligere DTO, API client, authoritative session tracker or permission service is required |
| `.Components.Patterns` | `NavigationSurface`, `ContentSurface`, `RecordListPage` | Compose public render slots while the host supplies routes, approved navigation, status and page content |

Some names exist in both Components and Primitives, including DataTable, PageHeading, BottomSheet, ToggleSwitch and UniformGrid. Consumers importing both namespaces should qualify the desired component. Their contracts are not interchangeable: for example the Components Dialog/BottomSheet supports `CanClose`, while the extracted Primitives BottomSheet exposes `IsBusy` and `OnClose`.

`ApplicationShell` owns its title/navigation/content tracks and accepts runtime `NavigationGroups`, `ApplicationTitle`, `TitleBarBrand` and `TitleBarStart`. Hosts must filter runtime navigation according to their authorization; the library does not grant access.

`NavigationSurface` owns the extracted primary/secondary rails and one application-stage scroll track. Its TitleBar, PrimaryNavigation, SecondaryNavigation and ChildContent slots receive host content; ShowSecondary, DocumentKey, ContentId, ContentClass and labels control presentation. `ContentSurface` provides a content track without navigation rails. Both require only browser JS, expose CSS class/style hooks and do not inject appearance services. `RecordListPage` composes heading/actions/status and a titled content section.

The Components `DataTable<T>` uses `TableColumn<T>` value/format delegates and `RowAction<T>` callbacks. It processes supplied local items with all/single-column search, locale/typed stable sorting, null-last ordering, 20-item default pagination, column visibility, list/cards and pointer/keyboard width control. Loading/error/empty states do not invent records. Row opening is explicit through `OnRowOpen`.

The Primitives `DataTable<T>` keeps the source table's column metadata, visibility/order controls, local filter/sort/pagination helpers, row/card templates and optional bulk-edit composition. `ITablePreferences` replaces host-specific preference injection; the default implementation is an in-memory scoped dictionary. These local processors do not turn a remote response page into a complete dataset, supply remote search, or implement server/cursor pagination.

## EditingGrid contract

`src/Flourish.Blazor/Flourish.Blazor.Shared/Primitives/GridContracts.cs` defines `GridColumn`, `GridRow`, `GridCell`, `GridCellChange`, `GridOption`, `GridText` and `GridEditorKind`. Framework owns `EditingGrid`, `GridInteractions` and `primitives/editing-grid.js`.

- Supply ordered columns and rows with one cell per column; column keys must be unique. The renderer rejects duplicate column keys and short/oversized cell arrays before rendering. Use stable row keys for retained DOM identity.
- Grid editor kinds are Text, Decimal, Date, Multiline, Select, Masked and Link. Raw Value and formatted Display are separate host-provided values; options, mask, maximum length, required state, hint and accessible labels are metadata.
- `CellChanged` supplies row/column indexes and a proposed value. `EditDisabled`, per-cell Disabled/ReadOnly and Hidden guard direct cell changes. Read-only native fields and grid cells expose their state; select editors are disabled where read-only. Link cells remain navigable.
- Primary and secondary host errors expose invalid state and resolvable description IDs on the cell/editor. Host text is HTML-encoded.
- `GridInteractions` provides Begin/End/Cancel, Undo/Redo/Save, LoadMore, streamed ApplyCells/Paste and ReportError callbacks. The browser bridge supplies selection, keyboard and clipboard interaction; domain parsing, validation, permission enforcement, transaction history and persistence remain host operations.
- Hidden disconnects interactions while retaining the component's layout state. MeasurementRevision signals changed display data. Width measurement/resizing remains browser behavior, not a business save.

There is no generic remote grid data service or automatic business draft pool. The Colligere editors map their existing drafts and operations to this presentation contract.

## Current Colligere consumption

Verified source references exist in `Arkheide.Colligere.Web.csproj` for Framework and Design, and in `Arkheide.Colligere.Presentation.csproj` for Framework. Web registers AddFlourish and AddFlourishDesign, loads Framework then Design styles in App.razor, and imports extracted Primitives/Patterns. This development linkage expects a sibling Flourish checkout; it is not a published-package deployment.

| Colligere source boundary | Current library consumption | Logic retained in Colligere |
| --- | --- | --- |
| MainLayout / UserLayout | NavigationSurface / ContentSurface; public render slots | Workspace/account identity, authorized menus, route links, theme profile DTO adaptation |
| Shared and Presentation components | Extracted Primitives; small adapters for reference dropdown, shell/service menu, navigation and interaction lock | Product taxonomy DTO conversion, authoritative session tracker, approved portal navigation |
| Ordinary list/form/detail pages | Shared headings, action grids, notices, identity and DataTable | Query limits, API reads/writes, permissions, route selection and models |
| ProductSpreadsheet / RegistrySpreadsheet | EditingGrid, GridColumn/GridRow/GridCell and GridInteractions | Drafts, edit history, validation, save/retry, parsing, reference checks and cursor loading |
| ApplicationPreferencesState | ITablePreferences implementation registered by Web | Existing host preference state |
| App/global/layout/page assets | Only generic layout, controls and interaction modules in Framework/Design | Business app.css mappings, real isolated page/layout styles and account/product/pricing/directory scripts |

Consumer-specific page selectors and isolation metadata stay entirely in their host. The user explicitly rejected library compatibility/page skins on 2026-10-03; the earlier ownership of these assets is superseded. There is no consumer-specific entrypoint or opt-in business stylesheet in the library. There is no claim that all business pages were replaced or that session/API behavior moved into the library. WPF/WinUI implementations are unchanged references; WPF informed builder API style only.

## Generic asset and loading boundary

The four library projects contain only reusable contracts, components and assets. NavigationSurface and ContentSurface use neutral class roots; FormSurface provides content/action/contents slots and an optional CssClass hook. Primitives.DataTable uses DataColumn, DataFilter, DataSorter, DataPagination, TablePurpose and CardField.Title/Identifier/Metric. Business field compositions, DTO conversion, page routes and modifiers stay in the host.

Framework and Design CSS sources remain modular in Git. build/CssBundle.targets and BundleCss.cs flatten local imports into one public CSS asset per package without an added package or Node dependency. The SDK static asset pipeline publishes only those bundles; JS remains demand-loaded. Gallery uses fingerprint-aware Assets links, host-level ASP.NET Warning filtering and short-circuited public static endpoints. Closed primitive sheets defer child mounting and JS import until first use, then retain mounted children to preserve drafts. Column measurement is invalidated by real DOM/layout changes and cleaned up on disposal.

## Verification and remaining limits

The earlier split completion had 47 passing checks after the EditingGrid and dropdown additions: local data algorithms, DI isolation/configuration, contrast, encoding, field/dialog/shell semantics, grid contracts and Framework-only Patterns/Primitives composition. The Framework-only Web fixture at `tests/Flourish.Blazor.Native` references Framework alone, registers no Design service and loads only functional CSS. Gallery references both packages and demonstrates the Design path, including `/patterns` generic layout composition.

SSR verifies composition and semantics, not post-render JS, pointer behavior or pixels. The final split builds, 47 library checks, 1595 passing Web checks, live native/Gallery/Workspace observations and independent offline NuGet consumers are recorded in the [completion record](currentproject-changelogs/2026-10-03_170436_split-blazor-ui-and-integrate-consumer.md). That record explicitly preserves the final logged-out dropdown browser recheck boundary. Earlier pre-split findings remain contextual evidence rather than certification of the split.

Known limits: no virtualization or generic remote querying; host-owned business transactions; some extracted Primitives still have Portuguese default labels; browser-feature support differs between the enhanced Components overlays and extracted native Popover/Dialog primitives. Cross-browser/assistive-technology acceptance, pure hover behavior and intermediate animation frames remain in the manual checklist. The code library/Gallery does not by itself revoke an existing consumer's approved prose visual contract.

The subsequent universal-asset/loading repair passed 49 library checks, 21 CSS parser checks, 62 isolated SDK/HTTP checks and the complete Web suite (1595 passed, 85 environment-gated skips). Actual packages retain gzip/Brotli for stable and fingerprint CSS; the optional styled pair transfers 28849 bytes with Brotli. These results supersede the earlier resource ownership proposal and 47-check totals, while earlier browser observations remain historical. See [current repair and evidence](bugfix-reports/2026-10-03_generic-blazor-assets-and-page-load.md); browser elapsed-time acceptance still requires a rebuilt VS host and the manual checklist.

The later dynamic-import regression exposed a Development Content-Encoding mismatch. Current builds retain SDK gzip only; packages preserve the CSS fingerprint expression and gzip relationship, while Publish generates Brotli. The expanded Development/Production SDK suite passes 194 checks and actual Gallery modules pass 56 decoded-response checks. The earlier 62-check acceptance covered Production only. See [current compression correction](bugfix-reports/2026-10-03_development-module-compression.md).
