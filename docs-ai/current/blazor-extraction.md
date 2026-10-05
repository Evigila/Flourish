# Blazor implementation and consumer boundaries

This is the current source guide for the four Blazor libraries, their Gallery and Framework-only verification host. It consolidates the former extraction, rendering-review and project-boundary documents. Historical implementation and run evidence remains in append-only [change records](currentproject-changelogs/) and [bug reports](bugfix-reports/). Use the [manual checklist](blazor-manual-tests.md) for acceptance, the [color contract](blazor-color-roles.md) for paint values and the [directory map](currentproject-architecture.md) for file locations.

## Projects, packages and solutions

All four Blazor libraries live under src/Flourish.Blazor/, target .NET 10 and are separately packable at version prefix 1.1.0. Configuration does not establish public-feed publication.

| Project / package | Current responsibility | Direct project references |
|---|---|---|
| Flourish.Blazor.Shared / Arkheide.Flourish.Blazor.Shared | Navigation/appearance records, control/table/selection/grid metadata, local filter/sort/pagination and mask helpers | Flourish.Core |
| Flourish.Blazor.Abstract / Arkheide.Flourish.Blazor.Abstract | Shell/navigation/layout/appearance builders and service interfaces, ITablePreferences and TableSortPreference | Shared |
| Flourish.Blazor.Framework / Arkheide.Flourish.Blazor.Framework | Razor Components, Primitives and Patterns; registration; functional CSS and browser interactions | Abstract, Shared |
| Flourish.Blazor.Design / Arkheide.Flourish.Blazor.Design | Optional visual foundation/skins, appearance palettes and scoped theme runtime | Framework, Abstract, Shared |

Framework does not reference Design. The Blazor projects have no WPF, WinUI or Colligere project reference and add no direct external PackageReference. Shared retains Core ApplicationTheme and NotificationSeverity identities without registering desktop hosting. Core's existing configuration, hosting-abstraction and logging-abstraction packages remain transitive dependencies.

The executable projects are Gallery.Flourish.Blazor, Gallery.Flourish.WPF and Gallery.Flourish.WINUI3. Verification projects are Tests.Flourish.Core, Tests.Flourish.WPF, Tests.Flourish.Blazor and Tests.Flourish.Blazor.Native. The root Flourish.slnx contains 14 projects: Core at root, platform groups for libraries/Galleries, Tests for verification and Solutions for the three platform solution files. Each platform solution uses the renamed paths. WPF retains UserSecretsId Gallery.WPF to preserve its existing settings identity.

Gallery.Flourish.Blazor is the Framework plus Design host on localhost:5188. Tests.Flourish.Blazor.Native is a Web verification executable under tests on localhost:5189, references Framework alone and loads framework.css without Design. Retain both: they exercise different supported consumers. Native is not a fifth library layer.

## Public API placement and future boundary work

Namespaces and assemblies are separate concerns. Registration extensions AddFlourishFramework and AddFlourishDesign declare ArkheideSystem.Flourish.Blazor while residing in Framework and Design respectively. AddFlourish remains Framework's obsolete compatibility entry point; new hosts use AddFlourishFramework. There is no current monolithic Flourish.Blazor.csproj.

Builder interfaces live in Abstract and its .Abstract namespace. NavigationItem and AppearanceState have that namespace but compile in Shared; ButtonVariant, SelectOption and TableColumn are Shared public contracts in .Components. Razor components are public Framework types. Shared therefore mixes contracts with implementation helpers; the presence of Abstract does not mean all public contracts already reside there.

WPF presently has one library project: Abstract is a directory/namespace within it. Its registration/builders can use the .Abstract namespace while their implementation remains in that assembly; controls expose their own public APIs.

A future boundary change may move public DTOs/enums/interfaces from Shared to Abstract, keep reusable algorithms in Shared and review exposure against consumer requirements. The resulting direction can be Abstract → Core, Shared → Abstract, Framework → both and optional Design → the framework/contracts it needs. This is a recommendation, not an implemented dependency graph. Merging Shared directly into Framework while Abstract still depends on its types would create a cycle; merging it wholly into Abstract would carry implementation algorithms into the contract package. Registration implementation belongs with the classes it composes. A desired registration namespace can change independently, but that migration has not been implemented.

## Framework consumption and optional Design

A native host references Framework and calls AddFlourishFramework. Registration freezes immutable singleton startup options, creates command/runtime state within the current scope and registers default ITablePreferences through TryAddScoped. It neither registers nor resolves IAppearanceService. Hosts retain ordinary ASP.NET Build/Run, routing, authentication and authorization.

ApplicationLayout emits the required Framework stylesheet through HeadContent. A custom layout rendering ApplicationShell directly must load _content/Arkheide.Flourish.Blazor.Framework/framework.css. Removing both library stylesheets is not a supported native mode: functional CSS owns accessibility/hidden content, scroll ownership, grids, popup placement, resize targets and responsive navigation.

To opt into Design, reference Design, call AddFlourishDesign separately and load _content/Arkheide.Flourish.Blazor.Design/design.css after Framework. IAppearanceBuilder.SetColors, SetTheme and SetFont configure startup appearance; the former IApplicationBuilder.ConfigureAppearance API is removed. ApplicationLayout consumes the optional scoped IThemeProvider without Framework depending on Design, applies the application Light/Dark/System classes and subscribes to theme changes. Hosts can replace preference storage and supply CSS role hooks.

Native CSS keeps real masked-input text visible, system-colored popup surfaces, relative-size icons, disclosure/switch state and named layout containers. Design owns decorative masks, fixed reading sizes, palettes and skin. There is one application theme; ThemeScope and its Gallery guide were removed. Host inline role variables retain CSS precedence.

## Program configuration, navigation and scroll ownership

AddFlourishFramework exposes ConfigureProject, ConfigureTopBar, ConfigureNavigation, ConfigureLayout and `SetCommandParser<TParser>`. `IFrameworkBuilder.ConfigureProject(Action<IProjectBuilder>)` owns project identity independently of its presentation. IProjectBuilder.SetProjectName sets the name, SetLogo supplies the logo path and optional alternative text, and SetFavicon supplies a separate browser tab icon.

| Project setting | Default / reset behavior |
|---|---|
| Project name | Application |
| Logo | Framework-bundled white browse SVG at _content/Arkheide.Flourish.Blazor.Framework/browse.svg; SetLogo() or null restores it |
| Logo alternative text | Empty by default; SetLogo(path, alt) supplies it |
| Favicon | Unset by default, resolving to Logo; SetFavicon(), null or an empty string clears the dedicated icon and restores Logo fallback |

ConfigureTopBar enables the bar and owns menus, search/navigation controls, component slots and the DisplayLogo(bool display=true)/DisplayProjectName(bool display=true) switches. Both display defaults are true. Logo/name paths and identity are configured in ConfigureProject rather than on the top-bar builder. Hiding either or both top-bar elements does not affect favicon resolution; hiding both omits the brand link while retaining other bar controls. Existing ApplicationTitle/TitleBarBrand runtime slots remain available and respect their display switches; they do not replace the configured favicon.

A host can configure a distinct logo and tab icon while leaving both top-bar elements visible:

    builder.Services.AddFlourishFramework(framework => framework
        .ConfigureProject(project => project
            .SetProjectName("My application")
            .SetLogo("app-logo.svg", "")
            .SetFavicon("app-favicon.svg"))
        .ConfigureTopBar(top => top.DisplayLogo().DisplayProjectName()));

Omit SetFavicon to use Logo for the browser tab. ApplicationLayout's existing HeadContent emits the resolved rel=icon link as well as library styles; the host keeps the standard HeadOutlet and does not need a hardcoded favicon link. Low-level ApplicationShell itself emits no HeadContent, so an embedded shell cannot replace its host's tab icon; custom low-level layouts retain head ownership.

Gallery configures project name Gallery, white gallery.svg Logo and separate gallery-favicon.svg in Light Primary #153A32. Its logo's internal 0.8 scale gives 28.8px visible browse geometry in the unchanged 48px brand box. The Framework default browse remains 36px visible within that box. These static SVG assets use the existing authorized Material Symbols source; no icon/API dependency is added.

DynamicComponent instantiates configured left/center/right component types in the current scope. Command parsers/dispatchers are also scope-owned; singleton options do not capture a component, scoped service or another circuit's mutable command handlers.

Navigation distinguishes primary routes, commands, secondary routes and bottom-fixed entries. A primary route has its own explicit target; it is not inferred from the first child. Nested AddSubNav configure callbacks support third-level entries while the original overload with positional Boolean arguments remains compatible. A landing destination may equal its own first child; duplicate siblings and routes shared by different branches/modules remain rejected. The most specific enabled route selects the destination and its ancestry. A disabled ancestor suppresses descendant matching and actions.

An entry with children composes SplitButton: its link navigates and closes narrow navigation; its separate triangle expands/collapses without navigating. Both halves retain persistent selection independently of expansion. Descendant navigation opens the matching branch; ordinary rerenders preserve a manual collapse. Route links alone expose aria-current; disclosure actions expose aria-expanded/controls and host-supplied disclosure label parameters. Expansion is local to a shell.

Navigation labels intentionally stay on one line with ellipsis; full accessible text/title remains available. Third-level siblings share a Surface panel. Rails/menu panels hide their scrollbar while retaining pointer, touch and keyboard scrolling. These approved project choices do not change the common requirement that organization/record identity text wrap. Narrow navigation retains its focus loop, Escape/outside close and focus return.

Configured top-bar menus use ActionMenu.OpenOnHover=true; standalone ActionMenu defaults to click. Hover opens without moving focus and uses a gap-free pointer corridor; keyboard/touch activation, arrow navigation, Escape, outside close and disabled/busy guards remain supported. Shared popup positioning uses the actual trigger, right alignment, a 12px viewport guard, above/below placement, available-height scrolling and scroll/resize repositioning. Click mode retains its 6px gap. Design ActionMenu option text uses ordinary 17px body type at weight 400; the trigger retains its existing emphasis. Disabled/destructive, hover, press, focus and command behavior are unchanged. Extracted ServiceMenu keeps its own entry contract.

ApplicationLayout.OwnsDocument defaults true. The marked full-page shell locks html/body scroll; its content owns vertical page scrolling, with overflow:clip preventing fragment navigation from moving chrome. Low-level ApplicationShell.OwnsDocument defaults false. Embedded Gallery previews explicitly use false and stay within their local viewport. Disposal removes the owned document lock.

Automatic PageHeading compaction starts beyond 96px only when the collapsed document retains at least 24px of scroll range. Otherwise it keeps the expanded heading without an extra spacer, expands below 24px and retries eligibility after content/viewport changes. Explicit Compact remains host-controlled. Shell and Pattern headings share the helper exported from existing shell.js; patterns/surfaces.js imports that module rather than introducing another runtime asset. Expanded return navigation sits above the title; compact mode puts a 48px icon-only Underline return control immediately beside the 38px title, with 12px block padding and a normal 72px minimum height. Wrapping/actions may increase height. Content skip-link landmarks remain focusable without a control-sized outline around the entire canvas; real interactive controls retain visible focus.

SectionNavigator belongs to its ContentId. It discovers the first H2 of owned top-level sections or direct content H2, excluding deeper demonstrations, nested mains and modal titles; explicit Entry values are supported. Its gutter dots do not narrow content, identify the current location and reveal the real heading on hover/focus. Activation scrolls/focuses that content heading, honors reduced motion and permits one bounded sticky-heading alignment correction. User interruption cancels it. Mutation/resize refreshes dynamic content; disposal restores owned heading IDs/tabindex and removes observers/listeners/timers.

## Approved Design choices

These explicit Flourish-specific choices supersede the corresponding portable defaults for Blazor only; common standards remain unchanged.

| Role or mechanism | Current approved behavior |
|---|---|
| Reading sizes | Page 50px, semantic H1 34px, H2 28px, H3 20px, body/labels/ordinary controls 17px |
| Compact heading | 38px exclusively in the compact state; no general utility |
| Art display | Explicit f-type-art and an approved size at least 42px |
| Font | Design defaults to Segoe UI; Gallery configures its approved Noto Sans / Noto Sans CJK SC host stack |
| Icon glyphs | Default 26px, primary navigation 24px, tools 26px, information/search 22px |
| Checkbox geometry | Design uses the standard 48px control-height token, including table display choices and the legacy visible proxy |
| Progress ring | 72px container, 68px SVG, 4.5-unit stroke and radius 23 in a 52-unit viewBox; toolbar hosts must size for their own slot |
| Application palette | Seventeen fixed roles, independent dark palette, Accent focus and application-level theme; see the color contract |
| Progress sheen | Solid Primary fill plus the authorized moving 105-degree, 47% Surface-light white overlay |

SetFont selects a host-provided stack; it downloads no text font. Gallery's stack is distinct from Design's default. Native Framework keeps its native reading presentation. The fixed title scale and compact-state role replace former responsive Blazor title sizes. Large names/descenders must remain readable at narrow widths and zoom.

Material Symbols Outlined is the already-authorized bundled variable icon font. Icon, AppIcon and Glyph share mapped Unicode glyphs/currentColor; enclosing icon-only controls own accessible names. Unknown names use help_outline, supported legacy aliases remain and IconCatalog exposes official names. Framework owns the font/license/provenance so native consumption works without Design. The pinned WOFF2 SHA-256 is 96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3; the 79338-byte embedded map SHA-256 is 225bd09137103cb7746bc93dc08d08764c9f0c3bd04f4b958d4a3c3c19432dd6. Default axes are FILL=0, wght=400, GRAD=0 and opsz=24. This does not imply that every former classic icon name exists in the pinned map.

ExpansionIndicator is the shared decorative disclosure triangle, with browser disclosure glyph, 17px Design geometry and reduced-motion-aware 140ms rotation. Its owner supplies the accessible name, focus and expanded/controls state. Sort indicators keep their distinct sorting meaning.

## Buttons, connected grids and display boards

Button retains content-width geometry, 48px minimum height, 12px radius and 17px text. Canonical variants are Filled, Outlined, Danger, Quiet, Underline and Elevated; Primary/Secondary remain compatibility aliases. Busy/Disabled are states. An icon with no Text/ChildContent infers an icon-only control and requires a name. The old IconOnly parameter is removed. Unavailable links lose their target/focusability and suppress navigation/callbacks. Busy decoration is contained and must not create document overflow.

UniformGrid defaults to Rectangle, with preferred 2:1 tracks, MaxCellHeight=260 and a preferred width cap of twice that height. Square uses 1:1 and MaxCellSize=280. The fit-content container uses available width for automatic rows/columns and shrinks tracks below the cap. Explicit Columns, Rows and NarrowColumns remain available. Home and Framework interactions leave Rows/Columns unset; the Foundations theme chooser deliberately uses three Rectangle columns and one row. FormActions keeps its separate full-width action composition.

Cells remain connected with thin 1px separators and shared outer rounding. The container has no filled rectangular backdrop or enclosing box shadow behind missing cells in a partial final row. Each cell owns its variant paint/outline; Elevated's alpha-aware drop shadow follows occupied cells. Filled uses Primary, Outlined Canvas/Border, Elevated Surface and Danger its semantic roles. Standalone cells retain their own corners. Ordinary Button inside FormActions does not become a full grid cell. The recommended sample composition uses Filled for the primary action and Outlined for alternatives; no first-child rule changes defaults.

UniformGridItem is passive; UniformGridButton forwards Button's native link/button semantics. Both support Title/Icon/Text/ChildContent and nullable IconSupport. A local value overrides the container, otherwise a nonempty Icon enables equal upper/lower regions. The icon aligns to the bottom of the upper half with 4px padding; the 34px title and 17px copy start at the top of the lower half with 4px padding, giving an 8px central gap. False keeps continuous vertical flow. Nested automatic grids reset inherited support; Busy hides the icon without collapsing reserved geometry. Long content scrolls inside the cell/lower region up to its cap. Form-action cells retain their separate 96px minimum.

SplitButton is a generic pair of independent actions with shared Selected and optional Expanded/SecondaryControls. It has a 3px gap, 3px corners facing that gap and normal outer corners. Button/SplitButton resolve fragment-only Href against the current path/query, replacing the old fragment; UniformGridButton inherits this. Routed/external links keep normal behavior.

DisplayBoard takes arbitrary ChildContent, optional CopyText, Dotted=true and Centered=true. Its Display board role covers both control previews and code. Code composes Dotted=false, Centered=false and CodeBlock; it starts upper-left without the former top reserve. One edge-aligned overflow viewport owns scrolling, with 24px content padding (16px narrow) and 24px board/following-content spacing without doubling. Supported custom scrollbars use 4px thumbs, transparent tracks/corners and no arrows. Copy stays upper-right outside the viewport, imports clipboard JS on demand and shows check for 1.6 seconds after actual success; failure is reported. Full-width forms/tables/layouts keep their required width; ordinary children/action groups center.

Card and IdentityCard use Primary/PrimaryInk in both themes. IdentityCard keeps grouped semantic dt/dd pairs, complete names, responsive facts and sidebar behavior. Its width/container geometry supports a centered DisplayBoard. FactList and its sample/API were intentionally removed without an alias; callers migrate to UniformGridItem or UniformGridButton.

## Gallery organization and executable documentation

The five primary destinations are Home, Framework, Controls, Foundations and Examples. Their updated symbols are home (unchanged), responsive_layout, crossword, shapes and explore respectively; the fixed theme command uses routine. The requested explorer artwork corresponds to the official explore glyph. Other navigation entries are unchanged. Home has no secondary rail. Other destinations show only the selected topic/category:

| Primary | Secondary content |
|---|---|
| Framework | Setup, top bar, navigation, commands, layout, built-in interactions, language/localization |
| Controls | Actions, inputs, data, overlays, feedback, progress, content, shell/layout |
| Foundations | Colors/theme, typography, spacing, radius/shadows, dimensions, icons |
| Examples | Index, form composition, live form, records, details, composed layout, enum selection, search/list |

Program declares navigation through explicit nested configure lambdas. The catalog supplies directories and guides without replacing that configuration with a loop. Category directories use Square UniformGridButton destinations; /controls/{category}/{sample-key} shows one guide and its third-level selection. Parameter reset prevents a prior child topic surviving navigation to its parent; /appearance redirects to /foundations.

Current catalog: 77 guides, 546 parameter rows and 253 documented non-null defaults. Each guide has five Section/H2 areas: introduction, control examples, API, scenario examples and scenario code. The large component title is not duplicated as another guide H2. Samples are compiled Razor and embedded by the Gallery project; the same code supplies the executable scenario and displayed/copyable source after removing only Gallery registration/namespace lines. These resources are active documentation, even without a literal tag reference.

API lists use DataTable and actual public Parameter metadata, including inherited parameters, types, enum choices, EditorRequired and reviewed initialized defaults. Empty strings are explicit double quotes; absent callbacks/templates/type-dependent values remain blank. Qualified UniformGrid container parameters on child guides describe composition rather than inventing child Shape APIs. UniformGridButton previews deliberately use six explicit desktop columns and two narrow columns; that sample is distinct from automatic Home layout.

Gallery uses standard Field/TextBox/Button/PrimaryNavigationItem/typed SelectBox composition. PageContent/PageContents retain their real semantic li/a markup. Field's email scenario uses EditForm, DataAnnotationsValidator, Required/EmailAddress and genuine validation rather than an invented initial error. SearchAutocomplete's scenario starts with real matching demonstration data. Foundations/Icons expose live roles, scenario settings and official font provenance/usage links.

## Forms, lists, menus and notices

Components and Primitives contain similarly named controls with different contracts. Qualify imports when necessary. Components Dialog/BottomSheet use CanClose; extracted Primitives.BottomSheet exposes IsBusy/OnClose and optional Actions (default null). Closed primitive sheets defer children/module mounting until first use, then retain mounted drafts. Action footers align right; decorative overlay dividers are removed while real control/outside outlines remain.

Ordinary focus selection excludes data-input-mask fields. MaskedInput/StandaloneMaskedInput keep a clicked caret; explicit selection/Ctrl+A still works and Design supplies visible selected text. Captured formatting precedes enhanced oninput binding. MultiSelectDropdown/ReferenceDropdown use a constrained 240px trigger and independent intrinsic popup width (480px default cap), reserve supplied-label width and cap/wrap within the field at 760px and below. Host width overrides remain supported.

SelectBox and table/search selectors keep native binding/keyboard semantics. Shared dropdown rules provide transparent idle triggers, standard 48px controls, 12px control corners, 16px popup panels, 6px padding and rounded 10px option rows of at least 40px. Enhanced native select retains right-edge placement with vertical flip; unsupported engines use native popup geometry. EditingGrid's dense editor retains its size while popup Text/Surface colors stay independent of selected-cell ink.

Components `DataTable<T>` consumes local items, TableColumn delegates and RowAction callbacks. It supports all/single-column search, typed/localized stable sort with null last, default page size 10, column visibility, list/cards and pointer/keyboard resizing. Page sizes 10/20/50/100 plus a custom positive current value remain synchronized and reset paging. ItemRangeFormat receives Items label, first/last visible item, Total label and filtered count; loading/error do not fabricate success counts. Gallery uses 项 and 共.

Automatic widths cap ordinary columns at 320px; the final visible data column is exempt, independent of spacer/action columns. Manual widths may exceed the cap; Home resets by the current role. Sort hover stays on the inner Quiet button. Standard ActionMenu targets are 48px; row/card action context uses 36px. Sticky action cells and popup positioning keep operations reachable during horizontal scrolling.

DataTable's display controller keeps native details/summary/checkboxes with the shared disclosure/popover lifecycle. Checkbox changes stay open, arrows skip disabled choices, Escape returns focus and outside close preserves the interacted destination. data.js connects/disposes both empty and populated states. Extracted Primitives.DataTable has its own metadata/preferences/templates/bulk-edit contract; its local processors do not supply remote querying or cursor pagination. Default ITablePreferences is scoped in-memory storage.

Notice and Primitives.StatusNotice share Design severity mappings. StatusNotice retains paragraph markup and independent Role/Announce API. NoticeTrigger uses a 17px Design circle (1em native) with cross/exclamation/check, real accessible severity text and description IDs; hover/press keep semantic paint. Information/Subtle compatibility use Info text, Warning its approved orange pair, Success Primary and Error Danger. Color does not replace status text or ARIA semantics.

ProgressBar retains numeric/unknown/Stopped semantics, solid Primary fill and the approved moving white sheen. ProgressRing provides the larger approved geometry so 100% fits at unchanged 17px body type. Reduced motion explicitly stops unknown fill movement/transforms as well as normal animation.

## EditingGrid and external consumers

Shared GridContracts defines columns, rows, cells, changes, options, text and editor kinds. Framework owns EditingGrid/GridInteractions and its browser bridge. Stable row keys, unique column keys and exactly one cell per column are required; malformed dimensions are rejected. Editors include Text, Decimal, Date, Multiline, Select, Masked and Link. Raw Value and formatted Display are host inputs.

CellChanged supplies a proposed edit. EditDisabled and per-cell Disabled/ReadOnly/Hidden guard UI edits; link cells retain intended navigation. Primary/secondary error descriptions are encoded and associated with cells/editors. GridInteractions provides edit lifecycle, undo/redo/save, load-more, streamed paste/apply and error callbacks. Host parsing, authorization, drafts, transactions, persistence and cursor semantics stay outside the renderer. MeasurementRevision/resize/disconnect are presentation lifecycle, not business saves.

Colligere uses local sibling project references: Web consumes Framework/Design, Presentation consumes Framework. Its MainLayout/UserLayout use NavigationSurface/ContentSurface; adapters use Primitives; ProductSpreadsheet/RegistrySpreadsheet map existing drafts/operations into EditingGrid. Host identity, approved navigation, DTO conversion, sessions, API calls, validation and history remain in Colligere. Runtime menu filtering is not authorization enforcement.

The libraries contain only generic contracts/components/assets. Consumer page selectors, business skins/scripts and isolation metadata remain in the host. NavigationSurface/ContentSurface provide slots and neutral classes; RecordListPage composes heading/actions/status. This development linkage does not establish a published package deployment or complete migration of all business pages.

## Assets, validation and remaining work

CSS stays modular in source. build/BundleCss.cs and CssBundle.targets flatten local imports into one public bundle per Framework/Design package without Node or another package. The SDK publishes the bundles; behavior JS is demand-loaded. Gallery uses fingerprint-aware Assets links, quiet ordinary ASP.NET static logging and public static-endpoint shortcuts. Development builds retain correctly related gzip assets; Publish also produces Brotli. A 200 response alone does not verify Content-Encoding decoding.

Run-specific totals/screenshots/timings belong in historical records. Current source inspection, builds, SSR/DOM/palette checks and HTTP/package checks cover different boundaries: SSR does not execute post-render JS; mocks do not certify live browser APIs; installed headless Edge evidence does not certify every engine, assistive tool, pure pointer timing or host composition. Use manual acceptance for those cases and for host business integration.

Known limits include no virtualization/generic remote query, in-memory default preferences/demo state, retained Portuguese defaults in some Primitives and different browser fallback between enhanced Components ActionMenu/overlays and native Primitives RowActionMenu/Popover. No universal localization or public NuGet publication is claimed.

Local Server/SSR Culture integration is implemented under the authorized 2026-10-05 task. Abstract exposes TextReference/ITextProvider; Framework stores explicit startup references in outer metadata and resolves them per circuit through its neutral TextComponentBase. The optional same-repository Flourish.Extensions.Culture.Blazor bridge connects Essential.Culture.Blazor, and Gallery has separate embedded app/framework catalogs, a standard top-bar language picker, bilingual Home and /framework/localization. String APIs, DTO placement and explicit control overrides remain compatible. Framework/Design have no mandatory Culture reference, and Native uses literal fallback. The [Culture integration guide](culture-web-integration.md) documents actual registration, scope, coverage and local source/package limits. Other guide/primitive translations, persistence, client/WASM validation and public package publication remain separate work.
