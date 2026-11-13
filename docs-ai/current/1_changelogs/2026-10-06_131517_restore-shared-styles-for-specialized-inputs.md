# Restore shared styles for specialized inputs

Local timestamp: 2026-10-06 13:15:17, America/Sao_Paulo.

## Cause and implementation

MaskedInput and StandaloneMaskedInput rounded only their outer span; their actual native inputs lacked the production f-input class. SearchAutocomplete likewise supplied input width but omitted shared input styling. These controls therefore displayed native browser height and square input borders rather than the standard Design input geometry.

All three actual inputs now use f-input. MaskedInput retains InputBase's computed CssClass, masked-input-control and field semantics; StandaloneMaskedInput retains its standalone binding and native attributes; SearchAutocomplete retains its combobox and suggestion behavior. In Design, shared styling supplies the configurable control-height token (48px by default), 12px radius, input padding/font, validation border and focus indication.

Both mask display layers use 16px default inline padding to match the standard input. EditingGrid's more specific rules retain the compact spreadsheet height, square cell geometry, border removal, inherited font, cell focus ownership and 10px mask padding. Mask transparency, overlay display and native Framework-only behavior are unchanged. No duplicated input skin or public API was added.

## Verification

Gallery Release builds to artifacts/input-style-verification with zero warnings and zero errors. Existing Blazor regressions pass 351/351, including mask normalization, bound/standalone transport, input migration, dropdown and interaction checks. Evidence: artifacts/input-style-regressions.log. Source inspection confirms the shared selectors do not override EditingGrid's more specific input/cell rules. These checks do not certify physical browser geometry or caret alignment.

## Manual acceptance

Restart Gallery and inspect Primitives.MaskedInput, Primitives.StandaloneMaskedInput and Primitives.SearchAutocomplete in light/dark themes and at narrow widths. Compare their rounded borders and height with TextBox/StandaloneTextBox. Check focus, disabled examples, validation, mask typing/pasting/deleting/caret movement and autocomplete filtering, keyboard selection and no-result behavior. Inspect masked editors inside EditingGrid to confirm compact cell dimensions and text/caret alignment remain appropriate.

No Computer Use, dependency/API change, running-process termination, package publication, Git commit or push occurred. Existing source changes and append-only history were preserved.