# Use the save bar for Gallery color settings

Date: 2026-10-29, America/Sao_Paulo (UTC-03:00). Status: implemented; automated build and Gallery checks passed. This follows the [reusable save/discard control](2026-10-29_save-changes-bar.md).

## Cause and boundary

The Appearance color example was an editable settings page with its own Apply/Reset row. The consumer rollout clarified that settings/detail drafts should use the standard bottom save bar, while dedicated entry forms retain their natural bottom action area. The page's EditForm exists for validation and does not make the surrounding settings view a dedicated record-entry page.

## Change

Appearance now uses the existing SaveChangesBar, comparing its color draft against the last applied palette. Save validates, applies and adopts the appearance service's normalized values; Discard restores the last applied snapshot without changing the service. Reverting both fields clears dirty state. The old Apply/Reset row is removed. Form, bar and field callbacks are tied to the originating context/draft; replacement/disposal rejects stale callbacks. Validation failure keeps the draft. Immediate theme selection remains an independent preference.

The form/record pages and Button, EditingGrid, FormActions, NavigationGuard and other mechanism samples intentionally retain their demonstration contracts. No other real pending-settings page was found in the Blazor Gallery. Core preference saves are internal persistence; WinUI Gallery has no editor. Native WPF's Appearance demo is outside the current web-page rollout and is not presented as migrated.

## Verification and limits

Gallery application and test-project Release builds passed with zero warnings/errors. The existing Gallery executable passed **8755 actual checks**, including **483 post-event** and **443 changelog** checks. New real-event coverage includes change/revert, invalid save, discard, stale callbacks and adopting the saved baseline. Evidence is under ignored `artifacts/editable-page-save-bar-20261029/`.

No public component, library source, package version/payload, external dependency or maintained file is added. The source inventory remains 721 files/121 directories, 22 projects/5 solutions and 84 components/785 parameter rows/368 defaults. Frozen .15 package verification belongs to its original report. No Computer Use, physical-browser acceptance, package publication, commit or push occurred.

## Manual acceptance

1. Open the color foundation page. Edit either color and verify one bottom Save/Discard bar, with no inline Apply/Reset row.
2. Revert or Discard and verify the actual previously applied palette is preserved. Save valid colors and verify the bar closes against the normalized palette; invalid colors must keep validation and the draft.
3. Check keyboard navigation, long localized labels, narrow widths, zoom and leaving/reloading with a draft. Confirm theme preference selection and dedicated form samples retain their behavior.
