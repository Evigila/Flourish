# Adopt save bar in appearance settings

Recorded 2026-10-29 12:37:43 America/Sao_Paulo (UTC-03:00), from 2026-10-29T15:37:43Z. This follows the [new SaveChangesBar](2026-10-29_113708_add-persistent-save-changes-bar.md).

Gallery color settings now use the existing SaveChangesBar for validation, save and restoration of the last applied palette. Form/control callbacks are scoped to their originating draft/context, and successful saves adopt the service's normalized colors. Dedicated record forms, mechanism samples and immediate preference selections retain their intended behavior. See [cause, inventory boundary and manual acceptance](../1_bugreports/2026-10-29_appearance-save-bar-adoption.md).

Gallery application and test-project Release builds passed with zero warnings/errors; 8755 actual checks passed, including 483 post-event and 443 changelog checks. New event coverage verifies invalid drafts, reverting values, discard, stale callbacks and the saved baseline. The maintained tree, public catalog, library source and dependency graph are unchanged; the .15 feed remains frozen without repacking or publication.

Architecture/dependency notes and the linked report record this application-only rollout. No Computer Use, physical-browser acceptance, native WPF/WinUI expansion, commit or push occurred. Existing append-only records and audit state remain unchanged.
