# Field input and operation tracks

Date: 2026-10-07, America/Sao_Paulo.

Colligere's required enumeration Field contained both an input and operation buttons. The standard space-between label placed its required marker over the entire row's right edge, visually above the action rather than the input. Putting Field inside InlineActions alone would lose the input's remaining-width rule and control alignment.

Field now has optional Actions. ChildContent receives the input context in the flexible first column; ordinary actions use an independent bounded natural-width column at the input row. Validation uses the existing renderer anchored to the input, without leaking required/error context into operations. At 760px the action track moves below. Fields without Actions retain their ordinary hierarchy. This is one generic capability on the existing control, not a new wrapper or host skin.

Production usage guidance, Field/InlineActions Gallery examples, parameter descriptions and three-language resources are updated. The [API guide](../component-api-organization.md) records the contract. Five new executable checks cover ordinary fields, context isolation, bound events, dynamic validation and subscription disposal; the previous whole-row fixture now uses Actions.

Verification: the full Blazor solution Release build passed with zero warnings/errors; library checks passed 408/408, Gallery checks passed 7,266 plus 124 actual post-event translation/state checks, and the focused toolbar CSS test passed 3/3. The six `1.1.4-preview.fields.1` packages passed the existing dependency/required-asset verifier and were restored by Colligere as packages, without source references. Stable VersionPrefix remains 1.1.3; the candidate was generated with a task-scoped build version. No public publication, tag, commit or WPF modification was performed.

Manual acceptance remains required: inspect required markers, long/wrapped labels and actions at wide/narrow widths; submit empty and valid input; check validation focus/clearing and save/cancel/disabled behavior. Restart the consuming application normally before acceptance. No Computer Use or live service test was performed.
