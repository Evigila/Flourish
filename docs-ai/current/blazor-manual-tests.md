# Blazor manual acceptance

This checklist applies to the four-project split and current Colligere consumption. Source inspection, console SSR and mocked DOM tests do not establish browser rendering or all user interactions. Record concrete run results in an append-only change record; unchecked items remain acceptance work.

## Run the two consumer modes

From the Flourish repository root:

```powershell
dotnet run --project src/Gallery.Blazor
dotnet run --project tests/Flourish.Blazor.Native
```

Run one host per terminal and open each printed local URL. Gallery explicitly references Framework + Design and registers both. The native fixture references Framework only, registers AddFlourish and loads only framework.css. Neither mode should need Gallery-specific styles copied into another consumer.

Reproducible non-browser checks, when source changes justify rerunning them:

```powershell
dotnet run --project tests/Flourish.Blazor.Test --no-restore
node --test tests/Flourish.Blazor.Test/tests.js
node tests/Flourish.Blazor.Test/controls-dom.mjs
node tests/Flourish.Blazor.Test/interaction-origin-dom.mjs
node --test tests/Flourish.Blazor.Test/measurement-lifecycle.test.mjs
./build/Test-CssBundle.ps1
./build/Test-CssAssets.ps1
```

The console suite covers data/DI/SSR contracts; it does not execute HtmlRenderer post-render JS. Browser clipboard, navigation, resizing and focus need the checks below. Use the Gallery's labeled demonstration data for create/edit/delete checks. Colligere writes require an authorized test workspace and remain business acceptance, not proof supplied by a source migration.

## Framework-only acceptance

- Confirm no Design stylesheet requests and no IAppearanceService registration. Open native controls, form, table, menu and dialogs without missing-service exceptions or failed Framework assets.
- Type/paste/select into a masked input: the real native value and caret remain visible without a decorative overlay. With Design loaded, repeat and check overlay/caret alignment and selection.
- Check close, navigation and pager SVGs: they paint at a useful relative size rather than appearing as empty or 300x150 elements. Auxiliary labels/captions stay visually hidden and available to assistive technology.
- Toggle switches with pointer and Space; check visible state and aria-checked. Open details by keyboard; the browser disclosure marker remains observable without Design.
- Focus a notice trigger and move focus away: content appears only while triggered and does not permanently cover the form. Manually check hover as well. Popup text remains readable over page content.
- Place identity facts in a narrow container: named container queries collapse the columns. Verify long labels/values do not create document overflow.
- Compose NavigationSurface/ContentSurface with Primitives in a separate native consumer if they are not included in the fixture page. Check actual stage scrolling and return-to-top behavior; passing SSR alone is not this browser check.

## Design and shell/layout acceptance

- Visit Gallery `/`, `/controls`, `/forms`, `/records`, a record detail, `/appearance` and `/patterns`. Check 1440/980/760/520/320px windows and 200% zoom. The document has one page-content scroll track; wide tables scroll within their own region.
- Inspect ApplicationShell and extracted NavigationSurface/ContentSurface. Header/rails remain visible; no phantom rail is reserved when a host omits secondary navigation. Check real route links, back/forward and the most-specific active secondary item.
- Supply host-filtered NavigationGroups and an organization title at runtime. Startup entries must not leak into the override. Authorization enforcement remains in the host even when a menu entry is absent.
- Tab from skip link through navigation to page actions. H1 receives navigation focus without a field rectangle; inputs/buttons/links retain visible keyboard focus.
- On narrow ApplicationShell, open/close expanded navigation, cycle Tab/Shift+Tab, press Escape, click outside and resize while open. Background content is inert only while expanded; focus returns appropriately.
- Scroll beyond 96px, then below 24px. Heading compaction uses hysteresis, wraps without clipping and resets for a new document. Check the back-to-top control after a long scroll.
- Check primary-only overview, brand and service slots, odd/even fact counts and narrow stacked identity facts. Compare source-derived 48px controls, large action cells, full-stage headings and list/search/view/pager geometry with the accepted Colligere surface.
- Change ThemeScope Primary/Accent and Light/Dark/System around both shell families. Extracted role variables must follow the public scope, including popup/background/focus and color-scheme. Host explicit inline theme roles should retain precedence. Nested scopes must remain independent.

## Inputs, validation and dirty forms

- Submit empty required fields and invalid email in the demonstration form. Errors identify their actual inputs and first-invalid focus lands correctly; an invalid submit must not create a demonstration record.
- Edit text, number, checkbox, select and multiline fields under a decimal-comma locale. Native number/date values remain browser-valid; displayed formatting does not corrupt raw edit values.
- Check labels, required markers, EditContext errors and explicit host errors; aria-invalid/describedby relationships must resolve. Native fallback select and enhanced base-select must remain aligned and operable.
- First focus selects existing ordinary single-line text; later pointer interaction edits the caret. Search/password/multiline fields retain their appropriate behavior.
- From `/forms`, change a field and select `/controls`. Continue editing preserves the draft; confirmed discard follows `/controls`, without a second prompt or fallback to `/records`. Repeat after canceling an earlier destination, using back/forward, query changes and reload/external navigation.
- Repeat submit/close rapidly while Busy. A blocked close or validation failure keeps values and does not duplicate writes.
- Check extracted reference search/create and multiple selection with an unlisted selection, empty choices, cancellation and failed host create callback. Focus, keyboard operation and selected values remain coherent; the host owns any remote create operation.

## Local lists and actions

- Inspect actual loaded count and 20-item default paging. Top/bottom controls agree; invalid page input clamps and deletion on a final page does not create a phantom empty page.
- Search all columns and one column, including hidden searchable data. Changing the search field retains text and resets the page. A loaded remote page must not be labeled as the complete remote dataset.
- Check descending/ascending/default restoration on the Components DataTable. Numeric/date values use typed keys, localized text uses configured culture, equal keys stay stable and null stays last. Check the extracted table's own sort/default-preference contract separately.
- Switch list/cards; corresponding fields, visibility and current page agree. Hide/show columns, retain a usable visible column, and test reorder where offered by Primitives.DataTable.
- Drag column width, then use arrows/Shift/Home/Escape as supported by that table's resizer. Natural width and manual width must survive rerender/view changes according to the component contract.
- Horizontally scroll and open a row menu near every viewport edge. Sticky action cells remain readable; the menu avoids clipping. Arrow/Home/End skip disabled items, Escape restores focus and outside interaction closes it.
- Explicit default-open and double-click actions agree. Do not infer an action for a row that has none. Canceling deletion keeps a record; confirming changes only the demonstration/authorized host data.
- Empty/loading/error states are distinct actual states; they do not invent counts or turn an error into an empty success.

## EditingGrid and Colligere adapters

- In an authorized test environment, inspect ProductSpreadsheet and RegistrySpreadsheet. Columns/rows map in the intended order, record keys remain stable and reload/hidden state does not duplicate rows or interaction handlers.
- Exercise Text, Decimal, Date, Multiline, Select, Masked and Link cells. Display values and raw editors match their host mappings; masks/choices and input constraints are retained. Link cells remain navigable where intended.
- Use pointer and keyboard selection, start/end/cancel editing, copy/paste a rectangular region and undo/redo. Check escaped/multiline clipboard content and incomplete/oversized input. Host parsing/validation must report errors without silently shifting columns.
- Check both primary and secondary errors: invalid cells/editors announce them, description IDs exist and error text is encoded. Per-cell read-only/disabled and whole-grid EditDisabled prevent edits while preserving readable records.
- Change permissions or busy state through the host: callbacks must not bypass its access checks. Save/cursor-load/history behavior remains the host's existing behavior; the UI renderer must not create a second draft pool or remote transaction.
- Resize columns after loading more rows and changing MeasurementRevision. Open/close or temporarily hide the grid, then navigate away/back. Widths, focus and edit state follow the intended lifetime without stale DotNet callbacks.

## Overlays, lifecycle and compatibility

- Distinguish Components Dialog/BottomSheet from Primitives BottomSheet. Check their own close API: visible title, internal scroll, Tab containment, Escape and focus return; Busy refusal and Components CanClose veto preserve the draft.
- Open an overlay from a row menu and then close it. Focus returns to a useful connected invoker, including after rerender or removal of the originating row.
- Check native Popover/Dialog feature support explicitly. Enhanced Components overlays contain fallback logic; extracted RowActionMenu directly uses the Popover API and must not be described as having the same older-browser fallback.
- Open a second circuit/tab: appearance and in-memory demonstration changes must not become another user's state. Reload/new circuit follows documented in-memory reset behavior; persistence is not provided by the default preference dictionary.
- Clear console/network errors and navigate repeatedly. There must be no failed `_content/Arkheide.Flourish.Blazor.Framework` or Design assets, disposed callbacks or accumulating active interactions.
- Verify Windows DPI and browser zoom at 100/125/150/200%, forced colors, reduced motion, screen-reader labels/headings/grid errors and keyboard-only use. Test Firefox/native select fallback and additional supported browsers.
- Manually inspect tooltip hover delay, pointer movement from trigger to tooltip, and animation timing/intermediate frames. Current browser automation supports click, keyboard and drag; it does not provide independent hover or a held animation-frame capture, so these are not implied by automated sampling.

## Packaging and third-party consumer

- Pack each intended project and inspect its dependency/asset paths. Framework has no Design dependency; Framework contains behavior JS/functional CSS, while skin/foundation assets belong to Design. Configured packages do not prove public-feed publication.
- Consume Framework alone from a local package feed in a separate .NET 10 Blazor host; then add Design explicitly. Verify both modes without sibling Colligere paths, internal API access or copied Gallery CSS.
- Change public render slots, labels/culture, appearance and table preferences. Qualify duplicate component names when importing Components and Primitives together. Localization of extracted fixed Portuguese labels remains a separate acceptance concern.

## Universal library and loading repair acceptance

- Stop the existing Visual Studio Gallery process, rebuild, then start it again and open http://localhost:5188/. The previous process does not receive rebuilt Program/configuration automatically.
- In a published host, request the versioned Framework/Design CSS with gzip or br and verify Content-Encoding, a smaller transfer and 304 for a matching ETag. Stable paths and fingerprint paths must both negotiate compression.
- In browser Network with cache disabled, confirm Gallery requests exactly two library CSS files, neither contains @import, and no consumer page stylesheet or removed colligere entry is requested. Framework-only must request one CSS file.
- Open and refresh the Gallery root. Console should retain useful startup/warning/error output while ordinary ASP.NET static request Information events stay filtered. Record cold/warm elapsed browser time; HTTP request measurements are not browser interactivity timing.
- In /patterns open a sheet for the first time, edit its local form, close and reopen it; check retained values, focus origin, Tab/Escape and Busy close policy. An unused closed sheet should not initialize its body controls or browser module.
- Change table text/data, paging, list/cards, visibility/order, fonts and viewport. Automatic widths must update; manually resized widths must survive. Home must resume automatic sizing. Navigate away repeatedly and check listeners/probes do not accumulate.
- In Colligere verify public/account/workspace pages load host app.css and scoped styles after the two library layers. Check the blue account theme, workspace role overrides, public sticky header and inventory/order form modifiers. These business pages remain host-owned.
- Repeat login, registration invitation, Standard directory entry and guest sign-out in authorized test data. The rate-limit policy identifiers remain unchanged; UI class names must never replace security policy names.

## Development dynamic-module compression regression

- Rebuild/restart Gallery in Visual Studio, then Ctrl+F5 the document to create a fresh circuit and module record. Visit /, /records, /controls and /patterns, opening menus, inputs and dialogs. No JSException or ERR_CONTENT_DECODING_FAILED should appear.
- Inspect the controls.js fingerprint request and other Framework modules. Under Development, a browser Accept-Encoding list should receive correctly decodable gzip (or identity when only br is offered); build Brotli endpoints must not be present.
- Separately publish and confirm stable/fingerprint CSS still negotiate gzip and Brotli with correct decoded content and 304 responses. Do not infer successful module decoding from HTTP 200 alone.
