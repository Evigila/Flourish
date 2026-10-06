# Refine the record toolbar and pagination

## Changes

The user requested concise record counts/page-size captions, icon-only display and advanced-edit disclosures, a persistent disabled advanced-edit entry when unavailable, and standard start-aligned menu items. Scope is Flourish only; Colligere is not changed.

- Default item ranges omit the Items prefix. Chinese renders 1-2 / 共2, with synchronized top/footer pagination and the same progressive browser template. Explicit host TableText overrides remain supported.
- Page-size captions are Quantity, 数量 and Quantidade. Updated the Framework catalog, neutral fallback contract and Gallery's explicit API/record/sample captions.
- Registry tools use remove_red_eye for display and stylus_fountain_pen for advanced editing. Both use the shared Secondary Button appearance, with localized accessible names/tooltips and no visible caption.
- Advanced editing remains present without declared bulk/grid capability and is disabled. BulkEditing/EditingBusy also disable it. Declared bulk callbacks and native grid links remain unchanged; menu contents follow the declared capabilities.
- ActionMenu and MultiSelectBox gain optional Icon and nullable TriggerVariant. Null retains the default presentation. Explicit variants reuse Button styles on native disclosure triggers; no interactive Button is nested inside summary. Gallery previews exercise both capabilities.
- Shared dropdown items explicitly align flex content at the start. Ordinary Button centers content; its inherited justify-content had centered composed menu entries despite text-align:start. The shared fix also applies to row/link menus.
- Fixed two build-blocking fixture issues observed during verification: nullable attribute dictionary typing in PresentationChecks, and aliased Razor component tags in NavigationSurfaceSample (replaced only with fully qualified names). Existing presentation/selection behavior is preserved.

## Verification

- Final Gallery Release build with TreatWarningsAsErrors: zero warnings/errors.
- Full Blazor solution Release build in an isolated output directory: zero warnings/errors; the user's active Gallery was not stopped or overwritten.
- Full component regressions: 362/362 passed, including capability/state combinations, real Button bulk callbacks, native grid links, icon/variant markup, empty/progressive count behavior and shared CSS cascade.
- Culture bridge: 12/12 passed. Catalog integrity: 3,036 checks passed; all 155 Gallery and 121 Framework keys retain the three languages.
- Actual final Gallery HTTP output confirmed Chinese 1-2 / 共2, no prefixed ranges, 数量 and both requested icon names. Three-language HTTP regression: 43 checks passed.
- Whitespace validation passed. No Computer Use test, dependency change, Colligere edit, commit or push was performed for this follow-up.

Evidence: artifacts/table-toolbar-solution-build.log, table-toolbar-gallery-build.log, table-toolbar-tests.log, table-toolbar-bridge.log, table-toolbar-http.log and table-toolbar-culture-http.log. An initial ordinary solution build was blocked by the running Gallery's locked output and a fixture nullability error; isolated final builds succeeded. Tests using an isolated executable directory initially failed existing fixed-depth source lookup; final tests used their normal directory. Old UI assertions were updated without weakening row-action or sorting semantics.

## Manual acceptance

1. Restart Gallery; check the record example and API tables show unprefixed ranges and 数量 in both pagers, including empty and two-record results.
2. Check the eye and pen controls have matching Secondary appearance, localized hover names and no visible trigger captions.
3. Without editing capability, confirm the pen remains visible and cannot open. In the bulk-edit example, open it, verify menu items align at the start, enter editing and confirm it disables until editing ends.
4. Exercise display selection/order, paging, table/cards and native row menus. Check pointer and keyboard opening, disabled items, links and focus restoration.

Visual/interactive acceptance remains user-run; automated evidence covers SSR output, component events and source/style contracts.
