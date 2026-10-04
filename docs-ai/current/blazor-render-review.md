# Blazor rendering review and acceptance boundaries

## Current source baseline

This active review now applies to the four-project split and Colligere's actual local consumption. Framework owns functional layout/visibility/scroll/resize and browser JS; Design owns visual foundations and skin and is an explicit opt-in. Shared/Abstract provide data and interfaces. The initial single-library rendering report does not by itself verify this final structure.

Colligere Web currently references Framework and Design and loads their styles in that order. MainLayout/UserLayout compose NavigationSurface/ContentSurface. Shared/Presentation adapters consume Primitives, and ProductSpreadsheet/RegistrySpreadsheet consume EditingGrid while retaining business drafts, validation, permissions, API operations and history. Gallery includes the Design shell plus the generic `/patterns` example. `tests/Tests.Flourish.Blazor.Native` is a separate Framework-only Web host.

The user clarified the universal-library boundary after this review. Earlier rendering observations remain historical; business page skins and scripts have returned to their host. The current library exposes only generic controls, surface patterns, tokens and browser interactions. Subsequent performance work uses source, build, automated and HTTP checks plus the manual checklist; the earlier browser evidence does not certify the changed asset boundary.

## Earlier rendered alignment context

The earlier 2026-10-03 review sampled real Workspace dashboard/list/form/identity/customization surfaces and the standalone Gallery. It identified useful source-correspondence requirements that remain acceptance targets:

| Area | Established correction/contract | Current owner |
| --- | --- | --- |
| Heading focus | Navigation H1 focus avoids a field rectangle while actionable controls retain keyboard focus | Framework behavior + Design focus presentation |
| Typography/cascade | Control reset must not override large action-cell typography | Design |
| Native/enhanced select | Picker arrow aligns horizontally; native fallback remains usable | Framework markup/browser behavior + Design |
| Shell/rails | Source selected treatment, no phantom secondary rail, brand/service slots and host-supplied navigation | Framework shell/Patterns + Design skin |
| Page/list rhythm | Full-stage title, accepted section spacing and source search/view/pager geometry | Design |
| Identity/facts | Grouped dt/dd, primary value hierarchy, complete wrapping and odd final fact spanning | Framework composition + Design |
| Floating surfaces | Correct role-token propagation, readable popup layers and full-width sheet divider | Framework browser behavior + Design |
| Dirty navigation | Discard follows the requested destination; keep-editing retains the draft | Gallery demonstration / Colligere host adapter |

The old report's 35 console checks, 79 Edge rendered checks and six-asset package inspection described the earlier single-library state. They must not be reused as counts or certification for the split packages or migrated Colligere grid. Historical evidence remains in change records; current source and acceptance scope are described here.

## Source review corrections in the split

- Framework has no Design project reference and AddFlourish registers no appearance service. The separate native host references Framework alone and loads only framework.css.
- Framework native CSS preserves real masked-input text without the decorative layer, hides assistive labels correctly, paints icons at relative sizes, establishes identity containers and retains disclosure/switch state. Design native-overrides explicitly restores decorative layers.
- Functional shell layout retains intrinsic tracks and actual stage overflow. The accepted fixed optical dimensions belong to Design. Table scroll/resizer hit areas remain functional assets.
- Design theme-aliases maps ThemeScope `--f-*` to the extracted workspace/account/semantic variables on the pattern roots, including color-scheme. Consumers can still provide explicit inline roles.
- EditingGrid rejects ambiguous column keys and mismatched cell counts. Its readonly/disabled and primary/secondary error metadata appear in native controls and grid ARIA; host parsing, business history and persistence remain outside the renderer.

These are implementation/source-review facts. They do not replace a browser check of CSS cascade, focus, clipboard, geometry or lifetime behavior.

## Verified non-browser evidence

The console suite currently passes 47/47 checks, including eight EditingGrid/native composition additions and four dropdown state checks. It checks typed/localized table processing, DI state/configuration, contrast, encoded text, controls/shell/dialog semantics, grid shape/editor/error/lock contracts and Framework-only Patterns/Primitives rendering with no IAppearanceService. HtmlRenderer does not run interactive post-render JS, and the JS mock does not prove a live browser's API behavior.

The [completion record](currentproject-changelogs/2026-10-03_170436_split-blazor-ui-and-integrate-consumer.md) records final Debug/Release and consumer builds, package contents, independent offline PackageReference restore/build, live browser observations and screenshots. It also records the final dropdown recheck awaiting an authenticated document. Hover/held intermediate frames, clipboard acceptance, CRUD and cross-browser results are not claimed. The focus/ARIA repair is documented in its [bug report](bugfix-reports/2026-10-03_blazor-sheet-origin-and-dropdown-aria.md).

## Current browser-review limits

Current browser automation supports click, keyboard input and drag. Independent hover and holding an intermediate animation frame are unavailable. Focus-triggered tooltip behavior can be checked independently; it is not proof of hover delay, hover corridors or animation timing. Final screenshots capture a state and cannot establish the behavior between states.

Use [blazor-manual-tests.md](blazor-manual-tests.md) for acceptance of:

- Framework-only mask/icon/tooltip/container behavior and the corresponding Design opt-in cascade.
- Gallery `/patterns` and authenticated Colligere navigation/forms/lists, including authorized menu/route boundaries and intact host business behavior.
- EditingGrid selection/edit/copy/paste/undo/redo, cursor loading, resizing, error announcement and readonly/busy policy in an authorized test workspace.
- Menu-to-dialog focus return, outside/Escape handling, disconnect/disposal and repeated navigation without stale callbacks.
- Pure pointer hover, intermediate animation states, OS DPI/zoom, forced colors/reduced motion, screen readers and additional browsers.
- A packed Framework-only third-party host followed by explicit Design adoption, with no sibling source requirement.

Known compatibility differences remain visible: extracted RowActionMenu calls native Popover directly; enhanced Components ActionMenu has fallback behavior. Extracted Primitives include retained Portuguese labels. Default preferences and Gallery demonstration state are in-memory. No generic remote-query, virtualized grid, universal localization or public NuGet publication is claimed.
