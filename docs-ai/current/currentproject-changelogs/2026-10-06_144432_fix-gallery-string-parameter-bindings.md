# Fix Gallery string parameter bindings

Local timestamp: 2026-10-06 14:44:32, America/Sao_Paulo.

The user clarified that access-method selection at /examples/display/access-methods raised NavigationChoices' invalid ActiveKey exception. An independent pre-fix instance reproduced HTTP 500; saved-account selection remained HTTP 200.

NavigationChoicesSample now passes its current getter with explicit Razor string expression syntax, and the copied example follows the same contract. A related bounded audit corrects ten runtime bindings across six samples and six API-example attributes: selection key, remote search, logos/title ID, form names, progress status and code text. Strict library validation and all native GET/business/account behavior remain unchanged.

The new build/Test-GalleryNavigation.ps1 verifies actual compiled Gallery endpoints and rendered state rather than only constructing library parameters in tests. It passes 191 HTTP checks. Isolated Gallery Release build has zero warnings/errors; existing component tests pass 356/356. Logs: artifacts/gallery-navigation-build.log, gallery-navigation-http-final.log and gallery-navigation-components.log; initial evidence is retained.

See [the defect report](../bugfix-reports/2026-10-06_144432_gallery-navigation-choice-literal-key.md) for diagnosis, initial test corrections, limitations and manual checks. The active API/architecture guides record explicit string expressions and this new test helper. No library packages needed rebuilding because this follow-up changes Gallery/sample metadata only.

Only independently started temporary test instances were stopped. The user's application remains running with its previous assembly and needs a manual restart. No Computer Use, dependency/business/data change, publication, deployment or Git commit/push occurred. Previous uncommitted changes and append-only records are preserved.
