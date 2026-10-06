# NuGet release and integration

## Current release state

Flourish 1.1.1 completed its GitHub release workflow and uploaded all six packages. Source commit [05ebb4d282eea7130fc9326ef2d15bf43593d5aa](https://github.com/Evigila/Flourish/commit/05ebb4d282eea7130fc9326ef2d15bf43593d5aa) is tagged v1.1.1. [Run 37540483235](https://github.com/Evigila/Flourish/actions/runs/37540483235) completed successfully: [build job 112531999414](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112531999414) passed, including the corrected CSS fixture; [publish job 112533073982](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112533073982) completed Trusted Publishing login and uploaded the six packages in manifest order with Created responses from 22:28:55 to 22:29:01 UTC on 2026-10-06.

All six Flourish 1.1.1 public flat-container indexes now return HTTP 200 and contain 1.1.1. Fresh-cache, public-only verification completed with exit code 0 across FrameworkOnly, MetaNative, MetaDesign and MetaCulture: all 129 checks passed, covering DI activation, SSR, CSS/woff2/JavaScript assets, three languages and generated keys. Evidence: Flourish/artifacts/public-culture-release-1.1.1.log and Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6. Its NuGet.Config contains one public NuGet.org source with a wildcard mapping. Every .nupkg.metadata for the six Flourish 1.1.1 packages and Essential.Blazor/Core/Generator 1.3.0 identifies https://api.nuget.org/v3/index.json; no local feed or source project reference is used. Essential's independent public-only consumer also passed. Publication, public indexing and public consumption are complete for both releases.

Essential.Culture 1.3.0 is published, publicly indexed and verified by fresh-cache public-only consumption. Source commit [983c636bdba7e1a7b74cee33b5c682c9bf06364e](https://github.com/Evigila/Essential.Culture/commit/983c636bdba7e1a7b74cee33b5c682c9bf06364e) is tagged v1.3.0. [Run 37537838178](https://github.com/Evigila/Essential.Culture/actions/runs/37537838178) succeeded, including Trusted Publishing login and six Created uploads. All six public flat-container indexes returned HTTP 200 and contain 1.3.0.

The public-source-only consumer at artifacts/public-consumers/blazor-1e4f292745bf4a2fbe68954d77b4132a restored into a fresh cache, built with warnings as errors and ran successfully. Its only direct package is Arkheide.Essential.Culture.Blazor; Core and Generator restore transitively. Generated keys, independent scoped languages, formatting culture and encoded rendering passed.

Both v1.1.0 Flourish runs failed in the isolated CSS package fixture before publication. The fixture incorrectly treated the default user package cache as a source; an absent-source reproduction returned NU1301. The corrected fixture uses its generated feed and preserves the actual assertions. The v1.1.0 tag was retained and all six Flourish versions moved together to 1.1.1. No production CSS/control change was needed for this recovery.

## Package scope and dependency graph

Essential's six 1.3.0 packages were released first: Generator, Core, Wpf, Avalonia, WinUI and Blazor under the Arkheide.Essential.Culture IDs. Flourish restores Essential from public NuGet; it does not build a sibling Essential source checkout.

| Publish order | Flourish 1.1.1 ID | Dependency boundary |
|---|---|---|
| 1 | Arkheide.Flourish.Core | Shared generic foundations |
| 2 | Arkheide.Flourish.Blazor.Abstract | Core |
| 3 | Arkheide.Flourish.Extensions.Culture.Blazor | Abstract and Essential.Culture.Blazor 1.3.0 |
| 4 | Arkheide.Flourish.Blazor.Framework | Abstract; no Essential dependency |
| 5 | Arkheide.Flourish.Blazor.Design | Framework/Abstract; no Essential dependency |
| 6 | Arkheide.Flourish.Blazor | Dependency-only umbrella: Abstract, Framework, Design and Culture bridge |

Exactly these six Core/Blazor packages are in scope. Flourish WPF and its WPF Culture bridge are excluded. Shared is retired. The umbrella installs the Culture bridge automatically; the bridge brings Essential.Blazor, Core and Generator transitively. Install Framework alone to omit both Design and Culture. Installation does not register services: hosts explicitly call AddFlourishFramework, AddFlourishDesign when desired, AddCultureBlazor and AddFlourishCulture.

Flourish Directory.Build.props owns VersionPrefix=1.1.1 and EssentialCultureVersion=1.3.0. Essential's module props own VersionPrefix=1.3.0. Each scripts/ReleaseSettings.psd1 fixes the IDs and publication order. Arkheide.* NuGet IDs remain unchanged by product-first project names or the renamed Essential.Culture GitHub repository.

## Package consumption and local preparation

Gallery now references only the packaged Culture bridge at VersionPrefix; its source bridge reference and direct Generator reference are removed. Restore assertions require the bridge and Essential.Blazor/Core/Generator as packages while Gallery's local Abstract/Core remain projects. Four independent consumers verify Framework alone, umbrella without Design registration, explicit Design registration and Culture activation.

Test-Release has three phases: restore/build the targeted umbrella's six-library graph and pack/verify the six candidates; restore the full Blazor solution using temporary candidate-source mapping and a fresh cache, assert Gallery adoption and run build/tests/JavaScript/CSS/catalog/launcher checks; run all four isolated package consumers. Packing before Gallery restore bootstraps a new bridge version before public indexing.

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

Maintain one Culture.json per owning project, with en-US, zh-CN and pt-BR translations under each semantic key. Gallery's Localization/Culture.json is both the Generator AdditionalFile and an embedded resource, Gallery.Texts.json. The bridge's transitive Essential packages supply Generator; no direct Generator reference is required. Register the immutable application catalog with AddCultureBlazor, select its default catalog/culture and supported cultures, then resolve generated string tokens through the scoped Localization.Parse service. Generator emits a static Key class; TextKey is Gallery's using alias for that generated class, not a runtime key type. For example, TextKey.Page_RecordCreate returns the token Key.Page_RecordCreate. ILocalizationService.Parse and ParseFrom accept strings; Culture is the UI-culture string and FormatCulture is the formatting CultureInfo.

Framework owns its separate single Culture.json for standard captions and usage descriptions, embedded as Flourish.Blazor.Texts.json. Register it as the Flourish catalog and call AddFlourishCulture to adapt Flourish's provider-neutral ITextProvider. This avoids a dependency from Framework to Essential while using the same Essential lookup/format contract. Application and library catalog IDs prevent key collisions; it does not require one combined file across different owning projects.

H1, H2, prose, field labels, accessibility text, sample status and validation output must resolve keys when rendered. Do not store already translated sentences in singleton metadata or mutable UI state. A LocalizedComponentBase or the production scoped text base reacts to Changed and disposes its subscription; inputs/selections/history retain their business values during language changes. Desktop adapters keep their established bindings; Blazor changes only its request/circuit scope and leaves cookie negotiation/persistence to the host.

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
