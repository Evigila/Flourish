# Share Display and complete production scenes

Local timestamp: 2026-10-06 04:32:15, America/Sao_Paulo.

## Request and implementation

The consumer audit identified genuine Flourish components used in the wrong scene and another embedded Display renderer. The library now has one production DisplayOptions/DisplayOption/DisplayOptionsChange renderer/controller shared by DataTable and LineChart. Old private table display handlers and dead input drag behavior are deleted. Chart visibility/order is keyed library state, not a consumer controller.

NavigationChoices provides standard native GET alternatives without pseudo-tabs; ValidationMessages shares Field's error renderer and supports real hidden/cross-field EditContext feedback. Usage metadata, parameter guidance, Gallery samples and AGENTS.md enforce production responsibilities and reject aliases/host cloning. Actual child-event tests replace old private-method adapters.

## Verification and handoff

Final Release preparation passes Core 367, Blazor 349, bridge 12, 90-component metadata coverage, configured Node/CSS gates, six package candidates and 99 isolated NuGet-consumer checks. Evidence is artifacts/semantic-ui-authority-release-final-2026-10-06.log; the first failed log is retained. Actual source/package/consumer Framework DLL hashes agree.

See [the report and manual checklist](../bugfix-reports/2026-10-06_shared-display-and-production-scenes.md). The user's existing consumer instance remains unchanged. No database, dependency version, public publication, deployment, commit or push changed. Retired tracked code remains recoverable through Git; physical/authenticated browser acceptance is manual.
