# Final local portal preview verification

Recorded 2026-10-19 16:25:44, America/Sao_Paulo (UTC-03:00). Final local source/package version: 1.4.3-preview.portals.3. The coordinating task ran the following checks and reviewed their logs; the documentation update also checked the Framework/Gallery summaries, host test and isolated PostgreSQL log summaries, and the six package filenames. Earlier .1 and .2 attempts remain recorded separately and their package artifacts are preserved.

| Verified scope | Result and evidence |
| --- | --- |
| Final Flourish Blazor solution, Release | Zero warnings and zero errors. |
| Framework renderer and public contracts | 452/452; [final log](../../../artifacts/portal-preview/framework-tests-final.log). |
| Gallery rendering, localization, samples and lifecycle | 8,422 checks; [final log](../../../artifacts/portal-preview/gallery-tests-final.log). The reported 474 post-event and 343 ChangeLog checks are included subsets and are not added to 8,422. |
| Focused JavaScript regressions | palette 26/26; organization-access-presentation 16/16; surface-form-layout 16/16; access-form-spacing 4/4. This identifies only the scripts actually run and does not certify every JavaScript test. |
| Exact preview version and package set | Test-PreviewVersion 12/12 and Verify-PackageSet passed all six packages. The exact local archives are in [the consumer local feed](C:/Users/RC_Auditoria/source/Repos/Colligere/artifacts/local-nuget/1.4.3-preview.portals.3). Gallery retains its 1.4.3-preview aggregate; stable publication helpers reject the suffix-bearing source. |
| Colligere local consumer restore and build | All 17 projects restored; Release build completed with zero warnings and zero errors against the exact .3 packages. |
| Colligere full solution, Release | 3,131 passed, 146 conditional PostgreSQL tests skipped, zero failed; [final test log](C:/Users/RC_Auditoria/source/Repos/Colligere/artifacts/portal-preview/tests-final.log). |
| Isolated organization PostgreSQL checks | 25/25; [organization log](C:/Users/RC_Auditoria/source/Repos/Colligere/artifacts/portal-preview/organization-postgres.log). Organization HTTP integration: 9/9; [HTTP log](C:/Users/RC_Auditoria/source/Repos/Colligere/artifacts/portal-preview/organization-http-postgres.log). These are separate targeted runs with their own database environment and are not added into a deduplicated grand total. |

The consumer implementation and manual acceptance list are recorded in [the Colligere portal implementation report](C:/Users/RC_Auditoria/source/Repos/Colligere/docs-ai/current/portal-login-implementation-2026-10-19.md). Library Sidebar choices retain native GET navigation, responsive geometry, keyed hidden panels and standard Button ActiveVariant rendering. The focused palette checks cover the default Light/Dark Primary selection; consumer palette behavior belongs to the host verification scope.

No Computer Use, normal AppHost restart, commit, push, public package publication or new public-feed consumer verification was performed in this local-preview task. Automated checks do not establish visual acceptance of the running normal application. Audit state remains unchanged.
