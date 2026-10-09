# NuGet release and integration

## Current source Culture integration after 2026-10-09

The user requested a later same-day redesign of browser culture persistence. ConfigureCulture is now an IFrameworkBuilder extension owned by Flourish.Extensions.Culture.Blazor; AddFlourishCulture and AddFlourishPreferences are removed without aliases. The optional Razor extension owns catalog loading, request negotiation, CultureSession, LocalizedComponentBase, LanguagePicker and its packaged browser-preferences.js. Essential.Culture.Blazor remains its internal single lookup/formatting dependency at 1.3.0. Framework retains provider-neutral contracts and has no Essential dependency.

The extension's project/package dependency changes from Abstract to Framework, which restores Abstract/Core transitively. The six package IDs and stable source VersionPrefix 1.1.3 remain unchanged; release manifest order now places Framework and Design before the Culture extension, then the umbrella. No release or new package version is implied by the source redesign. All publication/indexing/consumer results below are dated evidence for their named versions, not evidence that the new source contract has been published.

Gallery now uses same-repository ProjectReference entries, including the Culture extension, so its executable examples use current source rather than a cached bridge package. External Essential Core/Blazor/Generator remain package dependencies. Independent FrameworkOnly, MetaNative, MetaDesign and MetaCulture fixtures continue to restore and test actual NuGet packages with isolated caches. MetaCulture must use ConfigureCulture, the extension-owned selector and its own packaged static asset.

The host-facing service setup is AddFlourishFramework(builder.Configuration, framework => framework.ConfigureCulture(...)), optional AddFlourishDesign(builder.Configuration), and host business registrations. Framework loads optional appsettings.Flourish.json below host/business/environment/command-line overrides; the extension configures request middleware internally. Hosts supply only their application catalog identity/resource, explicit keys and relevant defaults. Per-browser cookie preferences never rewrite server JSON. See [Culture Web integration](culture-web-integration.md) for the current API and manual acceptance.

## Current stable release: 1.1.3

The user authorized publication of the earlier layout candidate and the table-width fixes on 2026-10-07. Source 6b5dc255fb97d4313da7e11bce9bab820cc06256 is tagged v1.1.3; [run 37653211373](https://github.com/Evigila/Flourish/actions/runs/37653211373) successfully uploaded the same six Core/Blazor packages. All six public indexes contain 1.1.3, and fresh package/HTTP caches with NuGet.org as the only source passed 129 consumer checks. Essential remains 1.3.0; WPF is excluded. Colligere may upgrade both central entries to 1.1.3 and remove its local candidate feed. Earlier candidate restrictions and version-specific sections below are dated history, superseded for this Blazor release. See [the width and portal report](bugfix-reports/2026-10-07_table-widths-and-portal-composition.md).

## Native WPF supersession on 2026-10-07

The former WPF single-project host/service implementation and its Culture hosting adapter have been deleted under the user's explicit reconstruction request. Current native projects are Flourish.WPF.Abstract, Framework, Design and the dependency-only Flourish.WPF aggregate, targeting net10.0-windows. The rebuilt optional EssentialTextProvider is owned/disposed by its native consumer and configured on FrameworkBuilder; Gallery consumes this bridge. Earlier WPF descriptions below are dated migration evidence, not current APIs. See [native WPF integration](wpf-native-integration.md).

The existing public six-package Core/Blazor 1.1.2 release and scripts/ReleaseSettings.psd1 remain unchanged. Native WPF build/pack verification creates local candidates only; this task does not publish, version-bump or add Windows targets to the already released Blazor packages.

## Current release: Blazor 1.1.2

The user approved release of the previously deferred source corrections and new generic identity/access/page/grid capabilities. Directory.Build.props owns VersionPrefix 1.1.2; Essential remains 1.3.0. The existing six-package manifest, IDs and dependency order remain unchanged. Full local preparation passed with zero build warnings/errors. Source commit 108fbff8950925b909a70b0b9830226ae8226521 and tag v1.1.2 are pushed; [run 37560362408](https://github.com/Evigila/Flourish/actions/runs/37560362408) successfully uploaded all six packages. Public indexing and independent public-only consumer verification are complete: 129 checks passed with fresh package and HTTP caches in artifacts/package-consumers/b06a15d2f8fc4661be3792ff26ebf464. [Release verification](release-verification.md) records the complete gate and propagation/cache evidence. Colligere may upgrade its two central entries together; no source/local-feed substitution is required. The completed 1.1.1 release below is historical evidence.

## Local validation candidate

The active user-authorized candidate is `1.1.4-preview.fields.3`, retaining Field.Actions and Boolean conversion from `.1`/`.2` and correcting direct PageBody InlineActions width. Its six packages are generated into `../Colligere/artifacts/local-nuget/1.1.4-preview.fields.3`; both Colligere central entries and its Flourish-only source mapping consume that feed. Directory.Build.props retains stable VersionPrefix 1.1.3. Candidate generation overrides Version=1.1.4-preview.fields.3, VersionPrefix=1.1.4 and VersionSuffix=preview.fields.3, with isolated SDK output/intermediates. Restore the umbrella graph before packing its Culture bridge. Verify-PackageSet confirms all six IDs, matching internal versions and required assets; restored Colligere metadata identifies the same local feed. Essential remains 1.3.0; WPF is excluded. Public 1.1.3 and both prior local candidates are preserved. No public upload, tag, push or commit is authorized by candidate generation. See the [page-action report](bugfix-reports/2026-10-08_pagebody-inline-actions-width.md) and historical [selection report](bugfix-reports/2026-10-07_boolean-select-native-conversion.md). Local verification does not replace public release preparation or indexing checks.

## Previous release state: 1.1.1

Flourish 1.1.1 completed its GitHub release workflow and uploaded all six packages. Source commit [05ebb4d282eea7130fc9326ef2d15bf43593d5aa](https://github.com/Evigila/Flourish/commit/05ebb4d282eea7130fc9326ef2d15bf43593d5aa) is tagged v1.1.1. [Run 37540483235](https://github.com/Evigila/Flourish/actions/runs/37540483235) completed successfully: [build job 112531999414](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112531999414) passed, including the corrected CSS fixture; [publish job 112533073982](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112533073982) completed Trusted Publishing login and uploaded the six packages in manifest order with Created responses from 22:28:55 to 22:29:01 UTC on 2026-10-06.

All six Flourish 1.1.1 public flat-container indexes now return HTTP 200 and contain 1.1.1. Fresh-cache, public-only verification completed with exit code 0 across FrameworkOnly, MetaNative, MetaDesign and MetaCulture: all 129 checks passed, covering DI activation, SSR, CSS/woff2/JavaScript assets, three languages and generated keys. Evidence: Flourish/artifacts/public-culture-release-1.1.1.log and Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6. Its NuGet.Config contains one public NuGet.org source with a wildcard mapping. Every .nupkg.metadata for the six Flourish 1.1.1 packages and Essential.Blazor/Core/Generator 1.3.0 identifies https://api.nuget.org/v3/index.json; no local feed or source project reference is used. Essential's independent public-only consumer also passed. Publication, public indexing and public consumption are complete for both releases.

Essential.Culture 1.3.0 is published, publicly indexed and verified by fresh-cache public-only consumption. Source commit [983c636bdba7e1a7b74cee33b5c682c9bf06364e](https://github.com/Evigila/Essential.Culture/commit/983c636bdba7e1a7b74cee33b5c682c9bf06364e) is tagged v1.3.0. [Run 37537838178](https://github.com/Evigila/Essential.Culture/actions/runs/37537838178) succeeded, including Trusted Publishing login and six Created uploads. All six public flat-container indexes returned HTTP 200 and contain 1.3.0.

The public-source-only consumer at artifacts/public-consumers/blazor-1e4f292745bf4a2fbe68954d77b4132a restored into a fresh cache, built with warnings as errors and ran successfully. Its only direct package is Arkheide.Essential.Culture.Blazor; Core and Generator restore transitively. Generated keys, independent scoped languages, formatting culture and encoded rendering passed.

Both v1.1.0 Flourish runs failed in the isolated CSS package fixture before publication. The fixture incorrectly treated the default user package cache as a source; an absent-source reproduction returned NU1301. The corrected fixture uses its generated feed and preserves the actual assertions. The v1.1.0 tag was retained and all six Flourish versions moved together to 1.1.1. No production CSS/control change was needed for this recovery.

## Gallery ChangeLog maintenance

Gallery `/changelog` reads the embedded `src/Gallery.Flourish.Blazor/Models/ChangeLog.json`; each record references concise three-language `ChangeLog_*` keys in the existing Gallery catalog. The initial `v1.1.0` tag has no record. Stable notes describe changes since the preceding stable tag; the highest record describes changes since the latest tag and uses the next patch version with `-preview`.

Before each release, promote the reviewed preview to the intended stable `VersionPrefix`, preserve older records, and add the next patch preview first in descending order. An empty next preview is valid and displays the localized no-changes caption. Add later unreleased changes to that preview. The runtime does not require Git or fetch remote tags; release text is reviewed source content.

`build/Test-ChangeLog.ps1` checks unique ordered versions, exactly one next preview, nonempty stable notes, existing translations and reachable stable-tag coverage excluding the initial tag. It runs in `Test-Release`/CI and in `Publish-Helper` before preparation and again after fetching tags before publication, including `-SkipBuild`. It validates content presence, not the editorial accuracy of a summary; review notes against the tag range before publishing. This page does not change package versions or publish tags.

## Package scope and dependency graph

Essential's six 1.3.0 packages were released first: Generator, Core, Wpf, Avalonia, WinUI and Blazor under the Arkheide.Essential.Culture IDs. Flourish restores Essential from public NuGet; it does not build a sibling Essential source checkout.

The earlier 1.1.1 release published the Abstract-only Culture adapter before Framework. The current source manifest below supersedes that dependency/order for future preparation while preserving the published package history.

| Publish order | Current package ID | Dependency boundary |
|---|---|---|
| 1 | Arkheide.Flourish.Core | Shared generic foundations |
| 2 | Arkheide.Flourish.Blazor.Abstract | Core |
| 3 | Arkheide.Flourish.Blazor.Framework | Abstract; no Essential dependency |
| 4 | Arkheide.Flourish.Blazor.Design | Framework/Abstract; no Essential dependency |
| 5 | Arkheide.Flourish.Extensions.Culture.Blazor | Framework and Essential.Culture.Blazor 1.3.0; owns its Razor control/catalog and browser persistence asset |
| 6 | Arkheide.Flourish.Blazor | Dependency-only umbrella: Abstract, Framework, Design and Culture bridge |

Exactly these six Core/Blazor packages are in scope. Flourish WPF and its WPF Culture bridge are excluded. Shared is retired. The umbrella installs the Culture extension automatically; the extension brings Essential.Blazor, Core and Generator transitively. Install Framework alone to omit both Design and Culture. Installation does not activate optional services: hosts call AddFlourishFramework, ConfigureCulture inside its callback when desired, and AddFlourishDesign separately when desired. A Web host receives standard interactive Razor registrations through Framework; service-only registration does not construct a host.

Flourish Directory.Build.props owns the current stable source VersionPrefix=1.1.3 and EssentialCultureVersion=1.3.0; it used VersionPrefix=1.1.1 for that historical release. Essential's module props own VersionPrefix=1.3.0. Each scripts/ReleaseSettings.psd1 fixes the IDs and publication order. Arkheide.* NuGet IDs remain unchanged by product-first project names or the renamed Essential.Culture GitHub repository.

## Package consumption and local preparation

The earlier package-only Gallery bridge adoption is superseded on 2026-10-09. Gallery references current source Flourish projects, including the Culture extension, and continues receiving Generator transitively from the existing Essential packages. Restore assertions now require the extension as a project and Essential.Blazor/Core/Generator as packages. Four independent NuGet consumers verify Framework alone, umbrella without Design registration, explicit Design registration and Culture activation; these fixtures retain the production packaging boundary.

Test-Release restores/builds the targeted umbrella's six-library graph and packs/verifies the six candidates, restores the full source-based Blazor solution with isolated output/cache settings and asserts Gallery/Essential dependency identities, runs the build/test/JavaScript/CSS/catalog/launcher checks, then runs all four isolated package consumers. Source Gallery validation and packaged consumer validation establish separate boundaries; Gallery no longer needs a new bridge package merely to compile its current source.

```powershell
Set-Location C:\Users\RC_Auditoria\source\Repos\Flourish
.\scripts\Test-Release.ps1 -ArtifactsPath .\artifacts\release-build
```

For future unpublished Essential candidates, explicitly add -EssentialPackageDirectory ..\Essential\artifacts\packages. It supplies a NuGet feed, never source ProjectReference dependencies. The implicit sibling feed and retired source-selection switches are removed. ArtifactsPath isolates both SDK output and intermediate files; do not override only OutDir. -VerifyOnly inspects existing packages; helper -SkipBuild retains Publish's Git/tag guards and cannot prove artifacts match current source.

The complete 1.1.1 preparation passed with exit code 0 and zero build warnings/errors: Core 367, Blazor 373/373, bridge 12, Gallery 7,234 including 124 actual-event language/state checks, Node 66, CSS 21/194, launcher 95, catalog 21,217 and four consumers totaling 129 checks. Evidence: artifacts/culture-release-1.1.1.log and artifacts/package-consumers/546b4983f8404423b078f00b5526ab74. The subsequent public-only four-mode consumer passed separately, establishing consumption of the indexed packages rather than only these local candidates.

## Public-only consumer verification

```powershell
Set-Location C:\Users\RC_Auditoria\source\Repos\Flourish
& .\build\Test-BlazorPackageConsumers.ps1 -PublicSource -Version 1.1.1
```

PublicSource uses one NuGet.org source and wildcard package mapping, with a new fixture/cache for each run. It rejects combination with PackageDirectory or EssentialPackageDirectory. The default local/CI candidate modes remain unchanged. The final command passed all 129 checks; the resulting metadata verifies that all Flourish and transitive Essential packages were restored from public NuGet.org.

The first public-consumer attempt registered the same NuGet.org URI as both Flourish and nuget.org. NuGet merged the duplicate source and retained a restrictive Flourish.* mapping, preventing restore of Microsoft.AspNetCore.App.Internal.Assets 10.0.11 with NU1100. The explicit PublicSource mode corrects the verification configuration; it does not change published packages. Failed fixture artifacts/package-consumers/d1afce7bb6784b64be6730553c6e64a3 retains its original NuGet.Config, FrameworkOnly/obj/project.assets.json and NU1100 diagnostics. The final successful fixture is artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6.

## Trusted Publishing configuration

Both repositories use .github/workflows/build.yml and the GitHub environment nuget. Tag-only publish jobs require a successful build and permissions contents: read plus id-token: write. NuGet/login@v1 reads vars.NUGET_USER || secrets.NUGET_USER: an Actions Variable takes priority, otherwise the existing Secret is used. NUGET_USER is the NuGet.org profile username; the user confirmed Evigila. The username is not hard-coded. Login supplies a temporary NUGET_API_KEY for dotnet nuget push; no long-lived local API key, extra nuget.exe or GitHub CLI is required.

| Policy field | Essential | Flourish |
|---|---|---|
| Owner | Evigila | Evigila |
| Repository | Essential.Culture | Flourish |
| Repository ID | 1327295083 | 1246107902 |
| Workflow filename | build.yml | build.yml |
| Environment | nuget | nuget |

The workflow field is build.yml without .github/workflows/. Successful login and Created uploads establish authorization for these exact released package sets, including first creation of the new IDs. They do not authorize unrelated future packages. An Arkheide.Flourish.* policy scope can support initial ID creation while the release manifest/verifier restrict publication to exactly the six Core/Blazor packages. See [official Trusted Publishing guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

## Single-file translations

Maintain one Culture.json per owning project, with en-US, zh-CN and pt-BR translations under each semantic key. Gallery's Localization/Culture.json is both the Generator AdditionalFile and an embedded resource, Gallery.Texts.json. The extension's transitive Essential packages supply Generator; no direct Generator reference is required. Register the application resource with ConfigureCulture(culture => culture.AddCatalog<Program>("Gallery", "Gallery.Texts.json").SetDefaultCatalog("Gallery")), then resolve generated string tokens through the scoped CultureSession.Parse service. Generator emits a static Key class; TextKey is Gallery's using alias for that generated class, not a runtime key type. For example, TextKey.Page_RecordCreate returns Key.Page_RecordCreate. CultureSession exposes Culture as a UI-culture string and FormatCulture as CultureInfo; Essential runtime registration/services remain internal implementation details.

Framework owns its separate single Culture.json for standard captions and usage descriptions, embedded as Flourish.Blazor.Texts.json. The extension owns its Culture.json for LanguagePicker/feedback/usage, embedded as Culture.Texts.json. ConfigureCulture automatically registers both under Flourish and Culture identities and adapts the provider-neutral ITextProvider through CultureSession. Framework still has no dependency on Essential. Application and library catalog IDs prevent key collisions; no combined cross-project dictionary is required. The extension packages its cookie module under _content/Arkheide.Flourish.Extensions.Culture.Blazor/browser-preferences.js; the former Framework path is retired.

H1, H2, prose, field labels, accessibility text, sample status and validation output must resolve keys when rendered. Do not store already translated sentences in singleton metadata or mutable UI state. The extension's LocalizedComponentBase or Framework's neutral text base reacts to Changed and disposes its subscription; inputs/selections/history retain their business values during language changes. Desktop adapters keep their established bindings. Blazor request negotiation and cookie persistence belong to the optional Culture extension, and personal selection changes only the current request/circuit/browser scope.

## Future release procedure

Prepare is the default of publish-helper.bat / scripts/Publish-Helper.ps1. Publish requires the authorized source commit, a clean tree including untracked files, master, HEAD equal to fetched origin/master, a stable three-part VersionPrefix and no existing matching tag. The helper asks for the exact vVersionPrefix value before creating and pushing an annotated tag. CI checks that the tagged commit is contained in origin/master. Branch pushes, pull requests and workflow_dispatch build/verify only.

The tag-only workflow downloads and re-verifies its artifact before pushing in manifest order with --skip-duplicate. Do not replace existing tags or released versions. Essential v1.3.0, Flourish v1.1.0 and v1.1.1 retain their original commits; the two v1.1.0 attempts skipped publication and the corrected six-package version is 1.1.1. For a partial or failed release, inspect actual workflow/package state and obtain the effective authorization for the next version. Verify workflow success, public indexing and a fresh-cache public-source consumer as separate acceptance steps.

The published tags retain their original source commits. Final technical documentation and the public-verification helper correction will be committed separately under the user's existing commit/publication authorization; they do not create a new release tag or republish packages. Future commits and publication follow AGENTS.md and the user's task-scoped authorization.

## User-operated acceptance

- Start Gallery through the original start.bat and check icons, bundled styles and centered layouts.
- Switch en-US, zh-CN and pt-BR on the same page; check collapsing H1, H2, body, documentation, labels/status and already-visible validation errors without resetting inputs or selections.
- Exercise access-method navigation, saved accounts, SplitButton/login composition, rounded inputs, MultiSelectBox and list display/advanced-edit controls.
- In a new NuGet-only application, manually check the indexed package's static assets, registration modes and culture switching. The automated public-only four-mode check has already passed.

No Computer Use acceptance was performed. [Release verification](release-verification.md) retains current evidence and dated historical snapshots; append-only changes and bug reports remain unchanged. The earlier historical prerequisite statements requiring a local API key, pending username or source-bridge migration are superseded by the current configuration and successful upload evidence above.
