# Repair masked focus, autocomplete candidates and independent dropdown widths

## Request and causes

The user reported an unexplained preview-colored rectangle on focus in both masked inputs, an autocomplete scenario with no apparent candidates, and reference/multi-selection fields that resized to the selected caption.

The masked input's native editing text is transparent in Design, but ordinary focus behavior selected all text, leaving a solid selection background over the visible mask layer. SearchAutocomplete already contained fake items but only opens for a nonempty query; the old example began empty without a useful typing cue. Selection triggers had intrinsic width derived from their caption, while absolute panels inherited both left/right edges and therefore the same shrinking width.

## Changes

- Exclude masked fields from both focus-selection handlers. Preserve the native clicked caret, manual selection and Ctrl+A; Design's intentional selected text uses Text on the existing approved preview paint. Ordinary field select-on-focus remains unchanged.
- Use enhanced oninput binding in both mask components. Format the DOM in capture phase before Blazor captures its value. A rapid-typing recheck exposed an existing stale-render overwrite; matching native formatted values and binding bookkeeping now preserves the complete entry. Mask normalization, external callbacks and validation remain unchanged.
- Autocomplete scenario starts with "活动" and two matching fake venues, plus reading/meeting alternatives. It demonstrates actual pointer/keyboard selection and selected name/ID, clears stale selection on input/data-state changes and retains no-match/no-data/clear cases. The generic empty-query contract is unchanged.
- ReferenceDropdown and MultiSelectDropdown use a stable 240px trigger constrained by their parent. Popups size independently from all option labels and search/create content, capped at 480px. An invisible, aria-hidden, noninteractive width reservation retains every label while filtering without adding a blank grid row. Multi-selection includes the checkbox footprint. Wider panels start at the trigger's left edge; at <=760px they are capped to the field and long text wraps. Selected caption truncation preserves full text and a title hint. Host CSS variables --f-selection-trigger-width and --f-selection-panel-max-width remain configurable.
- Add deliberately short/long fake options to both guides. Actual compiled scenario code remains the source of displayed examples. Public component parameter/default counts are unchanged. No dependency or business-specific resource was added.

## Verification

- Gallery/test builds: zero warnings and errors.
- .NET: 99/99, including a new real-filter regression that retains width labels without exposing hidden selectable candidates or bypassing HTML encoding.
- Node: 27 controls DOM checks, 8 RowActionMenu checks and 9 palette/style audits passed. New tests cover both focus-selection paths, preserved ordinary behavior, capture-phase formatting and mid-text caret normalization.
- Isolated headless Edge: both masked inputs keep a collapsed caret on click, support Ctrl+A and preserve zero-delay replacement strings as CD-5678 / 135-790. Autocomplete shows two initial matches, one reading match, pointer and ArrowDown/Enter selection, missing-result and clear states. Selection triggers remain 240px; multi/reference popup widths are 353px/310px in these examples, unchanged after short/long selections and filtering. At 320px they fit their field at 190px and wrap. The main run observed no browser exceptions.
- HTTP: all 78 guides retain the five standard sections and default-value column; generated public CSS assets remain available.
- Architecture remains 896 maintained files / 132 directories; every listed path exists. Git whitespace check passed.

## Manual acceptance and limits

See blazor-manual-tests.md for editing/paste/selection, keyboard/blur, candidate filtering, short/long/new options, width overrides, narrow windows, zoom, both themes and Framework-only presentation. Automated pixel/interaction evidence is from installed Edge; other supported browsers and assistive technologies need manual acceptance. Temporary verification hosts used independent ports and did not stop the user's VS host. No Computer Use or Git commit was performed. Prior dirty changes were preserved.