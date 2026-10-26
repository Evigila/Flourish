# Record Flourish 1.4.3 publication and public verification

Time: 2026-10-26 08:50 America/Sao_Paulo (UTC-03:00).

Stable tag `v1.4.3` identifies `d4e8cd95c58e87646684903a0fdb2f9767994931`. [Build, test, and publish run 40315212418](https://github.com/Evigila/Flourish/actions/runs/40315212418) completed successfully for that exact tag/commit. All six package uploads succeeded through Trusted Publishing from 11:33:09 to 11:33:16 UTC.

All six public NuGet indexes confirmed 1.4.3 at 11:46:01 UTC. The earlier missing/partial index responses were propagation delays; no package was republished and no tag was replaced. The public-only consumer gate then passed 143/143 with fresh package and HTTP caches. FrameworkOnly, MetaNative, MetaDesign and MetaCulture all restored, published and ran. Package metadata verifies six Flourish 1.4.3 and three Essential 1.4.0 packages from `https://api.nuget.org/v3/index.json`, without a local-feed substitution.

Evidence and public archive SHA-256 values are recorded in [release verification](../release-verification.md#completed-flourish-143-release-on-2026-10-26), with raw workflow/index/consumer/provenance artifacts under `artifacts/final-release-20261026/` and isolated consumer fixture `artifacts/package-consumers/0b49f656575a4c4ba07728e6883d64f5`.

This completion record supersedes the pending publication status in [preparation](2026-10-26_082500_prepare-flourish-143-stable.md). Only documentation changed while recording the result; no new tests, source edits, publication or tag changes were performed. The monthly audit state is unchanged.
