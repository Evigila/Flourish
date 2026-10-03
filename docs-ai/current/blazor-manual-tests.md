# Blazor manual acceptance

Run `dotnet run --project src/Gallery.Blazor` from the repository. Open the printed local URL. Tests below complement builds, pure-data/DI/SSR checks and HTTP/static-asset checks; browser visual/focus checks must be performed manually, never claimed from SSR alone.

## Workspace shell and layout

- Inspect wide, 980px, 760px, 520px and 320px windows and 200% zoom. The document has one content scroll track; header/navigation remain visible, forms stack, tables scroll inside their own region and pages do not overflow horizontally.
- Navigate between both primary groups and all secondary entries. Check current-state indication, full labels, real URLs, back/forward and active-group selection for record-detail routes.
- Tab from skip link through shell to page actions. Primary navigation tips appear immediately on focus/about 150ms on hover, can be hovered, close on Escape and never overlap one another.
- In a narrow window, open/close the expanded navigation, cycle Tab/Shift+Tab, press Escape, click outside and resize while open. Background content must stay inert only while expanded; focus returns to the menu trigger.
- Scroll a long page beyond 96px then back below 24px. Title compacts/expands once per threshold, wraps without clipping descenders, and resets on navigation. Enable reduced motion in the browser/OS.

## Inputs, validation and dirty forms

- Create a record with empty required fields and an invalid email. Save must not create data; visible errors identify their associated inputs and focus lands on the first invalid field. Required markers and labels remain ordinary 17px/400 except the semantic required marker.
- Edit all fields, checkbox, select and multiline input. Tab and paste work; first focus selects existing single-line text, subsequent pointer clicks edit the caret. Search/password/multiline fields are excluded from automatic selection.
- Save a valid record and open its detail. The new values appear in the current circuit. Navigate away from a dirty form: Continue editing keeps values; Discard leaves for the exact intended destination. Reload/external navigation uses the native browser warning. Same-route edit query changes must not retain an old discard target.
- Check busy/disabled controls and action cells. Rapid repeated submit/close must not duplicate writes or bypass busy state.

## Lists and actions

- Inspect 33 initial records and 20 per page. Top/bottom pagers stay synchronized; invalid page input clamps, and deletion on the last page does not create an empty phantom page.
- Search in all columns and one selected column, including a hidden searchable column. Change search column without losing text; filtering starts at page 1.
- Cycle each sort Default -> Descending -> Ascending -> Default. Numeric/date values sort by type, localized text by configured culture, equal values remain stable and null values stay last.
- Switch List/Cards; same fields/data/visibility and page state remain. Hide/show fields, retain at least one visible column, then resize by pointer and keyboard (arrow/Shift/Home/Escape). Full cell values remain accessible through title/DOM even where approved list/card ellipsis applies.
- Horizontally scroll the list and open a row menu near each viewport edge. The action column remains pinned; menu is outside clipping, arrows/Home/End skip disabled items, Escape returns focus, outside click/focus leaves close it, and selecting an action closes it. Double-click opening and the first Open action agree.
- Delete cancel keeps the record; confirmed deletion removes it. Empty/loading/error states are actual state messages, not fabricated metrics.

## Overlay, appearance and isolation

- Open both centered Dialog and BottomSheet. Visible title/close action, internal scrolling, Tab containment, Escape and focus return work. Busy and a false CanClose veto refuse close. Multiple instances do not exchange open state.
- Switch light/dark/system and business/account/control colors. Check popup, input, focus, selected state and notices, including very light colors, black/white and #777777. Browser System reacts to OS changes.
- Test complete long Chinese/Latin names and identifiers. Titles and identity names wrap; glyphs inherit foreground; no extra font/icon downloads occur.
- Open a second tab/circuit: theme and demonstration edits from the first tab must not become another user's state. A reload/new circuit resets in-memory demonstration data as documented.
- Clear the server/JS console and visit every route, then navigate repeatedly to forms/lists and back. Confirm no failed _content assets, disposed-DotNet calls or accumulating handlers. Native Popover API fallback should be manually checked in a browser without popover support.

## Third-party consumer

- Create a separate .NET 10 Blazor Web App using only the library's ProjectReference/packed package, AddFlourish and ApplicationLayout. It must render its own title/navigation/page without copying Gallery CSS or any Colligere services.
- Override render slots, TableText/Culture and appearance through public API; verify the implementation does not require access to internal types.