# Native split menu visibility

## Symptom and cause

The new SplitButton native details mode rendered its menu in static SSR, but the shared menu panel rules still required :popover-open or data-f-open. Native details supplies neither, so the generic .f-menu-panel and closed-popover selectors would keep the panel at display:none even when its details owner was open. This was identified and corrected before completing the integration.

## Correction and evidence

Framework adds .f-split-button > .f-split-button-menu[open] > .f-menu-panel with display:block. Its specificity is greater than the existing closed-interactive-panel selector; its scope cannot expose unrelated old popovers. Closed details still hides its panel. No parallel JavaScript controller or broad data-f-open rule was introduced.

SplitButtonChecks expands the real Framework and optional Design local CSS import cascades and verifies native open/closed plus existing interactive closed/open outcomes, including specificity and source order. The contract analyzer rejects unrecognized relevant selectors rather than treating them as a pass. It is not a browser layout simulation. The complete preparation passes 250 Blazor checks and CSS bundle/SDK integration 21/194; served bundled CSS additionally contains the corrected selector.

## Limits and regression checks

Native details intentionally retains disclosure keyboard semantics and normal Tab traversal. It does not promise automatic Escape/outside-click close, arrow-key menu navigation or viewport flipping. Confirm visually that the popup is visible only while expanded, its three login links are reachable, disabled/busy SplitButton exposes no usable links and opening the triangle cannot submit the host form. Existing interactive ActionMenu and RowActionMenu behaviors remain separately tested.
