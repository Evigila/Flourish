# Component API organization

Flourish has one Framework package but currently contains two independently maintained rendering families. The Primitives namespace is a historical extraction boundary, not a reliable indication of implementation level or production readiness. ComponentUsageCatalog and the executable Gallery now classify every exported component explicitly. This is a technical capability and migration guide, not another visual authority.

## Inventory and production entry points

The initial inventory contains 88 public Razor components: 47 Components, 38 Primitives and 3 Patterns. The earlier Gallery registered 78; it omitted DateBox, FilePicker, DropdownSurface, StaticDialog, TableSurface, four Standalone inputs and SecondaryNavigationItem. Nine new display and access components bring the maintained inventory and Gallery to 97. Current usage groups are General 22, Scenario 55, Compatibility 14 and BuildingBlock 6.

General and Scenario are supported production entry points. Scenario means a complete component with a specific purpose, not an experimental implementation. Compatibility identifies overlapping public APIs whose behavior must be preserved until a common core replaces them. BuildingBlock identifies a compositional surface or decorative helper without the full state, interaction or lifecycle needed by a standalone feature. Helpers can participate in production components; consumers must not mistake them for complete selectors, switches, dialogs or tables.

Primitives contains 22 supported scenario entry points, 14 compatibility entries and only 2 construction helpers. Colligere source directly references 33 of its 38 types. Therefore banning every Primitive would remove production functionality rather than standardize it. Namespace migration should follow capability convergence, with compatibility adapters where required; moving files alone does not unify APIs.

| Kind in Primitives | Maintained types |
| --- | --- |
| Production scenarios | AccessBrand, AccessSurface, DataPager, DataSearch, DataTable, EditingGrid, FormSurface, InteractionBoundary, MaskedInput, MultiSelectDropdown, NavigationGuard, NoticeTrigger, PageContent, PageContents, PrimaryNavigationItem, ReferenceDropdown, RowActionMenu, SearchAutocomplete, SecondaryNavigationItem, ServiceMenu, ShellHeader, StandaloneMaskedInput |
| Compatibility | AppIcon, Glyph, BottomSheet, DisclosureSection, FilledIdentityCard, FormActionBar, FormFields, PageHeading, PageLoading, RecordPageHeading, StatusNotice, ToggleSection, ToggleSwitch, UniformGrid |
| Construction helpers | SelectionDropdownSurface, ToggleIndicator |

Components also contains construction helpers: ExpansionIndicator, DropdownSurface, StaticDialog and TableSurface. Their namespace does not make them standalone production controls. Gallery shows the missing responsibilities and appropriate complete entry points, and separates compatibility/helper browsing from the default production list without breaking existing detail URLs.

The complete per-type inventory, scenario, guidance and truthful preferred entry are maintained in Framework/ComponentUsageCatalog.cs. Closed generics normalize to their open definitions. Unknown types fail lookup rather than receiving a default production label.

## Why duplicate APIs appeared

Commit 1fcc922 introduced the generic Components family. Commit 1b5b3d0, dated 2026-10-03 20:17 in America/Sao_Paulo, preserved that family while extracting a second host-shaped Primitives family, including a separate DataTable, DataColumn, DataSearch and DataPager. Later work upgraded different families independently. Neither DataTable is a wrapper around the other.

The previous CatalogChecks verified only manually registered entries. It did not compare registrations with all exported IComponent types. This permitted public APIs to be omitted from Gallery and allowed similar names to look equivalent despite different capabilities. The new export-inventory and Gallery checks close the omission gap; semantic capability review remains necessary before adding another renderer.

## Merge decisions by family

| Family | Target organization | Required preservation before convergence |
| --- | --- | --- |
| DataTable and Primitives.DataTable | One typed column contract and shared state/rendering core; browsing, cards and advanced editing are explicit capabilities, not independent table families. | Typed/culture-aware values, configurable paging and all-column search; separate display/sort/search values, templates, column order/preferences, Registry/Pool/Worklist, bulk edits and guarded native transports. Neither current API is a strict superset. |
| Dialog and BottomSheet | BottomSheet is a Dialog presentation variant. Keep the existing common facade; adapt the separate Primitive only after lifecycle union. | Busy/IsBusy, CanClose/OnClose, first-open lazy mounting, retained drafts, asynchronous refusal/concurrency and transient invoker restoration. |
| ToggleSwitch and ToggleSection | One boolean state/rendering core; section composition adds retained content, not a second switch. | Value/ValueChanged, OnLabel/OffLabel, controlled-region IDs, explicit state announcements and attributes. |
| Disclosure and DisclosureSection | One native details renderer; Label becomes a compatibility parameter mapping to Title. | Native opening, supplied child content and attributes. Disclosure expansion is not a business boolean switch. |
| PageHeading and RecordPageHeading | One business title with optional parent navigation and compact mode. | ParentHref/ParentLabel, actions and actual scroll compaction. PresentationHero is a different display scenario, not another business-title API. |
| Icon, AppIcon and Glyph | One Icon source and renderer; role/size facades only. | Existing aliases and size roles. AppIcon/Glyph already delegate rather than load a second icon library. |
| Notice and StatusNotice | One semantic severity contract and notification renderer; adapt old values by meaning. | Subtle, explicit Role, announcement policy and all severity paints. NoticeTrigger remains a distinct supplementary-information component. |
| IdentityCard and FilledIdentityCard | One presentation core with both semantic slots and an IdentityFact data adapter. | Main value, identity role, Facts/SideFacts and original data. Do not delete the data adapter merely because both look like cards. |
| FormFields and FormLayout | Use FormLayout as the field layout/behavior entry. | Native POST attributes, model validation and one focus/input behavior lifecycle. |
| FormActionBar and FormActions | Share layout/child contracts after separating ordinary actions from tile-shaped business actions. | Submit ownership and dimensions. AccessActions is for small access-page actions; marketing Hero owns its own action slot. |
| UniformGrid families | Do not treat fluid equal columns as a skin variant of capped equal-shape tiles. Reuse field layout for actual forms; establish a semantic fluid layout entry if broader arbitrary-content demand remains. | Responsive columns, content growth and native form controls. Tile geometry must not constrain login forms. |
| Bound and Standalone inputs | One input presentation/behavior core with explicit model-validation and native-protocol adapters. | Parsing, ValueExpression/EditContext, raw string values, generated names, autocomplete, limits, mask/caret behavior and server validation. They are valid protocol distinctions, not justification for duplicate skins. |
| ListView | Keep as a separate static read-only control sharing TableColumn, formatting and data appearance. | Complete input-order rows, row/column headers, readable templates and no dynamic controller. |
| EditingGrid | Keep as an independent spreadsheet editor, not a table appearance variant. | GridCell state, keyboard/clipboard, editor kinds, stable identities, errors and host save/authorization commands. |
| Search and selection | Keep SearchBox, SearchAutocomplete, ReferenceDropdown and MultiSelectDropdown as different semantic entry points sharing appropriate popup/input cores. | Query callbacks, candidate selection, nullable typed references, creation and multiple selected values respectively. |

No broad destructive API merge was implemented in this task. Compatibility entries remain operational and are not falsely advertised as thin facades where they still own state or rendering. The unpublished 1.1.0 candidate should receive an explicit convergence/release decision before public publication.

## Specialized usage boundaries

Business shells and navigation tracks own constrained application scrolling, not website banners. PageHeading owns business titles and optional record parents; FormActions owns business operation cells. DisplayBoard/CodeBlock demonstrate or copy content and are not default page skins. ListView is static comparison, whereas DataTable manages interactive records and EditingGrid edits cells.

AccessSurface owns an independent main landmark; AccessPanel fits an existing surface and must not create another main. AccessBrand names an access entry rather than drawing a marketing title. PresentationHero, PresentationBand and PresentationFooter own display-page composition. OfferStage/Card present host-supplied offers without acquiring billing or license authority.

NavigationGuard requires genuine dirty state and an interactive lifecycle; it is not persistence or authorization. InteractionBoundary temporarily locks interaction and is not a security boundary. NoticeTrigger is supplementary explanation, not a place to hide mandatory errors. FilePicker exposes selection, but file constraints and server checks remain host responsibilities. SectionNavigator follows an explicitly chosen business scroll region; it is not a website navigation menu.

## Implemented display and access extraction

- ContentContainer owns the shared centered width independently of background painting. PresentationBand owns full-width Canvas/Surface/Primary role paint and an inner ContentContainer. HeadingLevel selects semantic h1–h6 without introducing another heading API; ordinary headings use existing typography tokens.
- PresentationHero owns the extracted artistic title/subtitle geometry and ordinary description/action slots. PresentationFooter owns full-width role paint, centered identity/navigation and an aria-hidden decorative wordmark. Font family and actual buttons remain standard library implementations.
- Patterns.ContentSurface adds DocumentFlow and Footer instead of creating another shell. Document mode has natural page scrolling, a sticky header, one main landmark and a footer outside main; it does not register the fixed-stage compact-heading controller.
- OfferStage/Card share one progressive browser module. SSR is fully readable. Enhancement activates only on a wide, non-reduced-motion viewport and derives columns from actual card count. A real Quiet Button pauses/resumes automatic rotation, with scoped en-US/zh-CN/pt-BR defaults and explicit host overrides. Focus, pointer and hidden-document state also pause rotation; scoped native fragment links keep default navigation and modifier behavior. Controllers, listeners and timers are instance-owned and cleaned on updates, disposal or DOM removal. Duplicate IDs are rejected within a stage during component rendering; malformed externally authored collections remain static.
- AccessPanel explicitly supports compact/wide layouts. AccessFormSurface provides spacing and appearance without generating a form; the native form or EditForm, antiforgery and token protocol remain host-owned. AccessActions arranges real Button variants. AccessSurface no longer overrides standard button/input skins through broad descendant selectors.
- All nine new controls follow the established Class/AdditionalAttributes and ChildContent/named-slot conventions. PresentationTone belongs in Abstract; rendering/behavior lives in Framework, optional appearance in Design. No package, font, icon source or external dependency was added.

Gallery adds Examples > Display pages with product, pricing and access cases at /examples/display/product, /examples/display/pricing and /examples/display/access. Cases use fictitious data and no real authentication or checkout. Colligere consumes the same packaged controls, so homepage/footer/carousel composition no longer requires a host marketing exception.

## Final Blazor review on 2026-10-05

The final review is limited to Flourish. The 97 exported controls have complete source-owned usage classification, reflected API/default documentation and executable Gallery registrations. That is consistent inventory and usage governance, not proof that all renderers or public APIs have converged. No Colligere page was reviewed or modified in this follow-up.

| Family | Implemented consistency | Remaining contract work |
| --- | --- | --- |
| Buttons and grid action cells | Button variants, availability and native action/link semantics share one renderer; UniformGridButton delegates to it. Passive and interactive cells share their content builder. | Legacy fluid grids are not tile-grid aliases. Preserve their different layout purpose rather than silently mapping their API. |
| Icons and generic sheets | AppIcon/Glyph delegate to Icon; generic BottomSheet delegates to Dialog. | The independent legacy sheet still owns lazy mounting, retained drafts and a different close contract. |
| Read-only and general tables | ListView and generic DataTable share TableColumn and culture-aware TableData. | Advanced DataTable still has DataColumn, separate enums, state and browser behavior. Its processing helpers still select pt-BR rather than the scoped format culture. Implement the capability union before adapting either API. |
| Bound and standalone inputs | Value/ValueChanged bindings and explicit native-protocol adapters are documented. SelectBox now formats current value, options and parsing with the same scoped provider. CheckBox now emits its generated native field name. | The two families still have independent input behavior paths. InputBase uses native class/CssClass while other entries expose Class; attributes are not uniformly available. These are migration debt, not a reason to break existing form contracts. |
| Reference, multiple selection and autocomplete | Selection notifications retain their actual payloads. Gallery explicitly distinguishes nullable reference IDs from single-value toggle notifications; neither SelectionChanged name is advertised as a bind-compatible Changed pair. Busy/disabled and ARIA regressions cover the corrected behavior. | A future canonical binding API must retain existing callback semantics through adapters. Autocomplete still needs a browser-level Arrow/Enter default-action contract; its C# key handling alone cannot prevent an implicit native form submit. |
| Display and access composition | Presentation roles, centered content, artistic copy and standard action slots are library-owned production scenarios. AccessPanel composes inside an existing main; AccessSurface owns an independent main. | These are intentional scenario entries, not alternative business-shell APIs. Registration/recovery and other consumer-specific workflows need their own scenario acceptance when requested. |
| Compatibility layout and feedback | Each entry has an explicit scope and migration warning. | Toggle, disclosure, heading, notification, identity and form families still have independent renderers or lifecycles. Classification alone does not merge them. |

Generic DataTable View/PageSize currently act as incoming defaults with retained internal state: an unchanged value resent by the parent does not restore a rejected local change. StandaloneSelectBox with a ValueChanged delegate instead follows parent-provided state. A controlled/default distinction and refusal/rerender tests are required before changing either contract. Existing primitive NavigationGuard also needs a dedicated delayed-import/disposal review; it must not be treated as an authorization or persistence boundary. ReferenceDropdown's post-callback focus restoration and FocusSearch rendering branches also need disposal-race regressions; the fixed focus-query import race does not establish that every await branch is covered.

The follow-up corrects reproducible selection, native-name, ARIA, availability, reserved-attribute and asynchronous interop defects without renaming public APIs or deleting supported capabilities. Red tests established the old failures before the matching fixes; these regression checks are separate from the export inventory gate. The dated final-review record carries the final build/package/test results. Passing local checks does not close the remaining convergence work or constitute approval to publish 1.1.0 as a fully unified API.

### Executable access page coverage

Examples > Display pages now includes /examples/display/login and /examples/display/accounts, with /examples/display/access as the overview. The independent access layout supplies library resources and scoped theme state, while AccessSurface supplies the document's only main landmark. The AccessSurface component guide links to these actual pages instead of nesting another main inside ApplicationShell or adding a host-owned skin.

The local fixtures cover required/malformed fields, visible busy/disabled behavior, selected success/rejection/unavailability results, available/disabled/stale account selection, email prefill, removal, empty-state restoration and remembering a fictitious email. Passwords are cleared after completion or cancellation. No authentication service, token, credential persistence or network transport is implemented. The login form is disabled until interactive rendering attaches, so static HTML does not accidentally submit credentials. Language and light/dark controls use the existing scoped providers; theme paint and controls remain Flourish-owned.

## Convergence and regression order

1. Review the explicit inventory and select one production entry/core per semantic family. New components must first explain why existing capabilities or variants cannot cover the need.
2. Converge low-risk aliases and layout/title/disclosure contracts with equivalence tests; then unify dialog/menu lifecycle and shared input behavior.
3. Implement the union of DataTable capabilities in the typed canonical core, including host transport/template slots, culture and paging. Convert legacy entries to adapters only after advanced consumers pass the same contracts.
4. Move supported scenario entries out of the misleading historical grouping where beneficial. Retain compatibility only at a declared migration boundary; do not leave two renderers/controllers behind a rename.
5. Add multi-selection editing and progressive/static directory capabilities to the appropriate core rather than a third table family.

Each change must cover event traces, defaults, native names/forms, validation, culture, ARIA, busy/readonly, retained drafts, close/focus/disconnect/disposal and relevant advanced data capabilities. Verify Framework-only and Design-enabled consumers, plus the optional Culture bridge and isolated real NuGet consumption. Inventory equality prevents omissions, not behavioral duplication by itself. Browser geometry and authenticated interactions remain manual acceptance without Computer Use.
