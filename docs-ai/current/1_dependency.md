# Project dependency inventory

## Current release preparation 1.4.2, 2026-10-19

Current root VersionPrefix is **1.4.2** for the unchanged six Core/Blazor package manifest. EssentialCultureVersion and ReleaseSettings.EssentialVersion remain **1.4.0**; no external dependency or publication ID changed. Native WPF/WinUI remain outside the publication manifest. Version 1.4.1 was tagged from `66b0a5a`, but its [CI run](https://github.com/Evigila/Flourish/actions/runs/39253305330) failed on a Windows browser-fixture file lock before upload. The existing tag is retained and Gallery identifies that version as unpublished; its four product notes are included in 1.4.2.

Full `publish-helper.bat -Mode Prepare` passed for 1.4.2 with isolated release outputs and fresh restore caches. The six production project.assets.json files retain the same 16 external package/version pairs: three Essential Culture packages at 1.4.0 and thirteen Microsoft.Extensions packages at 10.0.8. Referenced Flourish projects identify 1.4.2. No fresh desktop restore outside this release gate is claimed.

All six local 1.4.2 packages passed identity/dependency/asset verification. Four isolated consumers passed 143 checks in `artifacts/package-consumers/a7bab80baad749498a5fe97023557614`; Flourish cache metadata identifies the local `artifacts/packages` feed, with Essential from public NuGet. Fresh public indexes still contained 1.4.0 and neither 1.4.1 nor 1.4.2 when checked before this preparation completed. Publication and public-only consumption remain pending. See [release verification](release-verification.md#flourish-142-preparation-and-141-publication-failure-2026-10-19).

**Audit baseline:** 2026-10-10 (America/Sao_Paulo, UTC-03:00), working tree based on `3e861b346a145240b8cdabbfe9b39cdd785fc819`. That inventory included the then-uncommitted Essential.Culture 1.4.0 bridge/module update and the installed AGENTS framework 1.0.0. It recorded inspection of declarations and available resolved artifacts; no new restore, build, runtime test, package publication or remote framework version check occurred during that audit.

**Release update, 2026-10-10:** Current root properties declare Flourish `VersionPrefix=1.4.0`, with Essential.Culture still 1.4.0. The same six Core/Blazor packages are published from tag v1.4.0; WPF and its Culture bridge remain outside that public manifest. Local/CI verification, public indexing and fresh public-only consumers passed; see [release verification](release-verification.md#completed-flourish-140-release-on-2026-10-10). The earlier 22-project audit snapshots below remain dated 1.1.3 source-version evidence; only separately identified release artifacts establish a new restore.

## Historical local popup candidate (.4), 2026-10-12

The final local candidate is 1.4.1-local.popup.20261012.4. It retains .3's standard native selection, FloatingActions and tutorial-segment APIs, adding one Design paint correction: enabled customized Quiet selects in ShellHeader inherit chrome foreground instead of the language-option foreground. Native pickers keep their independent option colors. The actual strict browser regression reproduces .3's black icon on blue chrome and verifies the correction while preserving black-on-white picker/options.

All six packages were built into fresh artifacts/local-popup-4/build, cache and nuget with the five version overrides below. Release compiled with zero warnings/errors; package verification and 143 independent consumers passed (fixture artifacts/package-consumers/28122b76194c45efa95982304ec491a6). The expanded actual menu/native/browser file passed 38/38 with zero browser skips. Colligere restored this exact closure and its final Release/full Web regression passed 2379 with 132 existing database/shared-fixture skips and zero failures. Earlier source-behavior checks remain documented under historical .3; this change only adds the verified chrome paint rule. Stable 1.4.0, Essential 1.4.0 and all earlier candidate bytes are preserved. No public push/tag was performed. See [the follow-up record](1_changelogs/2026-10-12_192640_preserve-native-quiet-select-chrome-colors.md).

| Produced package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.1-local.popup.20261012.4.nupkg | 94C503955B298F9085489CA7AE9DCDCC2FD180C153A47327E7B38EBBED5395F6 |
| Arkheide.Flourish.Blazor.Abstract.1.4.1-local.popup.20261012.4.nupkg | DA502CB167E5B96A56449A6FCB8C2D52F276892EFF1799F8A90DF9E971EF4F2E |
| Arkheide.Flourish.Blazor.Design.1.4.1-local.popup.20261012.4.nupkg | C0B9F4A1AA57D7A91049B5AEC4AC0CD72743DC062497BD1C8F1801645F229B77 |
| Arkheide.Flourish.Blazor.Framework.1.4.1-local.popup.20261012.4.nupkg | 53842EDBBA71090E83431887B6646F72940F06C273411C9642A37FDB1153537C |
| Arkheide.Flourish.Core.1.4.1-local.popup.20261012.4.nupkg | 60A47BBFD231E82F9909A076193D39B4C09123ABE30194339A9B96A7C1982035 |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.1-local.popup.20261012.4.nupkg | 11C7BDCCACD18F6BDB60050D6C22789B7A7F6169D66DFA4D0228B39422FAA028 |

## Historical local popup candidate (.3), 2026-10-12

The current candidate is 1.4.1-local.popup.20261012.3, built from the modified master checkout based on 0597f0a00640d686b0bf2bb4158fe59c60382bfb. It adds optional Icon/TriggerVariant to SelectBox, StandaloneSelectBox and LanguagePicker, plus ContentSurface.FloatingActions. It fixes percentage popup minimum width and reuses TutorialBoard segments for floating presentation navigation, with no footer tail row. Native selection remains browser-owned; unsupported rendering engines retain a readable text selector. No external dependency family or stable version declaration changed.

The same six-package Core/Blazor closure was restored and built with the five isolated candidate overrides below in artifacts/local-popup-3/build, cache and nuget. SDK 10.0.401 Release compilation had zero warnings/errors. Package verification passed for all six identities/dependencies/assets. Four package-only consumers passed 143 checks (fixture artifacts/package-consumers/acb146adc8dc4fbf8dc46e86eed7eccb). The preserved .1/.2 and public 1.4.0 bytes were not overwritten, and no package/tag was published. Candidate provenance and logs are in artifacts/local-popup-3; all 322 captured package/build inputs still match the packed source. A subsequent Gallery sample qualification fix is outside all six package inputs and does not alter their bytes.

Verified source checks: 451 Framework console checks, 27 Culture bridge checks, 8346 Gallery checks, 25 progress-controller checks and 37 menu/native/browser checks with zero browser skips. The other 18 maintained JavaScript suites passed. Tests use installed Edge with isolated profiles and DevTools trusted input for actual native picker selection/cancellation; nothing was installed. Colligere consumes this candidate and passed 2379 Web tests with 132 existing database/shared-fixture skips and zero failures. The focused host selection passed 103/103. See [the correction record](1_changelogs/2026-10-12_191504_correct-standard-select-and-floating-layout.md).

| Produced package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.1-local.popup.20261012.3.nupkg | 3EB00696E304F016BEC27DACABE0D0C00CDD975012C3EBAB26245F3FB315D08D |
| Arkheide.Flourish.Blazor.Abstract.1.4.1-local.popup.20261012.3.nupkg | 6A5C1C1051A5712FD69FF04E14EDF9E6627C6A9E955D883CB851625754181F92 |
| Arkheide.Flourish.Blazor.Design.1.4.1-local.popup.20261012.3.nupkg | B05C19494C9C9EB250B2954DE5422F44FA8FDF582AF630D552BDF5C2CC0EDA81 |
| Arkheide.Flourish.Blazor.Framework.1.4.1-local.popup.20261012.3.nupkg | 046194C823C54C34B084888920DEB852DFC97A4DB962C9AC3BDE8299A2D918DE |
| Arkheide.Flourish.Core.1.4.1-local.popup.20261012.3.nupkg | 60A4D41CA1C4144ECE9EB0FF6F2E2192EA29A6E05F33BBB60472704024AC5AE2 |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.1-local.popup.20261012.3.nupkg | B73E2390CB263D7DB3AAE7DF7818E6FB72ABC01E154EFA2D253E361702C10449 |

## Historical local popup candidate (.2), 2026-10-12

The currently verified local candidate is `1.4.1-local.popup.20261012.2`. It includes the subsequent fixed-surface sizing, popup top-layer/fallback markup, catalog and locale-description corrections from the frozen modified working tree based on `0597f0a00640d686b0bf2bb4158fe59c60382bfb`. The base commit alone does not identify that working-tree content. No further public API was added in this second candidate. Published 1.4.0 and the first `.1` candidate remain separate historical artifacts; do not rebuild `.1` from the current implementation.

Stable `Directory.Build.props` still declares `VersionPrefix=1.4.0` and Essential `1.4.0`. These candidate commands alone override `Version` and `PackageVersion` to `.2`, `VersionPrefix` to `1.4.1`, and `VersionSuffix` to `local.popup.20261012.2`. SDK `10.0.401` restored the six-project Core/Blazor closure into new isolated `artifacts/local-popup-2/build` and `artifacts/local-popup-2/cache` directories using the generated NuGet.org-only `artifacts/local-popup-2/NuGet.Config`. Release build passed with zero warnings and zero errors. Six packages were packed into `artifacts/local-popup-2/nuget`; `scripts/Verify-PackageSet.ps1` passed for their identities, candidate dependency versions, managed/metapackage boundaries and required assets. WPF/WinUI are outside this package manifest. No public push, tag or stable-version edit was performed.

| Package | Version | Direct candidate Flourish dependencies | Produced `.nupkg` SHA-256 |
| --- | --- | --- | --- |
| `Arkheide.Flourish.Core` | `1.4.1-local.popup.20261012.2` | None | `0F903CE73FF81ED002756071F6FFE189F030DEB90213599A6A4E765FF1979519` |
| `Arkheide.Flourish.Blazor.Abstract` | `1.4.1-local.popup.20261012.2` | Core | `7ADD47965486F7584C292DEC839E22A4729AADD4F36A196F7CF2A02AB175CACB` |
| `Arkheide.Flourish.Blazor.Framework` | `1.4.1-local.popup.20261012.2` | Abstract | `17FB2E6E3FD809F0515C66E19B4165F526CA9867B44BD83F37942B11C5CC3D18` |
| `Arkheide.Flourish.Blazor.Design` | `1.4.1-local.popup.20261012.2` | Framework, Abstract | `598836638210A08CC9F2F61199BBB85160C2FAB9A8D354326A17F2C3A8F6ABE6` |
| `Arkheide.Flourish.Extensions.Culture.Blazor` | `1.4.1-local.popup.20261012.2` | Framework | `08FE2B29A914F586C9DCE540C790275C5B0ADA900A700A86BCB6DD169DC53C16` |
| `Arkheide.Flourish.Blazor` | `1.4.1-local.popup.20261012.2` | Abstract, Framework, Design, Culture.Blazor | `FD06275017574BB019F41BAC743B270B0F4837760489C6462CA9CB7837902C35` |

The six new `build/obj/<project>/project.assets.json` files resolve the same **16 external package ID/version pairs** as `.1`: three Essential Culture packages at `1.4.0` and 13 Microsoft.Extensions packages at `10.0.8`, enumerated in the first-candidate section and the package tables below. Only the five referenced Flourish source libraries now identify `.2` in that production graph. This supplements the dated 22-project audit and does not establish a fresh restore of its remaining 16 projects.

Four fresh package-only modes (FrameworkOnly, MetaNative, MetaDesign and MetaCulture) passed **143 checks** via `build/Test-BlazorPackageConsumers.ps1`. The fixture is `artifacts/package-consumers/5531a25ec1aa4c958524bb2b4342f378`; all six candidate cache metadata entries identify `artifacts/local-popup-2/nuget`, with no source project references. Registration, SSR, publication assets and optional Culture contracts passed. Logs are `artifacts/local-popup-2/logs/{restore,build,pack,verify-package-set,package-consumers}.log`; local machine-readable provenance, source CSS input hashes and historical before-hashes are in `artifacts/local-popup-2/candidate-provenance.json`.

| Delivered asset | Actual producer input | SHA-256, identical in input/bundle, package and published consumers |
| --- | --- | --- |
| Framework `framework.css` | `build/obj/Flourish.Blazor.Framework/release/bundled-css/framework.css`, generated from maintained modular `wwwroot` CSS by `build/CssBundle.targets` | `056EC3E54A3930AB49EEA553EC22A9A5FCFCC677180EED150E367C6ADADA3D84` |
| Design `design.css` | `build/obj/Flourish.Blazor.Design/release/bundled-css/design.css`, generated by the same bundler | `36A63548077D1BF6B1BBB56733CBB03C605BBCBF32CA806754591D26CF85BA11` |
| Framework `controls.js` | Framework `wwwroot/controls.js` | `ED7C01FC25C0E32FA8E70F9EBF5495FDD162208B167C098F95BB6925325D8BF8` |
| Framework `presentation/progress.js` | Framework `wwwroot/presentation/progress.js` | `BC4FCC2FD287314E2AE3C7807B0A7B77897E79FC2F28E98251039A5C571F315E` |

The Framework assets match all four consumers; Design CSS matches the three meta consumers that carry its package. CSS hashes identify the generated bundle, not the entry file containing modular imports; provenance separately inventories the source files. All six `.1` feed archives, the five installed `.1` and five installed stable 1.4.0 global-cache archives, both absent metapackage cache entries, and `Directory.Build.props` retain their pre-build state. The `.2` cache and outputs are independent. A fresh host requires these six locally built packages and an explicit candidate source mapping/version; NuGet.org and the stable 1.4.0 declaration alone cannot restore this unpublished candidate.

The action-menu regression now optionally measures real SSR geometry using an already installed Chromium-compatible browser, without Playwright or a downloaded browser. `tests/Tests.Flourish.Blazor/action-menu-dom.mjs` accepts `FLOURISH_HEADLESS_BROWSER`, then probes existing Edge/Chrome/Chromium locations. The observed Windows tool is Microsoft Edge `155.0.4283.45`; the task's controller/installed-browser regression passed 33 checks (24 controller plus 9 geometry), with zero skips. If no supported executable is installed, the script explicitly reports `SKIP real-browser SSR geometry` for nine checks and still executes the controller checks. This is an optional test-tool dependency, not a production/browser-version requirement or a new npm/NuGet package.

### Reproduce the local candidate

Run these PowerShell commands from the Flourish repository root containing the `.4` implementation. They use the existing [six-package manifest](../../scripts/ReleaseSettings.psd1), [package verifier](../../scripts/Verify-PackageSet.ps1) and [four-mode consumer checks](../../build/Test-BlazorPackageConsumers.ps1), without a production cleanup/release script. Version overrides must remain five separate arguments. Treat consumed candidate versions as immutable: subsequent source changes require another version rather than replacement of cached bytes.

```powershell
$ErrorActionPreference = 'Stop'
$repo = (Get-Location).Path
$version = '1.4.1-local.popup.20261012.4'
$local = Join-Path $repo 'artifacts/local-popup-4'
$build = Join-Path $local 'build'
$cache = Join-Path $local 'cache'
$feed = Join-Path $local 'nuget'
$logs = Join-Path $local 'logs'
$config = Join-Path $local 'NuGet.Config'
$entry = Join-Path $repo 'src/Flourish.Blazor/Flourish.Blazor/Flourish.Blazor.csproj'
$settings = Import-PowerShellDataFile (Join-Path $repo 'scripts/ReleaseSettings.psd1')
New-Item -ItemType Directory -Path @($build, $cache, $feed, $logs) -Force | Out-Null
$nugetConfig = '<configuration><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources><packageSourceMapping><clear /><packageSource key="nuget.org"><package pattern="*" /></packageSource></packageSourceMapping></configuration>'
[IO.File]::WriteAllText($config, $nugetConfig)
$properties = @(
    "-p:Version=$version"
    '-p:VersionPrefix=1.4.1'
    '-p:VersionSuffix=local.popup.20261012.4'
    "-p:PackageVersion=$version"
    '-p:ContinuousIntegrationBuild=true'
)
& dotnet restore $entry --artifacts-path $build --packages $cache --configfile $config @properties 2>&1 | Tee-Object -FilePath (Join-Path $logs 'restore.log')
if ($LASTEXITCODE -ne 0) { throw 'Candidate restore failed.' }
& dotnet build $entry --configuration Release --no-restore --artifacts-path $build @properties -p:TreatWarningsAsErrors=true 2>&1 | Tee-Object -FilePath (Join-Path $logs 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Candidate build failed.' }
foreach ($package in $settings.Packages) {
    $project = Join-Path $repo $package.Project
    & dotnet pack $project --configuration Release --no-build --no-restore --artifacts-path $build --output $feed @properties 2>&1 | Tee-Object -FilePath (Join-Path $logs 'pack.log') -Append
    if ($LASTEXITCODE -ne 0) { throw "Candidate pack failed: $($package.Id)" }
}
& pwsh -NoLogo -NoProfile -File (Join-Path $repo 'scripts/Verify-PackageSet.ps1') -Version $version -PackageDirectory $feed 2>&1 | Tee-Object -FilePath (Join-Path $logs 'verify-package-set.log')
if ($LASTEXITCODE -ne 0) { throw 'Candidate package verification failed.' }
& pwsh -NoLogo -NoProfile -File (Join-Path $repo 'build/Test-BlazorPackageConsumers.ps1') -Version $version -PackageDirectory $feed 2>&1 | Tee-Object -FilePath (Join-Path $logs 'package-consumers.log')
if ($LASTEXITCODE -ne 0) { throw 'Candidate package consumers failed.' }
```

After verification, map only `Arkheide.Flourish.*` at `.4` to this local feed in the consuming host; keep other dependencies on their existing source. The feed and proof files are ignored local artifacts, so independent machines/remote CI must receive or build the candidate feed explicitly, or await a subsequent authorized public release.

## Local popup candidate, 2026-10-12

**Historical candidate .1:** The following results and bytes precede `.2` and remain retained for provenance.

The authorized popup/presentation implementation was built from the frozen modified working tree based on `0597f0a00640d686b0bf2bb4158fe59c60382bfb`. The base commit alone does not identify the modified candidate content. The stable declaration remains `VersionPrefix=1.4.0`; only these local commands override `Version` and `PackageVersion` to `1.4.1-local.popup.20261012.1`, `VersionPrefix` to `1.4.1`, and `VersionSuffix` to `local.popup.20261012.1`, with `ContinuousIntegrationBuild=true`. No public package push or tag was performed. This candidate does not replace the published 1.4.0 evidence above.

The six-package Core/Blazor closure below was restored and built in Release with SDK `10.0.401`, using isolated `artifacts/local-popup/build`, `artifacts/local-popup/cache`, and a generated restore configuration containing only NuGet.org. The build passed with zero warnings and zero errors. Six packages were packed into `artifacts/local-popup/nuget`, then `scripts/Verify-PackageSet.ps1 -Version 1.4.1-local.popup.20261012.1 -PackageDirectory <local-feed>` passed for all six identities, internal dependency versions, managed/metapackage boundaries and required static assets. WPF and WinUI remain outside this local package manifest; their source changes are not delivered by these six packages.

| Locally built package | Candidate version | Direct Flourish package dependencies at the same candidate version | SHA-256 of the produced `.nupkg` |
| --- | --- | --- | --- |
| `Arkheide.Flourish.Core` | `1.4.1-local.popup.20261012.1` | None | `B8E30D1271D473AD3B0A1503007D0C456A3D2E8A52F0A7B84F01EE24EDABE988` |
| `Arkheide.Flourish.Blazor.Abstract` | `1.4.1-local.popup.20261012.1` | Core | `0C934E99B9E7CD2F207DC0011E17BC9B2814027B269490DD4A2B5740E47A9DF7` |
| `Arkheide.Flourish.Blazor.Framework` | `1.4.1-local.popup.20261012.1` | Abstract | `C1BD233BC86ACA616CE4C4DF1BA762F7FA8E2FE3B767C2CEE5E4BE00D4B6AEFA` |
| `Arkheide.Flourish.Blazor.Design` | `1.4.1-local.popup.20261012.1` | Framework, Abstract | `9925DBFF1A44334D15804ACA7A80F303E22260482F1CFBB8A3C6863B551D0BE4` |
| `Arkheide.Flourish.Extensions.Culture.Blazor` | `1.4.1-local.popup.20261012.1` | Framework | `0F3B2BAC6E19BDA69DF1E08C43E953D2FBE38D9BEEF8CA3F243DAB538E4473C3` |
| `Arkheide.Flourish.Blazor` | `1.4.1-local.popup.20261012.1` | Abstract, Framework, Design, Culture.Blazor | `268300BD657F09A49C763293CD459E6FE594104261A611FE3681A3B55FB72517` |

The six fresh `artifacts/local-popup/build/obj/<project>/project.assets.json` files resolve **16 external package ID/version pairs**: the Essential Culture, Culture.Blazor and Culture.Generator packages at `1.4.0`, plus the 13 Microsoft.Extensions packages already listed below at `10.0.8` (Configuration, Configuration.Abstractions, Configuration.FileExtensions, Configuration.Json, DependencyInjection.Abstractions, Diagnostics.Abstractions, FileProviders.Abstractions, FileProviders.Physical, FileSystemGlobbing, Hosting.Abstractions, Logging.Abstractions, Options and Primitives). The five referenced Flourish source libraries resolve at the candidate version in that build graph. Production declarations, external package versions, source-reference edges and license defaults are unchanged. These six fresh snapshots supplement the dated 22-project audit; they do not establish a new restore of the other 16 maintained projects.

`build/Test-BlazorPackageConsumers.ps1` passed **143 checks** in four independently restored/published package-only modes: FrameworkOnly, MetaNative, MetaDesign and MetaCulture. Their fresh fixture is `artifacts/package-consumers/a6d1c533d44148a4981bb04bba1a1b3c`; its isolated cache metadata records the local candidate feed for all six Flourish packages. It verifies package graphs without source project references, registration, SSR, published assets and optional Culture behavior. The Culture dependency remains Essential `1.4.0`. A supplementary generated package-only probe in `artifacts/local-popup/api-probe` passed **45 exported API checks**, verifying the two `PopupStrength` values, all 14 maintained components' public `Strength` parameters and Weak defaults/Strong assignments, and the packaged `PresentationProgress` type. This probe checks the delivered API; popup interaction and viewport behavior are covered by the separate source/DOM suites recorded in the task verification report.

The new Framework `staticwebassets/presentation/progress.js` is present in the candidate archive and all four consumers' published assets. Its bytes match the frozen source and every published copy, SHA-256 `BC4FCC2FD287314E2AE3C7807B0A7B77897E79FC2F28E98251039A5C571F315E`. Evidence is saved locally in `artifacts/local-popup/candidate-provenance.json` and `artifacts/local-popup/logs/{restore-2,build,pack,verify-package-set,package-consumers,api-probe-2}.log`. All five previously installed Flourish 1.4.0 package archives retain their pre-build SHA-256 values; the 1.4.0 metapackage remains absent from that cache, and `Directory.Build.props` retains its pre-build hash. The candidate restore and probe caches are isolated; an authorized consuming host may separately install this new version in its own cache.

Reproduction requires building this candidate package closure before restoring a host configured to use this local feed. The feed and generated evidence are ignored local artifacts, not a published remote distribution. An independent machine or default remote CI cannot restore this candidate from NuGet.org; it needs the explicitly supplied candidate feed or a subsequent authorized public release. Use the dedicated local build/pack commands with all version overrides; the stable release/cleanup scripts target the declared 1.4.0 production output and were not used for this candidate.

### First candidate reproduction (historical)

The following `.1` commands are preserved as historical execution evidence. They require the first candidate implementation and must not be run against the current `.2` source tree.

Run the following PowerShell commands from the Flourish repository root containing the candidate implementation. They use the existing [package manifest](../../scripts/ReleaseSettings.psd1), [package verifier](../../scripts/Verify-PackageSet.ps1) and [package consumer checks](../../build/Test-BlazorPackageConsumers.ps1). The version parameters are separate quoted array entries; joining them into one argument does not perform a valid candidate restore. These commands do not call a production cleanup script, change `Directory.Build.props`, or publish packages. Keep a candidate version immutable after it has been consumed; a later implementation requires a different version rather than replacing cached candidate bytes.

```powershell
$ErrorActionPreference = 'Stop'
$repo = (Get-Location).Path
$version = '1.4.1-local.popup.20261012.1'
$local = Join-Path $repo 'artifacts/local-popup'
$build = Join-Path $local 'build'
$cache = Join-Path $local 'cache'
$feed = Join-Path $local 'nuget'
$restore = Join-Path $local 'restore'
$config = Join-Path $restore 'NuGet.Config'
$entry = Join-Path $repo 'src/Flourish.Blazor/Flourish.Blazor/Flourish.Blazor.csproj'
$settings = Import-PowerShellDataFile (Join-Path $repo 'scripts/ReleaseSettings.psd1')
New-Item -ItemType Directory -Path @($build, $cache, $feed, $restore) -Force | Out-Null
$nugetConfig = '<configuration><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources><packageSourceMapping><clear /><packageSource key="nuget.org"><package pattern="*" /></packageSource></packageSourceMapping></configuration>'
[IO.File]::WriteAllText($config, $nugetConfig)
$properties = @(
    "-p:Version=$version"
    '-p:VersionPrefix=1.4.1'
    '-p:VersionSuffix=local.popup.20261012.1'
    "-p:PackageVersion=$version"
    '-p:ContinuousIntegrationBuild=true'
)
& dotnet restore $entry --artifacts-path $build --packages $cache --configfile $config @properties
if ($LASTEXITCODE -ne 0) { throw 'Candidate restore failed.' }
& dotnet build $entry --configuration Release --no-restore --artifacts-path $build @properties -p:TreatWarningsAsErrors=true
if ($LASTEXITCODE -ne 0) { throw 'Candidate build failed.' }
foreach ($package in $settings.Packages) {
    $project = Join-Path $repo $package.Project
    & dotnet pack $project --configuration Release --no-build --no-restore --artifacts-path $build --output $feed @properties
    if ($LASTEXITCODE -ne 0) { throw "Candidate pack failed: $($package.Id)" }
}
& pwsh -NoLogo -NoProfile -File (Join-Path $repo 'scripts/Verify-PackageSet.ps1') -Version $version -PackageDirectory $feed
if ($LASTEXITCODE -ne 0) { throw 'Candidate package verification failed.' }
& pwsh -NoLogo -NoProfile -File (Join-Path $repo 'build/Test-BlazorPackageConsumers.ps1') -Version $version -PackageDirectory $feed
if ($LASTEXITCODE -ne 0) { throw 'Candidate package consumers failed.' }
```

After successful verification, a consuming host can map only `Arkheide.Flourish.*` at this candidate version to the generated local feed while continuing to obtain other dependencies from NuGet.org. The host must explicitly configure that source and version; the stable Flourish version declaration and NuGet.org alone do not select this candidate.

## Evidence and interpretation

All 22 maintained `.csproj` files under `src/` and `tests/` were inspected during the audit. Each had a matching `obj/project.assets.json` whose `project.restore.projectPath` identified that project (the WinUI Gallery path differed only in case). The 20 non-WinUI audit snapshots were written at approximately `2026-10-10T03:06:36Z`; the two WinUI snapshots were written at approximately `2026-10-09T20:30:53Z`. They are not evidence that the normal source `obj/` trees were restored at 1.4.0. Obsolete generated projects, renamed-project caches and temporary `artifacts/` consumers are excluded from the active source inventory.

These audit project snapshots contain **49 distinct package ID/version pairs**: 10 explicitly referenced IDs, one SDK-injected ID, and 38 IDs that are only transitive. WinUI additionally requests 10 SDK/runtime download packs outside the `libraries` graph. The local DocFX/CSharpier net10 tool payloads contain 68 distinct bundled external package ID/version pairs; these belong to development/documentation tooling, not the application package graph.

| Evidence | What it establishes |
| --- | --- |
| Current `Directory.Build.props`; `src/Flourish.Extensions/Directory.Build.props` | Flourish `VersionPrefix=1.4.1`, Essential property `1.4.0`, inherited version/license defaults; the extension props explicitly import the root props. |
| Audited `.csproj` declarations and their dated matching `obj/project.assets.json` | Direct package requirements, project edges, target frameworks, exact observed package versions/parents and SDK download requests at the audit baseline. |
| `artifacts/release-1.4.0/obj/Gallery.Flourish.Blazor/project.assets.json`, written `2026-10-10T04:10:14.3513983Z` | Fresh release restore identifies five referenced Flourish libraries as source projects at 1.4.0 and Essential Core/Blazor/Generator as packages at 1.4.0. It does not establish build/test completion or publication. |
| Fresh public consumer `artifacts/package-consumers/9f393f06c97c43efb834ffe4b84510d1`; `artifacts/release-1.4.0/public-consumer-provenance.json` | Four independent package-only consumer modes passed 143 checks. All six Flourish and three Essential 1.4.0 package metadata entries identify NuGet.org; no local feed or source project is used. |
| NuGet cache `<lowercase-id>/<version>/*.nuspec` and `.nupkg.metadata` | Package-declared licenses/source repositories and recorded acquisition feeds. All 49 application package pairs have local metadata recording NuGet.org. |
| `global.json`; `dotnet --version`; `dotnet --list-runtimes`; SDK `Microsoft.NETCoreSdk.BundledVersions.props` | SDK constraint and locally selected SDK/runtime/targeting pack observations. |
| `.config/dotnet-tools.json`; cached tool `.nuspec`, `.deps.json`, `.runtimeconfig.json` | Tool pins, available net10 payloads and their bundled dependencies. Tool package acquisition source is not present in their `.nupkg.metadata`. |
| `.github/workflows/build.yml`, `docs.yml`; `scripts/ReleaseSettings.psd1`; launch/release/build scripts | CI actions, hosted runners, Node requirement, feeds, package verification and optional diagnostic tooling. |
| Icon `source.json`, license files, SVG attribution comments; documentation CSS/configuration | Vendored icon provenance, OS font usage, DocFX template and online Roboto boundary. |
| `AGENTS.framework.json`, `AGENTS.lock.json`, `scripts/fetch-agents.ps1` | Installed instruction framework version, central source commit and optional Git synchronization contract. |

A plain NuGet `Version="x.y.z"` is recorded as the declared lower-bound requirement (`>= x.y.z`), separately from the version currently resolved. There are no maintained `packages.lock.json`, `Directory.Packages.props`, `package.json` or repository-wide NuGet.Config pins. Release checks additionally require the expected Essential 1.4.0 identities. Shared framework references can satisfy and prune Microsoft.Extensions packages in ASP.NET graphs: the consumer lists below describe the package entries actually present, not all projects that use the corresponding framework assemblies.

Project aliases below abbreviate complete consumer sets without omitting project identities. `Assets(Pnn)` means that row's `obj/project.assets.json`. `Cache(id/version)` means its local NuGet nuspec and metadata; cache evidence is generated/local, not a maintained repository file.

## Projects and source reference graph

| Alias | Maintained project | Target framework | Direct source project references |
| --- | --- | --- | --- |
| P01 | `src/Flourish.Blazor/Flourish.Blazor.Abstract/Flourish.Blazor.Abstract.csproj` | `net10.0` | P05 (`Flourish.Core`) |
| P02 | `src/Flourish.Blazor/Flourish.Blazor.Design/Flourish.Blazor.Design.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`)<br>P01 (`Flourish.Blazor.Abstract`) |
| P03 | `src/Flourish.Blazor/Flourish.Blazor.Framework/Flourish.Blazor.Framework.csproj` | `net10.0` | P01 (`Flourish.Blazor.Abstract`) |
| P04 | `src/Flourish.Blazor/Flourish.Blazor/Flourish.Blazor.csproj` | `net10.0` | P01 (`Flourish.Blazor.Abstract`)<br>P03 (`Flourish.Blazor.Framework`)<br>P02 (`Flourish.Blazor.Design`)<br>P06 (`Flourish.Extensions.Culture.Blazor`) |
| P05 | `src/Flourish.Core/Flourish.Core.csproj` | `net10.0` | None |
| P06 | `src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor/Flourish.Extensions.Culture.Blazor.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`) |
| P07 | `src/Flourish.Extensions/Flourish.Extensions.Culture.WPF/Flourish.Extensions.Culture.WPF.csproj` | `net10.0-windows` | P11 (`Flourish.WPF.Framework`) |
| P08 | `src/Flourish.WinUI3/Flourish.WinUI3.csproj` | `net10.0-windows10.0.26100.0` | None |
| P09 | `src/Flourish.WPF/Flourish.WPF.Abstract/Flourish.WPF.Abstract.csproj` | `net10.0-windows` | None |
| P10 | `src/Flourish.WPF/Flourish.WPF.Design/Flourish.WPF.Design.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`)<br>P11 (`Flourish.WPF.Framework`) |
| P11 | `src/Flourish.WPF/Flourish.WPF.Framework/Flourish.WPF.Framework.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`) |
| P12 | `src/Flourish.WPF/Flourish.WPF/Flourish.WPF.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`)<br>P11 (`Flourish.WPF.Framework`)<br>P10 (`Flourish.WPF.Design`) |
| P13 | `src/Gallery.Flourish.Blazor/Gallery.Flourish.Blazor.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`)<br>P02 (`Flourish.Blazor.Design`)<br>P06 (`Flourish.Extensions.Culture.Blazor`) |
| P14 | `src/Gallery.Flourish.WINUI3/Gallery.Flourish.WinUI3.csproj` | `net10.0-windows10.0.26100.0` | None |
| P15 | `src/Gallery.Flourish.WPF/Gallery.Flourish.WPF.csproj` | `net10.0-windows` | P12 (`Flourish.WPF`)<br>P07 (`Flourish.Extensions.Culture.WPF`) |
| P16 | `tests/Tests.Flourish.Blazor.Native/Tests.Flourish.Blazor.Native.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`) |
| P17 | `tests/Tests.Flourish.Blazor/Tests.Flourish.Blazor.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`)<br>P02 (`Flourish.Blazor.Design`)<br>P06 (`Flourish.Extensions.Culture.Blazor`) |
| P18 | `tests/Tests.Flourish.Core/Tests.Flourish.Core.csproj` | `net10.0` | P05 (`Flourish.Core`) |
| P19 | `tests/Tests.Flourish.Extensions.Culture.Blazor/Tests.Flourish.Extensions.Culture.Blazor.csproj` | `net10.0` | P06 (`Flourish.Extensions.Culture.Blazor`) |
| P20 | `tests/Tests.Flourish.Extensions.Culture.WPF/Tests.Flourish.Extensions.Culture.WPF.csproj` | `net10.0-windows` | P07 (`Flourish.Extensions.Culture.WPF`) |
| P21 | `tests/Tests.Flourish.WPF/Tests.Flourish.WPF.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`)<br>P11 (`Flourish.WPF.Framework`)<br>P10 (`Flourish.WPF.Design`)<br>P15 (`Gallery.Flourish.WPF`) |
| P22 | `tests/Tests.Gallery.Flourish.Blazor/Tests.Gallery.Flourish.Blazor.csproj` | `net10.0` | P13 (`Gallery.Flourish.Blazor`) |

Each edge above is a `ProjectReference` declared in the consuming project. These are repository source dependencies, not externally restored Flourish packages. Their current inherited source version is 1.4.1; the dated audit snapshots observed 1.1.3 before the release version change. The Blazor Abstract -> Core edge supplies platform-neutral services/contracts; Framework -> Abstract supplies production controls; Design -> Framework/Abstract supplies optional visual resources; Culture bridges -> their Framework preserve optional localization. Metapackages compose these entries. Gallery/test edges consume the actual entries. WPF's native Abstract/Framework/Design graph is independent of Core/Blazor. Neither WinUI placeholder project currently references a Flourish source project. Core has no source project dependency.

## Direct and SDK-injected NuGet packages

All rows use NuGet.org (`https://api.nuget.org/v3/index.json`) as the recorded acquisition feed. Licenses and repository links below come from each exact cached nuspec; a package license filename is not treated as a verified SPDX expression.

| Package | Declared requirement | Observed version | Direct consumers; all observed package consumers | Scope and purpose | Source / license | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `Arkheide.Essential.Culture.Blazor` | `$(EssentialCultureVersion)` = 1.4.0; restore >= 1.4.0 | `1.4.0` | Direct: P06<br>Observed: P04, P06, P13, P17, P19, P22 | Production: Scoped localization contexts and request-localization adapters used by the optional Blazor bridge. | [repository](https://github.com/Evigila/Essential); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P04, P06, P13, P17, P19, P22), Cache(`Arkheide.Essential.Culture.Blazor/1.4.0`) |
| `Arkheide.Essential.Culture.Wpf` | `$(EssentialCultureVersion)` = 1.4.0; restore >= 1.4.0 | `1.4.0` | Direct: P07<br>Observed: P07, P15, P20, P21 | Production: WPF localization provider, culture events and XAML integration used by the optional native bridge. | [repository](https://github.com/Evigila/Essential); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P07, P15, P20, P21), Cache(`Arkheide.Essential.Culture.Wpf/1.4.0`) |
| `Microsoft.AspNetCore.App.Internal.Assets` | Web SDK implicit >= 10.0.12 in observed SDK graph; no host PackageReference | `10.0.12` | Direct: P13, P16 (SDK)<br>Observed: P13, P16 | Production: Web SDK supplied Blazor bootstrap and browser runtime static assets. | [repository](https://github.com/dotnet/dotnet); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P13, P16), Cache(`Microsoft.AspNetCore.App.Internal.Assets/10.0.12`) |
| `Microsoft.Extensions.Configuration.Json` | >= `10.0.8` | `10.0.8` | Direct: P05<br>Observed: P04, P05, P18, P19 | Production: Core JSON configuration loading. | [repository](https://github.com/dotnet/dotnet); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Configuration.Json/10.0.8`) |
| `Microsoft.Extensions.Hosting.Abstractions` | >= `10.0.8` | `10.0.8` | Direct: P05<br>Observed: P04, P05, P18, P19 | Production: Core host lifecycle abstractions. | [repository](https://github.com/dotnet/dotnet); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Hosting.Abstractions/10.0.8`) |
| `Microsoft.Extensions.Logging.Abstractions` | >= `10.0.8` | `10.0.8` | Direct: P05<br>Observed: P04, P05, P18, P19 | Production: Core logging contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Logging.Abstractions/10.0.8`) |
| `Microsoft.NET.Test.Sdk` | >= `17.14.1` | `17.14.1` | Direct: P18, P20, P21<br>Observed: P18, P20, P21 | Test: VSTest discovery and execution for xUnit test projects. | [repository](https://github.com/microsoft/vstest); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P18, P20, P21), Cache(`Microsoft.NET.Test.Sdk/17.14.1`) |
| `Microsoft.WindowsAppSDK` | >= `2.3.1` | `2.3.1` | Direct: P08, P14<br>Observed: P08, P14 | WinUI production/build: WinUI 3 library and unpackaged placeholder Gallery platform/tooling entry. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Direct consumers' .csproj (or Web SDK), Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK/2.3.1`) |
| `Moq` | >= `4.20.72` | `4.20.72` | Direct: P18<br>Observed: P18 | Test: Mocking Core service contracts in tests. | [repository](https://github.com/moq/moq); `BSD-3-Clause` | Direct consumers' .csproj (or Web SDK), Assets(P18), Cache(`Moq/4.20.72`) |
| `xunit.runner.visualstudio` | >= `3.1.4` | `3.1.4` | Direct: P18, P20, P21<br>Observed: P18, P20, P21 | Test: VSTest xUnit adapter; PrivateAssets=all in all three direct consumers. | [repository](https://github.com/xunit/visualstudio.xunit); `Apache-2.0` | Direct consumers' .csproj (or Web SDK), Assets(P18, P20, P21), Cache(`xunit.runner.visualstudio/3.1.4`) |
| `xunit` | >= `2.9.3` | `2.9.3` | Direct: P18, P20, P21<br>Observed: P18, P20, P21 | Test: xUnit test framework metapackage. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Direct consumers' .csproj (or Web SDK), Assets(P18, P20, P21), Cache(`xunit/2.9.3`) |

The `PrivateAssets=all` xUnit runner references are test-only. The Culture generator is delivered transitively by Essential Core; it is enabled for Gallery.Blazor and the WPF bridge's module fixture, while library/test projects explicitly disable generation where their resources are manually supplied. Gallery.Blazor uses seven multilingual functional modules, generated typed keys and `CultureResources.Files`; it does not depend on a sibling Essential source checkout.

## Resolved transitive NuGet packages

Complete enumeration of the remaining 38 ID/version pairs in the 22 current project graphs. Parent edges are the union observed across targets; SDK/platform payloads being present does not establish that Gallery explicitly invokes every optional Windows feature.

| Package | Observed version | Consumers | Introduced by | Scope and purpose | Source / license | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `Arkheide.Essential.Culture.Generator` | `1.4.0` | P04, P06, P07, P13, P15, P17, P19, P20, P21, P22 | `Arkheide.Essential.Culture/1.4.0` | Build/code generation: Transitive Roslyn/MSBuild key generation, module manifest and build/publish resource deployment. | [repository](https://github.com/Evigila/Essential); `MIT` | Assets(P04, P06, P07, P13, P15, P17, P19, P20, P21, P22), Cache(`Arkheide.Essential.Culture.Generator/1.4.0`) |
| `Arkheide.Essential.Culture` | `1.4.0` | P04, P06, P07, P13, P15, P17, P19, P20, P21, P22 | `Arkheide.Essential.Culture.Blazor/1.4.0`<br>`Arkheide.Essential.Culture.Wpf/1.4.0` | Production: Catalog parsing/merging, fallback lookup, culture policy and isolated/static localization contexts. | [repository](https://github.com/Evigila/Essential); `MIT` | Assets(P04, P06, P07, P13, P15, P17, P19, P20, P21, P22), Cache(`Arkheide.Essential.Culture/1.4.0`) |
| `Castle.Core` | `5.1.1` | P18 | `Moq/4.20.72` | Test: Dynamic proxy implementation used by Moq. | [repository](https://github.com/castleproject/Core); `Apache-2.0` | Assets(P18), Cache(`Castle.Core/5.1.1`) |
| `Microsoft.CodeCoverage` | `17.14.1` | P18, P20, P21 | `Microsoft.NET.Test.Sdk/17.14.1` | Test: VSTest code coverage tooling. | [repository](https://github.com/microsoft/vstest); `MIT` | Assets(P18, P20, P21), Cache(`Microsoft.CodeCoverage/17.14.1`) |
| `Microsoft.Extensions.Configuration.Abstractions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.FileExtensions/10.0.8`<br>`Microsoft.Extensions.Configuration.Json/10.0.8`<br>`Microsoft.Extensions.Configuration/10.0.8`<br>`Microsoft.Extensions.Hosting.Abstractions/10.0.8` | Production: Configuration contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Configuration.Abstractions/10.0.8`) |
| `Microsoft.Extensions.Configuration.FileExtensions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.Json/10.0.8` | Production: File-backed configuration sources and reload support. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Configuration.FileExtensions/10.0.8`) |
| `Microsoft.Extensions.Configuration` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.FileExtensions/10.0.8`<br>`Microsoft.Extensions.Configuration.Json/10.0.8` | Production: Configuration builder/provider implementation. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Configuration/10.0.8`) |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Diagnostics.Abstractions/10.0.8`<br>`Microsoft.Extensions.Hosting.Abstractions/10.0.8`<br>`Microsoft.Extensions.Logging.Abstractions/10.0.8`<br>`Microsoft.Extensions.Options/10.0.8` | Production: Dependency injection contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.DependencyInjection.Abstractions/10.0.8`) |
| `Microsoft.Extensions.Diagnostics.Abstractions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Hosting.Abstractions/10.0.8` | Production: Hosting diagnostics contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Diagnostics.Abstractions/10.0.8`) |
| `Microsoft.Extensions.FileProviders.Abstractions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.FileExtensions/10.0.8`<br>`Microsoft.Extensions.Configuration.Json/10.0.8`<br>`Microsoft.Extensions.FileProviders.Physical/10.0.8`<br>`Microsoft.Extensions.Hosting.Abstractions/10.0.8` | Production: File provider contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.FileProviders.Abstractions/10.0.8`) |
| `Microsoft.Extensions.FileProviders.Physical` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.FileExtensions/10.0.8` | Production: Physical file provider for file-backed configuration. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.FileProviders.Physical/10.0.8`) |
| `Microsoft.Extensions.FileSystemGlobbing` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.FileProviders.Physical/10.0.8` | Production: File matching used by physical file providers. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.FileSystemGlobbing/10.0.8`) |
| `Microsoft.Extensions.Options` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Diagnostics.Abstractions/10.0.8` | Production: Options contracts and infrastructure. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Options/10.0.8`) |
| `Microsoft.Extensions.Primitives` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.Abstractions/10.0.8`<br>`Microsoft.Extensions.Configuration.FileExtensions/10.0.8`<br>`Microsoft.Extensions.Configuration/10.0.8`<br>`Microsoft.Extensions.FileProviders.Abstractions/10.0.8`<br>`Microsoft.Extensions.FileProviders.Physical/10.0.8`<br>`Microsoft.Extensions.Options/10.0.8` | Production: Change tokens and shared extension primitives. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Primitives/10.0.8`) |
| `Microsoft.TestPlatform.ObjectModel` | `17.14.1` | P18, P20, P21 | `Microsoft.TestPlatform.TestHost/17.14.1` | Test: VSTest object model and adapter interfaces. | [repository](https://github.com/microsoft/vstest); `MIT` | Assets(P18, P20, P21), Cache(`Microsoft.TestPlatform.ObjectModel/17.14.1`) |
| `Microsoft.TestPlatform.TestHost` | `17.14.1` | P18, P20, P21 | `Microsoft.NET.Test.Sdk/17.14.1` | Test: VSTest test host. | [repository](https://github.com/microsoft/vstest); `MIT` | Assets(P18, P20, P21), Cache(`Microsoft.TestPlatform.TestHost/17.14.1`) |
| `Microsoft.Web.WebView2` | `1.0.3719.77` | P08, P14 | `Microsoft.WindowsAppSDK.WinUI/2.3.0` | WinUI production/build: WinUI package dependency for WebView integration; native WPF implementation does not depend on it. | [package project](https://aka.ms/webview); Package license file `LICENSE.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.Web.WebView2/1.0.3719.77`) |
| `Microsoft.Windows.AI.MachineLearning` | `2.1.74` | P08, P14 | `Microsoft.WindowsAppSDK.ML/2.1.74` | WinUI production/build: Windows ML payload brought in by WindowsAppSDK.ML. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.Windows.AI.MachineLearning/2.1.74`) |
| `Microsoft.Windows.SDK.BuildTools.MSIX` | `1.7.251221100` | P08, P14 | `Microsoft.WindowsAppSDK.Base/2.0.4` | WinUI build/deployment: MSIX build tooling brought in by WindowsAppSDK.Base. | [package project](https://aka.ms/WinSDKProjectURL); Package license file `sdk_license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.Windows.SDK.BuildTools.MSIX/1.7.251221100`) |
| `Microsoft.Windows.SDK.BuildTools` | `10.0.26100.4654` | P08, P14 | `Microsoft.WindowsAppSDK.Base/2.0.4` | WinUI build/deployment: Windows SDK build tools brought in by WindowsAppSDK.Base. | [package project](https://aka.ms/WinSDKProjectURL); License identity unknown; nuspec supplies [license URL](https://aka.ms/WinSDKLicenseURL) | Assets(P08, P14), Cache(`Microsoft.Windows.SDK.BuildTools/10.0.26100.4654`) |
| `Microsoft.WindowsAppSDK.AI` | `2.3.4` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK AI feature payload. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.AI/2.3.4`) |
| `Microsoft.WindowsAppSDK.Base` | `2.0.4` | P08, P14 | `Microsoft.WindowsAppSDK.AI/2.3.4`<br>`Microsoft.WindowsAppSDK.DWrite/2.1.0`<br>`Microsoft.WindowsAppSDK.Foundation/2.3.5`<br>`Microsoft.WindowsAppSDK.InteractiveExperiences/2.1.3`<br>`Microsoft.WindowsAppSDK.ML/2.1.74`<br>`Microsoft.WindowsAppSDK.Runtime/2.3.1`<br>`Microsoft.WindowsAppSDK.Widgets/2.0.5`<br>`Microsoft.WindowsAppSDK.WinUI/2.3.0`<br>`Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Shared Windows App SDK build/runtime foundation. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.Base/2.0.4`) |
| `Microsoft.WindowsAppSDK.DWrite` | `2.1.0` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK DirectWrite feature payload. | [package project](https://aka.ms/WindowsAppSDK); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.DWrite/2.1.0`) |
| `Microsoft.WindowsAppSDK.Foundation` | `2.3.5` | P08, P14 | `Microsoft.WindowsAppSDK.AI/2.3.4`<br>`Microsoft.WindowsAppSDK.ML/2.1.74`<br>`Microsoft.WindowsAppSDK.WinUI/2.3.0`<br>`Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK foundational platform payload. | [package project](https://aka.ms/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.Foundation/2.3.5`) |
| `Microsoft.WindowsAppSDK.InteractiveExperiences` | `2.1.3` | P08, P14 | `Microsoft.WindowsAppSDK.Foundation/2.3.5`<br>`Microsoft.WindowsAppSDK.WinUI/2.3.0`<br>`Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK interaction feature payload. | [package project](https://aka.ms/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.InteractiveExperiences/2.1.3`) |
| `Microsoft.WindowsAppSDK.ML` | `2.1.74` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK ML feature payload. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.ML/2.1.74`) |
| `Microsoft.WindowsAppSDK.Runtime` | `2.3.1` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK runtime distribution. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.Runtime/2.3.1`) |
| `Microsoft.WindowsAppSDK.Widgets` | `2.0.5` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK widget feature payload. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.Widgets/2.0.5`) |
| `Microsoft.WindowsAppSDK.WinUI` | `2.3.0` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: WinUI controls and XAML tooling. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.WinUI/2.3.0`) |
| `Newtonsoft.Json` | `13.0.3` | P18, P20, P21 | `Microsoft.TestPlatform.TestHost/17.14.1` | Test: VSTest test-host JSON serialization. | [repository](https://github.com/JamesNK/Newtonsoft.Json); `MIT` | Assets(P18, P20, P21), Cache(`Newtonsoft.Json/13.0.3`) |
| `System.Diagnostics.EventLog` | `6.0.0` | P18 | `Castle.Core/5.1.1` | Test: Castle platform event-log dependency; not a Gallery business service. | [repository](https://github.com/dotnet/runtime); `MIT` | Assets(P18), Cache(`System.Diagnostics.EventLog/6.0.0`) |
| `System.Numerics.Tensors` | `9.0.0` | P08, P14 | `Microsoft.Windows.AI.MachineLearning/2.1.74` | WinUI production/build: Windows ML tensor dependency; not directly called by Gallery. | [repository](https://github.com/dotnet/runtime); `MIT` | Assets(P08, P14), Cache(`System.Numerics.Tensors/9.0.0`) |
| `xunit.abstractions` | `2.0.3` | P18, P20, P21 | `xunit.extensibility.core/2.9.3` | Test: xUnit abstraction contracts. | [package project](https://github.com/xunit/xunit); License identity unknown; nuspec supplies [license URL](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) | Assets(P18, P20, P21), Cache(`xunit.abstractions/2.0.3`) |
| `xunit.analyzers` | `1.18.0` | P18, P20, P21 | `xunit/2.9.3` | Test: xUnit compile-time analyzers. | [repository](https://github.com/xunit/xunit.analyzers); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.analyzers/1.18.0`) |
| `xunit.assert` | `2.9.3` | P18, P20, P21 | `xunit/2.9.3` | Test: xUnit assertions. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.assert/2.9.3`) |
| `xunit.core` | `2.9.3` | P18, P20, P21 | `xunit/2.9.3` | Test: xUnit test construction/build entry. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.core/2.9.3`) |
| `xunit.extensibility.core` | `2.9.3` | P18, P20, P21 | `xunit.core/2.9.3`<br>`xunit.extensibility.execution/2.9.3` | Test: xUnit discovery/extensibility contracts. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.extensibility.core/2.9.3`) |
| `xunit.extensibility.execution` | `2.9.3` | P18, P20, P21 | `xunit.core/2.9.3` | Test: xUnit execution implementation. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.extensibility.execution/2.9.3`) |

## SDKs, shared frameworks, operating systems and runtime packs

| Dependency / kind | Declared constraint | Observed version or contract | Consumers / scope / purpose | Evidence / source / license |
| --- | --- | --- | --- | --- |
| .NET SDK / build toolchain | `10.0.400`, `rollForward=latestPatch`, `allowPrerelease=false`; CI requests `10.0.x` | Locally selected `10.0.401`; `10.0.301` is also installed but does not satisfy this feature-band pin | P01–P22; build, restore, pack, publish, Roslyn, Razor/XAML compilation | `global.json`, CLI, CI; local Microsoft SDK distribution; its installed `LICENSE.txt` states Microsoft .NET Library terms, not an inferred uniform SPDX license for every SDK payload. |
| `Microsoft.NET.Sdk`, `.Web`, `.Razor` / SDK components | Included in selected .NET SDK; all projects target .NET 10 | SDK 10.0.401 | `.Web`: P13/P16; `.Razor`: P02/P03/P06; other projects `.Sdk`; build | `.csproj` Sdk attributes. `build/CssBundle.targets` uses SDK `RoslynCodeTaskFactory`/`Microsoft.Build.Tasks.Core.dll`; no CSS bundler package. |
| `Microsoft.NETCore.App` / shared runtime and targeting pack | `net10.0` family; SDK default runtime baseline 10.0.0 | 10.0.12 targeting pack / installed runtime; 10.0.9 also installed | All managed projects; application/test runtime | CLI, current assets frameworkReferences, SDK BundledVersions.props. Framework-delivered assemblies are covered by this distribution rather than separate NuGet rows. |
| `Microsoft.AspNetCore.App` / shared web framework | Explicit FrameworkReference in P01/P02/P03/P06/P17/P22, implicit in P13/P16 | 10.0.12 targeting pack / installed runtime; 10.0.9 also installed | Blazor controls, interactive server Gallery and console renderer tests; production/build/test | Current project assets, .csproj, SDK/CLI. Its runtime supplies hosting, routing, DataProtection, logging, JS interop and Blazor SignalR transport. |
| `Microsoft.WindowsDesktop.App.WPF` / native desktop framework | `UseWPF=true`, net10.0-windows | 10.0.12 targeting/runtime distribution; 10.0.9 also installed | P07/P09/P10/P11/P15/P20/P21; Windows-only WPF build/runtime/test | .csproj/current assets and CLI. P12 is a dependency-only metapackage. |
| Windows desktop OS / platform contract | WPF requires Windows; WinUI minimum `10.0.17763.0`, target `10.0.26100.0` | Installed OS version not inspected; declared minimum and target recorded separately | Native WPF/WinUI projects; production/build/test | UseWPF/UseWinUI and WinUI TargetPlatformMinVersion/SupportedOSPlatformVersion; vendor OS license/version not audited. |
| Windows XAML/WinUI build tools / SDK contract | Windows-compatible XAML tooling and architecture; WinUI library allows AnyCPU/x86/x64/ARM64; Gallery x86/x64/ARM64 | Runtime IDs `win-x86`, `win-x64`, `win-arm64`; tool install/version beyond resolved NuGet packs unknown | P08/P14; WinUI build/deployment. Gallery sets WindowsPackageType=None and WindowsAppSDKSelfContained=true | WinUI .csproj and assets. WindowsAppSDKSelfContained concerns that payload; it does not by itself prove a .NET self-contained release. |
| Windows DirectWrite / native asset conversion API | IDWriteFactory5.UnpackFontFile and IDWriteFactory6.CreateFontFaceReference in `dwrite.dll` | OS API contract; no package version | `scripts/Convert-MaterialIcons.ps1`; optional maintenance/build asset conversion | P/Invoke in the converter, icon source manifests. Not a downloaded conversion tool and not a separate WPF browser runtime. |
| Browser DOM/CSS APIs / client runtime | Browser supporting the library's ResizeObserver, MutationObserver, IntersectionObserver, pointer/focus, clipboard and modern CSS contracts | Browser/version matrix unknown; no browser package pin | P03/P06/P13/P16 and JS tests; production client behavior | Framework wwwroot modules, bridge browser-preferences.js and Design CSS. HTTP/browser protocol inputs do not establish external business services. |
| Tool framework runtimes / development | DocFX net10 payload needs NETCore.App/ASP.NETCore.App 10.0.0; CSharpier net10 payload needs NETCore.App 10.0.0; both Major roll-forward | Matching 10.0.12 runtimes available; DocFX MSBuild BuildHost requests NETCore.App 8.0.0 with Major roll-forward | Documentation/formatting tools only | Cached tool runtimeconfigs. The BuildHost can roll forward; exact runtime chosen for it was not measured. |

WinUI's current `project.frameworks[*].downloadDependencies` asks for the following **ten exact SDK-selected packs**. These are recorded separately because download packs are not application PackageReference graph entries. The nine .NET packs have cached MIT nuspec expressions; Windows SDK reference license identity is unknown (its nuspec supplies a Windows SDK license URL). All ten cache metadata files record NuGet.org.

| Download pack | Exact requested/resolved cache version | Consumers / purpose | Evidence |
| --- | --- | --- | --- |
| `Microsoft.AspNetCore.App.Runtime.win-arm64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.AspNetCore.App.Runtime.win-arm64/10.0.12`) |
| `Microsoft.AspNetCore.App.Runtime.win-x64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.AspNetCore.App.Runtime.win-x64/10.0.12`) |
| `Microsoft.AspNetCore.App.Runtime.win-x86` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.AspNetCore.App.Runtime.win-x86/10.0.12`) |
| `Microsoft.NETCore.App.Runtime.win-arm64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.NETCore.App.Runtime.win-arm64/10.0.12`) |
| `Microsoft.NETCore.App.Runtime.win-x64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.NETCore.App.Runtime.win-x64/10.0.12`) |
| `Microsoft.NETCore.App.Runtime.win-x86` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.NETCore.App.Runtime.win-x86/10.0.12`) |
| `Microsoft.WindowsDesktop.App.Runtime.win-arm64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.WindowsDesktop.App.Runtime.win-arm64/10.0.12`) |
| `Microsoft.WindowsDesktop.App.Runtime.win-x64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.WindowsDesktop.App.Runtime.win-x64/10.0.12`) |
| `Microsoft.WindowsDesktop.App.Runtime.win-x86` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.WindowsDesktop.App.Runtime.win-x86/10.0.12`) |
| `Microsoft.Windows.SDK.NET.Ref` | `10.0.26100.57` | P08/P14; Windows SDK .NET compile-time references | Assets(P08/P14) downloadDependencies; cached nuspec/metadata |

## Development and documentation tool packages

| Direct tool | Declared pin | Observed package | Consumer and purpose | Source / license / evidence |
| --- | --- | --- | --- | --- |
| DocFX | `2.78.5`, tool manifest rollForward=false | Cached `docfx/2.78.5`, with net8/net9/net10 payloads | Documentation workflow and human DocFX configs; metadata generation, templates and static-site build | `.config/dotnet-tools.json`; cached nuspec links `https://github.com/dotnet/docfx`, MIT; acquisition feed unknown in the cached tool metadata. |
| CSharpier | `1.3.0`, tool manifest rollForward=false | Cached `CSharpier/1.3.0`, with net8/net9/net10 payloads | Optional repository code formatting; not invoked by current release/build CI | Tool manifest; cached nuspec links `https://github.com/belav/csharpier`, MIT; acquisition feed unknown in the cached tool metadata. |

The next table completely enumerates the **68 distinct external bundled package pairs** in the available net10 tool payloads, including DocFX's nested MSBuild BuildHost. Relationship is *bundled transitive development dependency*, not a new project PackageReference. Versions are observed in the relevant `.deps.json`; matching cache nuspec metadata is used only when present. Missing per-package source/license metadata remains unknown rather than inheriting the tool's license. The net8/net9 alternative tool payloads are not the selected SDK's normal tool target and are outside this net10 observation.

`D` = cached `docfx/2.78.5/tools/net10.0/any/docfx.deps.json`; `H` = its `BuildHost-netcore/...BuildHost.deps.json`; `C` = cached `csharpier/1.3.0/tools/net10.0/any/CSharpier.deps.json`. `DocFXBuildHost` is DocFX's metadata extraction subprocess.

| Bundled package | Observed version | Tool consumer / purpose | Source / license | Evidence |
| --- | --- | --- | --- | --- |
| `Acornima` | `1.2.0` | DocFX: JavaScript parsing/execution support | Source repository unknown; License identity unknown | D |
| `HtmlAgilityPack` | `1.12.4` | DocFX: Markdown/HTML/template/text processing | Source repository unknown; License identity unknown | D |
| `Humanizer.Core` | `2.14.1` | DocFX: Markdown/HTML/template/text processing | [repository](https://github.com/Humanizr/Humanizer); `MIT` | D; matching cached nuspec |
| `ICSharpCode.Decompiler` | `9.1.0.7988` | DocFX: Code analysis and API metadata/formatting | Source repository unknown; License identity unknown | D |
| `ini-parser-netstandard` | `2.5.3` | CSharpier: Structured document/configuration parsing | Source repository unknown; License identity unknown | C |
| `Jint` | `4.6.0` | DocFX: JavaScript parsing/execution support | Source repository unknown; License identity unknown | D |
| `Json.More.Net` | `2.1.1` | DocFX: Structured document/configuration parsing | Source repository unknown; License identity unknown | D |
| `JsonPointer.Net` | `5.3.1` | DocFX: Structured document/configuration parsing | Source repository unknown; License identity unknown | D |
| `JsonSchema.Net` | `7.4.0` | DocFX: Structured document/configuration parsing | Source repository unknown; License identity unknown | D |
| `LovettSoftware.XmlDiff` | `1.1.0` | CSharpier: Structured document/configuration parsing | Source repository unknown; License identity unknown | C |
| `Markdig` | `1.0.0` | DocFX: Markdown/HTML/template/text processing | Source repository unknown; License identity unknown | D |
| `Microsoft.Bcl.AsyncInterfaces` | `9.0.0` | DocFX: Tool runtime support | Source repository unknown; License identity unknown | D |
| `Microsoft.Build.Framework` | `18.0.2` | DocFX: MSBuild/project/solution loading | [repository](https://github.com/dotnet/msbuild); `MIT` | D; matching cached nuspec |
| `Microsoft.Build.Locator` | `1.10.2` | DocFXBuildHost: MSBuild/project/solution loading | Source repository unknown; License identity unknown | H |
| `Microsoft.Build` | `18.0.2` | DocFX: MSBuild/project/solution loading | Source repository unknown; License identity unknown | D |
| `Microsoft.CodeAnalysis.Common` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.CodeAnalysis.Common` | `5.3.0` | CSharpier: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | C; matching cached nuspec |
| `Microsoft.CodeAnalysis.CSharp.Workspaces` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.CodeAnalysis.CSharp` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.CodeAnalysis.CSharp` | `5.3.0` | CSharpier: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | C; matching cached nuspec |
| `Microsoft.CodeAnalysis.VisualBasic.Workspaces` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | Source repository unknown; License identity unknown | D |
| `Microsoft.CodeAnalysis.VisualBasic` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | Source repository unknown; License identity unknown | D |
| `Microsoft.CodeAnalysis.Workspaces.Common` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.CodeAnalysis.Workspaces.MSBuild` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.Extensions.Configuration.Abstractions` | `8.0.0` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Configuration.Binder` | `8.0.2` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Configuration` | `8.0.0` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.DependencyInjection` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Logging.Abstractions` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Logging.Configuration` | `8.0.1` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Logging` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Options.ConfigurationExtensions` | `8.0.0` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Options` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Primitives` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.NET.StringTools` | `18.0.2` | DocFX: MSBuild/project/solution loading | Source repository unknown; License identity unknown | D |
| `Microsoft.Playwright` | `1.58.0` | DocFX: Bundled documentation rendering/diagram/PDF support; feature use not established | Source repository unknown; License identity unknown | D |
| `Microsoft.VisualStudio.SolutionPersistence` | `1.0.52` | DocFX: MSBuild/project/solution loading | [repository](https://github.com/microsoft/vs-solutionpersistence); `MIT` | D; matching cached nuspec |
| `Newtonsoft.Json` | `13.0.3` | DocFXBuildHost: Structured document/configuration parsing | [repository](https://github.com/JamesNK/Newtonsoft.Json); `MIT` | H; matching cached nuspec |
| `Newtonsoft.Json` | `13.0.4` | DocFX: Structured document/configuration parsing | [repository](https://github.com/JamesNK/Newtonsoft.Json); `MIT` | D; matching cached nuspec |
| `NReco.Logging.File` | `1.2.2` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `OneOf` | `3.0.271` | DocFX: Tool runtime support | Source repository unknown; License identity unknown | D |
| `PdfPig` | `0.1.13` | DocFX: Bundled documentation rendering/diagram/PDF support; feature use not established | Source repository unknown; License identity unknown | D |
| `PlantUml.Net` | `1.4.80` | DocFX: Bundled documentation rendering/diagram/PDF support; feature use not established | Source repository unknown; License identity unknown | D |
| `Spectre.Console.Cli` | `0.53.1` | DocFX: Tool command-line infrastructure | Source repository unknown; License identity unknown | D |
| `Spectre.Console` | `0.54.0` | DocFX: Tool command-line infrastructure | Source repository unknown; License identity unknown | D |
| `Stubble.Core` | `1.10.8` | DocFX: Markdown/HTML/template/text processing | Source repository unknown; License identity unknown | D |
| `System.Collections.Immutable` | `9.0.0` | DocFXBuildHost: Tool runtime support | [repository](https://github.com/dotnet/runtime); `MIT` | H; matching cached nuspec |
| `System.CommandLine` | `2.0.0-beta4.22272.1` | CSharpier: Tool command-line infrastructure | Source repository unknown; License identity unknown | C |
| `System.CommandLine` | `2.0.0-rtm.25509.106` | DocFXBuildHost: Tool command-line infrastructure | Source repository unknown; License identity unknown | H |
| `System.Composition.AttributedModel` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition.Convention` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition.Hosting` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition.Runtime` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition.TypedParts` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Configuration.ConfigurationManager` | `9.0.0` | DocFX: Tool runtime support | Source repository unknown; License identity unknown | D |
| `System.Diagnostics.EventLog` | `9.0.0` | DocFX: Tool runtime support | Source repository unknown; License identity unknown | D |
| `System.IO.Abstractions.TestingHelpers` | `22.0.16` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `System.IO.Abstractions` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `System.IO.Hashing` | `9.0.10` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `System.Reflection.MetadataLoadContext` | `9.0.0` | DocFX: Code analysis and API metadata/formatting | Source repository unknown; License identity unknown | D |
| `System.Security.Cryptography.ProtectedData` | `9.0.6` | DocFX: Tool runtime support | [repository](https://github.com/dotnet/runtime); `MIT` | D; matching cached nuspec |
| `TestableIO.System.IO.Abstractions.TestingHelpers` | `22.0.16` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `TestableIO.System.IO.Abstractions.Wrappers` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `TestableIO.System.IO.Abstractions` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `Testably.Abstractions.FileSystem.Interface` | `10.1.0` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `YamlDotNet` | `16.3.0` | DocFX, CSharpier: Structured document/configuration parsing | [repository](https://github.com/aaubry/YamlDotNet); `MIT` | D, C; matching cached nuspec |

Internal tool assemblies marked `type=project` in those payloads are also accounted for: `Docfx.App/2.78.5` (DocFX), `Docfx.Build.Common/2.78.5` (DocFX), `Docfx.Build.ManagedReference/2.78.5` (DocFX), `Docfx.Build.OverwriteDocuments/2.78.5` (DocFX), `Docfx.Build.RestApi/2.78.5` (DocFX), `Docfx.Build.SchemaDriven/2.78.5` (DocFX), `Docfx.Build.UniversalReference/2.78.5` (DocFX), `Docfx.Build/2.78.5` (DocFX), `Docfx.Common/2.78.5` (DocFX), `Docfx.DataContracts.Common/2.78.5` (DocFX), `Docfx.DataContracts.RestApi/2.78.5` (DocFX), `Docfx.DataContracts.UniversalReference/2.78.5` (DocFX), `Docfx.Dotnet/2.78.5` (DocFX), `Docfx.Glob/2.78.5` (DocFX), `Docfx.MarkdigEngine.Extensions/2.78.5` (DocFX), `Docfx.MarkdigEngine/2.78.5` (DocFX), `Docfx.Plugins/2.78.5` (DocFX), `Docfx.YamlSerialization/2.78.5` (DocFX), `docfx/2.78.5` (DocFX), `Microsoft.CodeAnalysis.Workspaces.MSBuild.BuildHost/5.0.0-2.25567.12` (DocFXBuildHost), `CSharpier.Core/1.3.0` (CSharpier), `CSharpier/1.3.0` (CSharpier). They are assemblies shipped by the pinned tool package, not local Flourish source projects. DocFX default/modern templates and their bundled browser assets are covered by that pinned tool distribution; individual embedded UI library provenance/versions were not extracted during this inspection.

## Build, test, CI and optional development tools

| Dependency / kind | Declared constraint | Observed version | Consumers / scope / purpose | Source / license / evidence |
| --- | --- | --- | --- | --- |
| Node.js / JS test runtime | CI major `24`; no npm/package-lock manifest | Local `v26.4.0` | Node built-in test runner and assertion modules in tests/*.mjs/.js; build/test | build.yml, ReleaseSettings.psd1, CLI. Node distribution source/license not independently inspected. Built-in modules are covered by Node. |
| PowerShell / script runtime | Launcher states 5.1+; fetch skill uses #requires 5.1; release helper invokes pwsh | Local PowerShell `7.6.5`; Windows PowerShell version not separately measured | start/fetch/release/check scripts, CI pwsh shell; build/dev/test | .bat launchers, Start-Project.ps1, fetch-agents.ps1 and workflows; vendor/runtime license identity unknown. |
| Git / source/release tool | No version pin | Local `2.56.0.windows.1` | Framework sync, release ancestry/tag checks, changelog tag discovery; development/release | git CLI, fetch-agents.ps1, Release-Common.ps1, Publish-Helper.ps1; distribution license/source not independently inspected. |
| Visual Studio / optional IDE | No version pin or required IDE startup path | Installed version unknown | Optional editing/debugging and WinUI XAML tool provisioning; no Blazor/WPF launcher requirement | scripts/StartSettings.json and launcher diagnostics. Development BrowserLink/Hot Reload assets are IDE/framework supplied, not project package additions. License unknown. |
| Playwright JS module / optional diagnostic | `require(PLAYWRIGHT_MODULE \|\| 'playwright')`; not declared in a repository package manifest | Unknown/unpinned | Only optional `build/Test-CultureIntegration.cjs` browser integration harness; not ReleaseSettings JavaScriptTests | Harness source. It is distinct from DocFX's bundled `Microsoft.Playwright` NuGet package. Source/license unknown for the caller-supplied JS installation. |
| Installed Edge/Chromium executable / optional headless test tool | No package/version pin; action-menu checks accept `FLOURISH_HEADLESS_BROWSER`, then probe installed Edge/Chrome/Chromium; Culture harness uses `BROWSER_EXECUTABLE` | Microsoft Edge `155.0.4283.45` observed from its installed Windows executable on 2026-10-12; other browsers unknown | `action-menu-dom.mjs`: optional nine SSR geometry regressions alongside controller checks; optional Culture integration harness | Executable file version and test source. No browser/package download; missing executable explicitly skips nine geometry checks. Browser vendor license/distribution identity beyond the installed Microsoft Edge executable remains unverified. |
| Installed AGENTS instruction framework / development workflow | Framework `1.0.0`; source ref `main` | Imported commit `03ec2f6ebacb443585056b83c581f89a99c8ee36` | Router, common standards, audit/sync skills; optional Git-based framework check/sync | AGENTS.framework.json/lock.json; `https://github.com/Evigila/AGENTS.md.git`; source license identity unknown locally. No remote update check performed here. |
| GitHub-hosted Windows/Linux runners / CI platform | `windows-latest` for build/publish/docs build; `ubuntu-latest` for Pages deploy | Image versions and preinstalled tool versions unknown | GitHub Actions workflows; build/test/publish/deployment | Workflow YAML. Floating runner labels are constraints, not exact environment locks; service terms/license identity not inspected. |

CI actions are version-tagged remote dependencies, not exact SHA pins. The complete set of distinct action/tag pairs is below. Their source repository identities are directly given by `uses:`; individual licenses and the commits currently behind tags are unknown in this audit.

| Action | Declared ref | Workflow consumer / purpose |
| --- | --- | --- |
| `actions/checkout` ([source](https://github.com/actions/checkout)) | `v7` | build.yml: source checkout for build/publish |
| `actions/checkout` ([source](https://github.com/actions/checkout)) | `v5` | docs.yml: source checkout |
| `actions/setup-dotnet` ([source](https://github.com/actions/setup-dotnet)) | `v5` | both workflows: .NET 10 installation |
| `actions/setup-node` ([source](https://github.com/actions/setup-node)) | `v7` | build.yml: Node 24 |
| `actions/upload-artifact` ([source](https://github.com/actions/upload-artifact)) | `v7` | build.yml: verified NuGet artifact transfer |
| `actions/download-artifact` ([source](https://github.com/actions/download-artifact)) | `v8` | build.yml: NuGet publication artifact retrieval |
| `NuGet/login` ([source](https://github.com/NuGet/login)) | `v1` | build.yml: OIDC Trusted Publishing login |
| `actions/configure-pages` ([source](https://github.com/actions/configure-pages)) | `v6` | docs.yml: GitHub Pages configuration |
| `actions/upload-pages-artifact` ([source](https://github.com/actions/upload-pages-artifact)) | `v5` | docs.yml: static-site artifact transfer |
| `actions/deploy-pages` ([source](https://github.com/actions/deploy-pages)) | `v5` | docs.yml: GitHub Pages deployment |

## External services and assets

| Dependency / kind | Version or contract | Consumers / scope / purpose | Source / license / declaration evidence |
| --- | --- | --- | --- |
| NuGet.org v3 / package feed | `https://api.nuget.org/v3/index.json`; package-style service version not applicable | Restore for all external package consumers; CI Trusted Publishing; build/release | Workflows and package-consumer/Release scripts. Service terms not audited. Cache metadata verifies the application packages' actual acquisition feed. No credentials retained here. |
| Local Flourish/optional Essential package directories / test feeds | Per-run local feed paths and declared release package IDs/versions | scripts/Test-Release.ps1 and build/Test-BlazorPackageConsumers.ps1; isolated build/test consumption | Source mappings route Flourish candidates locally; optional EssentialPackageDirectory routes explicitly supplied Essential candidates. These are local files, not an external live service. |
| GitHub source/artifact/Actions/OIDC services | Git remote and workflow artifact/OIDC contracts; not a package version | Repository checkout, optional AGENTS synchronization, release automation | Workflow YAML and configured source URLs; no runtime Gallery GitHub API dependency was found. |
| GitHub Pages / static documentation hosting | GitHub Pages deployment contract | docs.yml deploy job; optional documentation deployment | Environment github-pages and deploy-pages action. It is not a production Gallery backend requirement. |
| Google Fonts Roboto / remotely loaded documentation font | CSS API request `family=Roboto:wght@100;400;700&display=swap`; exact delivered font build unpinned | Human documentation's DocFX Material theme only; browser font request | `docs/material/material/public/main.css` imports fonts.googleapis.com; Roboto license identity/version not present in maintained evidence and remains unknown. It is not a Flourish Gallery runtime font dependency. |
| DocFX Material / vendored documentation template | Upstream revision/version unknown | DocFX docs templates; documentation build/display | `docs/material/LICENSE` verifies MIT, copyright Oscar Vásquez; `docs/material/docs/docfx.json` identifies `https://github.com/ovasquez/docfx-material`. Human documentation remains unchanged. |
| Material Symbols Outlined / vendored variable icon font and codepoint map | Upstream commit `737e3324305806514d7909874fa1818ae1808232`; axes FILL 0–1, GRAD -50–200, opsz 20–48, wght 100–700 | P03/P11 and Gallery consumers; production icon rendering | Both icon source.json files link google/material-design-icons; retained Apache-2.0 license. Blazor ships WOFF2; WPF ships DirectWrite-derived TTF and FILL=1 Brotli geometry. SHA-256/byte provenance exists in source manifests; no Google CDN request for these icons. |
| Material Symbols `browse` SVG / vendored icon derivative | Same upstream commit, symbols/web/browse/materialsymbolsoutlined/browse_24px.svg | Framework browse.svg and Gallery logo/favicon SVGs; production assets | Per-SVG attribution comments verify Google source and Apache-2.0. Gallery logo applies the documented scale derivative. |
| Segoe UI and system fallback fonts / OS browser font resource | OS font identity; no bundled font version or license redistribution | Blazor Design `--f-font: 'Segoe UI',system-ui,sans-serif`; WPF resources use Segoe UI | Design foundation.css and WPF Themes/Resources.xaml. Source is installed OS/browser fallback; exact file/version and OS font license are unknown. |

No database, cloud storage, cache server, telemetry endpoint, authentication provider or other mandatory external business service was found in maintained production source/configuration. Gallery data and translation resources are local. `example.test`/`example.invalid` URLs and localhost test endpoints are fixtures; XML namespace/schema URLs and outbound documentation links are identifiers or navigation, not live application service dependencies.

## Active constraints and unresolved facts

- Essential.Culture 1.4.0 is the current authorized dependency. Runtime adapters and generator are external NuGet packages even though Essential is maintained by the same organization. No sibling Essential source reference is present. Modular resources are selected through Essential CultureModule generation/deployment; no additional JSON merge or filesystem discovery dependency was introduced.
- Flourish source version is 1.4.1, prepared for the same six Core/Blazor packages; the latest verified public release remains 1.4.0 until the new publication completes. Native WPF and its Culture bridge remain excluded. The dated audit graphs retain their 1.1.3 source-version observation; separate release/public-consumer artifacts establish 1.4.0 restore and consumption.
- No dependency adoption or upgrade was performed for this documentation audit. Unknown licenses, source commits, runner/browser/OS versions and unpinned optional tools remain explicitly unknown. The scope is the resolved project/tool distributions above; it does not enumerate every component internal to an OS, browser, SDK or CI runner image.
- The human-maintained `docs/docfx.en-us.json` and `docs/docfx.zh-cn.json` currently name missing `src/Flourish.WPF/Flourish.WPF.csproj` and `src/Flourish.WPF/Assets/favicon.ico`. The actual meta project is P12. Successful documentation API/resource generation against those stale paths is unverified; ownership rules prevent silently editing the configs during this audit.
- Root `LICENSE.txt` contains MIT terms but still has `[year] [fullname]` attribution placeholders. Package MIT/Apache declarations and the retained third-party icon licenses are recorded as actual evidence; this inventory does not invent ownership attribution or repair the root license.
- Optional `build/Test-CultureIntegration.cjs` requires separately available JS Playwright and Edge/Chromium and has no maintained lock/installation manifest. This audit did not install or invoke them. Other Node regressions use built-in modules and do not need npm dependencies.
- Local Node 26.4.0 differs from CI major 24; local SDK 10.0.401 satisfies global.json, while CI's floating 10.0.x install alone does not guarantee it chooses the same feature band required by latestPatch. No compatibility failure is claimed without an actual build evidence change.
- Licenses for cached Microsoft.Windows SDK/App SDK payloads use their actual file/URL metadata, including Microsoft vendor terms; they must not be uniformly relabeled MIT. Unknown bundled tool dependency metadata is not a reason to guess or add packages.
