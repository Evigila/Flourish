# Current Flourish architecture

## Project constraints and SAF

Read [project constraints](project-constraints.md) alongside the root router and typed SAF configuration. SAF.json is the release manifest; SAF.lock.json separates bootstrap, installation and structural-audit evidence. Origin/local use the shared saf-core helper; their runtime backups/logs/leases are separate from discoverable skills. The project operation launchers are debug.bat and scripts/release.bat.

This is a responsibility/dependency map, not an exhaustive file tree or a fresh runtime acceptance claim. The complete pre-cleanup architecture and prior candidate narratives are archived. Source defaults remain VersionPrefix 1.4.7 and EssentialCultureVersion 1.4.0; the recorded 1.4.8-preview.14 consumer candidate is an explicit local override, not a public release.

## Source and package layers

| Boundary | Responsibility and dependency direction |
| --- | --- |
| Flourish.Core | Platform-neutral settings, project catalogs, commands, navigation, layout, text, profile and optional application services; no UI SDK, Windows target or source-project dependency |
| Blazor.Abstract | Public shell/component/text/configuration contracts; references Core |
| Blazor.Framework | Razor controls, geometry, browser behavior, progressive/native interaction, accessibility and static assets; references Abstract |
| Blazor.Design | Optional semantic palette, typography, theme/appearance and paint; references Framework/Abstract |
| Extensions.Culture.Blazor | Optional Essential.Culture.Blazor adapter, request negotiation, scoped text/session, browser preference persistence and LanguagePicker; references Framework |
| Flourish.Blazor aggregate | Dependency-only convenience package composing Abstract, Framework, Design and the Culture bridge; no aggregate assembly |
| WPF.Abstract/Framework/Design | Independent native contracts, controls/templates/geometry/lifecycle and optional paint; Framework uses Abstract, Design uses both; no Core/Blazor assembly dependency |
| Flourish.WPF aggregate | Dependency-only convenience package composing native layers; does not include its Culture adapter |
| Extensions.Culture.WPF | Optional Essential.Culture.Wpf adapter to native text contracts; caller-owned provider lifetime; references WPF.Framework |
| WinUI3 | Unpackable placeholder library and an empty-window Gallery; presence in the solution does not establish a functioning UI library |
| Gallery.Blazor | Interactive Server application, catalog/reflected API/default metadata, maintained executable examples and scoped fictional RecordStore |
| Gallery.WPF | Native application composing current production controls, optional Design and the Culture bridge |
| Tests and native diagnostic host | Maintained unit/render/event/DOM/STA/consumer checks and an optional Framework-only Web host; distinct from disposable outputs |

The root solution and four focused solutions group 22 projects and 33 declared local references. They are development graphs, not microservice orchestration. Exact project aliases/references, declarations and available package captures are in [dependencies](1_dependency.md#projects-and-source-reference-graph).

## Runtime, state and resources

The Blazor Gallery is one ASP.NET Core/Kestrel process with browser HTTP/static assets and Interactive Server circuits. The Framework-only diagnostic host is separate and intentionally omits Design/Culture registration. WPF and WinUI Gallery entries are native desktop processes. No mandatory application database, cache server, message broker, authentication provider or remote business backend is introduced by these libraries.

Gallery business examples are fictional in-memory data; examples do not authorize real account/billing/security behavior. Optional Core settings/catalog services may persist to host-selected local configuration/project files, and credential persistence is a caller-supplied optional port. Library text catalogs are embedded; Gallery's seven functional Culture modules are build/publish resources. Immutable catalogs are shared while browser/circuit selection remains scoped. Optional Design appearance state does not itself implement refresh persistence.

WPF Framework links the shared Blazor caption JSON and Gallery links the maintained SVG as source resources, not browser/Blazor assembly dependencies. Checked-in Material Symbols font/codepoint/vector resources retain provenance and licenses and are required assets, including the common browser subset and full fallback. Configured Segoe UI/system text fonts are OS/browser choices, not an added Google font CDN.

Framework and Design CSS bundles are compiled from maintained source by build/CssBundle.targets and BundleCss.cs. Generated bundles/intermediates, output packages and one-off consumer projects are recreated by verification. DocFX tooling, templates, generated API sites and Pages deployment are removed; docs/ retains only SAF's empty structural marker. A future human-documentation framework is undecided. Gallery remains the runnable component/API reference.

## Composition and contracts

Framework owns geometry/state/interaction; optional Design owns supported semantic paint. Hosts own business values, permission/session/transport/persistence and approved brand configuration. Read [component contracts](component-api-organization.md), [Culture integration](culture-web-integration.md) and [native WPF](wpf-native-integration.md) for their technical boundaries; none defines a competing visual standard.

ShellHeader defaults to the ordinary title/menu/action arrangement. Presentation hosts may opt into content-width following; added action controls use the public header slot and library spacing. NavigationTree shares the shell/surface renderer, stable identity and optional nested icons. Navigation labels are product data rather than inferred permissions.

Blazor PageHeading retains the full heading in document flow. An independent compact overlay enters after it leaves view, with original back/action controls retained rather than duplicated. Scroll range does not collapse. PageHeading loading observes existing active sources without intercepting arbitrary host tasks. Native WPF retains its own scroll/template contract and manual display-scaling acceptance.

Notice is durable inline explanation; FloatingNotice supplies transient zero-flow feedback with repeated-message generations, lifetime progress/pause/reverse exit and shared SaveChangesBar docking. The default timed feedback lifetime is 3,500ms, explicit zero is persistent and host business/hidden state is not overwritten by dismissal. UniformGrid elevation belongs to its occupied silhouette; internal separators do not create outside bottom/right borders or per-cell shadows.

Standalone selection geometry follows standard input height. Embedded choice indicators remain compact trailing marks with selected-row contrast. Avatar owns decode/crop/zoom/reset/encode/lifecycle, while the host owns authorized persistence; stale callbacks cannot save another identity's image. Standard ProgressBar paint is single-color while indeterminate movement and reduced-motion behavior remain library-owned.

Generic Dialog handles controlled/awaited and browser-controlled pre-rendered views, modality, cancellation/focus and lifecycle. Host reconnect composites supply fixed protocol identifiers/semantic transitions while reusing Dialog/ProgressBar/Button. TutorialBoard is a host-driven milestone overview with a header progress entry, not authorization or modal confirmation. Tables, multiple selection, references, static lists and spreadsheet editing retain their distinct data/lifecycle contracts.

## Operations and verification boundaries

| Entry or configuration | Responsibility |
| --- | --- |
| global.json, shared Build.props/targets and project files | SDK, source/Essential versions, identities, deterministic output and architecture-neutral package guards |
| debug.bat -> scripts/Start-Project.ps1 | Runnable-project allowlist, Gallery/diagnostic selection, restore/build/run with preserved configuration and working directory |
| scripts/release.bat -> Publish-Helper.ps1 | Prepare verification and separately confirmed stable tag publication |
| ReleaseSettings, Test-Release, Verify-PackageSet and build checks | Six dependency-ordered Core/Blazor packages, required assets, source/Gallery/JS/script/CSS/catalog/changelog and isolated consumer checks |
| .github/workflows/build.yml | Build/test gate and tag-only NuGet Trusted Publishing; source push is not package publication |
| scripts/Convert-MaterialIcons.ps1 and Subset-MaterialIcons.py | Explicit development regeneration of existing licensed assets; not normal consumer build dependencies |
| origin.bat/local.bat and SAF skills | Framework sync, scoped backup/archive maintenance and independent structural audit/memory workflows |

Use [startup](local-startup.md) and [release procedure](nuget-release-integration.md). Default source remains 1.4.7; old tags/packages and archived release receipts are immutable. WPF/WinUI are outside the six-package publishing manifest. Rebuild/restore can recreate deleted outputs but do not establish manual appearance, desktop prerequisites, real consumers or publication success.

Remaining acceptance includes actual browser/native geometry, keyboard/focus/OS dialogs, high contrast/reduced motion, desktop architecture/XAML tooling, WASM/Interactive Auto and deployment-specific consumers. Unknown resource/license/runner metadata stays unknown. This consolidation does not run a full application audit, publish a package, certify an environment or update SAF audit completion.
