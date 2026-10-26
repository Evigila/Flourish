# Add Notice titles and preserve inline body layout

Recorded: 2026-10-25 22:20:29 America/Sao_Paulo (UTC-03:00).

Blazor Notice now always displays a title: an explicit nonblank `Title` remains the business summary; otherwise the existing `Notice_<severity>` localized text is used. The component follows scoped language changes and releases its text subscription on disposal. Title and body are separate grid children, keeping inline body fragments and controls together. Design owns block title layout, inherited body font size, 700 weight and severity color; subtle notices retain their semantic title color while keeping muted body text. Role, Announce and severity validation retain their contracts.

Root version is `1.4.3-preview.portals.16`. Six exact-version local packages were built and verified in `../Colligere/artifacts/local-nuget/1.4.3-preview.portals.16`. Existing package IDs, dependency direction and Essential 1.4.0 remain. No new external library, font, service or production JavaScript was added. Earlier preview bytes were retained. No public publication or running-host restart is asserted.

Verification: Framework 474/474; Release production package entry build 0 warnings/errors; package identity/internal dependencies/required assets 6/6; CSS bundle 21/21; Colligere Release 17 projects 0 warnings/errors; consuming Web 2601 passed, 187 environment-dependent skipped, 0 failures. The first consuming run found six old structure/label assertions; the final run passed after explicitly locating Notice roots and asserting separate title/body and uniform timezone values. Both repositories passed `git diff --check`.

Evidence: consuming `artifacts/notice-titles-20261025/` contains logs, TRX, frozen dependency graphs and exact package/cache/resource hashes. No Computer Use was performed. Manual acceptance: restart the consuming app with the new package; check all severities and subtle notices in light/dark themes, long titles, inline countdowns, explicit summary, Chinese/English/Portuguese default titles and live language switching.
