# Culture Web integration

## Essential 1.4.0 and functional modules

The user authorized upgrading the optional Blazor and WPF bridges to public Essential.Culture 1.4.0 on 2026-10-09. Directory.Build.props pins that version and the release dependency checks use it. Flourish's own VersionPrefix remains 1.1.3; these source changes are not a new Flourish publication. Earlier 1.3.0 dependency statements below describe their dated implementation only.

CultureBuilder.AddCatalog(id, LocalizationCatalog) accepts Essential's immutable loaded catalog, including fallback and CatalogLoadOptions. AddCatalogFiles(id, paths, fallbackCulture = "en-US", options = null) eagerly delegates to Essential.FromFiles. Relative deployment paths resolve at AppContext.BaseDirectory; absolute caller-supplied paths are supported. Duplicate paths/keys, missing/malformed documents, missing fallback and language-policy conflicts fail startup. All catalogs must permit the configured UI choices. Existing bridge UI/format defaults and browser persistence remain in force; Essential's retained-language policy governs translation selection.

Gallery owns seven multilingual Localization/Culture.*.json modules: Shell, Pages, Components, Parameters, Samples, Access and ChangeLog. Each file groups a function's keys and retains its languages. Explicit CultureModule items generate the same Key/CultureResources inventory used for build/publish deployment and runtime registration. Program calls AddCatalogFiles("Gallery", CultureResources.Files, CultureResources.FallbackCulture) inside ConfigureCulture and retains its three service entries. Framework and extension captions remain separate embedded catalogs. No runtime directory scan, second merger or temporary resource extraction is added.

For a consumer project, enable EssentialCultureAutoInclude, set EssentialCultureNamespace and declare CultureModule items with stable relative DeploymentPath values. The generated CultureResources class is internal to that consumer. Keep global key names unique across its modules; optional language sets may differ by module and each module must supply the fallback. Alternatively, existing embedded/stream catalog registration remains valid. Gallery's Localization page demonstrates the module project configuration and registration.

The native adapter accepts a LocalizationCatalog or independent LocalizationContext through UseEssentialCulture, exposes AvailableCultures/SetCulture and owns its subscription until disposed. A static-facade consumer must call Localizer.Configure with its merged catalog before constructing providers or views. The static facade does not expose a format getter; the established adapter uses its explicit formatCulture override or follows the UI selection. Independent contexts retain the complete Essential UI/format state. Native library captions now share Essential's lookup and parent fallback rather than a second JSON dictionary parser.

## Framework registration supersession on 2026-10-09

The user requested that Gallery startup expose only AddFlourishFramework, optional AddFlourishDesign and its business RecordStore registration. The later redesign supersedes the earlier same-day persistence design: AddFlourishCulture, AddFlourishPreferences, Framework BrowserPreferences, the Gallery-owned LanguagePicker and direct Essential runtime calls are retired without aliases. Their original implementation and verification remain in the dated persistence report and append-only change records. The current optional Culture extension owns localization setup, request negotiation, browser persistence and the language/format selector.

Gallery calls AddFlourishFramework(builder.Configuration, ...), ConfigureCulture within that callback, AddFlourishDesign(builder.Configuration) and AddScoped<RecordStore>(). Framework registers the standard interactive Razor services when used by an ASP.NET host. The host still owns application routing, exception/status handling, antiforgery and endpoint mapping; a service-only consumer does not create a Web host by registering Framework.

This redesign is a source change after public 1.1.3. Existing released packages and tags retain their original contracts. No package version, package ID, external dependency version or public publication is changed by this source task. Current verification results belong to its new dated change record; historical totals below do not establish acceptance of the redesign.

## Native WPF supersession on 2026-10-07

The former WPF single-project host/service implementation and its Culture hosting adapter have been deleted under the user's explicit reconstruction request. Current native projects are Flourish.WPF.Abstract, Framework, Design and the dependency-only Flourish.WPF aggregate, targeting net10.0-windows. The rebuilt optional EssentialTextProvider is owned/disposed by its native consumer and configured on FrameworkBuilder; Gallery consumes this bridge. Earlier WPF descriptions below are dated migration evidence, not current APIs. See [native WPF integration](wpf-native-integration.md).

The existing public six-package Core/Blazor 1.1.2 release and scripts/ReleaseSettings.psd1 remain unchanged. Native WPF build/pack verification creates local candidates only; this task does not publish, version-bump or add Windows targets to the already released Blazor packages.

## Implemented local integration

The 2026-10-05 local integration supersedes this guide's earlier proposed APIs and read-only readiness assessment. Essential.Culture provides immutable catalogs, independent contexts and a scoped Blazor adapter; Gallery uses the separate optional Flourish.Extensions.Culture.Blazor bridge. The later approved package consolidation retires Shared, retaining Abstract/Framework/Design and adding a dependency-only convenience package. Desktop localization APIs remain separate.

This integration targets static SSR and Interactive Server on .NET 10. Pure WASM and Interactive Auto have not been validated. Current Framework, Abstract and Culture.Blazor projects use Microsoft.AspNetCore.App FrameworkReference, so this source build does not establish a client-compatible dependency graph.

## Ownership and dependency direction

- Gallery owns its translated application names, navigation labels, pages, executable examples and business data. It supplies its application catalog identity and generated module manifest and consumes the extension's current APIs.
- Framework owns behavior, provider-neutral text references and its embedded standard-caption catalog. It has no reference to Essential.Culture or the extension bridge.
- Abstract exposes TextReference, ITextProvider and the provider-neutral IFrameworkBuilder.ConfigureServices(Action<IServiceCollection, IConfiguration>) hook. Design remains optional and owns visual presentation only.
- src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor owns ConfigureCulture, CultureBuilder, CultureSession, LocalizedComponentBase, LanguagePicker, request negotiation and the browser module. Its package is Arkheide.Flourish.Extensions.Culture.Blazor; the Razor project references Framework and the existing Essential.Culture.Blazor package. It does not reference Design or WPF. Framework does not reference this optional extension, so the dependency graph remains acyclic.
- Essential.Culture.Blazor remains the internal request/circuit-local dictionary and formatting core; immutable parsed catalogs may be shared. Gallery does not register or resolve its runtime service directly. Essential.Culture has no Flourish dependency.
- Framework supplies a literal fallback provider when no bridge is installed. Framework-only Native retains its ordinary controls and English default captions.

ApplicationData.cs now belongs to Abstract after the approved Shared consolidation. Framework retains reference-identity, immutable startup TextReference metadata separate from navigation/menu records, preserving their public DTO signatures and catalog distinctions. The move changes assembly identity and requires rebuilding existing consumers; no Shared assembly is shipped.

## Why the WPF bridge stays separate

The desktop bridge is src/Flourish.Extensions/Flourish.Extensions.Culture.WPF, with package ID Arkheide.Flourish.Extensions.Culture.WPF and namespace ArkheideSystem.Flourish.Extensions.Culture.WPF. It targets net10.0-windows and connects caller-owned native windows/applications to an Essential context or its optional static facade. The reconstructed adapter has no Dispatcher/Singleton applicator/hosted-service hierarchy. Process-wide facade selection cannot represent separate Web users; independent native contexts and scoped Web sessions keep their respective ownership.

The WPF implementation and UseEssentialCulture entry retain their established identities. External Culture now comes from the shared root 1.4.0 setting. Gallery.Flourish.WPF consumes the optional bridge. Neither WPF package is included in the Core/Blazor release manifest.

The Blazor CultureSession is scoped. Its ITextProvider implementation maps Get to the same Essential TryParseFrom core, forwards Changed subscriptions and exposes the current UI/format pair. An internal IStartupFilter inserts standard request-localization middleware. No desktop Dispatcher, static Localizer setter, process-wide personal selection or permanent event subscription is used.

## Local source references and packaging

Gallery and both extensions reference their same-repository Flourish projects. All external Culture and generator references use NuGet packages at 1.4.0. This upgrade is restored and verified from NuGet.org; source-project substitution is not used. Release verification also accepts an explicitly supplied Essential package directory for a local candidate.

EssentialCultureRoot, UseLocalEssentialCulture, UseLocalFlourish and UseLocalCultureIntegration are retired. External Essential dependencies and independent consumer verification retain PackageReference boundaries. Gallery now references the same-repository Culture extension project so development cannot silently use an older cached bridge package; isolated consumer fixtures still exercise the packaged extension. Local and public NuGet feeds provide the same package identity/version boundary. See [NuGet release and integration](nuget-release-integration.md).

The maintained integration projects are WPF and Blazor. Only Arkheide.Flourish.Extensions.Culture.Blazor belongs to the six-package Core/Blazor release set established at 1.1.0; later stable releases retain those IDs. Both integrations use Essential packages at the root 1.3.0 version. Publish Essential first. Local verification does not publish packages. See the [bridge guide](culture-extension-bridge.md) for migration history and the [release contract](nuget-release-integration.md) for the current source dependency graph.

## Register catalogs through Framework

ConfigureCulture automatically loads two library-owned embedded resources:

- Framework assembly: Flourish.Blazor.Texts.json, catalog identity Flourish. Standard Framework references require this identity when a translating provider is installed.
- Culture extension assembly: Culture.Texts.json, catalog identity Culture. It supplies selector captions, persistence feedback and usage metadata.

The host registers its independent application catalog through a marker type and resource name. It does not open either library stream or create an Essential catalog in Program.

```csharp
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Extensions.Culture.Blazor;

builder.Services.AddFlourishFramework(builder.Configuration, framework => framework
    .ConfigureCulture(culture => culture
        .AddCatalog<Program>("Gallery", "Gallery.Texts.json")
        .SetDefaultCatalog("Gallery"))
    .ConfigureProject(project => project.SetProjectName(
        new TextReference("Gallery", "Project_Name", "Gallery")))
    .ConfigureTopBar(top => top.AddMenu(
        new TextReference("Gallery", "Menu_Pages", "页面"),
        menu => menu.AddMenuItem(new TextReference("Gallery", "Nav_Framework", "框架"), "open-framework")))
    .ConfigureNavigation(nav => nav.AddNav(
        new TextReference("Gallery", "Nav_Home", "主页"), "home", "/", exact: true)));
builder.Services.AddFlourishDesign(builder.Configuration);
builder.Services.AddScoped<RecordStore>();
```

ConfigureCulture is an IFrameworkBuilder extension in ArkheideSystem.Flourish.Extensions.Culture.Blazor. It uses Framework's generic registration hook, registers the translating provider and encapsulates request/circuit initialization and persistence. Framework and Culture configuration each complete once per host; retained builders cannot be modified afterward. No independent AddCultureBlazor, AddFlourishCulture, AddFlourishPreferences or UseRequestLocalization call belongs in Gallery Program.

CultureBuilder exposes AddCatalog<TAssemblyMarker>(catalogId, resourceName), AddCatalog(catalogId, Stream), SetDefaultCatalog, SetDefaultCulture(uiCulture, formatCulture?), AddSupportedCultures and SetRetentionDays. The marker overload owns/disposes its embedded stream; the stream overload reads from the current position and leaves ownership with the caller. Missing resources, duplicate catalog identities, unknown cultures, unsupported defaults and invalid retention fail during registration. Without explicit supported cultures, the default catalog supplies its cultures. Both UI and formatting selections must be supported. Configuration defaults and explicit callback overrides complete before the host is built.

Existing ordinary string builder calls remain literal, including strings that look like Key.Home. Only explicit TextReference overloads translate at render time. References are supported for project name, top-bar search/menu/items and primary/secondary/third/fixed navigation routes or commands. Route, command, icon, column and record identifiers are not translated. Do not call Localizer.Parse or resolve a scoped service at startup to store translated labels in singleton configuration.

## Validation isolation and browser assets

The 2026-10-06 compressed-resource incident supersedes the earlier assumption that changing OutDir alone safely isolates a Web/Razor verification build. OutDir redirects final binaries but can leave normal obj bundle/manifests shared. Use --artifacts-path with a task-specific directory so output and intermediate assets are separated by project. Do not regenerate a running normal Gallery's resources through a second validation build.

Test-GalleryCulture.ps1 now also checks real CSS, WOFF2 and Blazor JS delivery with identity, gzip and browser gzip/br negotiation, including MIME, decoded content size, CSS rules and hash equivalence. HTML localization checks alone do not establish that the browser can load its required resources. Run HTTP acceptance with the project's normal launch profile, or a properly published host; do not change application environment/resource configuration just to make a temporary test pass.

Example: dotnet run --project tests/Tests.Gallery.Flourish.Blazor/Tests.Gallery.Flourish.Blazor.csproj -c Release --artifacts-path artifacts/gallery-heading-validation. Normal Gallery source-bin/obj hashes should remain unchanged.

## Gallery collapsible page headings

The 2026-10-06 heading repair distinguishes catalog completeness from caller adoption. Navigation labels already resolved keys, but Framework, Examples, Appearance and component-category title dictionaries still contained literal Chinese values. PageHeading.Title is ordinary display text; installing the bridge does not automatically resolve arbitrary strings or infer a key from a navigation label.

Pages now inherit LocalizedComponentBase and parse their title keys while rendering. CatalogSections.TitleKeys and the topic dictionaries store generated keys rather than already-resolved or literal titles. Appearance calls the localization base during initialization and disposal while preserving its appearance-state subscription. Form and reconnection pages likewise retain their existing cleanup and release language subscriptions. API component names, product brands and actual record names remain semantic identities/data.

New record-create/edit/missing, not-found/error, reconnect, product-scenario, shell and category-back labels have complete en-US, zh-CN and pt-BR entries. That first repair covered rendered primary headings and corresponding browser page titles. The subsequent Gallery prose repair below completes explicit caller adoption for internal headings, descriptions and examples.

The 2026-10-06 Tests.Gallery.Flourish.Blazor harness exercised actual Gallery pages with the real scoped Essential service and HtmlRenderer, switching en-US to zh-CN to pt-BR and back without remounting, then checking subscription release. The current harness uses the extension-facing scope while preserving that underlying core. Test-GalleryCulture.ps1 checks real PageHeading H1 output in all three languages, so translated navigation alone cannot satisfy the regression; the historical 31-route count belongs to that earlier milestone.

## Single-source catalogs and generated access keys

The 2026-10-06 three-language maintenance established one Culture.json source file per owning Blazor project, with every access key containing en-US, zh-CN and pt-BR translations. That milestone counted 1,483 Gallery application/documentation keys and 281 Framework caption/usage keys. Current source also has the extension-owned Culture catalog; the catalog check reports current totals. Application and library catalogs remain separate, rather than duplicate per-language files or one mandatory global catalog.

```json
{
  "Nav_Home": {
    "en-US": "Home",
    "zh-CN": "主页",
    "pt-BR": "Início"
  }
}
```

Gallery passes Localization/Culture.json once as AdditionalFiles to Essential.Culture.Generator and embeds that same file as Gallery.Texts.json. Generator settings enable generation, select XamlFramework=none and disable automatic file creation/copying. The generator recognizes the Culture.json basename and expects exactly one such input per generated project. It produces Texts.Key accessors and CultureKey entries; it does not duplicate translated values into those accessors. For example, Texts.Key.Nav_Home is the stable token Key.Nav_Home, resolved through the scoped service while rendering.

Framework's physical source is now Localization/Culture.json, renamed from Texts.json without changing its 121 existing translations. Its embedded logical name remains Flourish.Blazor.Texts.json. Logical resource names are identifiers for compiled streams, not additional files to maintain. Framework deliberately has no Essential generator/runtime dependency: its provider-neutral TextReference tokens are declared by the control implementation and the optional bridge resolves them against the registered Flourish catalog. Consumers use the Essential generator for their own application keys.

The JSON schema and access-key lookup are shared with Essential.Culture's desktop targets. Platform lifecycle differs: WPF/Avalonia use the desktop process-wide Localizer and UI binding/dispatcher integration; WinUI uses its window-host/DispatcherQueue integration. Blazor uses immutable catalogs with request/circuit-local UI and format selections, renderer dispatch and subscription disposal. A server must not use a desktop global language setter for different users.

The extension resolves explicit keys and TextReference values. It does not discover or translate arbitrary Razor string literals or rewrite user-entered/business values. Gallery technical guides, API descriptions and samples use explicit key extraction and render-time lookup. API syntax and business/input data retain their identities. The new extension-owned LanguagePicker has its own usage entry, Gallery/API navigation and executable sample alongside the existing inventory.

## Razor usage and language changes

Gallery has one Culture.json AdditionalFile, an independent generated Texts.Key namespace, XamlFramework=none, and disabled automatic copying/creation. It embeds that file as Gallery.Texts.json. Framework's separate standard dictionary is not a second generator input for Gallery, so tokens remain separate by both catalog identity and generated namespace.

```razor
@using ArkheideSystem.Flourish.Extensions.Culture.Blazor
@using TextKey = ArkheideSystem.Gallery.Flourish.Blazor.Texts.Key
@inherits LocalizedComponentBase

<PageTitle>@Localization.Parse(TextKey.Nav_Home)</PageTitle>
<PageHeading Title="@Localization.Parse(TextKey.Nav_Home)" />
<p>@Localization.Parse(TextKey.Home_SetupDescription)</p>
<LanguagePicker />
<LanguagePicker FormatCulture="true" />

@code {
    private Task<bool> UseEnglish() => Localization.SelectAsync("en-US");
    private Task<bool> UseChineseWithBrazilianFormats()
        => Localization.SelectAsync("zh-CN", "pt-BR");
}
```

The extension's LocalizedComponentBase injects CultureSession, refreshes through the renderer when Changed fires and releases the exact subscription on disposal. Derived initialization overrides must call base. Gallery no longer imports Essential runtime components or its localization base. Framework components retain their provider-neutral TextComponentBase. ApplicationShell updates branding/menu/navigation/default ARIA strings and its live lang attribute, without resetting selected routes or manual tree expansion; the Gallery HTML root reads ITextProvider.Culture.

The extension's LanguagePicker composes the standard SelectBox, Dialog and Notice. MaxWidth optionally bounds the selector; FormatCulture defaults to false and selects UI culture, while true preserves the current UI culture and changes formatting only. Choices come from CultureSession.AvailableCultures and use each culture's NativeName. Saving temporarily disables the selector; a failed save displays the extension's localized warning and permits retry. ComponentUsageCatalog.For exposes its reviewed Scenario entry. Gallery demonstrates both modes without its own selector or persistence implementation. Deliberate code snippets, data and multilingual font demonstrations remain literal.

## Standard defaults and explicit overrides

The embedded Framework catalog currently covers shell navigation accessibility, heading back accessibility, Dialog close, DisplayBoard copy/status/failure and Components.DataTable captions/loading/empty/row fallback. Missing keys use TextReference.FallbackText or Token; unregistered catalogs fail explicitly. Arguments use the selected FormatCulture, and malformed composite-format strings retain normal errors. Output uses ordinary encoded Razor text.

Existing explicitly supplied string parameters win even if they equal an old default. TextComponentBase detects supplied parameters using ParameterView instead of comparing values. Explicit DataTable.Text, Culture, Label, LoadingMessage and EmptyMessage remain host-owned. When omitted, TableText defaults use the provider and formatting uses its FormatCulture. Caption lookup is cached per component/selection, and row values rebuild on language/format notifications while table paging/search/view/sort state remains local.

DataTable already uses its Culture parameter for local comparison and display. Hosts that require fixed domain comparison should continue supplying Culture and, if needed, independent column Format delegates. This task does not change primitive sorting policy, browser-native numeric/date transport, validation, currency, time zones or business rules.

## HTTP initialization and persistence

The user confirmed that appsettings.Flourish.json stores common framework defaults and each browser persists its own personal choices. AddFlourishFramework(builder.Configuration, ...) loads this optional file below existing host/business, environment and command-line providers. With ConfigurationManager, its JSON source is inserted at index zero; other IConfiguration inputs receive a snapshot of file defaults overlaid by host values. The file is not reloaded during a running configuration. AddFlourishDesign(builder.Configuration) consumes the appearance defaults through its own optional layer. appsettings.json remains an empty object in Gallery for business/host settings; browser choices never modify either server file. Core's singleton desktop file store is not connected to Web circuits.

ConfigureCulture configures RequestLocalizationOptions from the completed CultureBuilder. Its internal startup filter runs standard Cookie then Accept-Language providers before rendering, with configured defaults as fallback. Scoped initialization captures IRequestCultureFeature when present and otherwise uses the completed defaults. An existing supported personal cookie therefore takes precedence over browser headers and defaults for the first SSR frame.

CultureSession.SelectAsync(uiCulture, formatCulture?) validates both supported selections, lazily imports ./_content/Arkheide.Flourish.Extensions.Culture.Blazor/browser-preferences.js from an interactive event and saves the standard .AspNetCore.Culture cookie. Omitting formatCulture follows UI culture. SelectFormatAsync preserves the latest UI culture after any queued transaction. The default retention is 365 days, configurable from 1 to 3650; browser storage policy may shorten retention. The cookie is scoped to the application base path, uses SameSite=Lax and Secure on HTTPS, and contains only the two culture identifiers. No browser APIs run during prerendering. Module disposal tolerates disconnection/cancellation.

The complete save-and-apply transaction serializes within a circuit. CultureSession returns false for browser storage, cancellation or connection failures and keeps its old pair; a later selection can retry. Successful persistence applies the pair through the one internal Essential session and returns true. Invalid arguments remain visible errors. Consumers no longer coordinate a separate writer with SetCulture. LanguagePicker handles the boolean outcome with ordinary Dialog/Notice feedback. No page reload is required for successful selection, and the next reload restores both identifiers before SSR. No permanent subscription, HTTP mutation endpoint or process-wide default-thread culture setter is added.

```csharp
// From an interactive event; Localization is the scoped CultureSession:
var saved = await Localization.SelectAsync("zh-CN", "pt-BR");
// A custom consumer can display its ordinary feedback when saved is false.
```

This milestone implements UI/format persistence only. Theme and user-chosen palette are the next natural personal preferences; both currently reset with their scoped AppearanceService. DataTable sorting and future column/series visibility, order, width and page-size preferences require stable opt-in keys and bounded per-control storage. Navigation expansion is optional and needs stable navigation identities. Startup fonts/layout and temporary Gallery demonstrations are not automatically user preferences. Business drafts, search state, selected records and credentials remain outside the generic preference file/store. See [the persistence report](bugfix-reports/2026-10-09_culture-preference-persistence.md).

## Three-language verification on 2026-10-06

- Gallery Release build, using an isolated output directory: zero warnings/errors.
- build/Test-CultureCatalogs.ps1: 3,036 checks passed for duplicate/missing/empty keys, exactly three translations, format placeholders and line-break consistency.
- build/Test-GalleryCulture.ps1: 43 actual loopback HTTP checks passed for all three request languages, HTML language, generated Gallery text, language choices, access-method labels/actions, number/date formatting, Framework captions, separate requests and cookie precedence.
- build/Test-GalleryNavigation.ps1: 191 actual endpoint/navigation checks passed.
- Tests.Flourish.Blazor: 356/356 passed; Tests.Flourish.Extensions.Culture.Blazor: 12/12 passed.

Evidence is in artifacts/culture-three-build.log, culture-three-catalogs.log, culture-three-http.log, culture-three-navigation.log, culture-three-components.log and culture-three-bridge.log. These checks establish source/catalog consistency, real SSR output and component/bridge contracts. No Computer Use or browser automation was used in this follow-up; interactive visual acceptance remains user-run.

Manual acceptance for the current source: start Gallery through the existing launcher; switch the extension selector among English, Chinese and Brazilian Portuguese and reload; select separate formatting culture and reload; retain method/form/table state during a language change; use another browser profile to verify isolation; block browser storage and check unchanged values, warning and retry; override defaults through host/environment configuration; verify Framework-only consumers omit Culture registration and assets. Historical 2026-10-06 counts above predate this redesign.

## Verification and remaining work

Verified on 2026-10-05:

- Full Flourish.slnx Release build with TreatWarningsAsErrors=true: zero warnings/errors.
- 118/118 Flourish Blazor console checks passed, including seven new neutral-provider/lifecycle/reference tests.
- 12/12 real Culture bridge scope, catalog, format, fallback and registration checks passed.
- Existing WPF extension tests: 5/5 passed; its implementation remained intact.
- Headless installed Edge tested the published Gallery with separate English/Chinese browser contexts: SSR negotiation, independent live switching, translated home/title/chrome, Brazilian formatting, table captions, dialog/copy defaults, retained third-navigation selection/collapse and translated command routing. No page exceptions or failed framework assets were observed.
- Framework-only Native SSR returned 200, retained fallback captions and loaded neither Design nor Culture markup. This is narrower evidence than a complete Native browser acceptance run.
- Framework and bridge NuGet packages are prepared locally. Framework depends on Abstract; the bridge depends on Abstract/Culture.Blazor. Dictionaries are embedded without loose Culture.json/Texts.json. Earlier Abstract/Shared dependency evidence is superseded by the approved Shared consolidation.

Remaining integration work: validate any client/WASM graph and publish/version packages in dependency order. UI/format browser persistence is completed in source by the 2026-10-09 change; theme/control preference persistence remains separate follow-up scope. Gallery guide/sample UI translation is complete for the current inventory. Use the actual component tests and user-run acceptance for the affected runtime boundary, and the [directory map](currentproject-architecture.md) to locate source. Retired UI guides are not runtime authority.

## Gallery prose, metadata and validation completion

The follow-up on 2026-10-06 fixes the underlying caller-adoption gap for internal H2, body prose, control examples and documentation. Section still renders ordinary caller-supplied Title/ChildContent; it does not infer translation keys. Gallery pages and samples resolve their keys during rendering and retain paragraph breaks. Dynamic states retain keys and formatting arguments, rather than a translated result captured at the time of an event. Options, table columns, chart labels and API descriptions are rebuilt for the current scope without resetting inputs, selection, sorting or edit history.

Singleton catalog metadata stores PurposeKey, VariantsKey and parameter DescriptionKey. SampleFor.DescriptionKey identifies scenario prose shown outside DisplayBoard. Framework ComponentUsageInfo carries provider-neutral TextReference values; its 160 usage entries join the existing 121 captions in the single Flourish Culture.json. Missing external dependencies are not introduced. Removed Chinese TableText overrides let the library supply current default captions.

Gallery annotations store stable validation keys. Field and ValidationMessages receive MessageFormatter explicitly to translate existing errors at display time. Rules and raw EditContext messages remain unchanged; both scoped language and validation subscriptions are released on disposal. Consumers omitting the formatter continue to render their original messages.

Verification uses --artifacts-path artifacts/gallery-body-validation to isolate both bin and obj. Gallery builds with warnings as errors; 27 pages and all 80 component details are rendered through the real Essential/Flourish DI scopes in en-US, zh-CN, pt-BR and back to en-US. Stored annotation errors refresh without another Validate call. HTTP checks assert semantic H2/paragraph content and compare identity/gzip/browser-negotiated CSS/font/startup payloads. Source guards check missing access keys in callers as well as complete three-language JSON, placeholders and paragraph structure. See [the dated report](bugfix-reports/2026-10-06_gallery-prose-localization.md) for final counts and limitations.

Manual acceptance: switch languages on Framework, Appearance, examples and several control details; review Portuguese wrapping and paragraph spacing; create errors before switching; keep a selected option, search query, sorted table and edited grid while switching; verify font/icons and original start.bat startup. No Computer Use test is required or performed.
ReleaseSettings.ConsoleTests now includes Tests.Gallery.Flourish.Blazor, and CheckScripts includes Test-CultureCatalogs.ps1. Test-Release executes these existing console/PowerShell gates without changing launch or asset configuration.
