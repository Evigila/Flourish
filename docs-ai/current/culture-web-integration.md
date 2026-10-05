# Culture Web integration

## Implemented local integration

The 2026-10-05 local integration supersedes this guide's earlier proposed APIs and read-only readiness assessment. Essential.Culture now has public immutable catalogs and independent contexts plus a scoped Blazor adapter. Gallery consumes that adapter through a separate optional Flourish.Extensions.Culture.Blazor bridge. The four Flourish.Blazor libraries remain separate; no desktop localization implementation was replaced.

This integration targets static SSR and Interactive Server on .NET 10. Pure WASM and Interactive Auto have not been validated. Current Framework, Abstract and Culture.Blazor projects use Microsoft.AspNetCore.App FrameworkReference, so this source build does not establish a client-compatible dependency graph.

## Ownership and dependency direction

- Gallery owns its translated application names, navigation labels, pages, language picker, request negotiation and future preference persistence.
- Framework owns behavior, provider-neutral text references and its embedded standard-caption catalog. It has no reference to Essential.Culture or the extension bridge.
- Abstract exposes TextReference and ITextProvider. Design remains optional and owns visual presentation only.
- src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor owns the optional bridge. Its package is Arkheide.Flourish.Extensions.Culture.Blazor; it references only Flourish.Blazor.Abstract and Essential.Culture.Blazor. It does not reference Framework, Design or WPF.
- Essential.Culture.Blazor registers the request/circuit-local ILocalizationService; immutable parsed catalogs may be shared. Essential.Culture owns translation algorithms and formatting, without a Flourish dependency.
- Framework supplies a literal fallback provider when no bridge is installed. Framework-only Native retains its ordinary controls and English default captions.

ApplicationData.cs still belongs to Shared and is not moved in this task. Abstract already references Shared. Framework therefore stores TextReference metadata separately from the existing navigation/menu records, using reference identity and an immutable startup map. This avoids a Shared-to-Abstract cycle and preserves the public DTO signatures and assembly placement. Equal menu item records can still point to different catalogs. The wider public-contract placement recommendation remains separate future work.

## Why the WPF bridge stays separate

The desktop bridge is now src/Flourish.Extensions/Flourish.Extensions.Culture.WPF, with package ID Arkheide.Flourish.Extensions.Culture.WPF and namespace ArkheideSystem.Flourish.Extensions.Culture.WPF. It targets net10.0-windows, uses WPF Dispatcher, Singleton applicator/hosted service and static Localizer.Current. That process-wide desktop synchronization cannot represent separate Web users.

The WPF implementation and UseEssentialCulture entry remain unchanged. Its project, assembly, namespace, test identity and package name were migrated into Flourish. Its existing EssentialCultureVersion=1.1.0 default was preserved. Gallery.Flourish.WPF currently consumes Culture.Wpf directly; it does not itself reference this middle-layer package. These are distinct facts, rather than a claim that the current WPF Gallery already uses the extension.

The new Blazor bridge is Scoped. It maps Get to TryParseFrom, forwards Changed subscriptions directly and exposes the adapter's UI/format selection. It has no hosted service, Dispatcher, static Localizer access or permanent event subscription.

## Local source references and packaging

Gallery and both extensions reference their same-repository Flourish projects. All external Culture and generator references use NuGet packages at 1.3.0. Prepare Essential first; when sibling Essential/artifacts/packages exists, Flourish adds that directory as a local NuGet source. It does not select external source projects.

EssentialCultureRoot, UseLocalEssentialCulture, UseLocalFlourish and UseLocalCultureIntegration are retired. The current consumer boundary is PackageReference, including WPF. Local package feeds and public NuGet feeds provide the same package identity/version boundary. See [NuGet release and integration](nuget-release-integration.md).

The bridge packages are Arkheide.Flourish.Extensions.Culture.WPF and Arkheide.Flourish.Extensions.Culture.Blazor, version 1.1.0 alongside the six other Flourish packages. Essential package names, namespaces and desktop translation APIs remain unchanged. CI restores Essential 1.3.0 packages; publish Essential first. Local verification does not publish packages. See the [bridge guide](culture-extension-bridge.md) and [repository organization](solution-organization.md).

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
    .AddSupportedCultures("zh-CN", "en-US"));
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

Gallery's top-bar LanguagePicker composes the standard SelectBox. The home page and /framework/localization are bilingual. The latter demonstrates separate Brazilian formatting, a default-caption table, a localized dialog and code-copy boards. Other Gallery documentation/sample pages still contain literal Chinese text: this is a validated integration rollout, not a completed translation of every guide or primitive.

## Standard defaults and explicit overrides

The embedded Framework catalog currently covers shell navigation accessibility, heading back accessibility, Dialog close, DisplayBoard copy/status/failure and Components.DataTable captions/loading/empty/row fallback. Missing keys use TextReference.FallbackText or Token; unregistered catalogs fail explicitly. Arguments use the selected FormatCulture, and malformed composite-format strings retain normal errors. Output uses ordinary encoded Razor text.

Existing explicitly supplied string parameters win even if they equal an old default. TextComponentBase detects supplied parameters using ParameterView instead of comparing values. Explicit DataTable.Text, Culture, Label, LoadingMessage and EmptyMessage remain host-owned. When omitted, TableText defaults use the provider and formatting uses its FormatCulture. Caption lookup is cached per component/selection, and row values rebuild on language/format notifications while table paging/search/view/sort state remains local.

DataTable already uses its Culture parameter for local comparison and display. Hosts that require fixed domain comparison should continue supplying Culture and, if needed, independent column Format delegates. This task does not change primitive sorting policy, browser-native numeric/date transport, validation, currency, time zones or business rules.

## HTTP initialization and persistence

Gallery configures RequestLocalizationOptions with zh-CN default, zh-CN/en-US UI languages and zh-CN/en-US/pt-BR formatting languages. Providers are standard Cookie then Accept-Language. UseRequestLocalization runs before rendering. The scoped Culture service captures those request/circuit cultures for the first translated frame.

SetCulture changes only the current circuit and writes no cookie or ambient/default-thread culture. Reload therefore restores the request's cookie/header selection. The static root html language reflects SSR; live shell lang follows current circuit changes. Cookie writing or user preference storage remains host responsibility. No new persistence endpoint was added to Flourish, the bridge or Gallery in this task; Essential.Culture's independent web demo already shows an antiforgery-protected host POST example.

## Verification and remaining work

Verified on 2026-10-05:

- Full Flourish.slnx Release build with TreatWarningsAsErrors=true: zero warnings/errors.
- 118/118 Flourish Blazor console checks passed, including seven new neutral-provider/lifecycle/reference tests.
- 12/12 real Culture bridge scope, catalog, format, fallback and registration checks passed.
- Existing WPF extension tests: 5/5 passed; its implementation remained intact.
- Headless installed Edge tested the published Gallery with separate English/Chinese browser contexts: SSR negotiation, independent live switching, translated home/title/chrome, Brazilian formatting, table captions, dialog/copy defaults, retained third-navigation selection/collapse and translated command routing. No page exceptions or failed framework assets were observed.
- Framework-only Native SSR returned 200, retained fallback captions and loaded neither Design nor Culture markup. This is narrower evidence than a complete Native browser acceptance run.
- Framework and bridge NuGet packages were created locally. Framework nuspec depends only on Abstract/Shared; the bridge nuspec depends only on Abstract/Culture.Blazor. Published dictionaries are embedded with no loose Culture.json/Texts.json.

Remaining rollout: translate the other guides and primitive default captions, choose host preference persistence, validate any client/WASM graph, and publish/version packages in dependency order. These are separate from the completed local Server/SSR bridge experiment. See the [manual acceptance checklist](blazor-manual-tests.md) and [directory map](currentproject-architecture.md).
