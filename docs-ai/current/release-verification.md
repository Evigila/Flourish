# Local release verification on 2026-10-05

This records local verification, not NuGet.org publication. The approved first release now covers six Core/Blazor packages after Shared consolidation and addition of the dependency-only Blazor convenience package.

## Current Flourish-only final review preparation

The [final API and access-scenario review](currentproject-changelogs/2026-10-05_185253_finalize-blazor-api-and-access-scenarios.md) preserves the 97-component inventory, 707 parameter rows and 311 documented defaults. It adds 61 focused checks: five input/API checks, 49 reserved-attribute/interop lifecycle checks and seven local access-fixture checks. Final Blazor result is 227/227. Dedicated login and account-selection Gallery pages use existing production controls and an independent single-main layout, with no authentication service or credential persistence.

The complete scripts/Test-Release.ps1 preparation passed after the final source changes: Release build zero warnings/errors, Core 367 tests, Blazor 227 checks, Culture bridge 12 checks, all ten configured JavaScript files (53 Node-runner checks plus the mock suites), 21 CSS bundle checks and 194 CSS SDK integration checks. All six local 1.1.0 packages passed verification. Four fresh isolated NuGet-only consumers passed 95 checks at artifacts/package-consumers/efff6a1005f74ab6bda010c636e819ad. Package packing retains the expected README advisory under the repository's no-new-README ownership rule; compilation is warning-free.

Strict Gallery HTTP verification passed 119 routes (16 production/support directories, all 97 component guides and six display/access scenarios) plus four resources. Every scenario has one main; static login cannot accidentally submit before interactive connection. Development uses the existing http launch profile. An initial Production-mode run against the unpublished development directory returned missing static assets; it was stopped and the complete HTTP check rerun in Development with terminating error handling. Published package-consumer resources separately passed in the release preparation.

Current candidate SHA256: Abstract FBE265B31DB9825E49923F09E8DBE6791529C4B4FC0B8CAED5442F125459A35C; Framework F10B76DD8CB75722742C7EE36BA501154DFE1E4A8158BB920133026E3C513A45; Design 90C4849CEB9AA15603D21315542575F8E56D33A3E8DDFCA103251F9549A240A7. Earlier unpublished candidates were rebuilt locally; their recorded hashes remain historical. Colligere and Essential were not modified or revalidated in this follow-up. The user authorized a local Flourish commit, not a push, tag or NuGet publication. API convergence and manual browser acceptance remain explicit outstanding work, not hidden by passing inventory checks.

## Previous display and component inventory preparation

The [display extraction](currentproject-changelogs/2026-10-05_174832_classify-components-and-extract-display-scenes.md) adds nine production scene controls, complete 97-component usage/Gallery inventory and explicit API convergence boundaries. Blazor/Gallery Release build has zero warnings/errors; component checks pass 166/166 and Culture bridge passes 12. All ten configured JavaScript files pass (53 Node-runner checks plus the existing mock suites); CSS bundle verification passes 21 checks. Gallery passes 27 automatic HTTP SSR routes; browser acceptance remains manual without Computer Use.

The six-package set still passes validation with no new dependency/package. Four fresh NuGet-only consumers pass 95 checks, including actual display/offer/access SSR and offers.js assets in all activation modes. Final evidence: artifacts/package-consumers/7c7bc17721d342fda087b0b46793d2ff. Colligere restores matching candidates to artifacts/package-cache-presentation-final, builds its full Release solution without warnings/errors and passes 220 focused Web regressions, zero failed/skipped.

Current local candidate SHA256: Abstract C5B74615246AD0C842EB6FEAB5366F2EC47A82895694B55DB089677EB71A152E; Framework ABEEFEB86CBD9A5F088EA2C8F6CC02D7360AF1B731A3BDB8FDA42B94C2C045D4; Design 2F026D4DEC7D1EA7A4338861F231B30687EA949316CB70EA4DFF2816116BF4D7. These supersede prior unpublished candidate hashes/caches. No public publication occurred. Advanced table/lifecycle convergence remains pending; this is not a new full database/desktop/SDK-asset release run.

## Previous static ListView preparation

The [ListView follow-up](currentproject-changelogs/2026-10-05_165500_add-static-listview-and-grid-centering.md) adds the static generic control and opt-in UniformGrid centering without another package or an independent table skin/controller. The Blazor-only solution, including Gallery and Culture bridge, builds in Release with zero warnings/errors. Blazor passes 150/150 checks; the bridge passes 12. All nine configured JavaScript files pass, with 40 Node-runner tests plus the existing mock suites; palette/list/grid checks pass 13/13. CSS bundle verification passes 21 checks.

Abstract, Framework and Design were repacked as local unpublished 1.1.0 candidates; all six configured packages still pass verification. Four new NuGet-only consumers pass 79 registration/SSR/HTTP/static-asset checks, including all 25 static ListView rows per mode. Evidence: artifacts/package-consumers/7552c58c83694648ba18732442f0061c. Colligere restores matching candidates to artifacts/package-cache-pricing, builds its complete Release solution with zero warnings/errors and passes 118 focused Web regressions, including actual /pricing/ HTTP rendering and footer presence.

Current candidate SHA256: Abstract D5DE44B22F0D2CC019A49EE857BF593DC468830CAB637C008839F96B094560DF; Framework 44F8D91FD5E9075FE83EAC2D25B2733B8C9D9974C9F866D2C2436397F23E9F9E; Design 862E83C166766CAC8A2B6CD6E67E3801BA21EDD9878F751D6BB96AF74D9B20D2. These supersede the earlier local candidate hashes below. This is targeted Blazor/consumer preparation, not a new full Core/desktop/SDK-asset run or public release. Earlier unrelated full-suite failures retain their recorded scope. Browser geometry and theme acceptance remain manual, without Computer Use.

## Current Blazor-only preparation

The final scripts/Test-Release.ps1 completed with exit code 0 after consolidation, release-scope and consumer-regression fixes. Release build reported zero warnings/errors. Core tests: 367; Blazor checks: 135/135; Culture bridge checks: 12; DOM/browser tests: 38 plus passing interaction/mock suites; CSS bundle checks: 21; SDK asset checks: 194. Exactly six 1.1.0 packages passed identity, dependency, assembly/meta and static asset validation.

Four isolated NuGet-only consumers passed 67 registration/SSR/HTTP checks: Framework alone; convenience package with no Design registration; convenience package with Design; convenience package plus optional Culture bridge. Core restored transitively, Shared was absent, and only explicit registration activated Design. Initial consolidation evidence: artifacts/package-consumers/728f83c9c57140e1a850a87b67697989. A subsequent complete run after host-contract fixes (filled icons, native action grid, card value lines and hidden-column sorting) passed with evidence at artifacts/package-consumers/c40460ce21d94457b5f8fa3541084c7c. The remaining explicit sort-button ARIA-state follow-up is recorded separately when its verification finishes.

WPF and its Culture bridge remain source projects but are outside this release's package set and validation solution. Earlier full desktop/eight-package checks below are retained as historical evidence, not the current release contract. See the [new consolidation record](currentproject-changelogs/2026-10-05_115734_consolidate-blazor-packages-and-release-scope.md) and [current release guide](nuget-release-integration.md). No tag, push or public release was performed.

The ARIA/controller-order follow-up passed the complete preparation: explicit true/false sort/reorder ARIA states are tested in actual SSR, and the controller order is sorting, advanced editing, then Display. Its six-package validation and four-consumer 67-check evidence is artifacts/package-consumers/dd0ad177e0c94552bd390cbd3c9564ae. This supersedes the pending-verification note above.

The latest complete preparation additionally restores the original pagination contract: two page-button groups but only the top group's range/aria-live announcement. The bottom Pager explicitly sets ShowRange=false; an actual SSR regression guards against duplication. Final evidence: artifacts/package-consumers/aae97a0da9ff43ab82710e083fe5c955; all four consumers passed 67 checks. Final Framework package SHA256: 77C780A168A8EA21DA3C4D306408285127324CEFFA26A70DE557AC5BEDB82DBA.

## Current header button variant verification

The subsequent [header button variant correction](currentproject-changelogs/2026-10-05_162957_preserve-header-button-variants.md) repacked Design only, narrowing chrome overrides to available Quiet actions so explicit Elevated/other variants retain their paint. Its local Design SHA256 is D403A90E32BBCF0C509E240E3EFE9F29297A63F070E069431E005A03A62E6713. Blazor 135 checks, all nine JavaScript files, 21 CSS bundle checks, six-package verification and 72 focused Colligere consumer regressions passed. This is not a new complete release run or public upload; the unchanged Framework hash and earlier four-mode consumer evidence below retain their original scope.

## Current all-project naming verification

Following the project-only naming audit, root Flourish.slnx restore and Release build completed with zero warnings/errors, including all 18 projects, WPF, WinUI3 and three Galleries. Static validation resolved 22 ProjectReference paths and five solution files. The existing prepared six-package set passed Verify-PackageSet.ps1 without being repacked; Framework SHA256 remains 77C780A168A8EA21DA3C4D306408285127324CEFFA26A70DE557AC5BEDB82DBA.

Core 367 tests and WPF bridge five tests passed. WPF reported 900 passed and one pre-existing directory-shape failure: FlourishXamlArchitectureTests.ProjectFolders_FollowFeatureModuleThemeAndViewBoundaries requires BackgroundTasks, Localization and Shell/StatusBar directories that are absent from both the current tree and HEAD's maintained files. Neither WPF sources nor that test were changed by naming. Empty directories or weakened assertions were not added as an unrelated workaround. This current result supersedes treating the earlier 901-pass run below as the current desktop result.

Blazor verification separately passed 135 checks, and its Culture bridge passed 12 checks. Root automated-test TRX files are retained under artifacts/test-results/project-naming. These are project-reference/compilation/automated checks, not desktop window execution. No Computer Use, commit, tag, push or public release occurred.

## Earlier eight-package preparation

The full root publish-helper.bat -Mode Prepare completed with exit code 0 after the final source changes.

| Check | Result |
|---|---|
| Core automated tests | 367 passed |
| WPF automated tests | 901 passed |
| WPF Culture bridge tests | 5 passed |
| Blazor framework checks | 133/133 passed |
| Blazor Culture bridge checks | 12 passed |
| Node behavior tests | 37 passed, with the existing mock DOM checks passing |
| CSS bundle verification | 21 checks passed |
| SDK asset verification | 194 checks passed |
| Package verifier | All eight configured 1.1.0 packages passed |

The package IDs, dependency order and commands are maintained in [NuGet release and integration](nuget-release-integration.md). Essential's prior full preparation passed 89 tests, built all five demos and verified all six 1.3.0 packages. Its source was unchanged afterward.

## Final generic framework fixes

- Bound native inputs emit their SSR form names while preserving supplied native attributes.
- Field.ControlId supports a control ID distinct from the field/error container ID; labels and validation descriptions remain associated.
- aria-describedby combines existing descriptions with the field error ID without duplicates.
- Button preserves explicit link role and tabindex semantics, including its disabled link behavior.
- RecordPageHeading now uses the scoped text base and Heading_BackTo for its return label: English Back to {0}, Chinese 返回{0}, Portuguese Voltar para {0}.
- These changes remain generic framework behavior. Application pages, identities, permissions and business vocabulary remain in consuming hosts.

## Published local application assets

A standalone NuGet consumer using Framework without Design was built with dotnet publish. Its 18 tested static resources returned HTTP 200 from the published local application.

Colligere's locally published NuGet consumer returned HTTP 200 for all 23 tested static resources.

Here, published means local dotnet publish output served for HTTP verification. It does not mean that NuGet packages, a production application or a public website were released. This task did not use Computer Use for verification.

## GitHub and NuGet configuration

The coordinating task successfully created Evigila/Flourish's nuget environment. Its returned configuration had protection_rules=[] and branch_policy=null. NUGET_USER remains unconfigured; the requested NuGet profile username is still pending with the user. No API key was requested.

Essential already has the nuget environment and NUGET_USER secret name. Its environment likewise has no configured protection rules or branch policy. Secret values were not retrieved, and the NuGet.org account-side trust policies were not verified.

A pre-consolidation read-only query of the then-configured 14 IDs found no public target versions: all six Essential 1.3.0 and eight Flourish 1.1.0 versions were unpublished at the time of that query. Five existing Essential platform packages had latest version 1.2.0; Culture.Blazor and all eight Flourish IDs returned 404. This query is historical evidence, not a statement that eight Flourish packages are still in scope. The current required sequence is Essential's six 1.3.0 packages, then Flourish's six Core/Blazor 1.1.0 packages (including the optional Culture bridge), then the application's public-feed restore. The pending username, account policy/package scope and separately authorized Publish step remain release prerequisites. Environment creation alone does not establish publishing readiness or add an approval requirement.

## Approval review and Git boundary

Automatic approval review rejected execution of a Publish-mode negative test because that helper can fetch, create tags and push. A read-only PowerShell AST guard inspection was used instead; the Publish helper was not executed for the negative test.

No commit, release tag, push or NuGet publication was performed. The final Prepare and HTTP checks are local verification only. See the release guide for the clean master/origin/master and exact-tag confirmation requirements before any future Publish action.
