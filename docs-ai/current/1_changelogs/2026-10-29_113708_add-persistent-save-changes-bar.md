# Add persistent save/discard bar

Recorded 2026-10-29 11:37:08 America/Sao_Paulo (UTC-03:00); UTC source 2026-10-29T14:37:08Z. Previous append-only records remain unchanged.

Candidate **1.4.4-preview.shell.20261027.15** adds the library-owned SaveChangesBar with localized standard actions, page-owned dirty/saved data, callback busy protection, hard ordinary-navigation blocking and responsive measured bottom clearance. Browser close/reload uses its native prompt; the separate confirmation-based NavigationGuard is unchanged. See [behavior, limitations and manual acceptance](../1_bugreports/2026-10-29_save-changes-bar.md).

Seven maintained files are added, yielding **721 files/121 directories**, 22 projects/5 solutions and **84 public components/785 parameter rows/368 defaults**. Six local archives and **14 unique delivery entries (13 static resources plus one Framework DLL)** are frozen in artifacts/save-changes-bar-20261029/nuget, retaining twelve producer dependency graphs and 33 external package/version pairs with no new external dependency. [Dependency hashes](../1_dependency.md#current-15-archive-and-asset-evidence) identify .15; previous .14 evidence remains historical.

Release passed with zero warnings/errors; Framework **494/494**, Gallery **8750/8750** (474 post-event and 443 changelog checks included), Culture bridge **27/27**, save-bar Node **24/24** and CSS bundler **21/21** passed. No Computer Use, physical-browser measurement, public publication, commit or push occurred. This scoped tree/documentation update leaves common rules and audit state untouched.
