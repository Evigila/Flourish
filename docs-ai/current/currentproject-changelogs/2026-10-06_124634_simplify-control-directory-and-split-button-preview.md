# Simplify control directories and SplitButton preview

Local timestamp: 2026-10-06 12:46:34, America/Sao_Paulo.

## Implementation and cause

The shared Controls page displayed a directory introduction and repeated usage-kind labels in each UniformGridButton. These details belong to each component's guide rather than its navigation tile. The introduction, tile Text binding and unused directory UseLabel helper are removed across all control category directories. Tile names, icons, links, accessible names and purpose tooltips remain. ComponentGuide still displays the usage classification, scenario and guidance in its introduction.

SplitButtonSample previously rendered the complete-login navigation button outside its Preview condition, so both the control preview and scenario displayed it. That existing button and its menu now render only inside the non-preview scenario branch, alongside the scenario options and explanation. The control preview retains its ordinary, selected and disabled examples. Scenario navigation and the externally controlled export example retain their existing behavior.

## Verification

Gallery Release builds successfully to artifacts/gallery-catalog-verification with zero warnings and zero errors. The changed-file whitespace check passes. Source inspection confirms the catalog sends Preview=true only to the control example and the details guide retains its classification guidance. Existing component behavior and catalog regressions do not directly verify these Gallery-only branches; no implementation-mirroring test was added for this low-impact composition change.

## Manual acceptance

- Restart Gallery and open Buttons and menus: there is no explanatory paragraph above the UniformGrid and no usage-kind caption in its tiles.
- Check another control category and open a component detail: the directory stays concise while the detail introduction retains its classification and guidance.
- Open SplitButton: the control example shows the three basic states without a complete-login entry; the scenario still shows the login button and native menu links.
- Check scenario width/disabled options and export actions, including language switching.

No Computer Use, public API/dependency change, running-process termination, commit or push occurred. Existing uncommitted refactor and prior layout changes were preserved.