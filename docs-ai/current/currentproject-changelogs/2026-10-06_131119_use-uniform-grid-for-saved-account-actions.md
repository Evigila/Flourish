# Use UniformGrid for saved account actions

Local timestamp: 2026-10-06 13:11:19, America/Sao_Paulo.

## Implementation

The saved-account page previously used FormLayout with Card wrappers and ordinary choose/remove buttons. Each account now uses the production UniformGrid with two UniformGridButton actions. The Elevated account tile presents its name and email and invokes the existing choose callback; the Danger tile invokes the existing remove callback. Wide layouts show two columns, and NarrowColumns=1 stacks actions on narrow screens.

Stable account keys, email-qualified accessible action names and the unavailable-account warning are retained. Busy state disables both actions. An unavailable account disables selection while remaining removable. There are no Card wrappers or nested interactive elements in the account entries. Fixture persistence, validation, empty states and list-level actions are unchanged.

This supersedes the previous access-layout change record's decision to retain account Card presentation; that record remains historical evidence.

## Verification and manual acceptance

Gallery Release builds to artifacts/account-grid-verification with zero warnings and zero errors. The changed-file whitespace check passes. Existing handlers and the state model were inspected; this presentation-only change adds no duplicated source assertion test. Browser geometry remains subject to manual acceptance.

Restart Gallery and check the saved-account page in light/dark themes and at wide/narrow widths. Select an available account and verify the existing navigation/prefill behavior. Verify the unavailable tile cannot select, its remove tile still works, removing/clearing all entries displays the existing empty state, and restoring examples repopulates the grids.

No Computer Use, dependency/API change, state-model change, commit or push occurred. Earlier source edits and append-only documentation were preserved.