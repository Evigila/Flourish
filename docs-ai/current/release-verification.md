# Release verification

## Latest completed release: 2026-10-06 Flourish 1.1.2

The user's later 2026-10-06 instruction authorizes publication and the subsequent Colligere upgrade, superseding the preceding source-only restriction. All queued repairs below are included in 1.1.2, alongside IdentityCard heading/width/identifier geometry, per-instance PresentationFooter identity, Compact NavigationChoices variants, label-aware native access spacing, PageBody.FillHeight/CompactSpacing and UniformGrid.CellHeight. Essential remains 1.3.0 and the six-package manifest is unchanged.

Complete preparation passed: Core 367, Blazor 396/396, bridge 12, Gallery 7,266 including 124 actual post-event language/state checks, Node runner 99/99, CSS bundle 21 and SDK integration 194, launcher 95, catalog 21,426, and four isolated candidate consumers totaling 129 checks. Release builds completed with zero warnings/errors. Evidence: artifacts/release-1.1.2-preparation-final.log and artifacts/package-consumers/c6a3001a3d99416b979529e2fcbd886e. [The follow-up report](bugfix-reports/2026-10-06_identity-access-and-editor-release.md) records the repairs and cleanup.

Source commit 108fbff8950925b909a70b0b9830226ae8226521 is tagged v1.1.2. [Run 37560362408](https://github.com/Evigila/Flourish/actions/runs/37560362408) passed both build and Trusted Publishing jobs. All six packages returned Created between 02:09:52 and 02:09:56 UTC on 2026-10-07 (23:09 on 2026-10-06 in Sao Paulo), and all public indexes contain 1.1.2. Fresh package and HTTP caches with the unique public source passed all 129 consumer checks across FrameworkOnly, MetaNative, MetaDesign and MetaCulture. Evidence: artifacts/public-release-1.1.2.log and artifacts/package-consumers/b06a15d2f8fc4661be3792ff26ebf464. Earlier indexing-pending and stale-HTTP-index restore failures are retained separately; they were propagation/cache limitations, not successful validation or package compilation failures. Publication, indexing and public-only consumption are complete; Colligere may now upgrade its two central Flourish entries together.

### Earlier source-only verification history

The account-selection follow-up adds generic Button.Description/TrailingText for a stacked primary/secondary label and passive trailing status in one native action. Both default empty; structured mode requires Text and rejects mixed ChildContent. Contracts, defaults, three-language parameter descriptions, real Gallery example and five render/event/layout checks are updated. Release test/Gallery builds passed with zero warnings/errors and console checks passed 387/387. [The structured Button report](bugfix-reports/2026-10-06_structured-button-labels.md) records consumer limitations. No version/publication/consumer upgrade is authorized by this source repair.

The post-1.1.1 Workspace editing follow-up is source-only, with publication explicitly deferred. Include EmptyStateVariant.Watermark (large centered passive title), DataTable bulk-action end alignment and corrected single ordinary Concluir Gallery command, and EditingGrid's framed 65vh scroll/header/initial-width/change-marker restoration. Complete automatic NavigationSurface heading collapse, Field reference sizing, board form width and InlineActions alignment remain queued from the earlier source repairs. No new version has been assigned or published, and public 1.1.1 has not been overwritten.

Latest focused source verification: Release test/Gallery builds passed with zero warnings/errors, Blazor console checks passed 382/382, and surface/toolbar/page-body Node checks passed 19/19. These checks do not replace a future full newly versioned package preparation, fresh-cache package-consumer verification or user-run visual acceptance. [The editing and watermark report](bugfix-reports/2026-10-06_watermark-and-editing-layout.md) records the partial historical restoration and consumer limitations.

## Previous completed release: 2026-10-06 Flourish 1.1.1

Flourish 1.1.1 completed its GitHub release workflow and uploaded all six packages. Source commit [05ebb4d282eea7130fc9326ef2d15bf43593d5aa](https://github.com/Evigila/Flourish/commit/05ebb4d282eea7130fc9326ef2d15bf43593d5aa) is tagged v1.1.1. [Run 37540483235](https://github.com/Evigila/Flourish/actions/runs/37540483235) completed successfully: [build job 112531999414](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112531999414) passed, including the corrected CSS fixture; [publish job 112533073982](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112533073982) completed Trusted Publishing login and uploaded the six packages in manifest order with Created responses from 22:28:55 to 22:29:01 UTC on 2026-10-06.

All six Flourish 1.1.1 public flat-container indexes now return HTTP 200 and contain 1.1.1. Fresh-cache, public-only verification completed with exit code 0 across FrameworkOnly, MetaNative, MetaDesign and MetaCulture: all 129 checks passed, covering DI activation, SSR, CSS/woff2/JavaScript assets, three languages and generated keys. Evidence: Flourish/artifacts/public-culture-release-1.1.1.log and Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6. Its NuGet.Config contains one public NuGet.org source with a wildcard mapping. Every .nupkg.metadata for the six Flourish 1.1.1 packages and Essential.Blazor/Core/Generator 1.3.0 identifies https://api.nuget.org/v3/index.json; no local feed or source project reference is used. Essential's independent public-only consumer also passed. Publication, public indexing and public consumption are complete for both releases.

All six packages use 1.1.1 in manifest order: Arkheide.Flourish.Core, Arkheide.Flourish.Blazor.Abstract, Arkheide.Flourish.Extensions.Culture.Blazor, Arkheide.Flourish.Blazor.Framework, Arkheide.Flourish.Blazor.Design and Arkheide.Flourish.Blazor. Essential stays 1.3.0; Flourish WPF is excluded. Both earlier v1.1.0 runs skipped publication and their tag remains unchanged.

The complete 1.1.1 preparation completed with exit code 0 and zero warnings/errors: Core 367, Blazor 373/373, bridge 12, Gallery 7,234 including 124 actual-event localization/state checks, Node 66, CSS bundle 21 and SDK integration 194, launcher 95, catalog 21,217 and four isolated package consumers totaling 129 checks. Evidence: artifacts/culture-release-1.1.1.log and artifacts/package-consumers/546b4983f8404423b078f00b5526ab74. The successful release build also passed the previously failing CSS fixture. The earlier bug-report limit on CI/tag-run confirmation is superseded by this successful run; public Flourish consumption is also verified; physical browser acceptance remains user-operated.

The workflow reads vars.NUGET_USER || secrets.NUGET_USER under environment nuget. Profile Evigila is configured, and runtime login/upload success verifies this exact release scope without a local long-lived API key. Generated application keys continue to come from one owning Culture.json with en-US/zh-CN/pt-BR; the bridge and Generator are consumed transitively through the production package graph.

Manual acceptance remains user-operated: original Gallery start.bat, icons/styles/layout, same-page H1/H2/body/status/validation translation with retained inputs/selections, access navigation and representative input/multi-select/table/menu controls. No Computer Use was performed. All public indexing and automated public-only consumption checks are complete.

Gallery's conventional bin/obj Release build also passed public restore/build with TreatWarningsAsErrors=true: zero warnings/errors. Command: dotnet build src/Gallery.Flourish.Blazor/Gallery.Flourish.Blazor.csproj -c Release --configfile artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6/NuGet.Config -p:TreatWarningsAsErrors=true. Evidence: Flourish/artifacts/gallery-public-build-1.1.1.log. This verifies the normal build-output path; Gallery was not launched in a GUI and no actual browser/manual acceptance is claimed.

## Final public verification and documentation boundary

The public-only command & ./build/Test-BlazorPackageConsumers.ps1 -PublicSource -Version 1.1.1 completed with exit code 0. Its single-source NuGet.Config maps all packages to NuGet.org and rejects local package-directory parameters; no project reference or sibling feed participates. All six Flourish package metadata entries and the three transitive Essential metadata entries identify public https://api.nuget.org/v3/index.json.

The initial public attempt failed NU1100 because duplicate names for the same NuGet.org URI collapsed to a restrictive Flourish.* mapping, excluding Microsoft.AspNetCore.App.Internal.Assets 10.0.11. Failed fixture Flourish/artifacts/package-consumers/d1afce7bb6784b64be6730553c6e64a3 retains its original config/assets/diagnostics. The explicit PublicSource verification mode fixes that fixture configuration without altering default local/CI modes or any published nupkg. Final evidence is Flourish/artifacts/public-culture-release-1.1.1.log and Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6.

The published tags retain their original source commits. Final technical documentation and the public-verification helper correction will be committed separately under the user's existing commit/publication authorization; they do not create a new release tag or republish packages. Future commits and publication follow AGENTS.md and the user's task-scoped authorization.

## Historical preparation, recovery and configuration snapshots

The retained sections below predate the successful v1.1.1 upload. Their pending source/commit/policy/tag/publication statements, version-specific hashes and counts describe those earlier runs. Current completed release evidence is above; upload, indexing and public consumption have each passed. The pending statements below describe only their dated earlier snapshots.

## Previous recovery: 2026-10-06 Flourish 1.1.1

The six Essential 1.3.0 packages have been uploaded by successful Trusted Publishing run 37537838178, indexed publicly, and consumed from a fresh public-only NuGet cache. Flourish v1.1.0 runs 37539504631 and 37539506569 failed in the isolated CSS package fixture; both publish jobs were skipped. Existing tags are preserved.

The CSS fixture no longer lists a default user cache as a package feed. A controlled absent-source reproduction produced NU1301; the corrected fixture passes all 194 SDK integration checks using its generated local feed alone. SDK failure output is now included in CI diagnostics. Evidence: artifacts/css-assets-786fc32fa3ad471dbc851e043a8050e3/package-build.log and artifacts/css-assets-a5d5845f3a434c979c37fa2a1bb2c0ae.

Flourish's synchronized version is now 1.1.1, with the same six Core/Blazor packages and Essential 1.3.0 dependency. Complete preparation is in progress using public Essential dependencies; publication and public Flourish package consumption are pending. The following 1.1.0 preparation sections retain their original scope and do not claim publication.

## Previous preparation: 2026-10-06 Culture package integration

This is the current local release preparation; the older sections below retain their historical scope. The complete staged Flourish preparation completed with exit code 0, zero build warnings/errors and exactly six fresh Core/Blazor 1.1.0 packages. No WPF package is in the manifest. Local packages and successful consumers do not establish public NuGet publication or account-policy completion.

| Check | Result |
|---|---|
| Core automated tests | 367 passed |
| Blazor component checks | 373/373 passed |
| Culture bridge checks | 12 passed |
| Gallery rendered/event localization checks | 7,234 passed, including 124 real-event language-switch and state-retention checks |
| Node behavior checks | 66 passed |
| CSS bundle / SDK integration checks | 21 / 194 passed |
| Component catalog checks | 21,217 passed |
| Four isolated package consumers | 129 checks passed |
| Exact six-package verification | Fresh 1.1.0 candidates passed; WPF excluded |

Evidence: artifacts/culture-final-release.log and artifacts/package-consumers/01e52f8b29ce48f89c139dfc26a2bfbb. Gallery's restored .nupkg.metadata identifies the current artifacts/packages bridge candidate, confirming it was not an older cached package. The initial Culture consumer HTTP 500 was a Minimal API test-fixture binding error: [FromServices] was missing. The fixture correction restored its check without changing production behavior.

Gallery now has one bridge PackageReference at VersionPrefix, with its source bridge and direct Generator references removed. Test-Release first restores/builds the targeted umbrella dependency graph, packs the six libraries and verifies them. It then restores the complete Blazor solution through a temporary NuGet source mapping and fresh cache, verifies Gallery's bridge-package and transitive Essential dependencies, and runs full build/tests/checks. The final phase runs all four NuGet-only consumers. Explicit EssentialPackageDirectory supplies unpublished Essential candidates; no implicit sibling feed or source fallback is used. This ordering resolves first-release Gallery bootstrap before the bridge exists publicly.

Essential's preceding complete preparation passed 106 tests (61 Core, 20 Generator, 21 Blazor, 4 WinUI), five Gallery builds with zero warnings/errors and six fresh 1.3.0 packages. A subsequent metadata-only repack verified the current Essential.Culture URL in all six nuspec files; functional tests were not repeated for metadata changes.

The user supplied NuGet profile Evigila and is configuring Flourish's Trusted Publishing policy/secret. Current account-policy coverage and configuration completion remain unconfirmed. An Arkheide.Flourish.* policy scope can support creation of the initial package IDs; the release manifest remains strictly the six Core/Blazor packages. Historical OIDC success, environment existence or the profile name alone does not confirm current authorization. The user authorized Trusted Publishing execution, but AGENTS.md still requires the user's answer before a new commit. No new commit, tag, push or public package publication occurred.

Manual acceptance remains user-operated, without Computer Use:

- Start Gallery with the original start.bat and check icons, bundled styles, centered widths and page layouts.
- Switch en-US, zh-CN and pt-BR on the same page; check collapsing H1, H2/body, documentation, sample status and already-visible validation messages while inputs and selections remain unchanged.
- Exercise access-method navigation, saved-account selection, SplitButton menus and login layout.
- Check rounded MaskedInput/StandaloneMaskedInput/SearchAutocomplete controls, MultiSelectBox search/add/select, table display/advanced-edit controls and selection retention.
- After publication, restore a clean consumer solely from public NuGet sources and repeat culture switching/static-asset checks; current local evidence does not perform that public-index acceptance.

## Historical verification snapshots from 2026-10-05 and earlier 2026-10-06 audit

The following results and configuration observations are retained as dated history. Their earlier current/latest wording, hashes, package scopes and pending requests describe those runs; the latest section above and active release guide supersede them.

This records local verification, not NuGet.org publication. The approved first release now covers six Core/Blazor packages after Shared consolidation and addition of the dependency-only Blazor convenience package.

## Previous canonical Account table preparation

The [Account table follow-up](currentproject-changelogs/2026-10-05_230030_support-native-canonical-table-actions.md) passes the complete final scripts/Test-Release.ps1: Release compilation zero warnings/errors; Core 367, Blazor 269/269, Culture bridge 12; all ten configured JavaScript files, 53 Node-runner checks and existing mock suites including 46 controls-DOM cases; CSS bundle 21 and SDK integration 194. Six local 1.1.0 candidates pass identity/dependency/static-asset verification. Four fresh isolated NuGet-only modes pass 95 checks at artifacts/package-consumers/94244e241b8f4867a9daa3793b54b429. Expected package README advisories remain separate from warning-free compilation.

Catalog coverage is 98 components, 741 parameter rows and 326 defaults. Six strict Development HTTP routes pass: DataTable, ActionMenu, LoadingState, Field, UniformGridButton and Pricing. The served Framework/Design bundles and DataTable module pass the native in-flow fallback, compact ContentSurface heading and genuine native activation contracts. Temporary Gallery was stopped afterward. These nine HTTP checks and mock DOM tests are not physical browser/popup acceptance.

Current SHA256: Framework 696850A12C19CF785DC1F8D1157359490BBA2027F118E1D1756D190012385329; Design 0BBDC8735FE1677DE941AB19969C7F9417157455E4EB32345C7832F62D11A748. Colligere restores verified candidates to artifacts/nuget/c19c3ebd9982ec1b691e73a7a0d418ff, builds its complete Release solution/final Web tests with zero warnings/errors, and passes 391 focused regressions with zero failures. Two PostgreSQL redemption cases are skipped because the isolated test environment is unset. Evidence: artifacts/test-results/account-standard-controls/account-standard-controls-accepted.trx. Its running old instance is retained; no Debug/live Aspire restart or database acceptance is claimed.

The initial preparation found an unreachable specialized Gallery parameter-description arm and an unrecognized new selector in the deliberately strict SplitButton CSS-cascade test. The description now precedes its generic fallback; the cascade test explicitly models the new native menu branch and still guards open/closed SplitButton and unrelated popover behavior. No analyzer, test or release gate is disabled. Essential, desktop scope and dependency versions are unchanged; no new commit, push, public upload or deployment occurred. Broader advanced-table capability convergence remains outstanding.

## Previous Pricing component preparation

The [Pricing follow-up](currentproject-changelogs/2026-10-05_221717_refine-pricing-composition-and-root-scroll.md) passes the complete final scripts/Test-Release.ps1 preparation: Release build zero warnings/errors; Core 367; Blazor 259/259; Culture bridge 12; all ten configured JavaScript files, including 53 Node-runner checks and the existing mock suites; CSS bundle 21 and SDK integration 194. Six local 1.1.0 candidates passed package identity/dependency/static-asset validation. Four fresh isolated NuGet-only consumers passed 95 checks at artifacts/package-consumers/8068db7229dc44e58ae790505dbcfe11. Expected package README advisories remain separate from warning-free compilation.

The catalog covers 98 components, 737 parameter rows and 326 defaults. Strict Development HTTP checks passed six actual routes: Pricing, product, login and the Card, OfferStage and PresentationFooter guides. Both actual Framework/Design CSS bundles passed the root-scroll, title, prominent-card and Underline foreground contracts. The owned Gallery process was stopped; browser geometry and physical touchpad acceptance remain manual without Computer Use.

Current candidate SHA256: Framework 2BE9B5B1E13EC610B892774D895A447A7C2D6BE87759C1248C023D187D82D907; Design 04385A7C1FFAAAE87CD856371A3AD41F70D889D3B461337883D1BB41A247F470. Colligere restored verified candidates to artifacts/nuget/e18a71e169832473771a21746699fd72, built the complete Release solution and final Web test project with zero warnings/errors, and passed 249 focused Web regressions. Evidence: artifacts/test-results/pricing-presentation/pricing-presentation-accepted.trx. Its existing running instance was retained at the user's request; no Debug build or live Aspire restart is claimed. Essential, desktop release scope and dependency versions are unchanged. No new commit, push, public upload or deployment occurred.

## Previous login component preparation

The [login follow-up](currentproject-changelogs/2026-10-05_215327_add-logo-displayer-and-native-split-menus.md) passes the complete scripts/Test-Release.ps1 preparation: Release build zero warnings/errors; Core 367; Blazor 250/250; Culture bridge 12; all ten configured JavaScript files, including 53 Node-runner checks and the existing mock suites; CSS bundle 21 and SDK integration 194. Six local 1.1.0 candidates passed package validation. Four fresh isolated NuGet-only consumers passed 95 checks at artifacts/package-consumers/9639717e6d2249bd9837a9c95c1cf2c4. Expected package README advisories remain separate from compilation.

The catalog covers 98 components, 733 parameter rows and 324 defaults. After a final Gallery-only adjustment removing its duplicated login Back action, Gallery rebuilt in Release with zero warnings/errors. Five actual routes passed strict Development HTTP checks: login, accounts, access and the LogoDisplayer/SplitButton guides. The two actual bundled Framework/Design CSS endpoints returned 200 with CSS MIME and the current full-height, native-open and artistic-name rules. Individual source CSS partials are not public bundle endpoints. The owned Gallery process was stopped; browser geometry and interactive visual acceptance remain manual.

Current candidate SHA256: Framework 65B97E2B16D7C26AAFF1D5460FA7F2EC6D10381E4048BE9045440EA1B9F27211; Design BA1EF8CDBE94504C4192268B43CF040F1DE10F8265ADF4A742B2CDB9835B8446. Colligere restored verified candidates to artifacts/nuget/5cdc531239b9a8431147911d306a58bb, built its complete Release solution with zero warnings/errors and passed 248 focused Web regressions. Its existing running instance was retained at the user's request; no live Aspire restart is claimed. Essential, desktop release scope and dependency versions are unchanged. No new commit, push, public upload or deployment occurred.

## Previous banner and footer preparation

The [banner and footer follow-up](currentproject-changelogs/2026-10-05_212420_standardize-banners-and-project-footer.md) reuses the existing production controls. The complete scripts/Test-Release.ps1 run passed: Release solution and Gallery build with zero warnings/errors, Core 367 tests, Blazor 234/234, Culture bridge 12, all ten JavaScript files (53 Node-runner checks and the existing mock suites), 21 CSS bundle checks and 194 CSS SDK integration checks. All six local 1.1.0 candidates passed package verification; four fresh NuGet-only consumers passed 95 checks at artifacts/package-consumers/5871b0bae0ab481b916ceb6832a8bb25. Expected package README advisories remain distinct from warning-free compilation.

The catalog retains 97 exported components and now documents 712 parameter rows and 315 defaults. Strict Development HTTP checks passed seven actual Gallery routes: product, pricing, access, account selection and the three presentation-control guides. Additional checks confirm the opt-in dotted sample and automatic Gallery title/watermark. These are SSR/static contracts, not visual browser acceptance. The task-owned Gallery process was stopped afterward.

Current local SHA256: Framework 499D62016504047E8C0FB072E9360ED6B1CDFF8C251922E9441018F09C034BAC; Design 1EC7CB8AC744E0833DB03CEBDEA59BF6A2CDC35FBAD181A258D899F3E7238F55. Essential and the desktop release scope are unchanged. Public NuGet publication, push and deployment remain outside this preparation.

## Previous Flourish-only final review preparation

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

## Previous Blazor-only preparation

The final scripts/Test-Release.ps1 completed with exit code 0 after consolidation, release-scope and consumer-regression fixes. Release build reported zero warnings/errors. Core tests: 367; Blazor checks: 135/135; Culture bridge checks: 12; DOM/browser tests: 38 plus passing interaction/mock suites; CSS bundle checks: 21; SDK asset checks: 194. Exactly six 1.1.0 packages passed identity, dependency, assembly/meta and static asset validation.

Four isolated NuGet-only consumers passed 67 registration/SSR/HTTP checks: Framework alone; convenience package with no Design registration; convenience package with Design; convenience package plus optional Culture bridge. Core restored transitively, Shared was absent, and only explicit registration activated Design. Initial consolidation evidence: artifacts/package-consumers/728f83c9c57140e1a850a87b67697989. A subsequent complete run after host-contract fixes (filled icons, native action grid, card value lines and hidden-column sorting) passed with evidence at artifacts/package-consumers/c40460ce21d94457b5f8fa3541084c7c. The remaining explicit sort-button ARIA-state follow-up is recorded separately when its verification finishes.

WPF and its Culture bridge remain source projects but are outside this release's package set and validation solution. Earlier full desktop/eight-package checks below are retained as historical evidence, not the current release contract. See the [new consolidation record](currentproject-changelogs/2026-10-05_115734_consolidate-blazor-packages-and-release-scope.md) and [current release guide](nuget-release-integration.md). No tag, push or public release was performed.

The ARIA/controller-order follow-up passed the complete preparation: explicit true/false sort/reorder ARIA states are tested in actual SSR, and the controller order is sorting, advanced editing, then Display. Its six-package validation and four-consumer 67-check evidence is artifacts/package-consumers/dd0ad177e0c94552bd390cbd3c9564ae. This supersedes the pending-verification note above.

The latest complete preparation additionally restores the original pagination contract: two page-button groups but only the top group's range/aria-live announcement. The bottom Pager explicitly sets ShowRange=false; an actual SSR regression guards against duplication. Final evidence: artifacts/package-consumers/aae97a0da9ff43ab82710e083fe5c955; all four consumers passed 67 checks. Final Framework package SHA256: 77C780A168A8EA21DA3C4D306408285127324CEFFA26A70DE557AC5BEDB82DBA.

## Previous header button variant verification

The subsequent [header button variant correction](currentproject-changelogs/2026-10-05_162957_preserve-header-button-variants.md) repacked Design only, narrowing chrome overrides to available Quiet actions so explicit Elevated/other variants retain their paint. Its local Design SHA256 is D403A90E32BBCF0C509E240E3EFE9F29297A63F070E069431E005A03A62E6713. Blazor 135 checks, all nine JavaScript files, 21 CSS bundle checks, six-package verification and 72 focused Colligere consumer regressions passed. This is not a new complete release run or public upload; the unchanged Framework hash and earlier four-mode consumer evidence below retain their original scope.

## Previous all-project naming verification

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

## Previously observed GitHub and NuGet configuration

The coordinating task successfully created Evigila/Flourish's nuget environment. Its returned configuration had protection_rules=[] and branch_policy=null. NUGET_USER remains unconfigured; the requested NuGet profile username is still pending with the user. No API key was requested.

Essential already has the nuget environment and NUGET_USER secret name. Its environment likewise has no configured protection rules or branch policy. Secret values were not retrieved, and the NuGet.org account-side trust policies were not verified.

A pre-consolidation read-only query of the then-configured 14 IDs found no public target versions: all six Essential 1.3.0 and eight Flourish 1.1.0 versions were unpublished at the time of that query. Five existing Essential platform packages had latest version 1.2.0; Culture.Blazor and all eight Flourish IDs returned 404. This query is historical evidence, not a statement that eight Flourish packages are still in scope. The current required sequence is Essential's six 1.3.0 packages, then Flourish's six Core/Blazor 1.1.0 packages (including the optional Culture bridge), then the application's public-feed restore. The pending username, account policy/package scope and separately authorized Publish step remain release prerequisites. Environment creation alone does not establish publishing readiness or add an approval requirement.

## Approval review and Git boundary

Automatic approval review rejected execution of a Publish-mode negative test because that helper can fetch, create tags and push. A read-only PowerShell AST guard inspection was used instead; the Publish helper was not executed for the negative test.

No commit, release tag, push or NuGet publication was performed. The final Prepare and HTTP checks are local verification only. See the release guide for the clean master/origin/master and exact-tag confirmation requirements before any future Publish action.

## 2026-10-06 Trusted Publishing correction and current evidence

This section supersedes treating the earlier secret/profile request or local API-key note as the current release boundary. The user has chosen agent-executed Trusted Publishing. Both repositories already use NuGet/login@v1 with NUGET_USER, id-token: write and the nuget environment. NUGET_USER is a NuGet.org profile username; no local long-lived API key, extra nuget.exe or GitHub CLI is required. New source commits still require the user's answer under AGENTS.md.

Read-only checks identified the current repositories as Evigila/Essential.Culture (ID 1327295083) and Evigila/Flourish (ID 1246107902), using build.yml and nuget policy fields. Both public environment API responses confirmed existence, zero protection rules and null branch policy. Secret presence/value and the current NuGet account policy/package permissions remain unverified. In particular, historical success must not be treated as authorization for creating the new Blazor package ID or publishing the new target versions.

The [Essential v1.2.0 run 33030395528](https://github.com/Evigila/Essential.Culture/actions/runs/33030395528) completed Trusted Publishing login and push steps successfully. The [Essential master run 37291575812](https://github.com/Evigila/Essential.Culture/actions/runs/37291575812) succeeded without publishing. The [Flourish run 37291596951](https://github.com/Evigila/Flourish/actions/runs/37291596951) failed during its older full-solution restore because Essential.Blazor was absent and Wpf 1.3.0 was not published; publish was skipped before OIDC login. This is not a Trusted Publishing authentication failure or evidence of the result of today's focused release preparation.

The active [release guide](nuget-release-integration.md) now records exact package scope/order, the umbrella's automatic Culture bridge, explicit EssentialPackageDirectory instead of an implicit sibling feed, isolated ArtifactsPath and the pending Gallery source-bridge consumption migration. Earlier preparation totals and external observations retain their dated scope. This documentation/audit task performed no new commit, release tag, push or NuGet publication; current preparation and public-index checks must be reported separately.

## Completed 1.1.1 preparation on 2026-10-06

The corrected full preparation completed with exit code 0 using public NuGet Essential 1.3.0 dependencies. Production and validation builds have zero warnings/errors. Core 367 tests, Blazor 373/373 checks, Culture bridge 12 checks, Gallery 7,234 checks including 124 actual-event localization/state checks, Node behavior checks, CSS bundle 21, CSS SDK 194, launcher 95, culture/catalog 21,217 and all four package consumers totaling 129 checks passed. Exactly six fresh Core/Blazor 1.1.1 packages passed verification; no Flourish WPF package was prepared.

Evidence: artifacts/culture-release-1.1.1.log and artifacts/package-consumers/546b4983f8404423b078f00b5526ab74. The fixture correction has passed local isolated checks and complete release preparation. The new GitHub tag run and public Flourish consumption remain separate acceptance steps; this preparation does not claim their success.
