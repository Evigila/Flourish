# Essential 1.4.0 bridge and modular resources

## Request and evidence

The user requested adapting Flourish.Extensions.Culture.* to the newly published Essential.Culture version and its functional Culture.json modules. Public NuGet indexes and the Core nuspec confirm 1.4.0. The sibling Essential source and its v1.4.0 delivery record were read as technical evidence; Flourish restores the public packages rather than substituting those source projects. The root AGENTS.md and its ensure file were read; the monthly documentation check is not due.

Essential 1.4.0 adds eager LocalizationCatalog.FromFiles, immutable CatalogLoadOptions, generated CultureResources deployment metadata and startup selection-policy checks. Each module keeps key -> culture -> string; modules divide features, not languages. A module must supply its fallback, optional language sets may differ, and keys must be globally unique inside one logical catalog. Existing single-file catalogs remain valid.

## Implementation

Directory.Build.props and release verification now expect Essential 1.4.0 for both bridges and transitive Core/Generator. Flourish VersionPrefix remains 1.1.3. No package ID, unrelated dependency, public version, tag or publication changed.

Blazor CultureBuilder accepts a loaded LocalizationCatalog and provides AddCatalogFiles(id, paths, fallbackCulture = "en-US", options = null). Relative paths resolve at AppContext.BaseDirectory; absolute startup paths remain caller-owned. The bridge delegates parsing, merging and policy to Essential and rejects incompatible supported UI choices before completing configuration. Existing embedded/stream loading, stream ownership, configuration freezing, request negotiation and per-browser save-before-apply persistence remain supported.

WPF UseEssentialCulture accepts a loaded catalog and creates an isolated context; EssentialTextProvider exposes AvailableCultures and SetCulture through Essential's policy and event mechanisms. Native framework captions no longer use a separate JSON dictionary/parser: one Essential catalog/context handles stable tokens, formatting and parent/fallback selection. Subscription disposal remains caller-owned. Static facade users must configure modules before constructing providers or views. The established explicit static format override/follows-UI behavior remains because the upstream facade exposes no format getter; isolated contexts retain the complete UI/format pair.

Gallery's former single embedded catalog is replaced by seven multilingual modules: Shell, Pages, Components, Parameters, Samples, Access and ChangeLog. All 1,553 original keys and translations remain, except the requested description update for the new architecture; two new explanatory/preview keys bring the total to 1,555. Explicit CultureModule items generate keys and CultureResources. Runtime registration consumes exactly that generated manifest while preserving the three service entries. The Localization page demonstrates project items and AddCatalogFiles. Catalog/ChangeLog/HTTP validation now reads the union and rejects duplicates.

A publish check found that the Razor extension's embedded six-key catalog was also copied as ordinary Localization/Culture.json content. Explicit None/Content removal now leaves it embedded only, preventing host module-path collisions. Fresh Gallery output has exactly seven source-identical module files. A packaged consumer regression rejects extra copied library catalogs.

## Verification

- Blazor solution and WPF solution Release builds, plus Gallery publish: zero warnings/errors. The first WPF test compilation lacked System.IO imports in two new files; adding them resolved all four compiler errors.
- Blazor bridge: 26 grouped executable checks, including module union, different optional languages, eager loading, deployment paths, duplicates, malformed/missing/fallback errors, retained-language policy, registration freezing and stream ownership.
- Framework: 436/436. Gallery: 8,291 checks, including 474 post-event and 235 ChangeLog checks. Browser preference Node suite: 4/4.
- WPF bridge: 11/11; native controls: 139/139. Module tests use actual generated keys/manifest and deployment files.
- Culture validation: 24,202 checks, including 1,555 globally unique Gallery keys across seven modules. ChangeLog: 97 checks across four versions.
- Published Production Gallery: seven module hashes match source and no copied library catalog is present. Eight HTTP preference/asset checks and 583 three-language HTTP checks passed on the corrected output. Navigation HTTP checks also passed: 240. Temporary hidden verification hosts were stopped.
- Six Blazor candidate package dependency/asset checks passed. Fresh isolated NuGet consumers passed 143 checks, including generated module keys/manifest, nested build/publish paths, no unused catalogs, scoped formatting and SSR. Final evidence: artifacts/package-consumers/62e1691ea2244da0aba3d4cb2cfdf946.
- Independent WPF package-only consumer built with zero warnings/errors, then passed 51 checks from build output and another 51 from publish output. Essential Core/Generator/Wpf 1.4.0 cache metadata points to NuGet.org. Candidate dependency checks confirm bridge -> Framework 1.1.3 + Essential.Wpf 1.4.0. Evidence: artifacts/culture-1.4-wpf-consumer/verification.json.
- git diff --check passes. Candidates/fixtures are ignored local artifacts and have not been published.

No Computer Use was performed. Automated checks establish runtime, generation, deployment and lifecycle behavior; visible browser/native acceptance remains manual.

## Manual acceptance

1. In Gallery, switch between English, Chinese and Portuguese on Home, components, examples and ChangeLog. Confirm content from all functional modules translates and no Key.* token appears.
2. Choose UI language and number/date format, refresh, and open a second browser profile. The first profile restores its pair and the second remains independent. Inspect the Localization page's module configuration example.
3. In a consumer, add one feature CultureModule and rebuild/publish. Confirm its generated key and declared deployment path load through AddCatalogFiles. Duplicate a key or omit a module/fallback; startup/build should give a clear failure rather than partial translations.
4. Use CatalogLoadOptions with an enabled/disabled language policy and matching host choices. A disabled language should not be selectable; an incompatible host choice should fail configuration without altering an active session.
5. In a native consumer, configure the facade before creating views or use isolated catalog providers. Check module text, independent formatting, two-window selection and disposal. Confirm library captions retain their fallback and caller-owned lifetime.
