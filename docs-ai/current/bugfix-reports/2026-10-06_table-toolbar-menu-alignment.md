# Button-composed record menu entries centered unexpectedly

## Symptoms and cause

Advanced-edit and row menu entries composed from Button centered their content. Design controls.css gives ordinary f-button justify-content:center. The more-specific dropdown.css f-dropdown-panel .f-dropdown-item rule reinstates display:flex over Framework's block menu items, but formerly left justify-content inherited. text-align:start therefore did not determine flex alignment. Native generated ActionMenu items lacked f-button and did not expose the same centering.

## Mitigation and evidence

The shared dropdown item rule now declares justify-content:flex-start and text-align:start. This preserves the one generic menu renderer, including Button links, rather than adding a DataTable-specific skin. DataTableChecks expands Framework/Design imports and verifies the applicable cascade. Capability-state tests exercise real Button events and native links. Full component checks passed 362/362; the final Gallery and isolated Blazor solution built with zero warnings/errors.

Related toolbar changes replace display/advanced-edit captions with accessible Secondary icon disclosures, keep unavailable advanced editing disabled/present and simplify pagination. See the active component API guide and the new record-toolbar change record for current contracts.

## Limits and regression acceptance

No Computer Use test was used. Users should check real pointer/keyboard menu opening, Button and anchor start alignment, disabled rows, hover feedback and restored focus. Explicit host text/capability contracts and Colligere behavior were not changed.
