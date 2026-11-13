# Unify generic multiple selection

Local timestamp: 2026-10-06 13:37:00, America/Sao_Paulo.

## Decision and implementation

The user requested replacing the display-specific DisplayOptions production scenario with a general MultiSelectBox beside SelectBox in the Inputs Gallery, and explicitly confirmed merging the existing searchable/creatable MultiSelectDropdown into the same unique control. This supersedes the earlier semantic audit's separate-control recommendation. Table/chart ownership of display state remains valid and unchanged.

Components.MultiSelectBox is now the sole general multi-selection renderer/controller. It accepts stable MultiSelectOption keys with selection, fixed-membership, fixed-position and disabled flags. Changed publishes complete MultiSelectChange ordered/selected-key snapshots. General defaults allow zero selected items and disable ordering/search; optional minimum/maximum bounds, label search, asynchronous business creation, localization and ordering reuse one lifecycle. Omitted labels summarize zero/one/multiple selections. Static rendered choices use the same markup/controller and flourish-selection-change event; search and asynchronous creation require interactive rendering.

Both component and controller enforce limits and fixed/disabled members. Filtering retains every keyed member while hiding unmatched rows and refuses reordering; pending creation locks changes and concurrent creation. Static summary captions track native selection changes. The search input keeps shared input styling; checkbox paint is restricted to checkbox inputs. DataTable only accepts its own selection-root events, so a business selector inside a table cell cannot change display preferences.

DataTable and LineChart map display members to the same control, explicitly supply localized Display labels and enable ordering where appropriate. They retain visibility/order state, processing and preferences. Old DisplayOptions/DisplayOption/DisplayOptionsChange, Primitives.MultiSelectDropdown and their duplicate assets/styles are deleted without aliases. Package scripts and asset checks use multi-select-box.js/css. The ShowDisplayOptions chart setting remains a semantic visibility option, not a retired component alias.

Gallery has one MultiSelectBox entry immediately after SelectBox, with tags, search/creation, caps, fixed/disabled options and optional ordering. EditingGrid's tag template reuses it. The final catalog covers 89 exports, 723 parameter rows and 316 defaults. AGENTS.md and the active API/architecture guides record the authorized supersession; earlier history is preserved.

## Verification

Core passes 367/367, the Blazor component suite passes 353/353, and the Culture bridge passes 12/12. Actual embedded Blazor text regressions verify Chinese and Portuguese reorder labels and encoded option text. Final control DOM checks pass 67/67, palette 18/18 and table script checks 6/6. All eleven configured JavaScript files pass; CSS bundle and SDK asset gates pass 21/21 and 194/194. Isolated Gallery Release compilation has zero warnings/errors. No physical browser geometry is claimed.

Six existing 1.1.0 local candidate packages were rebuilt in dependency order and verified, without a new dependency or public publication. Four fresh NuGet-only consumer modes pass 99 checks against the final bytes, including registration, SSR and published assets. Evidence: artifacts/multiselect-final-package-set.log, artifacts/multiselect-final-package-consumers.log and artifacts/package-consumers/f9faaace731949b8ac35fc32853d9440. Other gates are recorded in artifacts/multiselect-all-js.log, multiselect-css-bundle.log, multiselect-css-assets.log, multiselect-core-final.log and multiselect-culture-bridge.log.

An intermediate check caught a misplaced new reorder token in the Core language catalog, which unintentionally added pt-BR to Core's supported locale list. The token was moved to Framework/Localization/Texts.json, where the Blazor three-language texts already belong; Core source and locale configuration are unchanged. The initial failing Core evidence remains in artifacts/multiselect-core.log and multiselect-tests/multiselect-core.trx; the final Core TRX is green. Final packages and isolated consumers were rebuilt after this correction.

## Manual acceptance

1. Restart Gallery and verify MultiSelectBox appears beside SelectBox, with no retired multi-selection entries.
2. Check zero/one/many summaries, search/no results, successful/failed creation, selection caps, fixed/disabled choices, light/dark themes and narrow screens.
3. With ordering enabled, verify whole-row drag and arrow-key order; fixed slots stay fixed and filtered lists refuse ordering. Check Escape, outside dismissal, focus and async creation.
4. Check DataTable visibility/order, Cards restrictions, stored preferences and native progressive pages; a selector inside a cell must not change columns. Change LineChart data after visibility/order choices and confirm those choices remain.
5. Check EditingGrid tag cells, pending edits and save, including keyboard/clipboard behavior.

No Computer Use, running-application termination, public publication, Git commit or push occurred. Colligere's real consumers are migrated separately with business validation and permissions retained; their verification is recorded in that repository.
