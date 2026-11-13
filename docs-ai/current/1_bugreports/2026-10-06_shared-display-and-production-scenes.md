# Shared Display controller and production scenes

Status: accepted local Release/package/automated gates on 2026-10-06. Manual browser acceptance remains outstanding. No public package publication or Git mutation occurred.

## Symptoms and cause

The consumer Workspace dashboard used genuine MultiSelectDropdown but for the wrong responsibility: form/business value selection was being presented as a chart Display controller, with host visibility state. DataTable still embedded its own display menu and order handlers, so chart/table reuse was not a single production capability. Name-only adoption checks and test adapters invoking old private methods failed to expose this boundary.

The companion audit found native access-method navigation being reconstructed as host pseudo-tabs and validation messages rendered separately from Field. A construction-helper namespace or a real Flourish type does not automatically identify a complete control for the intended scene. These are library capability/usage gaps, not an authorization to copy host CSS.

## Current implementation

- Abstract DisplayOption/DisplayOptionsChange describe stable members and a complete order/visibility snapshot. Framework DisplayOptions is the only production menu renderer/controller. It validates unique known keys, fixed/disabled members, minimum visible count and reorder refusal before emitting Changed. It reuses controls.js disclosure/popover lifecycle and detaches its JS/.NET references on disposal.
- DataTable delegates actual Display presentation to that child. Its separate display markup and ToggleColumn/MoveColumn/StartColumnDrag/DropColumn/ReorderColumnWithKeyboard methods are deleted rather than forwarded. Its record data, header sorting, views, widths, retained editing and paging remain the same pipeline. Progressive SSR consumes the shared validated display event; it is not another host drag controller.
- LineChart embeds the same DisplayOptions and maintains keyed order/visibility internally across business-data refresh. Series colors remain stable when another series is hidden. DataSeries.Visible establishes initial state for a new key; consumers must not rebuild a parallel toggling controller. MultiSelectDropdown remains correct for form collection values, not this scene.
- Dead data-column-drag-key handling is removed from input-behaviors.js. Framework owns the display controller/structural CSS; optional Design owns paint. NuGet asset checks require display-options.js and actual consumers verify published assets.
- NavigationChoices/NavigationChoiceItem are the complete native same-site GET alternative-navigation scene. Stable unique keys, a valid active choice and safe root-relative destinations are required. The control owns standard UniformGridButton choices, aria-current and retained panels. It is not ARIA tabs, authentication, POST handling or a compatibility facade.
- ValidationMessages is one error renderer shared by Field and standalone hidden/cross-field presentation. It reads real EditContext fields, supports explicit encoded errors, updates/clears on validation changes, switches context subscriptions and disposes them. It does not recreate forms or validation business rules. A first regression caught a literal rather than bound Field.Error; the binding was corrected before acceptance.

Complete ComponentUsageCatalog/ParameterMeaningCatalog/Gallery metadata classifies these as production scenes. Gallery adds a Display sample shared with table/chart usage, a native access-method example at /examples/display/access-methods and hidden-field validation, while correcting earlier LineChart/MultiSelect examples. The usage guide and AGENTS.md require selecting production responsibility, not just matching component names or a namespace.

## Verification

Accepted evidence: artifacts/semantic-ui-authority-release-final-2026-10-06.log.

- Core: 367 passed; Blazor: 349/349 passed; Culture bridge: 12 passed.
- Catalog: 90 exported components, 725 parameter rows and 319 documented defaults, complete Gallery registration.
- Configured Node runner: 62 passed, zero failed. Embedded DOM mock assertions and configured CSS/palette gates also pass; they are not added to the 62 runner count as independent tests.
- Six verified local package candidates: Core, Blazor.Abstract, Blazor.Framework, Blazor.Design, Blazor meta and Extensions.Culture.Blazor. Package dependencies and required assets pass. Package authoring reports missing README recommendations; this is not a claimed public publication.
- Isolated actual NuGet consumers: 99 checks pass across FrameworkOnly, MetaNative, MetaDesign and MetaCulture. Evidence: artifacts/package-consumers/20fab9f4a62e4c9b9bcdd72b400eaf0d.

Display regressions invoke the actual captured child ApplyAsync contract on the renderer dispatcher and inspect the resulting parent pipeline. Invalid snapshots, fixed/disabled visibility/order, Card/busy refusal, minimum visibility, localized defaults, instance separation and drag-state invalidation are covered. Validation tests exercise real cascading context switching and encoding. Native choice tests reject ambiguous/unsafe declarations and preserve GET semantics. Initial failed evidence remains at artifacts/semantic-ui-authority-release-2026-10-06.log; no failing assertions were suppressed.

The consumer restored the new candidate through its content-addressed cache and built Release against it. Framework SHA256 matches library output, actual package DLL and Colligere application output: 8C098CB865794A39416C5D10C5C6D9A76D2F006B5B5A9446B6B2E72DBC6F2512. Thus package verification is not based only on source rendering. The consumer's existing running Debug instance was retained and is not evidence that new bytes were loaded in that process.

## Manual regression checks and limitations

1. Compare standalone Display, DataTable and LineChart: entry widths, hover/focus, visibility and keyboard/whole-entry drag ordering in both themes and narrow layouts.
2. Verify fixed/disabled members, minimum-visible limits and Cards/busy ordering refusal; malformed/late drag actions must not mutate the current snapshot.
3. Refresh chart values under stable keys; hidden/order state and color identity must survive. Remove/add keys and verify intended initialization; check empty series.
4. Open/close concurrent instances, scroll their intended surfaces, navigate away and return; no duplicate controller or stale focus should remain.
5. Native access choices: GET destinations/current-page state, disabled choices and readable no-JS behavior; no application-tab semantics should appear.
6. Ordinary and hidden-field validation: real EditContext updates, clear/context switch, encoded content and no duplicated Field error. Consumer native form transport remains its responsibility.

No Computer Use, physical-browser certification, authenticated workflow acceptance, database/schema/security change, external dependency update or public upload was performed. Deleted tracked display code remains recoverable from Git. This report supersedes earlier assertions of complete consumer scenario conformance, not the valid retirement of the old table APIs. Future control changes require production-scene/event/state tests; another renderer or alias is not an acceptable workaround.
