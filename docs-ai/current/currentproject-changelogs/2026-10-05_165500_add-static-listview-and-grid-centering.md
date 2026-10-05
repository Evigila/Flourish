# Static ListView and optional tile centering

The user requested a read-only ListView with Flourish table appearance for Colligere's license comparison, without DataTable's dynamic operations. ListView now renders semantic static tables using existing column metadata, formatting and surface styles. No new NuGet, external dependency or separate table controller is introduced.

## Implementation boundaries

- Abstract adds TableCellContext<TItem>(Item, Column, Text) beside TableColumn<TItem>; consumers retain their business data and read-only display templates. Existing TableColumn constructors and dynamic DataTable behavior are unchanged.
- Framework adds Components.ListView<TItem>. Items and Columns are editor-required. Optional ItemKey retains row identity; RowHeaderKey selects an existing column for th scope=row; CellTemplate receives the item, shared column and culture-formatted text. Label, Caption, EmptyMessage, Culture, Class and root AdditionalAttributes complete the static contract.
- ListView renders all items and columns in supplied order. TableColumn's dynamic Sortable/Searchable/CanHide metadata has no effect. There are no search, sort, paging, selection, resize/reorder, cards, loading/query callbacks, preference storage, row opening or action APIs. Empty lists preserve headers and an accessible status. Invalid inputs, ambiguous column keys and unknown row-header keys fail before rendering.
- Shared TableData.Display handles typed dates, numbers, null and custom formatting. TextComponentBase provides scoped defaults/format culture, explicit override precedence, renderer dispatch and subscription cleanup. Default headers/cells/captions are HTML encoded; custom templates retain their normal Blazor content responsibility.
- The control uses f-data, f-data-scroll, f-data-table and f-data-cell, with native caption/header semantics and a labelled keyboard-accessible scroll region. It does not inject IJSRuntime or mark the table for data.js initialization. Framework-only rendering needs no Design service.
- Design retains the existing table border, radius, palette roles and 76px base row/header geometry. Static text wraps instead of relying on measured clipping/hover tools; row headers use the data surface rather than header paint. The shared last-row border rule now includes row-header th. No independent ListView palette, hover/pressed skin or host CSS is introduced.
- UniformGrid.Centered defaults false and opts into margin-inline:auto without changing cell shapes, dimensions, row/column layout or appearance. This addresses a missing alignment contract without turning ordinary content into a DisplayBoard.
- Gallery registers the new public control, describes the shared/static API and includes an executable 25-row/empty example with long/null/currency values, row headers and accessible boolean symbols. Registered catalog coverage is now 78 components, 577 parameter rows and 272 documented defaults; it is not an inventory of every exported helper.

This task does not implement the earlier two-table API convergence proposal, dynamic SSR directory support, EditingGrid multi-selection or the deferred host carousel. WPF and other desktop projects remain outside this release/task. Existing dirty project renames, retired UI-document deletions and human DocFX content are preserved.

## Verification and package evidence

- Blazor-only Release solution/Gallery build: zero warnings/errors. Components: 150/150 checks, including fourteen ListView regressions and one opt-in centering regression. Culture bridge: 12 checks passed.
- Static-list tests deliberately omit IJSRuntime and cover all 25 rows/input order, shared metadata/formatting, encoded text, optional row headers/templates, captions/empty state, invalid contracts, attributes, scoped culture refresh and disposal. Existing dynamic table tests remain intact.
- All nine configured JavaScript files passed: 40 Node-runner tests plus existing mock suites. Palette/ListView/grid checks: 13/13. CSS bundle checks: 21.
- Abstract, Framework and Design were repacked locally at the still-unpublished 1.1.0 candidate version. All six configured packages passed identity/dependency/assets verification. The meta package still installs Abstract/Framework/Design and Core transitively; activation remains opt-in. No Shared/WPF package was added.
- Four fresh NuGet-only consumers passed 79 checks, including static ListView rows and semantic headers in Framework-only, meta without Design registration, meta with Design and meta with Culture. Evidence: artifacts/package-consumers/7552c58c83694648ba18732442f0061c. Temporary local HTTP processes were stopped by the existing verification script.
- Current SHA256: Abstract D5DE44B22F0D2CC019A49EE857BF593DC468830CAB637C008839F96B094560DF; Framework 44F8D91FD5E9075FE83EAC2D25B2733B8C9D9974C9F866D2C2436397F23E9F9E; Design 862E83C166766CAC8A2B6CD6E67E3801BA21EDD9878F751D6BB96AF74D9B20D2. Colligere's artifacts/package-cache-pricing restores these exact candidates.
- Colligere's full Release build reports zero warnings/errors; focused Web tests pass 118/118, including the actual /pricing/ HTTP response, complete layout/footer and unchanged seven-by-five license facts. Full Core/desktop/SDK-asset or Colligere solution tests were not rerun. Prior unrelated baseline failures are not claimed fixed.

No Computer Use, commit, tag, push, public NuGet upload, deployment or data reset occurred. The candidate version may be locally repacked only while unpublished; existing same-version caches require an explicitly matching isolated restore, not global-cache deletion or replacement of a public version.

## Manual acceptance

1. Open Gallery's ListView example in Light, Dark and System modes. Compare border/header/row appearance with DataTable, confirm every full-example row and the empty state, and inspect long/null/currency values.
2. At desktop and narrow widths, verify complete readable text, horizontal scrolling confined to the list, and keyboard access to the labelled region. Screen readers should announce caption and row/column headers; no sort, pager, action, selection or resize UI should appear.
3. Compare UniformGrid with Centered omitted/false/true while varying shapes and dimensions. Only container alignment should change; existing cell sizing/variants and clickable-cell behavior remain intact.
4. Test Colligere Pricing using its documented fresh cache. Confirm centered sections/price tiles, original comparison data, footer artwork, native registration and retained carousel in both themes. Existing Workspace lists and optional no-Design consumers must remain unaffected.
