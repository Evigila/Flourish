# Masked selection paint and shrinking option surfaces

## Symptoms

Clicking MaskedInput or StandaloneMaskedInput displayed a flat light-preview rectangle until typing or collapsing the selection. ReferenceDropdown and MultiSelectDropdown shrank their field and popup to a short selected label. Autocomplete's scenario appeared to have no candidate data.

## Cause and evidence

Both ordinary focus handlers called select() on populated masked fields. Design intentionally overlays text above a transparent native editing value, so a selected range painted a background without visible selected native text. A further zero-delay typing check exposed a mismatch between client mask formatting and captured Blazor input values, allowing an old formatted value to replace newer DOM text. Selection surfaces lacked a stable trigger width; absolute popup left/right constraints tied them to it. Autocomplete had data but an empty initial query, while its open contract requires a nonempty query.

## Mitigation

Keep masked native caret placement on focus, retain intentional selection with readable foreground, and synchronize capture-phase formatting with enhanced input binding. Keep selection-trigger width independent of current selection and popup width independent of that trigger. Reserve full supplied-label widths in an inaccessible/noninteractive zero-height grid item so filtering cannot discard the longest-label width; cap narrow popups and wrap text. Demonstrate actual autocomplete candidates with an initial fake query, working callbacks and honest empty states.

## Regression checks

99 .NET checks passed, including real reference filtering, hidden measurement semantics and encoding. 27 controls DOM checks cover both selection paths and formatting/caret order; 9 style audits and 8 menu checks passed. Automated Edge verified masked caret/Ctrl+A/rapid replacement, initial/filtered autocomplete candidates and pointer/keyboard selection, fixed 240px fields, independent 353px/310px panels, filtering stability and 320px containment. All 78 guide HTTP routes retain their normal contract.

## Limitations and manual checks

Manual checks must include drag selection, pasting, read-only/disabled masks, normal field focus behavior, long/new labels, host width overrides, no-data search, keyboard/blur, zoom, other browser engines and Framework-only styling. Width is constrained by available field space in narrow windows; longest text wraps there. This repair adds no remote queries, business operations, dependency or component parameter. Existing history is unchanged.