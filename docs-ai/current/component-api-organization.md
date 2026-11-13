# Component contracts and composition

Use actual component source, styles, public parameters and executable Gallery/API metadata as authority. This current map consolidates dated layout/convergence reports; archived originals retain exact history and tests. It does not authorize API changes or supply a competing visual specification.

## One production entry per responsibility

General and Scenario are complete production entries; Scenario is purpose-specific rather than experimental. BuildingBlock is a composition helper without a complete standalone lifecycle. Source folders/Primitives naming are not a separate deployment or compatibility classification. Retired public entries are removed rather than forwarded by aliases.

| Responsibility | Current composition |
| --- | --- |
| Interactive records | DataTable/TableColumn/TableSearchRequest/TableData: local or remote search/sort, paging, list/cards/display, retained editors and declared bulk operations |
| Static comparisons | ListView shares column formatting without an interactive record controller |
| Spreadsheet editing | EditingGrid owns cell state, composite editors, keyboard/clipboard and explicit host-accepted snapshots |
| Multiple selection | MultiSelectBox with MultiSelectOption and complete MultiSelectChange snapshots; shared selection renderer also serves supported display members |
| Scalar/reference/candidate choice | Bound or Standalone SelectBox, nullable ReferenceDropdown and SearchAutocomplete have distinct parsing/query/selection contracts |
| Native choice navigation | NavigationChoices/NavigationChoiceItem supplies same-site GET alternatives and current-page semantics, not authentication or ARIA-tab transport |
| Validation | Field and standalone ValidationMessages share one message renderer; hidden/cross-field errors remain accessible |
| Dialog/confirmation | Dialog handles retained content, awaited result, centered/bottom presentation, modality/focus and browser-controlled views; only explicit acceptance may advance host mutation |
| Command menus | ActionMenu generated Actions or mutually exclusive native ChildContent, plus SplitButton/ServiceMenu for their actual supported semantics |
| Tutorials | TutorialBoard presents host-owned milestone completion/eligibility through a compact progress entry and nonmodal overview |
| Feedback | Notice/NoticeTrigger for supported explanation; FloatingNotice wraps transient feedback and shared action docking |
| Forms/actions/tiles | FormLayout, Field, FormGroup, FormActions and InlineActions; UniformGridButton is an actual tiled finite action/choice |
| Identity/content | Avatar, LogoDisplayer, Card, CopyText, DisplayBoard and Icon retain their distinct information/accessibility/lifecycle responsibilities |
| Shell/navigation/titles | ApplicationShell, NavigationSurface, NavigationTree, ShellHeader, PageHeading, SectionNavigator and BackToTop |
| Appearance/text | Optional Design AppearancePalette/service and optional Culture adapter through neutral text contracts |

Do not merge similar names or children before the replacement preserves complete behavior, semantics, accessibility and lifecycle. Bound inputs require the EditContext/parsing/validation protocol; Standalone inputs preserve native unbound GET/POST values. Disclosure and controlled ToggleSection, Card and staged OfferCard, or ProgressBar and ProgressRing are not interchangeable simply because their paint resembles another control.

## Data and action ownership

Hosts retain business records, stable IDs, permission checks, versions, server search/paging, confirmation meaning, persistence and error state. Component visibility is not an access boundary. A loaded browser page is not the complete remote dataset. Shared table paging must not be wrapped in an independent competing pager.

ReferenceDropdown uses Value/ValueChanged for a nullable scalar reference. MultiSelectBox Changed carries a complete validated order/selection snapshot rather than a scalar toggle. Search/create/bounds/fixed-member/order contracts must be preserved across asynchronous changes. Scalar null is distinct from false; invalid parsing must not silently notify or overwrite a model.

Supported Class/AdditionalAttributes and semantic/named slots are composition contracts, not authorization for host geometry or skin substitutes. Field.Actions holds associated ordinary actions separately from the input context. Native input names, ValueExpression, disabled/busy behavior, caret/masks and progressive form semantics remain explicit.

Button structured Text/Description/TrailingText remains one encoded action rather than several independently interactive elements. Native and interactive menu owners retain their own focus/disclosure behavior. A native details fallback is not a promise of enhanced arrow navigation before JavaScript.

## Layout, interaction and lifetime

Framework owns bounded centered/expanded/fluid content, header action spacing, popup measurement/placement/clipping, width growth, focus and disposal. Design supplies canonical theme/contrast paint. PageHeading's full Blazor heading stays in flow while the independent compact strip slides; original contextual controls remain available. A scroll/title refactor cannot change route, focus or unsaved-draft ownership.

Weak/Strong popup behavior follows the actual supported PopupStrength contract. Incidental outside/Escape/focus/hover dismissal must respect Strong, unavailable/pending state and explicit Done/Close/Cancel. Repeated activation and asynchronous completion cannot duplicate a business action or attach late callbacks to a retired context. Dialog owns close veto, cancellation, awaited completion, focus entry/return and nested-view visibility; consumers own business decisions.

FloatingNotice preserves ordinary Notice severity/content/live-region semantics in a zero-flow surface. It coordinates height/envelope with SaveChangesBar, moves vertically, replays new occurrences and returns on reverse exit; the save area changes horizontally. The lifetime pauses for interaction/background state and shows remaining progress. Persistent explanations stay inline and explicit zero duration remains persistent. Dismissal never rewrites host business/hidden state.

Avatar's crop editor includes Reset, restores the source's initial centered crop without saving, and keeps the dialog open. Host persistence remains separately authorized/version-bound. Selector forms preserve native input/ARIA semantics, compact trailing selected markers and standalone input-height geometry. Group elevation/separators and standard single-color progress are library behavior rather than host CSS fixes.

## Gallery, localization and manual acceptance

Gallery reflects exported production entries, parameters/defaults and usage metadata and supplies actual examples in the same controls. Fictitious login/account/billing/business samples are maintained examples, not real protocols. Technical explanation belongs outside executable preview backgrounds, while necessary labels/validation/status remain inside controls.

Culture and formatting are separate explicit preferences; component/library captions and application translation modules have separate owners. Use [Culture integration](culture-web-integration.md), [native WPF](wpf-native-integration.md) and [project constraints](project-constraints.md).

Automated render/event/DOM/actual-CSS/catalog/package-consumer checks are maintained. They do not certify physical browser or OS-window behavior. Human acceptance should cover:

1. Narrow/long-content layouts, scroll boundaries, title/back/action movement, footer/progress and nested clipping.
2. Pointer/keyboard/focus, unavailable/busy states, explicit/incidental popup dismissal, repeated opening, stale completion and disposal.
3. SSR/native forms with and without enhancement; validation, null/false parsing, failed preference persistence and retry.
4. Data paging/sort/search, retained drafts, column resize/order/visibility, bulk edits and grid clipboard/undo boundaries.
5. Avatar crop/reset/save/cancel; feedback docking/replacement/timing/pause/exit; screen-reader names/live regions.
6. Light/dark/system, approved role colors, high contrast, reduced motion, language/format changes and browser zoom.
7. Native WPF bindings, actual owner windows, OS file/date dialogs, display scaling and lifecycle cleanup.

No Computer Use, real authentication, new dependency, publication or fresh visual acceptance is performed by this document consolidation.
