# NuGet release and integration

This is the current release contract for Flourish 1.1.0 and its optional Culture integrations. Source scripts and package metadata are authoritative. Local preparation has been verified; this task did not create or push release tags or publish packages to NuGet.org.

## Dependency and release order

Prepare and publish Essential.Culture 1.3.0 first. Wait until all six packages are available from NuGet.org before publishing Flourish 1.1.0. The Flourish workflow restores Essential packages from NuGet; it does not check out or build the Essential source repository.

| Order | Essential.Culture 1.3.0 packages |
|---|---|
| 1 | Arkheide.Essential.Culture.Generator |
| 2 | Arkheide.Essential.Culture |
| 3 | Arkheide.Essential.Culture.Wpf |
| 4 | Arkheide.Essential.Culture.Avalonia |
| 5 | Arkheide.Essential.Culture.WinUI |
| 6 | Arkheide.Essential.Culture.Blazor |

| Order | Flourish 1.1.0 packages |
|---|---|
| 1 | Arkheide.Flourish.Core |
| 2 | Arkheide.Flourish.WPF |
| 3 | Arkheide.Flourish.Blazor.Shared |
| 4 | Arkheide.Flourish.Blazor.Abstract |
| 5 | Arkheide.Flourish.Blazor.Framework |
| 6 | Arkheide.Flourish.Blazor.Design |
| 7 | Arkheide.Flourish.Extensions.Culture.WPF |
| 8 | Arkheide.Flourish.Extensions.Culture.Blazor |

The four Blazor projects retain the optional Design boundary. Framework references Abstract and Shared; Design references Framework. The optional Blazor Culture bridge references Abstract and Essential.Culture.Blazor, rather than Framework or Design. The WPF bridge references Flourish.WPF and Essential.Culture.Wpf. The six non-extension Flourish packages must have no Essential.Culture package dependency.

Flourish root Directory.Build.props owns VersionPrefix=1.1.0 and EssentialCultureVersion=1.3.0. Essential owns its 1.3.0 version in src/Essential.Culture/Directory.Build.props. Each repository's scripts/ReleaseSettings.psd1 defines its exact package set and push order.

## Package consumption and local verification

Flourish libraries, extensions, tests and Gallery use ProjectReference for projects inside Flourish. External Culture references and Gallery's Culture generator are PackageReference dependencies at 1.3.0. The former EssentialCultureRoot and UseLocalEssentialCulture source selection is retired. UseLocalFlourish and UseLocalCultureIntegration are also retired.

When sibling Essential/artifacts/packages exists, Flourish's RestoreAdditionalProjectSources adds that local feed. This changes where NuGet obtains a package; it does not replace PackageReference with a source ProjectReference. Prepare Essential first so that local Flourish builds resolve the same package boundaries used by external consumers.

Essential demos default to PackageReference. Their explicit -p:UseLocalCulture=true mode builds the Culture source and generator for development. Essential's release helper uses that mode to compile the five demos; its package verifier separately inspects the six packed packages. A demo source build alone does not prove package consumption.

Colligere consumes Framework, Design and the optional Culture bridge through NuGet PackageReference. Its local feed permits verification before publication. A package verified in a local feed is not proof that NuGet.org has that version. After publication, restore a clean external consumer from NuGet.org and confirm its resolved package versions and static web assets.

For repeated local work with unpublished packages at the same version, use a fresh isolated NuGet cache for the consumer validation, or explicitly retire only the affected package versions after stopping consumers. An existing global cache can otherwise retain an earlier package with the same ID and version.

## Local release preparation

Use Windows, PowerShell 7, the .NET 10 SDK and the existing desktop build requirements. Flourish also runs its existing Node behavior checks. These commands run in the respective roots:

```powershell
Set-Location C:\Users\Evigila\source\repos\Essential
.\publish-helper.bat -Mode Prepare

Set-Location C:\Users\Evigila\source\repos\Flourish
.\publish-helper.bat -Mode Prepare
```

The batch entry forwards its arguments to scripts/Publish-Helper.ps1. Prepare is the default. It calls Test-Release.ps1, which restores, builds Release with warnings as errors and no incremental build, runs the configured tests/checks, packs the configured libraries and verifies the resulting packages. Essential additionally compiles its demo solution.

Preparation replaces .nupkg and .snupkg files inside that repository's artifacts/packages directory before packing. It does not change Git branches, commits, tags, remotes or NuGet.org.

The verifier checks the exact .nupkg file set, package IDs and versions, internal dependency versions, required dependencies, managed assemblies and configured assets. Flourish verification also checks that only the extensions depend on Essential 1.3.0 and that Framework/Design ship their required static assets. Framework-only and styled consumers remain separate acceptance cases.

Parameters:

| Entry | Parameter | Meaning |
|---|---|---|
| publish-helper.bat / Publish-Helper.ps1 | -Mode Prepare | Runs local preparation; default. |
| publish-helper.bat / Publish-Helper.ps1 | -Mode Publish | Prepares, validates Git state, asks for the exact release tag and pushes that tag. |
| publish-helper.bat / Publish-Helper.ps1 | -SkipBuild | Runs package verification instead of fresh restore/build/test/pack. Does not waive Git checks in Publish. |
| Test-Release.ps1 | -VerifyOnly | Checks existing packages without rebuilding. |
| Verify-PackageSet.ps1 | -PackageDirectory <path> -Version <version> | Checks an explicit package directory/version; omitted values use repository settings. |

Use a full Prepare for release acceptance. SkipBuild only verifies artifacts already present and does not establish that they match the current source.

## Trusted Publishing setup owned by the user

Create a GitHub environment named nuget in each repository. Add its NUGET_USER secret containing the NuGet.org profile username, not an email or API key. Apply the desired environment approval policy. Register a NuGet.org Trusted Publishing policy with the correct package owner and package scope, using these repository identities:

| Field | Essential | Flourish |
|---|---|---|
| Repository owner | Evigila | Evigila |
| Repository | Arkheide.Essential.Culture | Flourish |
| Workflow file | build.yml | build.yml |
| Environment | nuget | nuget |

The workflow filename is entered without .github/workflows/. Both publish jobs grant id-token: write and use NuGet/login@v1 to obtain a temporary API key. The workflow uses that result for package push; no long-lived API key is required locally. These setup details follow the [official NuGet Trusted Publishing guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing). Account policy creation, package ownership and environment approvals require the user's authenticated configuration; local scripts do not configure or verify them.

Essential's local folder name is Essential, but its current remote repository remains Evigila/Arkheide.Essential.Culture. Use the remote name in its policy.

## Configuration and publication status on 2026-10-05

Read-only GitHub inspection by the coordinating task found the nuget environment and NUGET_USER secret name in Evigila/Arkheide.Essential.Culture. Secret values were not retrieved. Their presence does not verify the username's correctness or the NuGet.org account-side trust policy.

The coordinating task subsequently created Evigila/Flourish's nuget environment successfully. Its returned protection_rules=[] and branch_policy=null match the observed Essential environment: no required-reviewer protection or branch policy is currently configured. Flourish's NUGET_USER remains unconfigured while the user supplies its NuGet profile username. No API key was requested.

The coordinating task queried every package ID in both ReleaseSettings files. None of the six Essential 1.3.0 or eight Flourish 1.1.0 target versions is publicly available. Five existing Essential platform packages have latest version 1.2.0; Culture.Blazor and all eight Flourish package IDs returned 404. All 14 target versions therefore remain pending publication; local verification is preparation, not a public release.

User-owned NuGet.org policies and package ownership/scopes have not been verified. The two environments currently have no protection rules or branch policy; do not assume an environment approval gate exists. Prepare can run without this publishing configuration. Publish remains gated by user confirmation, the script's Git checks and a working Trusted Publishing configuration. See [final local verification](release-verification.md) for the completed checks and remaining release prerequisites.

## User-confirmed publishing

Review, commit and push the intended source and scripts first. Publish requires a clean working tree, including untracked files, the master branch, and HEAD equal to origin/master after a fetch. The version must be a stable three-part VersionPrefix. The tag must not exist and must match that version.

Only after the user approves the release, run:

```powershell
Set-Location C:\Users\Evigila\source\repos\Essential
.\publish-helper.bat -Mode Publish
# At the confirmation prompt, type: v1.3.0
```

The helper creates and pushes an annotated v1.3.0 tag. Monitor the GitHub Build, test, and publish workflow and approve its nuget environment if configured. Confirm all six packages are indexed before continuing:

```powershell
Set-Location C:\Users\Evigila\source\repos\Flourish
.\publish-helper.bat -Mode Publish
# At the confirmation prompt, type: v1.1.0
```

The Flourish helper creates and pushes an annotated v1.1.0 tag. Regular master pushes, pull requests and manual workflow dispatch build and verify but do not publish. A matching pushed version tag enables the publish job after the build job succeeds. The publish job downloads and re-verifies the build's packages, then pushes them in ReleaseSettings order with --skip-duplicate.

A rejected confirmation creates no tag. If CI or publication fails after a tag was pushed, inspect the workflow and package state before acting. Do not replace an existing release tag or overwrite a released NuGet version. --skip-duplicate permits a rerun to skip already published package versions; it does not replace them.

## Acceptance checklist

- Both full Prepare runs pass, including package-set verification and required static assets.
- Essential's six 1.3.0 packages are available before the Flourish tag is pushed.
- Both remotes use the reviewed master commit, and each repository has the correct nuget policy/environment.
- The unstyled Framework host works without Design; the styled Gallery resolves Design assets.
- A clean NuGet consumer resolves Flourish 1.1.0 and Essential 1.3.0 without sibling source projects.
- SSR/Interactive Server text selections remain scoped; changing one browser session does not alter another.
- Public publication is reported only after its workflow and NuGet.org package visibility have been confirmed.

This guide supersedes the old independent extension 1.0.0 release and culture-vX.Y.Z tag instructions. Existing dated verification records remain historical evidence.
