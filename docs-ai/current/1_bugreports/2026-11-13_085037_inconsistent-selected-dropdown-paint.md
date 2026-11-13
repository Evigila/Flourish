# Inconsistent selected dropdown paint

Recorded: 2026-11-13 08:50:37 -03:00 (America/Sao_Paulo)

Status: implemented in local preview **1.4.8-preview.13**; browser acceptance is manual.

## Symptom and cause

The language picker displayed a checkmark without the same selected-row boundary observed in a normal time-zone selector. LanguagePicker composes SelectBox with the optional enhanced popup; its generated choices expose aria-checked. The old Design rule targeted is-selected and explicitly kept the row transparent. Native option:checked also had no explicit shared selected-row paint, leaving browser-owned selection/focus appearance outside a framework contract. The observed native border therefore was not a reliable shared selected-state rule.

## Change

The optional Design now paints both native and enhanced selected choices with semantic text/surface roles in reverse and a one-pixel inset boundary. Checked checkbox options and selected ReferenceDropdown rows use the same paint instead of their earlier independent primary fill. Hover, press, focus and disabled selection retain the boundary; unselected choices keep their previous feedback. Compact checkmarks inherit the selected foreground and retain no separate box or fill. System high contrast uses Highlight/HighlightText and a visible outline. No host-specific CSS, JavaScript controller, input protocol or new public component is involved.

## Evidence and limits

The focused cascade suite passed **40/40**, including selected and unselected native/enhanced/reference/multiple choices, pointer/keyboard/disabled states, compact marker geometry and system high contrast. The isolated producer Release build passed with zero warnings/errors. Six local packages passed identity/dependency/asset checks, and both packaged CSS bundles match the compiled bytes. The local feed contains provenance.json and css-evidence.json; the external closure matches preview.12.

These are source/cascade and package checks, not browser screenshots or native popup rendering tests. No Computer Use, production deployment or public publication occurred.

## Manual acceptance

1. Open language and ordinary time-zone choices; confirm the current row has the same inverse fill, boundary and trailing check.
2. Hover, press and keyboard-focus selected/unselected choices, then change selection; confirm selection remains recognizable and the row does not resize.
3. Check ReferenceDropdown and multiple choices, including disabled entries, in light, dark and Windows high-contrast modes.