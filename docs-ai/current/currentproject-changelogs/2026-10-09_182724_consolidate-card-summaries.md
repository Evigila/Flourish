# Consolidate card summaries

- Removed IdentityCard from Blazor and native WPF, including independent styles, Gallery/navigation/API registration and retired parameter guidance. Card already covers ordinary content, identity and record summaries; one current entry avoids duplication. No alias, forwarding renderer or new Card parameter was added.
- Migrated RecordDetails to Card with its original four-field semantic definition list and callbacks. CardSample now demonstrates the localized member title with CopyText. Card retains its existing H3 Title; the former IdentityCard HeadingLevel, Columns and sidebar entry points are retired.
- Kept the native Card identity example's name, department, status, identifier-copy and Edit interactions. The native catalogue now has 74 entries.
- Updated current architecture/API/convergence/native guides and three-language production guidance. Added the concise removal reason to 1.1.4-preview while preserving the published 1.1.2 note and append-only historical records.

Verification: zero-warning/error Release builds for Blazor, Gallery and WPF; 429/429 Blazor checks with 80 components, 720 parameter rows and 329 defaults; 7,772 Gallery checks including 171 actual-event examples and 199 ChangeLog assertions; 20/20 focused Node style/PageBody checks; 76/76 WPF catalogue checks including 74 actual offscreen Gallery templates and the Card identity Edit event; 22,038 Culture checks; 70 ChangeLog checks; 21 CSS bundle checks; clean git diff --check.

See [the consolidation report and manual checklist](../bugfix-reports/2026-10-09_card-summary-consolidation.md). No dependency, package version, Git tag or publication changed. No Computer Use, browser UI testing or Git commit was performed. This record uses America/Sao_Paulo local time.
