# Component API organization

This is the current technical usage guide, not a second visual specification. Flourish source, ComponentUsageCatalog and the executable Gallery own UI/UX. The 2026-10-06 pre-production breaking refactor supersedes the previous compatibility/adaptor recommendations in this file. Their historical scope remains in append-only change records.

## One entry per responsibility

General and Scenario are production entries. Scenario means a complete control for a particular purpose, not experimental status. BuildingBlock means a composition helper that lacks the complete interaction or lifecycle of a standalone feature. The Primitives namespace is a historical grouping; select by the maintained usage catalog, not by namespace. There is no Compatibility classification or supported retired entry.

| Responsibility | Current entry and shared contract |
| --- | --- |
| Interactive records | Components.DataTable with TableColumn, TableSearchRequest and TableData. One f-data renderer/controller supplies local/remote search, header sorting, paging, List/Cards, Display, widths, Registry/Pool/Worklist, retained editors and declared bulk operations. |
| Multiple selections | Components.MultiSelectBox is the unique general selection renderer/controller. Business options and DataTable/LineChart display members share MultiSelectOption and complete MultiSelectChange snapshots, with optional search, creation, selection bounds and ordering. |
| Native choice navigation | NavigationChoices with NavigationChoiceItem owns same-site GET alternatives, current-page semantics and retained panels. It is not ARIA tabs, authentication or POST handling. |
| Validation messages | Field and standalone ValidationMessages share one error renderer; the latter covers hidden protocol/cross-field messages, not another input layout. |
| Static comparisons | ListView shares TableColumn and formatting, without an interactive record controller. |
| Spreadsheet editing | EditingGrid with GridContracts. Cell state, keyboard/clipboard and composite editors make this a distinct scenario, not another DataTable API. |
| Search | Components.DataSearch is the only table-search renderer, also usable independently. SearchBox and SearchAutocomplete have different query/candidate-selection responsibilities. |
| Dialogs | Components.Dialog is the single entry for controlled opening, awaited results, Centered/BottomSheet presentation and browser-controlled pre-rendered views. It owns retained content, native modality, cancellation and focus. |
| Feedback | Notice and NoticeTrigger use Core NotificationSeverity. Notice supports explicit Role and announcement policy; NoticeTrigger is supplementary explanation, not a mandatory error surface. |
| Confirmation | Consumers and NavigationGuard instantiate Dialog and ordinary Button directly. ShowAsync awaits a caller-defined result; only explicit acceptance advances the business operation. There is no confirmation host/service or browser confirm API. |
| Command menus | ActionMenu owns generated MenuAction commands and mutually exclusive native ChildContent actions. Both share the same top-layer controller, disabled/focus rules, asynchronous invoker origin and disposal. RowActionMenu is removed, not forwarded. Multi-selection disclosures use the same controller without command-dismiss behavior. |
| Circuit interruption | A host protocol composite directly supplies Dialog BrowserControlled views with Notice, ProgressBar and Button. Fixed Blazor identifiers/reconnect/resume remain host-owned; generic view visibility, native modality and focus remain Dialog-owned. |
| Identity and icons | IdentityCard's semantic slots and Icon's official Material Symbols names. No IdentityFact facade, alternative icon renderer or legacy name aliases. |
| Forms and operations | FormLayout, Field, FormGroup and FormActions. InlineActions arranges ordinary actions; UniformGridButton is the actual tile-shaped action. No host FormSurface/FormActionBar/field-layout facades. |
| Titles and navigation | Components.PageHeading, SectionNavigator and BackToTop. ShellHeader is the production shell heading entry. Static PageContents and separate record-heading APIs are removed. |
| Appearance | AppearancePalette and scoped AppearanceService resolve canonical --f-* roles. No ThemePalette facade or legacy CSS variable bridge. |

Application registration uses AddFlourishFramework with IFrameworkBuilder, IProjectBuilder, ITopBarBuilder, INavigationBuilder and ISubNavigationBuilder. Project branding, top-bar configuration and navigation are configured through those contracts. Old AddFlourish/application/title/group builders and runtime NavigationGroups adapters are removed. ApplicationShell's guide links to the independent /examples/shell page instead of mounting a second configured shell in the Gallery document.

ShellHeader's Services, LeadingActions and EndActions fragments and ApplicationShell's start, center and end tracks use the same library-owned top-bar slot. Chrome-scoped selectors override document-flow alignment and block margins on direct roots, native form controls and direct InlineActions members without changing variants or popup geometry. The two-class structural specificity wins over later-loaded Button and InlineActions design defaults; no host patch or important override is required. Brand/logo tracks retain their separate stretch contract. Gallery's ShellHeader example switches between unauthenticated Elevated actions and an identity with a direct Quiet sign-out action.

## Binding and composition

Project-owned composition parameters use Class, AdditionalAttributes and semantic child/named slots where the control supports them. ContentSurface, NavigationSurface and ShellHeader no longer expose a CssClass alias. Inherited ASP.NET InputBase validation/native-class infrastructure remains the bound-input protocol, not a second Flourish appearance API.

ReferenceDropdown uses Value and ValueChanged for its nullable reference value, including @bind-Value. MultiSelectBox's Changed notification carries a complete validated order/selection snapshot; it is not a single-value toggle or scalar @bind alias. Bound inputs require EditContext/ValueExpression where appropriate; Standalone inputs serve native forms and unbound values. Native name, parsing, validation, mask/caret and transport contracts must remain explicit rather than being silently interchanged.

ButtonVariant contains Primary, Secondary, Danger, Quiet, Underline and Elevated only. UniformGridVariant contains Elevated, Filled, Outlined and Danger only; there is no Outline alias or Filled boolean. These two enums describe different component responsibilities, not two names for the same API. UniformGridButton delegates actual interaction to Button.

Library Button recognizes role=menuitem and supplies menu-item presentation itself. SplitButton's native MenuContent uses DropdownSurface and real Button entries. Each entry stretches to the same menu width even when labels differ; the 48px square triangle remains independent of primary width. Busy/Disabled block both sides. Controlled secondary-action and native disclosure modes cannot be combined. A native details disclosure does not imply interactive ActionMenu arrow navigation.

Gallery string-valued dynamic component parameters use explicit Razor expressions (for example ActiveKey="@ActiveMethod"); plain attribute text is a literal. NavigationChoices callers must supply an available key rather than weakening library validation. build/Test-GalleryNavigation.ps1 checks the actual compiled sample, native links and retained panels through local HTTP, including query fallbacks and repeat requests.

## General dialog contract

The user's 2026-10-06 general-control request supersedes the former confirmation host/service and reconnect scenario entries. ConfirmationHost, ConfirmationService, ReconnectDialog, StaticDialog and BottomSheet are deleted without forwarding aliases. The [convergence audit](generic-control-convergence.md) records all nine removed entries and the remaining distinct capabilities.

IsOpen/IsOpenChanged retains controlled opening. Alternatively, a directly referenced Dialog.ShowAsync(CancellationToken) awaits any caller-defined object result; CloseAsync(result) follows Busy/CanClose and returns that result. Dismissal, cancellation or disposal returns null. Concurrent ShowAsync, an already controlled-open dialog and browser-controlled mode refuse an awaiting request. HasPendingResult exposes the request state; callers must not overwrite a pending request's business message. Confirmation is just ordinary content and Button actions: CloseAsync(true) is explicit acceptance and CloseAsync() is cancellation. No global confirmation renderer or service is mounted by a shell.

A controlled dialog still mounts lazily and retains child content after its first opening. Presentation chooses Centered or BottomSheet. Dismissible defaults to true and governs the header close action and native Escape; explicit CloseAsync remains available for actual actions. Busy and CanClose refuse closing without completing the waiting task. A canceled request resolves independently of a pending veto, and late close results cannot revive it.

BrowserControlled=true pre-renders a closed native dialog without importing or requiring circuit callbacks. Its lifetime cannot switch to circuit ownership, and IsOpen/IsOpenChanged/CanClose/OnClose cannot be combined with that mode. The host imports controls.js and calls synchronizeDialog(dialog, isOpen, null); native focus, Escape policy and cleanup stay in the same Dialog core. AdditionalAttributes carries native protocol metadata. Reconnection uses Dismissible=false and actual native Button event handlers.

Views accepts unique nonempty DialogView(Key, Content) entries. View selects a key; null selects the first view, and unknown keys are rejected. ChildContent remains available as common content. Browser owners use setDialogView(dialog, key), which hides other views and synchronizes standard actions declaring space-separated data-f-dialog-views keys. Empty associated action footers collapse, and focus moves out of newly hidden content. This is general keyed content, not a reconnect state enum or protocol controller.

Standalone numeric text uses StandaloneTextBox Type=number with native min/max/step/inputmode. PageBody/PageHeading/Section replace RecordListPage. The reference selector's structural wrapper is private markup rather than a SelectionDropdownSurface public component. ToggleIndicator had no real production consumer and is deleted.

## Gallery explanation placement

SampleFor.Description carries technical scenario notes. ComponentGuide presents those notes, usage guidance and code captions outside DisplayBoard, preserving explicit paragraph breaks. Boards contain actual controls, fictional scene data, current state and only short necessary hints. Required form/error/status content is not erased to shorten a sample.

## General multi-selection contract

The user's 2026-10-06 request to merge both entries explicitly supersedes the earlier display-versus-business split in this guide and AGENTS.md. DisplayOptions, DisplayOption, DisplayOptionsChange and Primitives.MultiSelectDropdown are removed, without aliases or adapters. Earlier audit reports remain historical evidence; their separation recommendation is superseded by this single general control. The [consolidation record](currentproject-changelogs/2026-10-06_133700_unify-generic-multiple-selection.md) records implementation, final package gates and manual acceptance.

Components.MultiSelectBox accepts Items as IReadOnlyList<MultiSelectOption>. Each option has Key, Label, Selected=false, CanDeselect=true, CanReorder=true and Disabled=false. Keys are unique and stable; required selections set Selected=true and CanDeselect=false. Changed returns MultiSelectChange(OrderedKeys, SelectedKeys), including all keys in order and the complete selected set. The host accepts the snapshot and updates its supplied options. Missing, unknown or duplicate keys, illegal fixed/disabled changes and invalid counts are rejected.

MinimumSelected defaults to 0, MaximumSelections to int.MaxValue, CanReorder to false and Searchable to false. EmptyText defaults to "No options available.", EmptySelectionText to "None selected", MultipleSelectionText to "items selected" and NewItemMaximumLength to 100; untouched text defaults follow scoped localization. Id, Label and LabelledBy are optional. An omitted Label summarizes the current selection. Search filters by label while retaining the complete keyed state; ordering is refused while filtered. Disabled locks interactions. Non-reorderable and disabled options keep fixed positions; fixed selection and fixed position are independent choices.

CreateRequested is an optional Func<string, Task<bool>>. The host creates the business option and updates Items before returning success; the library does not save objects. Creation is locked against concurrent selection/order changes, honors the selection limit and maximum name length, and refuses duplicate labels. Static reuses the same rendered native choices and emits flourish-selection-change; search and asynchronous creation require an interactive component.

DataTable and LineChart explicitly supply the localized Display label and enable reordering when using this general entry. Their existing column/series state, preference persistence and data processing stay owned by those controls. Business forms use the same options and snapshot renderer without inheriting display-specific state. The Inputs Gallery places MultiSelectBox immediately after SelectBox and demonstrates business tags, search/creation, limits, fixed/disabled options and optional ordering. EditingGrid's composite tag cell also uses this production entry.
## DataTable interaction contracts

There is no sorting toolbar or ShowSortControls parameter. Sorting is changed through column headers. Display items themselves are draggable, not small independent handle buttons; DataTable delegates to MultiSelectBox with its localized Display label and CanReorder=true, which shares the menu lifecycle with controls.js and owns drag/keyboard order, complete-state validation and fixed slots. The table retains its own data/view/width controller but no second display menu renderer. Cards, busy state, non-reorderable items and unsupported targets refuse ordering. Colligere does not attach another drag, sort, paging or table controller.

Remote search does not re-filter server results. Typed display/search/sort values, scoped culture, declared visibility/order/width preferences and stable ItemKey belong to the common pipeline. Pool and Worklist retain keyed editors across paging. Host slots supply DTO content, permissions, validation and persistence, not copied table geometry.

RowOpenHref is a real GET. OnRowOpen is the interactive callback alternative. NativeRowOpen requires complete RowActionsContent and a unique eligible native transport; these modes cannot be combined. Actions are rendered by actual library menus and Button entries. Disabled/unavailable and ambiguous row-opening targets fail closed.

Progressive enhances the same authorized visible SSR directory without a circuit or hidden DTO payload. It rejects hidden defaults, independent search/sort payloads and editing. Without JavaScript its native content remains readable; this is progressive enhancement of one renderer, not an obsolete-browser replacement renderer.

## Specialized production scenarios

Business navigation owns constrained application scrolling; presentation pages own document scrolling. ContentContainer supplies centered content independently of full-width backgrounds. PresentationBand defaults to an 800px minimum with natural growth; this supersedes the earlier 720px setting. FullHeight is opt-in and establishes max(100dvh, MinHeight). Dotted defaults to false. PresentationHero delegates the same band, adding artistic copy/actions. Adjacent tones are recommended to differ, not automatically rewritten.

UniformGridButton is the production action/finite-choice cell for both forms and multi-step flows. Pure choice steps compose only UniformGrid and button text, without independent visible headings, explanations or dropdown selectors; actual form fields, required business confirmations and error/status feedback remain their distinct responsibilities. Gallery's /examples/display/wizard demonstrates instance-local fictional choices without a new control/API family. Rectangle grids fill their container width and share it across columns; MaxCellHeight caps height only, while twice that value is an automatic-column wrapping threshold, not a width maximum. Square grids retain fit-content, aspect 1:1 and MaxCellSize on both axes. Centered changes placement only; Columns, Rows and NarrowColumns use the same shape-aware layout core.

UniformGridItem centers direct Button children, including the icon-copy slot, through the shared grid stylesheet. Button default start alignment must not override passive-cell centering; nested menus retain their own layout. Gallery /patterns explicitly sets PageBody Fluid=false to use centered content independently of the application-wide fluid default.

PresentationFooter's title and decorative wordmark always use configured ProjectName, including scoped text refresh. Copyright is supplied separately by the consumer. BrandName and Watermark override aliases are removed. AttributionFooter is the distinct small attribution scenario.

ContentSurface with DocumentFlow and PresentationBand FullHeight directly compose access documents, preserving a single main, native scrolling, Tone and Dotted without an AccessSurface wrapper. AccessPanel fits inside an existing document. LogoDisplayer supplies the artistic project identity. AccessFormSurface owns spacing without creating an authentication form or protocol.

Card.Prominent is the presentation variant of the existing Card, not a second callout renderer. Its default is left-actions/right-copy with narrow-screen stacking. Stacked=true keeps copy above actions at every width, using matching DOM and focus order without changing ordinary cards, H1-sized paragraph copy, role paint or native action semantics. The Pricing Gallery custom-license scene uses this option; its free-start card retains the horizontal default. OfferStage/Card retain responsive and reduced-motion behavior, native fragments and instance-owned rotation. DisplayBoard and CodeBlock are preview/copy scenarios, not general page skins. ImagePreview owns image/caption/action presentation; CopyText owns selectable encoded code content.

OfferCard's Title identifies an offer inside the selectable collection. It renders as the library's offer label rather than introducing a page heading. The decorative compact duplicate remains hidden from assistive technology; active-state visibility and transitions target the same label class. Consumers can compose Pricing with a single PageHeading while retaining offer identities, descriptions, prices and accessible region labels.

InteractionBoundary is not a security boundary. NavigationGuard requires actual dirty state and an interactive lifecycle. FilePicker does not replace server-side file validation. FormGroup retains native disabled-fieldset semantics while using actual FormLayout. SectionNavigator requires the intended business scroll region.

ReconnectDialog is removed. The host supplies the fixed .NET protocol IDs, countdown target, state mapping, localized business text and reconnect/resume/reload behavior to actual generic controls. The /examples/overlays/reconnect Gallery scene demonstrates local fictional state changes without server calls or a second controller.

## Browser and ownership baseline

Current native dialog, Popover, ResizeObserver and required media/observer APIs are the browser baseline for their respective features. Missing required APIs reject enhancement rather than activating legacy portals/backdrops or independent old controllers. Clipboard uses navigator.clipboard only; failure is reported without an execCommand fallback. Responsive layout, reduced motion, readable SSR and defensive asynchronous disposal remain current behavior, not compatibility adapters.

Consumers configure approved brand/color roles and business/backend protocols. They must not implement generic controls, skins, drag/sort logic, browser confirmation dialogs or layout wrappers. If a generic capability is missing, implement it once in Flourish with Gallery coverage and focused regressions, then consume that production entry.

## Examples and verification boundary

Gallery registers every exported component and reflects current parameters/defaults. Examples include product, Pricing, access overview, login, account selection, native access-method choice, general multi-selection and reused display selection, hidden-field validation and a standalone configured shell. Authentication, billing and credentials are fictitious fixtures; no real service or credential persistence is introduced.

The gate compares exports, usage metadata and Gallery coverage, and rejects retired public types, obsolete members and aliases. Behavioral tests separately cover native forms, culture, accessibility, availability, callback refusal, retained drafts, confirmation, drag/order, top-layer lifecycle and disposal. Release preparation verifies Framework-only, Design-enabled, Culture-bridge and real isolated NuGet consumers.

Passing source/build/automated gates does not certify physical browser geometry or authenticated workflows. Those require the manual acceptance list in the dated refactor report. No public package upload is implied.

## Organization access presentation corrections

The initial scene reconstruction read Colligere source/history only. Its subsequent adoption uses these production entries directly while retaining business sessions, access policies, antiforgery and native protocol endpoints. No specialized portal control, host skin or compatibility composition is required. Existing organization taglines use Description's body scale rather than an artistic Subtitle.

AccessFormSurface clears its direct Section's page-spacing inset. On Primary, Field and label use the paired ink; Quiet uses the shared Button stylesheet's available-state Primary interaction roles, while explicit variants, inputs and disabled actions retain their own standard skin. Framework owns full-width operation actions. Split titles have explicit 860/560px font scaling and remain bounded/wrapping. See [the correction report](bugfix-reports/2026-10-06_organization-access-presentation-corrections.md); its import-order cascade checks retain the strict ban on presentation CSS repainting standard controls.
