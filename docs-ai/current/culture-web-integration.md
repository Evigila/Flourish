# Culture Web integration

## Implemented local integration

The 2026-10-05 local integration supersedes this guide's earlier proposed APIs and read-only readiness assessment. Essential.Culture provides immutable catalogs, independent contexts and a scoped Blazor adapter; Gallery uses the separate optional Flourish.Extensions.Culture.Blazor bridge. The later approved package consolidation retires Shared, retaining Abstract/Framework/Design and adding a dependency-only convenience package. Desktop localization APIs remain separate.

This integration targets static SSR and Interactive Server on .NET 10. Pure WASM and Interactive Auto have not been validated. Current Framework, Abstract and Culture.Blazor projects use Microsoft.AspNetCore.App FrameworkReference, so this source build does not establish a client-compatible dependency graph.

## Ownership and dependency direction

- Gallery owns its translated application names, navigation labels, pages, language picker, request negotiation and future preference persistence.
- Framework owns behavior, provider-neutral text references and its embedded standard-caption catalog. It has no reference to Essential.Culture or the extension bridge.
- Abstract exposes TextReference and ITextProvider. Design remains optional and owns visual presentation only.
- src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor owns the optional bridge. Its package is Arkheide.Flourish.Extensions.Culture.Blazor; it references only Flourish.Blazor.Abstract and Essential.Culture.Blazor. It does not reference Framework, Design or WPF.
- Essential.Culture.Blazor registers the request/circuit-local ILocalizationService; immutable parsed catalogs may be shared. Essential.Culture owns translation algorithms and formatting, without a Flourish dependency.
- Framework supplies a literal fallback provider when no bridge is installed. Framework-only Native retains its ordinary controls and English default captions.

ApplicationData.cs now belongs to Abstract after the approved Shared consolidation. Framework retains reference-identity, immutable startup TextReference metadata separate from navigation/menu records, preserving their public DTO signatures and catalog distinctions. The move changes assembly identity and requires rebuilding existing consumers; no Shared assembly is shipped.

## Why the WPF bridge stays separate

The desktop bridge is now src/Flourish.Extensions/Flourish.Extensions.Culture.WPF, with package ID Arkheide.Flourish.Extensions.Culture.WPF and namespace ArkheideSystem.Flourish.Extensions.Culture.WPF. It targets net10.0-windows, uses WPF Dispatcher, Singleton applicator/hosted service and static Localizer.Current. That process-wide desktop synchronization cannot represent separate Web users.

The WPF implementation and UseEssentialCulture entry retain their established behavior after migration into Flourish. Its external Culture version now comes from the shared root 1.3.0 setting, replacing the former extension-local 1.1.0 default. Gallery.Flourish.WPF consumes Culture.Wpf directly rather than this bridge. Neither WPF package is included in the current Core/Blazor release.

The new Blazor bridge is Scoped. It maps Get to TryParseFrom, forwards Changed subscriptions directly and exposes the adapter's UI/format selection. It has no hosted service, Dispatcher, static Localizer access or permanent event subscription.

## Local source references and packaging

Gallery and both extensions reference their same-repository Flourish projects. All external Culture and generator references use NuGet packages at 1.3.0. Prepare Essential first; when sibling Essential/artifacts/packages exists, Flourish adds that directory as a local NuGet source. It does not select external source projects.

EssentialCultureRoot, UseLocalEssentialCulture, UseLocalFlourish and UseLocalCultureIntegration are retired. The current consumer boundary is PackageReference, including WPF. Local package feeds and public NuGet feeds provide the same package identity/version boundary. See [NuGet release and integration](nuget-release-integration.md).

The maintained bridge projects are WPF and Blazor, but only Arkheide.Flourish.Extensions.Culture.Blazor is in the current six-package 1.1.0 Core/Blazor release. Both use Essential packages at the root 1.3.0 version. Publish Essential first. Local verification does not publish packages. See the [bridge guide](culture-extension-bridge.md) and [release contract](nuget-release-integration.md).

## Register catalogs and the bridge

Program loads two independent embedded resources:

- Gallery assembly: Gallery.Texts.json, catalog identity Gallery.
- Framework assembly: Flourish.Blazor.Texts.json, catalog identity Flourish. Standard Framework references require this identity when a translating provider is installed.

```csharp
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;

using var appStream = typeof(Program).Assembly.GetManifestResourceStream("Gallery.Texts.json")
    ?? throw new InvalidOperationException("Missing Gallery catalog.");
using var frameworkStream = typeof(ApplicationShell).Assembly.GetManifestResourceStream("Flourish.Blazor.Texts.json")
    ?? throw new InvalidOperationException("Missing Framework catalog.");

builder.Services.AddCultureBlazor(culture => culture
    .AddCatalog("Gallery", LocalizationCatalog.Load(appStream))
    .AddCatalog("Flourish", LocalizationCatalog.Load(frameworkStream))
    .SetDefaultCatalog("Gallery")
    .SetDefaultCulture("zh-CN")
    .AddSupportedCultures("zh-CN", "en-US", "pt-BR"));
builder.Services.AddFlourishCulture();
builder.Services.AddFlourishFramework(framework => framework
    .ConfigureProject(project => project.SetProjectName(
        new TextReference("Gallery", "Project_Name", "Gallery")))
    .ConfigureTopBar(top => top.AddMenu(
        new TextReference("Gallery", "Menu_Pages", "页面"),
        menu => menu.AddMenuItem(new TextReference("Gallery", "Nav_Framework", "框架"), "open-framework")))
    .ConfigureNavigation(nav => nav.AddNav(
        new TextReference("Gallery", "Nav_Home", "主页"), "home", "/", exact: true)));
// Register the command parser and optional Design as usual.
```

AddFlourishCulture is an IServiceCollection extension in Microsoft.Extensions.DependencyInjection. It neither registers nor reconfigures Culture catalogs, Framework, Design or HTTP negotiation. It can precede or follow AddFlourishFramework; repeated calls remain idempotent. Keep all registrations before building the host.

Existing ordinary string builder calls remain literal, including strings that look like Key.Home. Only explicit TextReference overloads translate at render time. References are supported for project name, top-bar search/menu/items and primary/secondary/third/fixed navigation routes or commands. Route, command, icon, column and record identifiers are not translated. Do not call Localizer.Parse or resolve a scoped service at startup to store translated labels in singleton configuration.

## Validation isolation and browser assets

The 2026-10-06 compressed-resource incident supersedes the earlier assumption that changing OutDir alone safely isolates a Web/Razor verification build. OutDir redirects final binaries but can leave normal obj bundle/manifests shared. Use --artifacts-path with a task-specific directory so output and intermediate assets are separated by project. Do not regenerate a running normal Gallery's resources through a second validation build.

Test-GalleryCulture.ps1 now also checks real CSS, WOFF2 and Blazor JS delivery with identity, gzip and browser gzip/br negotiation, including MIME, decoded content size, CSS rules and hash equivalence. HTML localization checks alone do not establish that the browser can load its required resources. Run HTTP acceptance with the project's normal launch profile, or a properly published host; do not change application environment/resource configuration just to make a temporary test pass.

Example: dotnet run --project tests/Tests.Gallery.Flourish.Blazor/Tests.Gallery.Flourish.Blazor.csproj -c Release --artifacts-path artifacts/gallery-heading-validation. Normal Gallery source-bin/obj hashes should remain unchanged.

## Gallery collapsible page headings

The 2026-10-06 heading repair distinguishes catalog completeness from caller adoption. Navigation labels already resolved keys, but Framework, Examples, Appearance and component-category title dictionaries still contained literal Chinese values. PageHeading.Title is ordinary display text; installing the bridge does not automatically resolve arbitrary strings or infer a key from a navigation label.

Pages now inherit LocalizedComponentBase and parse their title keys while rendering. CatalogSections.TitleKeys and the topic dictionaries store generated keys rather than already-resolved or literal titles. Appearance calls the localization base during initialization and disposal while preserving its appearance-state subscription. Form and reconnection pages likewise retain their existing cleanup and release language subscriptions. API component names, product brands and actual record names remain semantic identities/data.

New record-create/edit/missing, not-found/error, reconnect, product-scenario, shell and category-back labels have complete en-US, zh-CN and pt-BR entries. That first repair covered rendered primary headings and corresponding browser page titles. The subsequent Gallery prose repair below completes explicit caller adoption for internal headings, descriptions and examples.

Tests.Gallery.Flourish.Blazor exercises actual Gallery pages with the real scoped Essential service and HtmlRenderer, switching en-US to zh-CN to pt-BR and back without remounting the page, then checking subscription release. Test-GalleryCulture.ps1 checks the real PageHeading H1 on 31 HTTP routes in all three languages, so translated navigation alone cannot satisfy the regression.

## Single-source catalogs and generated access keys

The 2026-10-06 three-language maintenance uses one Culture.json source file per owning Blazor project, with every access key containing en-US, zh-CN and pt-BR translations. Gallery now owns 1,483 application/documentation keys; Framework owns 281 standard-caption/component-usage keys. An application and a library are separate catalogs, not duplicate per-language files or one mandatory global catalog.

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

The bridge adapts explicit text references only. It does not discover or translate arbitrary Razor string literals, add catalogs, create language preferences or rewrite user-entered/business values. Gallery technical guides, API descriptions and all 80 samples now use explicit key extraction and render-time lookup. API syntax and business/input data retain their identities.

## Razor usage and language changes

Gallery has one Culture.json AdditionalFile, an independent generated Texts.Key namespace, XamlFramework=none, and disabled automatic copying/creation. It embeds that file as Gallery.Texts.json. Framework's separate standard dictionary is not a second generator input for Gallery, so tokens remain separate by both catalog identity and generated namespace.

```razor
@using ArkheideSystem.Essential.Culture.Blazor
@using TextKey = ArkheideSystem.Gallery.Flourish.Blazor.Texts.Key
@inherits LocalizedComponentBase

<PageTitle>@Localization.Parse(TextKey.Nav_Home)</PageTitle>
<PageHeading Title="@Localization.Parse(TextKey.Nav_Home)" />
<LocalizedText Key="@TextKey.Home_SetupDescription" />

@code {
    private void UseEnglish() => Localization.SetCulture("en-US");
    private void UseChineseWithBrazilianFormats()
        => Localization.SetCulture("zh-CN", "pt-BR");
}
```

LocalizedText refreshes itself; direct Parse properties refresh because LocalizedComponentBase subscribes to the scoped service. Framework components use their own neutral TextComponentBase, dispatch notifications with InvokeAsync and release subscriptions on disposal. ApplicationShell updates branding/menu/navigation/default ARIA strings and its live lang attribute, without resetting selected routes or manual tree expansion.

Gallery's top-bar LanguagePicker composes the standard SelectBox. The home page, /framework/localization and the keyed access examples support English, Chinese and Brazilian Portuguese. The latter demonstrates separate Brazilian formatting, a default-caption table, a localized dialog and code-copy boards. The subsequent Gallery prose completion covers documentation/sample UI throughout the current component inventory. Deliberate code snippets, data and multilingual font demonstrations remain literal.

## Standard defaults and explicit overrides

The embedded Framework catalog currently covers shell navigation accessibility, heading back accessibility, Dialog close, DisplayBoard copy/status/failure and Components.DataTable captions/loading/empty/row fallback. Missing keys use TextReference.FallbackText or Token; unregistered catalogs fail explicitly. Arguments use the selected FormatCulture, and malformed composite-format strings retain normal errors. Output uses ordinary encoded Razor text.

Existing explicitly supplied string parameters win even if they equal an old default. TextComponentBase detects supplied parameters using ParameterView instead of comparing values. Explicit DataTable.Text, Culture, Label, LoadingMessage and EmptyMessage remain host-owned. When omitted, TableText defaults use the provider and formatting uses its FormatCulture. Caption lookup is cached per component/selection, and row values rebuild on language/format notifications while table paging/search/view/sort state remains local.

DataTable already uses its Culture parameter for local comparison and display. Hosts that require fixed domain comparison should continue supplying Culture and, if needed, independent column Format delegates. This task does not change primitive sorting policy, browser-native numeric/date transport, validation, currency, time zones or business rules.

## HTTP initialization and persistence

Gallery configures RequestLocalizationOptions with zh-CN default, zh-CN/en-US/pt-BR UI and formatting languages. Providers are standard Cookie then Accept-Language. UseRequestLocalization runs before rendering. The scoped Culture service captures those request/circuit cultures for the first translated frame.

SetCulture changes only the current circuit and writes no cookie or ambient/default-thread culture. Reload therefore restores the request's cookie/header selection. The static root html language reflects SSR; live shell lang follows current circuit changes. Cookie writing or user preference storage remains host responsibility. No new persistence endpoint was added to Flourish, the bridge or Gallery in this task; Essential.Culture's independent web demo already shows an antiforgery-protected host POST example.

## Three-language verification on 2026-10-06

- Gallery Release build, using an isolated output directory: zero warnings/errors.
- build/Test-CultureCatalogs.ps1: 3,036 checks passed for duplicate/missing/empty keys, exactly three translations, format placeholders and line-break consistency.
- build/Test-GalleryCulture.ps1: 43 actual loopback HTTP checks passed for all three request languages, HTML language, generated Gallery text, language choices, access-method labels/actions, number/date formatting, Framework captions, separate requests and cookie precedence.
- build/Test-GalleryNavigation.ps1: 191 actual endpoint/navigation checks passed.
- Tests.Flourish.Blazor: 356/356 passed; Tests.Flourish.Extensions.Culture.Blazor: 12/12 passed.

Evidence is in artifacts/culture-three-build.log, culture-three-catalogs.log, culture-three-http.log, culture-three-navigation.log, culture-three-components.log and culture-three-bridge.log. These checks establish source/catalog consistency, real SSR output and component/bridge contracts. No Computer Use or browser automation was used in this follow-up; interactive visual acceptance remains user-run.

Manual acceptance: restart the Gallery, switch among English, Chinese and Brazilian Portuguese on the home, localization and access-method pages; switch language with a selected method/form value and confirm state is retained; use a second independent browser profile to confirm isolation; verify separate Brazilian numeric/date formatting and reload according to the existing cookie/Accept-Language selection. Live SetCulture does not persist a preference by itself.

## Verification and remaining work

Verified on 2026-10-05:

- Full Flourish.slnx Release build with TreatWarningsAsErrors=true: zero warnings/errors.
- 118/118 Flourish Blazor console checks passed, including seven new neutral-provider/lifecycle/reference tests.
- 12/12 real Culture bridge scope, catalog, format, fallback and registration checks passed.
- Existing WPF extension tests: 5/5 passed; its implementation remained intact.
- Headless installed Edge tested the published Gallery with separate English/Chinese browser contexts: SSR negotiation, independent live switching, translated home/title/chrome, Brazilian formatting, table captions, dialog/copy defaults, retained third-navigation selection/collapse and translated command routing. No page exceptions or failed framework assets were observed.
- Framework-only Native SSR returned 200, retained fallback captions and loaded neither Design nor Culture markup. This is narrower evidence than a complete Native browser acceptance run.
- Framework and bridge NuGet packages are prepared locally. Framework depends on Abstract; the bridge depends on Abstract/Culture.Blazor. Dictionaries are embedded without loose Culture.json/Texts.json. Earlier Abstract/Shared dependency evidence is superseded by the approved Shared consolidation.

Remaining integration work: choose host preference persistence, validate any client/WASM graph, and publish/version packages in dependency order. Gallery guide/sample UI translation is now complete for the current inventory. These are separate from the completed local Server/SSR bridge experiment. Use the actual component tests and user-run acceptance for the affected runtime boundary, and the [directory map](currentproject-architecture.md) to locate source. Retired UI guides are not runtime authority.

## Gallery prose, metadata and validation completion

The follow-up on 2026-10-06 fixes the underlying caller-adoption gap for internal H2, body prose, control examples and documentation. Section still renders ordinary caller-supplied Title/ChildContent; it does not infer translation keys. Gallery pages and samples resolve their keys during rendering and retain paragraph breaks. Dynamic states retain keys and formatting arguments, rather than a translated result captured at the time of an event. Options, table columns, chart labels and API descriptions are rebuilt for the current scope without resetting inputs, selection, sorting or edit history.

Singleton catalog metadata stores PurposeKey, VariantsKey and parameter DescriptionKey. SampleFor.DescriptionKey identifies scenario prose shown outside DisplayBoard. Framework ComponentUsageInfo carries provider-neutral TextReference values; its 160 usage entries join the existing 121 captions in the single Flourish Culture.json. Missing external dependencies are not introduced. Removed Chinese TableText overrides let the library supply current default captions.

Gallery annotations store stable validation keys. Field and ValidationMessages receive MessageFormatter explicitly to translate existing errors at display time. Rules and raw EditContext messages remain unchanged; both scoped language and validation subscriptions are released on disposal. Consumers omitting the formatter continue to render their original messages.

Verification uses --artifacts-path artifacts/gallery-body-validation to isolate both bin and obj. Gallery builds with warnings as errors; 27 pages and all 80 component details are rendered through the real Essential/Flourish DI scopes in en-US, zh-CN, pt-BR and back to en-US. Stored annotation errors refresh without another Validate call. HTTP checks assert semantic H2/paragraph content and compare identity/gzip/browser-negotiated CSS/font/startup payloads. Source guards check missing access keys in callers as well as complete three-language JSON, placeholders and paragraph structure. See [the dated report](bugfix-reports/2026-10-06_gallery-prose-localization.md) for final counts and limitations.

Manual acceptance: switch languages on Framework, Appearance, examples and several control details; review Portuguese wrapping and paragraph spacing; create errors before switching; keep a selected option, search query, sorted table and edited grid while switching; verify font/icons and original start.bat startup. No Computer Use test is required or performed.
ReleaseSettings.ConsoleTests now includes Tests.Gallery.Flourish.Blazor, and CheckScripts includes Test-CultureCatalogs.ps1. Test-Release executes these existing console/PowerShell gates without changing launch or asset configuration.
