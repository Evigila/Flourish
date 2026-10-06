# Unavailable Elevated actions lost their appearance

Status: Design cascade corrected and source/rendering/package regressions passed. Browser geometry/theme acceptance remains manual. See [the change record](../currentproject-changelogs/2026-10-06_012952_add-local-launcher-and-presentation-defaults.md) for complete verification.

## Symptoms and cause

The personalized-license button in Colligere Pricing appeared not to use Elevated. Its source already selected ButtonVariant.Elevated; however, the generic disabled rule set `background:transparent` and `box-shadow:none`. The ordinary `.f-button-elevated` selector has specificity `(0,1,0)` while `.f-button:disabled` and `.f-button[aria-disabled=true]` have `(0,2,0)`. The latter therefore removed the surface and elevation regardless of source declaration or component variant selection.

This was a library appearance defect, not a missing host custom skin. The action was already unavailable and had no approved contact/purchase endpoint. Enabling it would have changed behavior beyond the UI request.

## Correction

Design's `.f-button.f-button-elevated:disabled` and corresponding aria-disabled selector have specificity `(0,3,0)` and restore only `var(--f-surface)` and `var(--f-shadow-control)`. Disabled foreground, border and not-allowed cursor remain inherited from the shared disabled rule. Existing hover/active selectors continue excluding both unavailable forms. Other button variants retain their disabled transparent/no-shadow behavior. Theme role values resolve normally in Light/Dark; no new palette, renderer or interaction controller is introduced.

Colligere now passes the real `Disabled=true` parameter for this action instead of relying solely on an unmatched native attribute. It remains `type=button` with no link/callback. The Gallery demonstrates the same unavailable Elevated scenario with fictional copy.

## Evidence and regression checks

The CSS test resolves matching, specificity, `:not`, important declarations and source order rather than checking for a selector's mere presence. It covers native disabled and aria-disabled, hover/press combinations, unchanged other variants and enabled Elevated idle/hover/press. The old stylesheet fails the unavailable background assertion.

C# checks render actual Button submit/link instances under Disabled, Busy and both, verify native/ARIA availability, remove link navigation even with conflicting attributes, and invoke click twice with zero host callbacks. Re-enabling restores one callback and the original link. Final complete Blazor tests are 303/303; JavaScript and isolated NuGet-only consumers pass. Colligere's actual SSR/HTTP Pricing assertions pass after accommodating legitimate template whitespace, without weakening the disabled/Elevated/label checks.

## Limits and manual acceptance

No contact channel, purchasing behavior, endpoint or permission changed. Real browser appearance still needs user acceptance: inspect disabled Elevated cards in Light/Dark, mouse/keyboard/busy states, small widths and zoom. Verify surface/shadow persist but no hover/press/click/navigation is acquired, and other disabled variants look as before. Do not add a consuming-app override to compensate for this library rule.
