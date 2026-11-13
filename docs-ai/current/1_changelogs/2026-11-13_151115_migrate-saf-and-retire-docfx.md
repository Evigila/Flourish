# Migrate to SAF, remove DocFX and clean project output

Recorded on 2026-11-13 at 15:11:15 America/Sao_Paulo (UTC-03:00). The user authorized SAF migration, project cleanup, DocFX/docs removal and commit/push. They confirmed that docs/ should retain only SAF's empty .gitkeep marker. No replacement documentation framework or package publication was requested.

## Framework and entry points

Installed SAF 2.0.0 from Evigila/SAF at 29c1f652956d0f5ce2f140d031df2204e5aa8012 through the verified official bootstrap/origin handlers, without managed-file conflicts or Force. Old installation/audit evidence was imported before the old metadata, fetch launcher and sync skill were retired. Typed project configuration selects the canonical SAF upstream/main, Flourish, Computer Use denied and the established commit style. Naming, public identity and library/host ownership requirements remain in [project constraints](../project-constraints.md).

The existing startup BAT becomes debug.bat with its original allowlist/parameters and scripts/Start-Project.ps1 implementation. Production preparation/publication moves to scripts/release.bat and the existing Publish-Helper.ps1. The two old root entry points are removed, without compatibility wrappers. Tag/clean-source/remote/typed-confirmation safeguards remain intact; source push does not publish a NuGet package.

Before documentation consolidation, local backup completed at .agents/backup/20261113T175746Z-32105dcd5f3d43a9a14508fc18f3c53b/ and all 368 scoped hashes were checked. This is framework/AI-document recovery, not a full source backup. Deleted human-site sources remain recoverable from Git at 43c129e; no previously uncommitted human edits were present.

## DocFX and source preservation

Removed the 118 DocFX site/template/content files other than docs/.gitkeep, the standalone Pages build/deploy workflow, DocFX's local tool entry and its generated-path ignore rules. CSharpier stays at 1.3.0. No replacement site framework is introduced, and existing GitHub Pages hosting/deployments were not changed through a remote administrative operation.

Gallery, maintained diagnostic samples, all 148 test files, 13 build-source/check files, public fonts/icons/licenses, XML package documentation and all runtime/library code remain. The build/ directory is maintained source, not disposable build output. Source defaults remain Flourish 1.4.7 and Essential 1.4.0; local 1.4.8-preview.14 provenance is historical consumer evidence, not a version promotion. No tag/package/site was published.

## Cleanup and current project memory

After successful verification, deleted 44 bin/obj directories and the complete disposable artifacts tree, including old output feeds, one-off consumers, logs, scripts and acceptance results. Also removed the two exact CSS fixtures created by the final check under the system temporary directory, after validating names, parent roots and nonlinked contents. Generated removal totals 54,607 files and 4,241,978,746 bytes, about 4.24 GB; this excludes the separately removed human-site files. Required source and recovery snapshots remain.

Consolidated active Markdown from 14 files / 618,298 bytes to 9 files / approximately 83 KB. Architecture is now a responsibility/dependency map; the inventory retains the 22-project/33-edge graph, full 51-pair mixed-age capture and CSharpier's 25 bundled pairs, explicitly distinguishing declarations from historical restore data. Eight superseded guides/evidence files and seven complete prior snapshots remain archived with status/replacement context. DocFX-only dependencies are historical, not active prerequisites.

Moved 65 early bug records and 135 early change records into the standard 1_bugreports/ and 1_changelogs/ folders, preserving names/bytes and refusing collisions. Removed empty legacy folders. Mechanical Markdown target repair preserves all 321 original historical narratives; eight archived sources and seven complete snapshots were compared against recovery material. No diagnosis or past result was reinterpreted as new verification.

## Verification

The first full gate exposed two stale selected-dropdown expectations from the previously authorized dark selected-row styling. [Diagnosis and focused repair](../1_bugreports/2026-11-13_selected-dropdown-regression-expectations.md) preserves the failure, cause and stronger checks. Runtime CSS/components were not changed to satisfy the tests.

The repeated full scripts/Test-Release.ps1 gate passed: six packages/dependencies/assets, Core 367 tests, Framework 540 checks, maintained Culture/Gallery suites including 9,298 Gallery checks, 339 Node tests with zero failures/skips, 95 launcher checks, 21 CSS bundler checks, 194 SDK CSS checks, 26,290 catalog checks, 218 changelog checks and 155 isolated package-consumer checks. Generated verification outputs were then removed; no subsequent build recreates them.

SAF inspection passed required structure, schemas, 16 managed/5 bootstrap hashes, three skill packages and core links. Set-SafState recorded actual structural completion for 2.0.0 at 2026-11-13T18:12:19.7055425Z while preserving prior question/bootstrap/installation evidence. Final origin Check returned Current/0 and preserved exact bytes of all 18 framework/config/state files. Current/core document links and anchors are checked separately. This structural result does not certify application behavior or native desktop acceptance.

Recommended manual acceptance: debug.bat -List; choose the intended Gallery after a normal rebuild; inspect browser/native layout, keyboard/focus, selected rows, high contrast/reduced motion and avatar/dialog behavior. WPF/WinUI desktop prerequisites, WASM/Interactive Auto and deployment-specific consumers retain their existing separate acceptance limits. Production publication still needs its own deliberate version/tag/review procedure.
