# Prepare Flourish 1.4.10 stable

Recorded on 2026-11-22 in America/Sao_Paulo (UTC−03:00).

Both hosted v1.4.9 release runs failed while checking keyboard focus in a fixture that opened a platform select before testing the hover-enabled language menu. The new regression exercises the controls in independent browser sessions and checks the actual keyboard opening, focus, selection, change event and closing behavior. The earlier tag remains linked to its original commit. [Flourish 1.4.9 release run](https://github.com/Evigila/Flourish/actions/runs/45064526354)

Hosted Edge reported a visible open panel whose enabled option buttons had computed visibility hidden. Positioning now uses a temporary opacity mask, preserving option visibility and allowing the controller to focus the first or last option immediately.

Version 1.4.10 carries the reviewed Blazor changes and uses the same package verification and NuGet Trusted Publishing process.
