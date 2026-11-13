# Selected-dropdown regression expectations after shared styling

Recorded on 2026-11-13, America/Sao_Paulo (UTC-03:00).

## Symptom and evidence

The full release gate failed during repository cleanup: the initial Node run reported 337 passes and two failures out of 339, with zero skips. Palette lookup could not find the old exact .f-dropdown-panel .f-dropdown-item.is-selected rule. The actual native-picker check expected every option to use the ordinary theme foreground, while the selected option correctly used inverse text.

## Cause

Commit 43c129e had unified the selected row's dark fill, inverse foreground and inset border in shared dropdown rules. The two older tests still required transparent selected paint and ordinary foreground for the selected native option. The focused selection/cascade suite already exercised the intended behavior. DocFX removal did not change the runtime stylesheet; these were stale regression contracts, not evidence that the requested selected styling should be reverted.

## Repair

Updated only tests/Tests.Flourish.Blazor/palette-checks.mjs and action-menu-dom.mjs. Palette validation now locates the shared persistent selected rule and checks foreground, fill and inset border while retaining ordinary hover/press distinctions. The browser calculation distinguishes selected/unselected options, checks root-theme roles, inset geometry and checkmark foreground inheritance. Assertions were strengthened rather than removed or skipped. No component, stylesheet or public API was changed.

## Verification and limits

The three focused palette/native-select/action-menu suites passed 77/77, zero skips. ActionMenu's internal 44/44 checks included actual headless-browser computed styles with no real-browser skips. The repeated full release gate then passed all 339 Node tests and the existing build/package/.NET/Gallery/Culture/script/CSS/consumer stages. See the [cleanup record](../1_changelogs/2026-11-13_151115_migrate-saf-and-retire-docfx.md) for the complete scope.

These are maintained automated checks, without Computer Use. Interactive/native platform acceptance remains separate. Local logs and transient browser/consumer fixtures were removed after durable results were recorded; the tests remain the repeatable verification mechanism.
