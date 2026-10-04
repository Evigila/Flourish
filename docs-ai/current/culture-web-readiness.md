# Essential.Culture readiness for Blazor

## Conclusion and scope

Essential.Culture's platform-neutral parsing and key generation can be reused. Its current public API cannot safely provide independent user languages in Blazor Server. A Web adapter also needs an instance API in Culture Core; a scoped service that calls the existing static Localizer would still share one process-wide language.

This report is a read-only source audit of `C:/Users/Evigila/source/repos/Essential.Culture` on 2026-10-03. That checkout has no AGENTS.md, AGENTS.ensure.json or docs-ai directory. No files, packages or references were changed there, and its tests were inspected rather than rerun. Proposed APIs below are design suggestions, not implemented functionality.

## Existing support

| Package | Current target | What can be reused |
|---|---|---|
| Core | net10.0 | JSON validation, frozen catalog, culture fallback, formatted token resolution |
| Generator | netstandard2.0 | Roslyn generation of CultureKey and Key constants; configurable generated namespace |
| Wpf | net10.0-windows | Desktop XAML bindings and change subscriptions; not usable in Razor |
| Avalonia | net10.0 | Avalonia markup extensions and UI dispatch; not usable in Razor |
| WinUI | net10.0-windows10.0.19041.0 | Window localization and lifetime handling; not usable in Razor |

Core does not depend on WPF, Avalonia or WinUI. Its .NET 10 target matches Flourish. The catalog validates duplicate keys, cultures, fallback translations and composite-format placeholders. Resolution uses the selected culture, parent cultures and fallback. Formatting uses the snapshot's FormatCulture explicitly. Frozen dictionaries, cached CompositeFormat values and atomic snapshots are useful infrastructure for a new context API.

Generator already works without a desktop adapter: its auto mode generates keys but does not create a XAML Localize facade. Razor can consume generated token constants through a DI localizer. Generator changes are needed for resource distribution and multi-catalog support, not just to recognize Razor syntax.

## Concrete blockers

1. **Shared language selection.** `Localizer.Current`, `Parse`, `Contains` and `Changed` all delegate to `LocalizationRuntime.Shared`. `LocalizationRuntime` is internal, and its constructor is internal. External consumers cannot construct supported isolated contexts. It follows from this implementation that user A changing culture changes user B's next static Parse result in the same server process.
2. **File-only initialization.** The shared holder synchronously loads `AppContext.BaseDirectory/Culture.json`, initially using en-US. There is no public Stream, JSON text, embedded-resource or asynchronous HTTP loading API. A server can deploy this file, but the default runtime cannot load a browser-hosted WASM resource through this path.
3. **One output catalog.** Generator's transitive target creates a root Culture.json when missing and copies it to output and publish directories. A framework library and an application cannot each blindly publish their own catalog to the same target name. They need separate catalog identities or a deliberate merge and override order.
4. **No Web lifecycle.** There is no request/circuit DI lifetime, language initialization from browser preference, SSR-to-circuit restoration, cookie/localStorage integration or Razor rerender subscription. Changed is raised synchronously on the calling thread. Razor subscribers must marshal through InvokeAsync and unsubscribe on disposal.
5. **Formatting is not application globalization.** Core supplies its own CultureInfo for translated format strings. It does not set CurrentCulture, CurrentUICulture or thread defaults. Native dates/numbers, other .NET formatting and third-party components still need host globalization configuration.

Source evidence:

- [Localizer.cs](../../../Essential.Culture/src/Essential.Culture/Localizer.cs): public static facade, lines 6-35.
- [LocalizationRuntime.cs](../../../Essential.Culture/src/Essential.Culture/LocalizationRuntime.cs): internal shared runtime and file constructor, lines 9-45; state/change events, lines 56-75; explicit formatting, lines 95-104; fallback, validation and format culture, lines 305-576.
- [CultureGenerator.cs](../../../Essential.Culture/src/Essential.Culture.Generator/CultureGenerator.cs): namespace configuration, lines 79-80; framework detection, lines 95-127; key/facade generation, lines 293-330 and 485-489.
- [Generator targets](../../../Essential.Culture/src/Essential.Culture.Generator/buildTransitive/Arkheide.Essential.Culture.Generator.targets): automatic catalog creation and output copies, lines 27-41.

## Hosting assessment

| Mode | Current status | Required work |
|---|---|---|
| Static SSR | Parsing can run on the server, but the static language is shared | Initialize an isolated request context from a validated culture; provide rendering notifications only where interactive |
| Interactive Server | A process-wide selected language is unsuitable for independent users | Scoped circuit context, consistent initial SSR language and connection restoration, disposable UI subscriptions |
| WebAssembly | Core's algorithm is platform-neutral; its default loading path is not a browser resource loader | Embedded or HTTP-loaded catalog, client context, optional localStorage, appropriate globalization data in the published client |
| Auto / mixed rendering | No existing adapter coordinates server and browser state | Separate local contexts with matching initial culture, catalog identity and preference persistence |

These assessments are deductions from the source, not results of a Web integration test. Microsoft's [.NET 10 Blazor localization guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-10.0) describes request localization, a server culture cookie and client local storage. A culture-changing endpoint should validate supported cultures and use a local redirect; response cookies cannot be added arbitrarily during an established interactive render.

## Proposed minimum change

1. **Culture Core:** expose an immutable LocalizationCatalog with Stream/JSON initialization and an instance culture context/localizer. Keep static Localizer as a desktop compatibility facade. Share parsed catalog data while isolating selected culture and Changed events per context. Specify catalog conflicts and fallback explicitly.
2. **Essential.Culture.Blazor:** provide DI registration, optional language scope/text components, change subscriptions and disposal. Initialize Server scopes from the validated request language; use an independent client context for WASM. A browser storage provider must only run after interactivity is available.
3. **Optional ASP.NET integration:** coordinate supported languages, localization middleware and cookie persistence through a host endpoint. Keep framework rendering and host security/persistence separate. Do not change DefaultThreadCurrentCulture process-wide for different Server users.
4. **Catalog distribution:** give library and application catalogs distinct identities and resources. Define merge precedence if they are combined. Configure generated key namespaces separately to avoid duplicate Key/CultureKey classes. Adjust the generator's single Culture.json copy policy for libraries.
5. **Flourish bridge:** accept a runtime text provider/token alongside ordinary string labels. Resolve navigation, menu, validation, tooltip and ARIA text during rendering. Keep the Culture dependency in an optional adapter; Framework and Design must remain usable without it.

The exact interface names and package division should be agreed during implementation. A Core catalog/context API plus one Blazor adapter is sufficient for the first Gallery integration; an ASP.NET-only package is optional if non-Blazor Web hosts need it.

## Existing Flourish boundaries

Gallery.Flourish.WPF already references `Arkheide.Essential.Culture.Wpf` 1.2.0 and carries its own Localization/Culture.json. That desktop consumer does not demonstrate Web compatibility.

Flourish.Core separately owns an ILocalizationService, embedded Assets/FlourishCulture.Json and custom catalog registration/reload behavior. Its JSON has the same basic key-to-culture object shape as Essential.Culture, but its registration, fallback and lifetime contracts are separate. Migration must map or preserve those contracts rather than silently replace them. It is not necessary to rewrite the WPF stack to start a Blazor adapter.

Blazor's `ApplicationOptions` is registered as a singleton and holds startup label strings. Translating in Program would freeze those strings at startup. The optional bridge should resolve immutable tokens in the current user's scope instead. Current Blazor registration does not register Core's ILocalizationService or reference Essential.Culture.

## Validation needed before integration

The inspected checkout contains 43 Fact tests: Core 8, WPF 10, Avalonia 10, Generator 11 and WinUI markers 4. Core concurrency checks establish consistent reads of one runtime; they do not establish independent users. Test execution is globally nonparallel in the inspected Core test assembly. No Web/circuit/SSR/WASM integration coverage was found.

Required new checks:

- Two independent Server scopes and connections retain different cultures during simultaneous reads and switches.
- SSR, interactive startup, refresh and reconnect use the same selected language without cross-user state.
- Missing translations, parent-language fallback, formatting arguments and unsupported cultures retain defined behavior.
- Library and application catalogs coexist without output-file, key-type or token collisions.
- Components rerender after changes, release subscriptions and do not access browser storage during prerender.
- WASM publication loads catalogs and necessary globalization data without filesystem assumptions.
- Flourish's default labels, validation and ARIA text can update while explicit host strings remain supported.

Implementation order: Culture Core instance API → Web/Blazor adapter → Gallery verification → optional Flourish bridge and text resources → Colligere adoption.
