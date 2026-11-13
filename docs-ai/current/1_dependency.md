# Current dependency inventory

## Evidence and interpretation

Source declarations remain Flourish VersionPrefix 1.4.7, no suffix, and EssentialCultureVersion 1.4.0. The SDK constraint is 10.0.400 with latestPatch roll-forward and prerelease disabled. This consolidation changes no application package version or project reference. SAF 2.0.0 replaces instruction tooling; DocFX and its documentation-only assets/workflow are removed under explicit authorization.

The recorded local 1.4.8-preview.14 candidate is an explicit build override, independently consumed in Colligere. It does not promote the source version or establish a new public release. Last recorded stable publication is v1.4.7; detailed prior candidates and exact release receipts remain in archived source documents.

The complete 2026-11-10 machine-readable capture is [archived evidence](1_archived/dependency-evidence-2026-11-10.json). It records declarations/references, matching asset timestamps/hashes, immediate dependency parents, full consumer sets, cache metadata and SDK download requests. Its 22 mixed-age asset captures contain 51 external package/version pairs. Blazor resolves Essential 1.4.0, while older native caches still show 1.3.0 despite current 1.4.0 declarations; the stale native results do not verify the current restore. The following captured table is retained without changing its package/version/consumer/source/license values.

Generated bin/obj, output feeds, test consumers and verification directories are removed by cleanup. Paths/timestamps/hashes to those outputs describe historical acquisition only; this document does not claim those files remain or that a fresh restore/build ran. The full original inventory, older 49-pair captures and all former DocFX tool provenance remain in the complete pre-cleanup snapshot. NuGet lower-bound declarations are distinct from observed exact versions; unknown license/source metadata remains unknown.

## Captured complete external package closure

| Package | Observed version | Relationship | Consumer projects | Cached license metadata | Source metadata |
| --- | --- | --- | --- | --- | --- |
| Arkheide.Essential.Culture.Blazor | 1.4.0 | Direct P06 | P04, P06, P13, P17, P19, P22 | MIT | [declared source](https://github.com/Evigila/Essential) |
| Arkheide.Essential.Culture.Generator | 1.3.0 | Transitive | P07, P15, P20, P21 | MIT | [declared source](https://github.com/Evigila/Arkheide.Essential.Culture.git) |
| Arkheide.Essential.Culture.Generator | 1.4.0 | Transitive | P04, P06, P13, P17, P19, P22 | MIT | [declared source](https://github.com/Evigila/Essential) |
| Arkheide.Essential.Culture.Wpf | 1.3.0 | Direct P07 | P07, P15, P20, P21 | MIT | [declared source](https://github.com/Evigila/Arkheide.Essential.Culture.git) |
| Arkheide.Essential.Culture | 1.3.0 | Transitive | P07, P15, P20, P21 | MIT | [declared source](https://github.com/Evigila/Arkheide.Essential.Culture.git) |
| Arkheide.Essential.Culture | 1.4.0 | Transitive | P04, P06, P13, P17, P19, P22 | MIT | [declared source](https://github.com/Evigila/Essential) |
| Castle.Core | 5.1.1 | Transitive | P18 | Apache-2.0 | [declared source](https://github.com/castleproject/Core) |
| Microsoft.AspNetCore.App.Internal.Assets | 10.0.11 | Web SDK injected | P13, P16 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.CodeCoverage | 17.14.1 | Transitive | P18, P20, P21 | MIT | [declared source](https://github.com/microsoft/vstest) |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.Configuration.FileExtensions | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.Configuration.Json | 10.0.8 | Direct P05 | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.Configuration | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.Diagnostics.Abstractions | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.FileProviders.Abstractions | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.FileProviders.Physical | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.FileSystemGlobbing | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.Hosting.Abstractions | 10.0.8 | Direct P05 | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.Logging.Abstractions | 10.0.8 | Direct P05 | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.Options | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.Extensions.Primitives | 10.0.8 | Transitive | P04, P05, P18, P19 | MIT | [declared source](https://github.com/dotnet/dotnet) |
| Microsoft.NET.Test.Sdk | 17.14.1 | Direct P18, P20, P21 | P18, P20, P21 | MIT | [declared source](https://github.com/microsoft/vstest) |
| Microsoft.TestPlatform.ObjectModel | 17.14.1 | Transitive | P18, P20, P21 | MIT | [declared source](https://github.com/microsoft/vstest) |
| Microsoft.TestPlatform.TestHost | 17.14.1 | Transitive | P18, P20, P21 | MIT | [declared source](https://github.com/microsoft/vstest) |
| Microsoft.Web.WebView2 | 1.0.3719.77 | Transitive | P08, P14 | LICENSE.txt | [declared source](https://aka.ms/webview) |
| Microsoft.Windows.AI.MachineLearning | 2.1.74 | Transitive | P08, P14 | license.txt | [declared source](https://github.com/microsoft/windowsappsdk) |
| Microsoft.Windows.SDK.BuildTools.MSIX | 1.7.251221100 | Transitive | P08, P14 | sdk_license.txt | [declared source](https://aka.ms/WinSDKProjectURL) |
| Microsoft.Windows.SDK.BuildTools | 10.0.26100.4654 | Transitive | P08, P14 | https://aka.ms/WinSDKLicenseURL | [declared source](https://aka.ms/WinSDKProjectURL) |
| Microsoft.WindowsAppSDK.AI | 2.3.4 | Transitive | P08, P14 | license.txt | [declared source](https://github.com/microsoft/windowsappsdk) |
| Microsoft.WindowsAppSDK.Base | 2.0.4 | Transitive | P08, P14 | license.txt | [declared source](https://github.com/microsoft/windowsappsdk) |
| Microsoft.WindowsAppSDK.DWrite | 2.1.0 | Transitive | P08, P14 | license.txt | [declared source](https://aka.ms/WindowsAppSDK) |
| Microsoft.WindowsAppSDK.Foundation | 2.3.5 | Transitive | P08, P14 | license.txt | [declared source](https://aka.ms/windowsappsdk) |
| Microsoft.WindowsAppSDK.InteractiveExperiences | 2.1.3 | Transitive | P08, P14 | license.txt | [declared source](https://aka.ms/windowsappsdk) |
| Microsoft.WindowsAppSDK.ML | 2.1.74 | Transitive | P08, P14 | license.txt | [declared source](https://github.com/microsoft/windowsappsdk) |
| Microsoft.WindowsAppSDK.Runtime | 2.3.1 | Transitive | P08, P14 | license.txt | [declared source](https://github.com/microsoft/windowsappsdk) |
| Microsoft.WindowsAppSDK.Widgets | 2.0.5 | Transitive | P08, P14 | license.txt | [declared source](https://github.com/microsoft/windowsappsdk) |
| Microsoft.WindowsAppSDK.WinUI | 2.3.0 | Transitive | P08, P14 | license.txt | [declared source](https://github.com/microsoft/windowsappsdk) |
| Microsoft.WindowsAppSDK | 2.3.1 | Direct P08, P14 | P08, P14 | license.txt | [declared source](https://github.com/microsoft/windowsappsdk) |
| Moq | 4.20.72 | Direct P18 | P18 | BSD-3-Clause | [declared source](https://github.com/moq/moq) |
| Newtonsoft.Json | 13.0.3 | Transitive | P18, P20, P21 | MIT | [declared source](https://github.com/JamesNK/Newtonsoft.Json) |
| System.Diagnostics.EventLog | 6.0.0 | Transitive | P18 | MIT | [declared source](https://github.com/dotnet/runtime) |
| System.Numerics.Tensors | 9.0.0 | Transitive | P08, P14 | MIT | [declared source](https://github.com/dotnet/runtime) |
| xunit.abstractions | 2.0.3 | Transitive | P18, P20, P21 | https://raw.githubusercontent.com/xunit/xunit/master/license.txt | [declared source](https://github.com/xunit/xunit) |
| xunit.analyzers | 1.18.0 | Transitive | P18, P20, P21 | Apache-2.0 | [declared source](https://github.com/xunit/xunit.analyzers) |
| xunit.assert | 2.9.3 | Transitive | P18, P20, P21 | Apache-2.0 | [declared source](https://github.com/xunit/xunit) |
| xunit.core | 2.9.3 | Transitive | P18, P20, P21 | Apache-2.0 | [declared source](https://github.com/xunit/xunit) |
| xunit.extensibility.core | 2.9.3 | Transitive | P18, P20, P21 | Apache-2.0 | [declared source](https://github.com/xunit/xunit) |
| xunit.extensibility.execution | 2.9.3 | Transitive | P18, P20, P21 | Apache-2.0 | [declared source](https://github.com/xunit/xunit) |
| xunit.runner.visualstudio | 3.1.4 | Direct P18, P20, P21 | P18, P20, P21 | Apache-2.0 | [declared source](https://github.com/xunit/visualstudio.xunit) |
| xunit | 2.9.3 | Direct P18, P20, P21 | P18, P20, P21 | Apache-2.0 | [declared source](https://github.com/xunit/xunit) |

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
| P14 | `src/Gallery.Flourish.WinUI3/Gallery.Flourish.WinUI3.csproj` | `net10.0-windows10.0.26100.0` | None |
| P15 | `src/Gallery.Flourish.WPF/Gallery.Flourish.WPF.csproj` | `net10.0-windows` | P12 (`Flourish.WPF`)<br>P07 (`Flourish.Extensions.Culture.WPF`) |
| P16 | `tests/Tests.Flourish.Blazor.Native/Tests.Flourish.Blazor.Native.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`) |
| P17 | `tests/Tests.Flourish.Blazor/Tests.Flourish.Blazor.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`)<br>P02 (`Flourish.Blazor.Design`)<br>P06 (`Flourish.Extensions.Culture.Blazor`) |
| P18 | `tests/Tests.Flourish.Core/Tests.Flourish.Core.csproj` | `net10.0` | P05 (`Flourish.Core`) |
| P19 | `tests/Tests.Flourish.Extensions.Culture.Blazor/Tests.Flourish.Extensions.Culture.Blazor.csproj` | `net10.0` | P06 (`Flourish.Extensions.Culture.Blazor`) |
| P20 | `tests/Tests.Flourish.Extensions.Culture.WPF/Tests.Flourish.Extensions.Culture.WPF.csproj` | `net10.0-windows` | P07 (`Flourish.Extensions.Culture.WPF`) |
| P21 | `tests/Tests.Flourish.WPF/Tests.Flourish.WPF.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`)<br>P11 (`Flourish.WPF.Framework`)<br>P10 (`Flourish.WPF.Design`)<br>P15 (`Gallery.Flourish.WPF`) |
| P22 | `tests/Tests.Gallery.Flourish.Blazor/Tests.Gallery.Flourish.Blazor.csproj` | `net10.0` | P13 (`Gallery.Flourish.Blazor`) |

The 22 source/diagnostic/test projects declare 33 local edges. Their source identities are not externally restored Flourish packages. Blazor Abstract uses Core, Framework uses Abstract, and optional Design/Culture use Framework. Native WPF is an independent graph; WinUI placeholders declare no Flourish source dependency. The aggregate packages are dependency-only, while Gallery/tests consume real production entries. No sibling Essential source reference exists.

## Direct and SDK-injected NuGet declarations

The declared requirements and purposes below retain their original capture metadata. The complete 51-pair table above and machine-readable evidence are authoritative for that later mixed-age observation; the older observed values in this declaration-purpose table are not a fresh restore.

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

## SDKs, shared frameworks and runtime downloads

These are retained declaration/capture rows. Existing caches, SDK-injected packs and download dependencies can have different dates/platforms; cleaning an obj tree or observing one Web restore does not recapture native/WinUI runtimes. Current global.json remains the SDK selection contract.


The observed SDK/pack/runtime versions in this table are the historical audit environment, not an assertion that those versions remain installed for the current local candidate. The candidate retains the existing `global.json` constraint; its actual selected SDK/assets are recorded with the coordinating build evidence above. No SDK or runtime installation is performed by this update.

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
| Common icon asset regeneration / optional maintenance | Existing Python 3.12.14 standard-library zlib/struct; PowerShell 7 and Windows PowerShell/WPF GlyphTypeface.ComputeSubset | No new package or installation; native/standard-library tools only | scripts/Subset-MaterialIcons.py, common-icons.json; original glyph IDs and all retained variation deltas checked independently | Checked-in WOFF assets mean normal source builds and consumers need no Python, WPF or regeneration step. |
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

## Optional formatter tool

The retained local tool is CSharpier 1.3.0, rollForward=false, for optional code formatting. Its cached nuspec points to https://github.com/belav/csharpier and MIT; original acquisition source is unknown. DocFX is no longer a declared tool, workflow or runtime asset. Its former transitive payload is historical evidence only.

The table below preserves all captured CSharpier net10 bundled package pairs, including the shared YamlDotNet row with the removed DocFX consumer/evidence stripped. These are bundled development dependencies rather than project PackageReferences. C means the captured CSharpier.deps.json; exact source/license evidence stays unknown when originally unavailable. The alternative net8/net9 tool payloads are outside that selected-target capture.

| Bundled package | Observed version | Tool consumer / purpose | Source / license | Evidence |
| --- | --- | --- | --- | --- |
| `ini-parser-netstandard` | `2.5.3` | CSharpier: Structured document/configuration parsing | Source repository unknown; License identity unknown | C |
| `LovettSoftware.XmlDiff` | `1.1.0` | CSharpier: Structured document/configuration parsing | Source repository unknown; License identity unknown | C |
| `Microsoft.CodeAnalysis.Common` | `5.3.0` | CSharpier: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | C; matching cached nuspec |
| `Microsoft.CodeAnalysis.CSharp` | `5.3.0` | CSharpier: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | C; matching cached nuspec |
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
| `NReco.Logging.File` | `1.2.2` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `System.CommandLine` | `2.0.0-beta4.22272.1` | CSharpier: Tool command-line infrastructure | Source repository unknown; License identity unknown | C |
| `System.IO.Abstractions.TestingHelpers` | `22.0.16` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `System.IO.Abstractions` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `System.IO.Hashing` | `9.0.10` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `TestableIO.System.IO.Abstractions.TestingHelpers` | `22.0.16` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `TestableIO.System.IO.Abstractions.Wrappers` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `TestableIO.System.IO.Abstractions` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `Testably.Abstractions.FileSystem.Interface` | `10.1.0` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `YamlDotNet` | `16.3.0` | CSharpier: Structured document/configuration parsing | [repository](https://github.com/aaubry/YamlDotNet); `MIT` | C; matching cached nuspec |

## Build, test and maintenance tooling

| Tool or service | Constraint and use | Evidence and limitation |
| --- | --- | --- |
| Node.js | CI major 24; built-in test/assert/fs/vm helpers, no repository npm install | ReleaseSettings and build.yml; historical local observations differ by date |
| PowerShell | Existing Windows 5.1 startup compatibility and available pwsh for release/build checks | Debug and Release wrappers plus implementation scripts; no runtime installed by cleanup |
| Git | Framework provenance, source/tag/ancestry and release checks | Existing CLI, origin/release scripts; no global configuration change |
| SAF | 2.0.0 instruction tooling, Development only | SAF.json/config/lock and origin/local handlers; MIT upstream, uses existing Git/CMD/PowerShell |
| CSharpier | Optional pinned formatter described above | No replacement documentation engine is adopted |
| Visual Studio / Windows XAML tools | Optional editor; declared WinUI/native toolchain prerequisites | No Blazor launcher requirement or certified WinUI acceptance |
| Optional Playwright JS / installed browser harness | Caller-supplied module/browser for the maintained optional Culture harness | No pinned npm manifest or browser download; unknown installation/license details remain unknown |
| Installed Edge/Chromium | Optional headless geometry checks using configured/installed executables | Missing engines explicitly skip their checks; old observed browser versions are historical |
| GitHub Actions windows-latest | Build/test/Trusted Publishing CI | Floating runner/tool versions are not exact environment locks |

Current build workflow action refs remain tagged dependencies: checkout v7, setup-dotnet v5, setup-node v7, upload-artifact v7, download-artifact v8 and NuGet/login v1. They are not SHA pins; individual resolved commits/licenses were not rechecked here. Documentation-only Pages and older docs checkout action usages are removed with docs.yml.

## External services and required assets

| Dependency / kind | Version or contract | Consumers / scope / purpose | Source / license / declaration evidence |
| --- | --- | --- | --- |
| NuGet.org v3 / package feed | `https://api.nuget.org/v3/index.json`; package-style service version not applicable | Restore for all external package consumers; CI Trusted Publishing; build/release | Workflows and package-consumer/Release scripts. Service terms not audited. Cache metadata verifies the application packages' actual acquisition feed. No credentials retained here. |
| Local Flourish/optional Essential package directories / test feeds | Per-run local feed paths and declared release package IDs/versions | scripts/Test-Release.ps1 and build/Test-BlazorPackageConsumers.ps1; isolated build/test consumption | Source mappings route Flourish candidates locally; optional EssentialPackageDirectory routes explicitly supplied Essential candidates. These are local files, not an external live service. |
| GitHub source/artifact/Actions/OIDC services | Git remote and workflow artifact/OIDC contracts; not a package version | Repository checkout, optional AGENTS synchronization, release automation | Workflow YAML and configured source URLs; no runtime Gallery GitHub API dependency was found. |
| Material Symbols Outlined / vendored variable icon font and codepoint map | Upstream commit `737e3324305806514d7909874fa1818ae1808232`; axes FILL 0–1, GRAD -50–200, opsz 20–48, wght 100–700 | P03/P11 and Gallery consumers; production icon rendering | Both icon source.json files link google/material-design-icons; retained Apache-2.0 license. Blazor ships a derived common WOFF with disjoint ranges and the unchanged complete WOFF2 fallback; WPF ships DirectWrite-derived TTF and FILL=1 Brotli geometry. SHA-256/byte provenance exists in source manifests; no Google CDN request for these icons. |
| Material Symbols `browse` SVG / vendored icon derivative | Same upstream commit, symbols/web/browse/materialsymbolsoutlined/browse_24px.svg | Framework browse.svg and Gallery logo/favicon SVGs; production assets | Per-SVG attribution comments verify Google source and Apache-2.0. Gallery logo applies the documented scale derivative. |
| Segoe UI and system fallback fonts / OS browser font resource | OS font identity; no bundled font version or license redistribution | Blazor Design `--f-font: 'Segoe UI',system-ui,sans-serif`; WPF resources use Segoe UI | Design foundation.css and WPF Themes/Resources.xaml. Source is installed OS/browser fallback; exact file/version and OS font license are unknown. |

NuGet.org/GitHub services are build/release/maintenance boundaries rather than Gallery business backends. The optional local candidate feeds are regenerated inputs for an intentional verification run, not required old artifact directories. Checked-in icon/font/vector/license resources are required source assets and survive cleanup.

No mandatory application database, cloud storage/cache, message broker, telemetry or authentication provider is identified by this library graph. Example URLs and local test endpoints are fixtures or navigation identifiers. Google Fonts Roboto, the vendored DocFX Material template and Pages hosting were documentation-only dependencies and are no longer active.

## Constraints and unresolved evidence

- Essential.Culture 1.4.0 is the declared optional dependency; its generator/runtime remain real external packages even when maintained by the same organization. Framework does not acquire a direct Essential dependency.
- The six-package Core/Blazor manifest, required assets and source version remain unchanged. Native WPF and WinUI are outside publication; stale captures do not prove Windows/platform restore or execution.
- Root LICENSE.txt retains its existing attribution placeholders. This inventory records MIT/package and third-party icon provenance without inventing legal attribution or modifying license ownership.
- Unknown OS/browser/runner/source-license facts and unpinned optional diagnostics remain unknown. The inventory enumerates declared projects and captured packages/tools rather than every internal OS/SDK/browser component.
- This cleanup removes no maintained automatic test, installs no replacement tool/library/documentation framework, and does not certify current restore, build, publication, consumer serving or visible UI behavior.

See [architecture](1_architecture.md), [startup](local-startup.md), [release procedure](nuget-release-integration.md) and [project constraints](project-constraints.md).
