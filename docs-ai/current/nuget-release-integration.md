# NuGet release and integration

This is the current release contract for Flourish 1.1.1 and Essential.Culture 1.3.0. Release scripts and package metadata are authoritative. The user has authorized commits and agent-executed NuGet Trusted Publishing. Essential 1.3.0 is published and verified through public-only restoration. Flourish v1.1.0 failed before publication; its immutable tag is retained. The corrected Flourish release is 1.1.1 and remains pending verification/publication in the recovery section below.

## Dependency and release order

Publish and confirm all six Essential.Culture 1.3.0 packages before publishing Flourish 1.1.1. The Flourish workflow restores Essential from NuGet rather than checking out or compiling its source repository.

| Order | Essential.Culture 1.3.0 package |
|---|---|
| 1 | Arkheide.Essential.Culture.Generator |
| 2 | Arkheide.Essential.Culture |
| 3 | Arkheide.Essential.Culture.Wpf |
| 4 | Arkheide.Essential.Culture.Avalonia |
| 5 | Arkheide.Essential.Culture.WinUI |
| 6 | Arkheide.Essential.Culture.Blazor |

| Order | Flourish 1.1.1 package |
|---|---|
| 1 | Arkheide.Flourish.Core |
| 2 | Arkheide.Flourish.Blazor.Abstract |
| 3 | Arkheide.Flourish.Extensions.Culture.Blazor |
| 4 | Arkheide.Flourish.Blazor.Framework |
| 5 | Arkheide.Flourish.Blazor.Design |
| 6 | Arkheide.Flourish.Blazor |

The current manifest covers exactly these six Core/Blazor Flourish packages. Flourish WPF and its Culture bridge remain outside this release. Shared is retired: contracts live in Abstract and processing implementations in Framework. Abstract depends on Core, Framework on Abstract and Design on Framework/Abstract.

Flourish.Blazor is a dependency-only convenience package that automatically installs Abstract, Framework, Design and Flourish.Extensions.Culture.Blazor. The bridge depends on Abstract and Essential.Culture.Blazor, which brings in the Essential runtime and Generator. Consumers of the umbrella do not need a separate Culture bridge installation. To omit both Design and Culture dependencies, install Framework directly. Framework and Design themselves do not depend on Essential. Installing packages does not register services: hosts call AddFlourishFramework, AddFlourishDesign when desired, AddCultureBlazor and AddFlourishCulture to activate those features.

Flourish root Directory.Build.props owns VersionPrefix=1.1.1 and EssentialCultureVersion=1.3.0. Essential owns VersionPrefix=1.3.0 under src/Essential.Culture/Directory.Build.props. Each repository's scripts/ReleaseSettings.psd1 defines the exact package set and publication order. Arkheide.* NuGet identities remain stable even though the current Essential GitHub repository is named Essential.Culture.

## Source and package consumption

Projects within Flourish use their existing ProjectReference boundaries. External Essential dependencies use PackageReference. Gallery now consumes the Culture bridge through its single Arkheide.Flourish.Extensions.Culture.Blazor PackageReference at VersionPrefix. Its source bridge reference and direct Generator reference are removed. Restore assertions verify the bridge as a package, Abstract/Core as source projects and Essential.Blazor/Core/Generator as transitive packages. Four isolated release consumers independently verify complete package consumption.

The former EssentialCultureRoot, UseLocalEssentialCulture, UseLocalFlourish and UseLocalCultureIntegration source-selection switches are retired. The implicit sibling Essential/artifacts/packages restore feed has also been removed. A nearby checkout must not silently change package resolution. For local verification of unpublished Essential packages, explicitly supply EssentialPackageDirectory to scripts/Test-Release.ps1. That parameter supplies a NuGet feed; it never substitutes source ProjectReference dependencies. CI and public consumers restore published Essential packages through their configured NuGet sources.

Use a fresh isolated NuGet cache when validating regenerated packages with the same unpublished version. Do not infer package adoption from a source build or a populated shared cache. Four isolated consumers cover Framework alone, the umbrella without Design registration, explicit Design registration and the Culture integration. Public-source restore after publication is a separate acceptance step.

## Local preparation

Use the existing PowerShell 7, .NET 10 and Node tooling. No additional nuget.exe, GitHub CLI or local long-lived NuGet API key is required by this release path. The batch entry forwards to scripts/Publish-Helper.ps1; Prepare is its default and does not publish.

```powershell
Set-Location C:\Users\RC_Auditoria\source\Repos\Essential
.\scripts\Test-Release.ps1 -ArtifactsPath .\artifacts\release-build

Set-Location C:\Users\RC_Auditoria\source\Repos\Flourish
.\scripts\Test-Release.ps1 -ArtifactsPath .\artifacts\release-build `
    -EssentialPackageDirectory ..\Essential\artifacts\packages
```

Essential prepares its six libraries, tests and five demos, then validates its isolated Blazor-only package consumer. Flourish preparation now has three phases: restore/build the targeted umbrella and its six-library dependency graph, pack all six candidates and verify the package set; restore the complete Blazor solution using a temporary package-source mapping and fresh cache, assert Gallery package adoption, then build and run tests/JavaScript/CSS/catalog checks; finally run all four isolated package consumers. The candidate Flourish feed bootstraps Gallery before the new bridge version is public. The Essential feed is included only when EssentialPackageDirectory is explicitly supplied. No sibling source dependency or prepopulated shared cache is required.

ArtifactsPath uses the SDK artifacts layout to isolate output and intermediate files together. Do not isolate only OutDir while sharing normal project obj directories: static-web-asset and compressed-resource manifests can become inconsistent. Preparation replaces local .nupkg/.snupkg files in artifacts/packages; it does not create commits, tags or public releases.

| Entry | Parameter | Meaning |
|---|---|---|
| publish-helper.bat / Publish-Helper.ps1 | -Mode Prepare | Local preparation; default. |
| publish-helper.bat / Publish-Helper.ps1 | -Mode Publish | Prepare, validate Git state, confirm and push a release tag. |
| publish-helper.bat / Publish-Helper.ps1 | -SkipBuild | Verify existing packages; retain all Publish Git/tag checks. |
| Test-Release.ps1 | -ArtifactsPath | Isolate SDK build output and intermediate files. |
| Flourish Test-Release.ps1 | -EssentialPackageDirectory | Explicit local Essential NuGet feed for preparation and consumer verification. |
| Test-Release.ps1 | -VerifyOnly | Inspect existing packages without restore/build/test/pack. |
| Verify-PackageSet.ps1 | -PackageDirectory / -Version | Inspect an explicit package set and version. |

SkipBuild or VerifyOnly cannot prove that old artifacts match current source. Use a complete preparation for release acceptance and keep its evidence separate from earlier successful runs.

## Trusted Publishing configuration

Both existing .github/workflows/build.yml files already implement the same authentication mechanism. Their publish jobs require a matching pushed version tag and a successful build job, target the GitHub nuget environment, grant contents: read and id-token: write, and call NuGet/login@v1 with secrets.NUGET_USER. That action supplies a temporary NUGET_API_KEY output for dotnet nuget push. No local or committed long-lived API key is part of this contract.

NUGET_USER is the NuGet.org profile username, not an email or API key. The user supplied Evigila on 2026-10-06 and is configuring the Flourish policy/secret. Completion of the current GitHub secret and NuGet.org account policy has not yet been confirmed. This audit did not read secret values. The October 5 observation that Flourish lacked NUGET_USER is historical and cannot establish its current state.

| Trusted Publishing field | Essential | Flourish |
|---|---|---|
| Repository owner | Evigila | Evigila |
| Repository | Essential.Culture | Flourish |
| GitHub repository ID | 1327295083 | 1246107902 |
| Workflow file | build.yml | build.yml |
| Environment | nuget | nuget |

Use the current GitHub repository identity in the NuGet.org trust policy, rather than the checkout folder or former Arkheide.Essential.Culture repository name. The workflow filename is build.yml, without .github/workflows/. See [official NuGet Trusted Publishing guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

Policy permissions must cover creating new package IDs and publishing versions for the intended package owner. Essential's successful five-family 1.2.0 release does not prove that its existing policy includes the new Arkheide.Essential.Culture.Blazor package. Confirm all six Essential IDs. For the initial Flourish policy, the Arkheide.Flourish.* glob can support creating the new IDs; the release manifest and verifier still restrict publication to exactly the six Core/Blazor IDs above. WPF packages remain outside this release. The repository environment alone does not establish NuGet ownership or account-side package authorization.

Public API checks on 2026-10-06 confirmed both [Essential.Culture's nuget environment](https://api.github.com/repos/Evigila/Essential.Culture/environments/nuget) and [Flourish's nuget environment](https://api.github.com/repos/Evigila/Flourish/environments/nuget). Each returned zero protection rules and null branch policy. Do not assume an environment approval gate exists. Secret presence/value and the NuGet.org account policy remain unverified by this check; the later user-provided profile name Evigila does not by itself confirm configuration completion.

## Workflow evidence checked on 2026-10-06

| Repository/run | Observed result | What it establishes |
|---|---|---|
| [Essential.Culture v1.2.0, run 33030395528](https://github.com/Evigila/Essential.Culture/actions/runs/33030395528) | Trusted Publishing login and package-push steps succeeded. | Trusted Publishing worked for that historical release and scope. |
| [Essential.Culture master, run 37291575812](https://github.com/Evigila/Essential.Culture/actions/runs/37291575812) | Build succeeded; no publication. | A normal branch build does not enter the tag-only publish job. |
| [Flourish, run 37291596951](https://github.com/Evigila/Flourish/actions/runs/37291596951) | Restore failed; publish was skipped. | That older full-solution run required absent Essential.Blazor and unpublished Wpf 1.3.0 packages. It did not fail at OIDC login. |

The failed Flourish run predates the current focused Core/Blazor release manifest and does not establish the outcome of current preparation. Public 1.3.0/1.1.0 package availability and current policy scope still require verification before reporting a new release.

## Local preparation verified on 2026-10-06

The complete staged preparation succeeded after the Gallery package migration: Release build zero warnings/errors; Core 367 tests, Blazor 373/373 checks, Culture bridge 12 checks, Gallery 7,234 checks including 124 real-event language-switch/retention checks, Node 66 checks, CSS bundle 21 and SDK integration 194 checks, catalog 21,217 checks and four package consumers totaling 129 checks. Exactly six fresh Flourish 1.1.0 packages passed verification; no WPF package was prepared. Evidence is artifacts/culture-final-release.log and artifacts/package-consumers/01e52f8b29ce48f89c139dfc26a2bfbb. The initial Culture consumer HTTP 500 came from its Minimal API fixture missing [FromServices]; correcting that fixture resolved the check without a production behavior change.

Essential's preceding full preparation passed 106 tests (Core 61, Generator 20, Blazor 21, WinUI 4), all five Gallery builds with zero warnings/errors and all six fresh 1.3.0 packages. This round changed only release repository metadata afterward: six packages were repacked and verified, and all six nuspec RepositoryUrl/PackageProjectUrl values were checked against the current Essential.Culture URL. Functional tests were not rerun for that metadata-only repack. These results establish local candidates, not public NuGet indexing or account-policy readiness. See [local release verification](release-verification.md) for the retained history and manual acceptance checks.

## Historical observations from 2026-10-05

The coordinating task observed the then-named Evigila/Arkheide.Essential.Culture nuget environment and NUGET_USER secret name without reading the secret value or NuGet account policy. It created Flourish's nuget environment with no protection rules or branch policy; Flourish's username request was pending at that time.

The then-current public queries found no target 1.3.0/1.1.0 versions. Earlier eight-package Flourish checks and their 404 results remain historical; the current Flourish manifest contains six Core/Blazor packages. These observations do not establish current remote configuration, credentials, policy or public availability. The historical release-prerequisite note suggesting a required local API key is superseded by the user's Trusted Publishing selection and this verified workflow contract; existing change records remain append-only.

## Authorized publication and remaining boundary

The user has authorized the agent to execute release publication through Trusted Publishing. New source commits still require the user's answer under AGENTS.md. Review and commit the intended release, synchronize master, verify package preparation and policy scope, then use the existing release helper. This documentation task did not execute any commit, tag, push or package publication.

Publish requires a clean tree including untracked files, master, and HEAD equal to fetched origin/master. VersionPrefix must be a stable three-part version; a new annotated tag must exactly match vVersionPrefix. The helper asks for that exact tag before creating and pushing it. CI additionally checks that the release commit is an ancestor of origin/master. Regular master pushes, pull requests and workflow_dispatch only build and verify.

```powershell
# After preparation, commit review and policy checks:
Set-Location C:\Users\RC_Auditoria\source\Repos\Essential
.\publish-helper.bat -Mode Publish
# Exact confirmation: v1.3.0

# After all six Essential packages are publicly indexed:
Set-Location C:\Users\RC_Auditoria\source\Repos\Flourish
.\publish-helper.bat -Mode Publish
# Exact confirmation: v1.1.1
```

The workflow downloads and re-verifies the build's package artifact before pushing in manifest order with --skip-duplicate. Do not replace an existing tag or released package version. A partial publication or failed rerun requires inspection of the actual workflow/package state. Report success only after workflow completion and public NuGet indexing, then restore a clean consumer without sibling source or local-feed assumptions.

## Source commit authorization on 2026-10-06

The user explicitly authorized one release-preparation commit in Essential and one in Flourish, including the verified pending localization, control, project-naming, package-consumption and release-workflow changes. This resolves the commit-answer requirement under AGENTS.md. NuGet account-policy and NUGET_USER setup completion remains to be confirmed before version-tag publication. This authorization does not claim that packages are publicly available.

## Confirmed Trusted Publishing profile on 2026-10-06

The user reported completing the corresponding NuGet Trusted Publishing configuration and confirmed the profile username Evigila. The login action now receives that public profile name directly as user: Evigila, following the official action contract. A GitHub variable called nuget is not the same as secrets.NUGET_USER. The workflow therefore no longer requires a NUGET_USER secret or variable; no long-lived API key is introduced. This supersedes the earlier pending username/secret setup statements. The policy's actual authorization and new-package scope still require successful OIDC login and package publication as runtime evidence. The user has also authorized both release-preparation commits and agent-executed publication.

## Clarified environment username on 2026-10-06

The user clarified that nuget names the existing GitHub environment and that NUGET_USER has been configured. Login now reads the named GitHub configuration as vars.NUGET_USER || secrets.NUGET_USER: an Actions Variable is used when present, otherwise the existing Secret is used. The username is not hard-coded. This supersedes the preceding direct-profile input decision while preserving existing Essential secret storage and the user's Flourish variable storage. The user has reported Trusted Publishing setup complete; workflow execution will verify actual login and package-scope authorization. The two agent-created commits are still local and will be updated before their first push.
## 2026-10-06 CI recovery for Flourish 1.1.1

The Essential release source commit is 983c636bdba7e1a7b74cee33b5c682c9bf06364e, tagged v1.3.0. Its [release run 37537838178](https://github.com/Evigila/Essential.Culture/actions/runs/37537838178) successfully signed in through Trusted Publishing and uploaded all six Essential packages. All six public version indexes include 1.3.0; a fresh-cache, public-only Blazor consumer restored, built with warnings treated as errors, and passed its runtime checks.

Flourish source commit 444ab7f6c19c6e29901fc0008f5e4cf5346e5b0b was pushed and tagged v1.1.0. Runs [37539504631](https://github.com/Evigila/Flourish/actions/runs/37539504631) and [37539506569](https://github.com/Evigila/Flourish/actions/runs/37539506569) failed in Test-CssAssets.ps1's package-build fixture after production compilation and the main tests succeeded. Both publish jobs were skipped, so these failures do not indicate a Trusted Publishing authentication problem or a partial package upload.

The fixture treated USERPROFILE/.nuget/packages as a second package source, although it needs only its own generated package. A controlled reproduction with a nonexistent cache source produced NU1301 at the same fixture. The correction uses only the generated fixture feed and prints captured SDK output on failure. Its complete 194-check CSS integration suite passes without a global cache source. This does not change shipped CSS, controls, assets, or test assertions.

The existing v1.1.0 tag is preserved. Publish-Helper rejects replacement of an existing tag, so all six Flourish package versions move together to 1.1.1 while Essential remains 1.3.0. The nuget environment and NUGET_USER variable already exist; the workflow uses vars.NUGET_USER with the established secret fallback. No local long-lived API key is required. Final 1.1.1 preparation, tag, publication and public consumption results must be recorded separately when observed.
## Completed 1.1.1 preparation on 2026-10-06

The corrected full preparation completed with exit code 0 using public NuGet Essential 1.3.0 dependencies. Production and validation builds have zero warnings/errors. Core 367 tests, Blazor 373/373 checks, Culture bridge 12 checks, Gallery 7,234 checks including 124 actual-event localization/state checks, Node behavior checks, CSS bundle 21, CSS SDK 194, launcher 95, culture/catalog 21,217 and all four package consumers totaling 129 checks passed. Exactly six fresh Core/Blazor 1.1.1 packages passed verification; no Flourish WPF package was prepared.

Evidence: artifacts/culture-release-1.1.1.log and artifacts/package-consumers/546b4983f8404423b078f00b5526ab74. The fixture correction has passed local isolated checks and complete release preparation. The new GitHub tag run and public Flourish consumption remain separate acceptance steps; this preparation does not claim their success.
