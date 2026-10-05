# Culture integration plan for Blazor

This is the canonical integration plan for Essential.Culture and Flourish.Blazor, based on source inspection on 2026-10-04. It consolidates and supersedes the earlier culture-web-readiness.md audit. This is a proposal, not an implemented adapter or a claim of Web compatibility. No Culture source, project file, package reference or public API was changed by this analysis.

## Decision and implementation boundary

Reuse the existing .NET 10 catalog parser, stable tokens, fallback selection and composite formatting. First expose a supported public catalog/context API in Essential.Culture Core. Then introduce an optional Essential.Culture.Blazor adapter with a separate context for each server circuit and browser application. Keep Framework and Design usable without Culture. A scoped wrapper around the existing static Localizer does not solve user isolation.

The minimum deliverable is a public, independently constructible Core catalog/context, a Server adapter and a Gallery demonstration. An ASP.NET-only package, a Flourish-specific package, plural rules, live catalog reload and a generator rewrite are not prerequisites. They should be separate proposals if consumer needs justify them. The current request authorizes analysis and this plan; Culture implementation and new dependencies require a later task and approval.

## Verified source facts

Paths identify the inspected checkout. Line numbers refer to this source snapshot, not a published package or future implementation.

| Area | Evidence | Observed behavior |
|---|---|---|
| Public API | [Localizer.cs](../../../Essential.Culture/src/Essential.Culture/Localizer.cs), lines 6-83 | Every Current/Parse/TryParse/Contains operation delegates to LocalizationRuntime.Shared. Current.Changed subscribes to that same instance. No public factory, constructor or explicit-culture resolution overload exists. |
| Lifetime/loading | [LocalizationRuntime.cs](../../../Essential.Culture/src/Essential.Culture/LocalizationRuntime.cs), lines 9-48 | Runtime and constructor are internal. SharedHolder lazily loads AppContext.BaseDirectory/Culture.json synchronously, with current/fallback en-US. |
| State/events | Same runtime, lines 50-78 and 260-287 | Selection uses Lock, Volatile reads/writes and frozen lookup snapshots. Declared-culture snapshots are cached; arbitrary requested tags are not. Changed fires synchronously after the lock, on the caller's thread. |
| Lookup/formatting | Same runtime, lines 80-235 and 238-277 | Raw keys and Key.-prefixed tokens resolve. Parse returns unknown valid tokens/keys unchanged; malformed unresolved values throw. TryParse returns false and empty text for missing/malformed values, but formatted overloads can throw FormatException for insufficient/invalid arguments. A params array cannot be null. |
| Fallback | Same runtime, lines 289-331 and 565-577 | Selected tag, suffix-stripped parents, fallback and its parents. This textual parent chain is not a full negotiation engine. Formatting follows the selected tag even when text comes from fallback; unusable custom cultures use invariant formatting. |
| Schema | Same runtime, lines 333-532 | Root object maps each flat key to a culture-to-string object. Every key must declare the same normalized culture set and configured fallback. Values must be nonblank strings. Duplicate keys/cultures, invalid JSON/formats and unequal placeholder-index multisets are rejected. |
| Key/culture validation | [KeyToken.cs](../../../Essential.Culture/src/Essential.Culture/KeyToken.cs), lines 4-35; [KeyValidation.cs](../../../Essential.Culture/src/Essential.Culture/KeyValidation.cs), lines 89-153 | Keys are C# identifiers: dotted keys are invalid; underscores work. Cultures trim, replace underscores and normalize case. Custom/undeclared tags are accepted; this is not a supported-language allowlist. |
| Generator | [CultureGenerator.cs](../../../Essential.Culture/src/Essential.Culture.Generator/CultureGenerator.cs), lines 55-127, 165-330, 474-550 and 610-625 | Exactly one AdditionalFile with basename Culture.json per active compilation. Generates CultureKey and static Key string properties returning Key.*; these are not const fields. Configurable namespace and auto/none/wpf/avalonia/winui mode. Without a desktop adapter, Razor receives keys without a XAML facade. |
| Distribution | [Generator props](../../../Essential.Culture/src/Essential.Culture.Generator/buildTransitive/Arkheide.Essential.Culture.Generator.props), lines 3-23; [targets](../../../Essential.Culture/src/Essential.Culture.Generator/buildTransitive/Arkheide.Essential.Culture.Generator.targets), lines 2-43 | Defaults can create root Culture.json and copy it to output/publish. EssentialCultureAutoInclude=false disables that target; explicit AdditionalFiles still work. AutoCreate can also be disabled. Distinct namespaces do not change Key.* token values. |
| Existing tests | [AssemblyInfo.cs](../../../Essential.Culture/src/Essential.Culture/Properties/AssemblyInfo.cs); [LocalizerTests.cs](../../../Essential.Culture/tests/Essential.Culture.Test/LocalizerTests.cs), lines 7-171; [TestAssembly.cs](../../../Essential.Culture/tests/Essential.Culture.Test/TestAssembly.cs) | Tests construct internal runtimes through friend-assembly access, not a supported public API. Concurrency tests cover one mutable runtime, not two users. Combined runtime/WPF/Avalonia tests disable parallelization. |

### Packages and desktop adapters

Directory.Build.props records version 1.2.0. Core targets net10.0, has no runtime PackageReference and does not depend on a desktop adapter. It references Generator as an analyzer. Generator targets netstandard2.0; Microsoft.CodeAnalysis.CSharp 4.14.0 is its private build dependency. Core matches Flourish's .NET 10 target; earlier target support is not established.

- Wpf targets net10.0-windows and uses WPF bindings/markup. Its singleton change source forwards global events (WpfLocalizeExtensionBase.cs, lines 236-255).
- Avalonia targets net10.0 with Avalonia 12.1.0. Its singleton signal dispatches through Dispatcher.UIThread (AvaloniaLocalizeExtensionBase.cs, lines 225-255).
- WinUI targets net10.0-windows10.0.19041.0 with Microsoft.WindowsAppSDK 2.3.1. It tracks window properties and uses DispatcherQueue. Attach subscribes; Dispose and last-window Detach unsubscribe (WinUILocalizationHost.cs, lines 74-99, 170-218, 244-285 and 334-337). Marker tests do not verify actual windows.

These adapters are not usable as Razor integrations. Current Core provides one catalog per runtime, no public stream/JSON/embedded/HTTP loader, no registration/reload/unregistration, no async initialization, no DI/request/circuit lifetime, no browser persistence and no Razor refresh component. There is no plural/select/ICU MessageFormat engine: “Count: {0}” is composite formatting. Generator validates JSON/root keys but does not enforce all runtime translation/fallback/placeholder rules at build time.

## Safety assessment by hosting mode

| Mode | Reusable now | Blocker before supported integration |
|---|---|---|
| Static SSR | Server file deployment and parsing. A fixed application-wide language is possible under controlled static use. | Independent requests cannot select separate languages through static Current. Public request context is needed. |
| Interactive Server | Algorithms and immutable catalogs. | Circuit-local selection/subscriptions. Never switch static Current for a user or set process-wide DefaultThreadCurrentCulture for separate users. |
| Standalone WASM | Key generation and non-UI algorithms are candidates; browser applications ordinarily isolate client runtimes. | Default file initialization is not a browser asset loader. Published loading/trimming/globalization verification is absent. |
| Interactive WASM / Auto | Separate local server/client contexts are feasible. | Match initial selection/catalog version across prerender and activation. Server static state remains unsafe even when eventual rendering is client-side. |

Concurrent snapshots make a read coherent, but one user's switch changes every other user's next static Parse result. Global Changed broadcasts other users' changes. Subscriber exceptions propagate after state commits; concurrent setters can notify without captured previous/current state. Do not use a lock around temporary global switching, reflection to construct internals or a copied parser inside Flourish.

WASM/trimming is unverified rather than impossible. Core uses JsonDocument and generated regex instead of reflective DTO deserialization, reducing obvious reflection concerns. No inspected browser test, trimmed/AOT publish or IsTrimmable declaration establishes support. Anchor embedded resources explicitly and avoid reflective discovery. Flourish Abstract/Framework also carry Microsoft.AspNetCore.App FrameworkReferences, and Gallery is a Server host. Audit that full graph in a browser publish; net10.0 alone does not prove browser compatibility.

## Proposed Core API

All API examples below are design outlines, not available version 1.2.0 members or compiled code.

Separate immutable catalog ownership from selected culture. A catalog contains validated translations and reusable snapshots; a context contains a current selection and event. Public factories/constructors let applications and ordinary tests create two contexts without InternalsVisibleTo.

```csharp
// Proposed signatures only; implementation is intentionally omitted.
public sealed class LocalizationCatalog
{
    public static LocalizationCatalog FromJson(
        string json, string fallbackCulture = "en-US");
    public static LocalizationCatalog FromStream(
        Stream stream, string fallbackCulture = "en-US");
    public IReadOnlyList<string> AvailableCultures { get; }
    public string FallbackCulture { get; }
}

public sealed class LocalizationContext
{
    public LocalizationContext(
        LocalizationCatalog catalog,
        string uiCulture,
        CultureInfo formatCulture);
    public string UiCulture { get; }
    public CultureInfo FormatCulture { get; }
    public event EventHandler? Changed;
    public void SetCultures(string uiCulture, CultureInfo formatCulture);
    public string Parse(string token, params object?[] arguments);
    public bool TryParse(string token, object?[] arguments, out string value);
}
```

Also preserve nonformatted and useful generic formatting overloads. Atomic SetCultures avoids an intermediate render with mismatched text/formatting. Keep static Localizer as a desktop compatibility facade around a default context. New contexts must never touch that facade, mutate shared selection, set thread defaults or perform I/O in constructors.

Caller owns the stream; factories fully materialize data before returning. Preserve existing strict validation, fallback, unknown-token and error behavior initially. Document that formatted TryParse is not a catch-all exception shield. Share immutable snapshot caches at catalog level so circuits do not repeatedly parse/freeze identical content; bound caches by declared cultures or another measured limit.

UI language and formatting are separate. For uiCulture=zh-CN and formatCulture=pt-BR, Chinese labels and Brazilian Portuguese number/date formats can coexist. Current Core ties these to one tag; explicit formatting is a new capability. Language selection does not automatically alter currency, timezone, sorting, native input wire syntax or domain validation.


## Optional Culture adapter and Flourish contracts

Create Essential.Culture.Blazor in the Culture solution, with normal namespace/package identifiers. It depends on Core and required Blazor/DI APIs, never Flourish or desktop adapters. Generator remains build-time only. Choose exact references/versions and request approval under dependencydesign.md during implementation.

Adapter responsibilities: DI registration; explicit catalog IDs; selection/context initialization; optional LocalizedText and CultureScope components; renderer dispatch; disposal. Preference storage stays optional behind a host-provided contract. An ASP.NET-only adapter may be extracted later for MVC/API consumers; it is not necessary for the first Server integration.

Immutable public catalogs may be singleton per host. Server text provider, selection/context and preferences are scoped per circuit; static SSR uses a separate request scope. Each browser application owns an independent client context. Standard client scoped lifetime often behaves application-wide. When using .NET 10 RegisterPersistentService, use server scoped and client singleton registration so restoration/injection share one instance; Microsoft documents a client mismatch with scoped registration. [Blazor DI](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-10.0), [prerendered service persistence](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/prerendered-state-persistence?view=aspnetcore-10.0).

Proposed registration, not an existing extension:

```csharp
// Server: proposed options compose embedded catalogs and a scoped context.
// A host initializer supplies the validated SSR/circuit selection.
builder.Services.AddCultureBlazor(options =>
{
    options.AddEmbeddedCatalog(
        "gallery", typeof(ResourceAnchor).Assembly,
        "Gallery.Texts.json", fallbackCulture: "en-US");
    options.SupportedUiCultures = ["en-US", "zh-CN", "pt-BR"];
    options.DefaultUiCulture = "en-US";
    options.DefaultFormatCulture = "pt-BR";
});
```

Registration must not resolve a user context at startup or derive ongoing circuit state from an arbitrary later HttpContext. A host initializer supplies the initial pair before first text render, then the adapter restores it at activation. WASM uses explicit client setup; asynchronous HTTP catalogs load before the context is exposed as ready.

### Provider-neutral consumption

Keep the four Flourish.Blazor library projects. Framework consumes a small contract in Abstract; Design remains optional. A Gallery-owned bridge maps it to the Culture adapter. Do not add mandatory Culture references to Shared, Abstract, Framework or Design, or create a fifth Flourish library merely for the experiment.

Proposed generic contracts:

```csharp
public sealed record TextReference(
    string CatalogId, string Token, string? FallbackText = null);

public interface ITextProvider
{
    CultureInfo FormatCulture { get; }
    event EventHandler? Changed;
    string Get(TextReference text, params object?[] arguments);
}
```

Culture Core does not depend on Flourish types. Framework does not own negotiation, storage or SetCulture. The host bridge uses one selected pair for its catalog contexts. Catalog identity is separate from token: generated Key.Close in two assemblies is still the same string.

At inspection, navigation/menu DTOs are physically in Shared and Abstract references Shared. Putting Abstract text contracts inside those DTOs requires the planned contract relocation first, or an outer Framework projection. Avoid Shared-to-Abstract plus Abstract-to-Shared cycles. See [public API placement and future boundary work](blazor-extraction.md#public-api-placement-and-future-boundary-work).

Retain ordinary string parameters/builders. Add distinct TextReference overloads or optional reference parameters instead of guessing that Key.-prefixed strings require translation. Proposed precedence: explicitly supplied string, explicit reference, default framework reference/fallback. Detect explicit parameters directly; equality with an old default string does not mean it was omitted. Leave user-entered content outside framework localization.

ApplicationOptions is singleton and currently stores literal labels. It should store immutable references and resolve them during scoped rendering. Never translate in Program and freeze the result or capture a scoped provider in a singleton callback. Translate labels/tooltips/accessible text; preserve route, command and table identity keys.

## Catalog packaging without a mandatory generator rewrite

A minimal Server/RCL rollout can use the current generator: one Culture.json AdditionalFile per project, distinct generated namespace, automatic creation/copy disabled, and a unique embedded logical resource name. This example uses existing generator properties; a new stream loader is still required.

```xml
<PropertyGroup>
  <EssentialCultureNamespace>ArkheideSystem.Flourish.Blazor.Localization</EssentialCultureNamespace>
  <EssentialCultureXamlFramework>none</EssentialCultureXamlFramework>
  <EssentialCultureAutoInclude>false</EssentialCultureAutoInclude>
  <EssentialCultureAutoCreate>false</EssentialCultureAutoCreate>
</PropertyGroup>
<ItemGroup>
  <AdditionalFiles Include="Localization/Culture.json" />
  <EmbeddedResource Include="Localization/Culture.json"
                    LogicalName="Framework.Texts.json" />
</ItemGroup>
```

The analyzer basename remains Culture.json; deployment identity is Framework.Texts.json inside that assembly. The application uses another assembly/resource/catalog ID/namespace. Neither overwrites a shared output Culture.json. Current Shared cannot load these resources directly.

Do not supply multiple Culture.json AdditionalFiles in one active compilation. Different namespaces prevent Key/CultureKey type conflicts but do not prevent token collisions. For the first version, keep packs separate, fail on duplicate catalog IDs, and select each pack explicitly. Avoid an implicit merged catalog or implicit host overrides.

Later override support requires explicit base/override pairing, ordered precedence, culture-set validation and duplicate-key diagnostics. Existing strict same-language schema means sparse language packs cannot simply be concatenated. Generator work is warranted if a single compilation needs multiple generated catalogs, typed catalog identity, stronger schema diagnostics or first-class pack discovery. Razor key generation alone does not require it.

For a first WASM verification, prefer embedded data or an explicitly versioned public static asset loaded through HttpClient. HTTP requires async-ready/loading/error behavior, cache/version rules and matching SSR content. Translation assets contain no secrets; public version/hash metadata can identify the catalog used at activation. Core accepts data rather than owning HTTP or browser filesystem policy.

## Request language, prerendering and preferences

Proposed host precedence: validated explicit preference (account setting or cookie), an explicitly approved URL override if the product chooses that feature, negotiated Accept-Language, configured fallback. Do not enable a query provider accidentally through unrelated middleware defaults. Match a bounded UI-language allowlist: Core normalization/custom-tag acceptance is not that allowlist. Maintain separate formatting-culture defaults/lists.

Establish initial request UI/format culture with ASP.NET request localization before rendering. Keep supported lists consistent with adapter options. Culture JSON replaces neither request localization nor .resx. This plan assumes no new Microsoft.Extensions.Localization dependency.

Persist the pair through a host endpoint/service. Server cookie changes need a real HTTP response with validated application-local return navigation; do not write cookies after interactive response headers start. A full reload after saving server preferences is the first path. In-place text-only switching can leave ambient formatting/native inputs inconsistent.

SSR renders the selected pair and public catalog identities/versions. Restore these in the new circuit/client context before translated children render. Serialize primitive preferences/IDs rather than streams, Catalog objects, CultureInfo internals or callbacks. Revalidate restored/client input. .NET 10 propagates server CurrentCulture/CurrentUICulture to prerendered WASM by default; keep that behavior and initialize Culture's separate context consistently. Request localization, browser storage, ICU data and propagation are covered by [Microsoft's .NET 10 guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-10.0).

LocalStorage/JS runs only when interactive, never during server prerender. A cookie/server preference seeds SSR; optional localStorage must not silently overwrite restored selection during hydration. Circuits are independent while a shared cookie affects later requests. A retained circuit reconnects with its current selection; a new circuit uses persisted/restored validated state.

Set document lang from UI language; add direction only from supported metadata. Gallery currently hardcodes html lang=zh-CN in Components/App.razor. A child scoped service cannot update that prerendered outer document automatically; supply host markup integration and controlled document updates if in-place switching is supported.

## Rendering and refresh lifecycle

Events do not automatically rerender Razor. LocalizedText/CultureScope and the Flourish bridge subscribe to their local context, marshal through ComponentBase.InvokeAsync, and unsubscribe the same handler on Dispose/DisposeAsync. Never subscribe components to static Current.Changed. Avoid duplicate subscriptions on parameter changes or SSR/interactive reinitialization.

Use a selection revision/immutable changed snapshot if notifications race, read current state during rendering and coalesce redundant refreshes. Handlers must not block on async rendering, leak fire-and-forget exceptions or call StateHasChanged after disposal. Disconnection/cancellation and browser-module disposal are adapter lifecycle concerns. Static-only rendering needs no long-lived subscription.

Update accessible names/status at the same time as visible labels. Recompute language-dependent projections/table text and invalidate affected display caches. Preserve navigation expansion, record selection, column preferences, route matching and command dispatch. Explicit host literals retain precedence.


## Existing Flourish behavior to preserve

Gallery.Flourish.WPF uses Arkheide.Essential.Culture.Wpf 1.2.0 with Localization/Culture.json. That is desktop consumption, not Web proof. Blazor integration must not require a desktop migration or break static compatibility.

Flourish.Core separately owns ILocalizationService, embedded Assets/FlourishCulture.Json, file registration/reload/unregistration and richer events. Its Format uses CultureInfo.CurrentCulture. See [LocalizationContracts.cs](../../src/Flourish.Core/Abstract/LocalizationContracts.cs), lines 123-141, and [LocalizationService.cs](../../src/Flourish.Core/Localization/LocalizationService.cs), lines 15-222. Core's existing embedded catalog has dotted keys such as TitleBar.Back ([FlourishCulture.Json](../../src/Flourish.Core/Assets/FlourishCulture.Json)); Essential.Culture rejects that key grammar. Importing this catalog therefore requires an explicit key mapping and generated-call-site migration, alongside verification of language sets/fallback. Do not silently replace the service; no Core/WPF localization rewrite is needed for the Blazor experiment.

Blazor currently registers neither Core ILocalizationService nor Essential.Culture. [ServiceCollectionExtensions.cs](../../src/Flourish.Blazor/Flourish.Blazor.Framework/ServiceCollectionExtensions.cs), lines 30-46, registers singleton ApplicationOptions and scoped command/table services. [ApplicationOptions.cs](../../src/Flourish.Blazor/Flourish.Blazor.Framework/Hosting/ApplicationOptions.cs) holds startup literal strings; [ApplicationData.cs](../../src/Flourish.Blazor/Flourish.Blazor.Shared/ApplicationData.cs), lines 11-32, contains navigation/menu string records.

Text surfaces include ApplicationShell captions/ARIA names (lines 105-111), TableText (Shared/Components/TableContracts.cs, lines 109-131), Dialog/BottomSheet close captions, display-board copy status, validation/notices, search/selection/pager messages and row-action names. Both English and Portuguese defaults exist. Inventory internal literals as well as public Label parameters.

Formatting needs independent treatment: DataTable accepts explicit Culture and uses it for comparison/display/range formatting. ProgressBar/ProgressRing use ambient CurrentCulture for percentages. NumberBox uses invariant native number syntax. Shared primitive DataFilter/DataSorter use fixed pt-BR comparison. These behaviors or potential product decisions do not authorize sorting/parsing changes during localization. Initially pass FormatCulture where supported and preserve invariant transport/HTML formats.

## Phased implementation and acceptance gates

1. **Core isolation:** public immutable catalog and public context construction; separate UI/format culture; stream/JSON factories; static compatibility facade. Tests create two contexts sharing one catalog through public APIs. Gate: no cross-context state/events or ambient-culture mutation; current validation/fallback/formatting and desktop regression behavior pass.
2. **Resource and Server adapter:** explicit embedded catalog IDs, scoped circuit/request contexts, initialization/restoration and disposable subscriptions. Retain current generator with distinct namespaces and disabled automatic copies. Gate: library/application packs coexist; simultaneous different-language circuits remain independent; static SSR/prerender restoration is deterministic.
3. **Gallery bridge and generic contracts:** after necessary contract relocation, add provider-neutral references/render-time resolution. Start with shell/menu/accessibility text plus a formatted sample/table. Keep explicit literals and optional Design. Gate: Native works without Design/Culture; translated Gallery preserves navigation/commands/selection.
4. **Framework text coverage:** localize control defaults, empty/error/validation/copy/loading/tooltip/ARIA messages. Gate: fallback behavior documented; overrides stable; route/command IDs never treated as text. Product-specific validation remains host-owned.
5. **WASM/Auto verification:** when requested, add a real browser-targeting host, audit framework references, load embedded/HTTP data, persist primitives and select sufficient globalization data. Gate: Release trimming and optional AOT publish verified. New packages/workloads need approval. Do not advertise browser support before publication checks.
6. **Downstream adoption:** after supported packages and Gallery gates, integrate Colligere's language/preference policy while preserving authorization/session/business behavior.

New automated acceptance checks:

- Separate public contexts and real Server circuits under concurrent reads/switches; no cross-user state/event changes.
- Event-after-commit behavior, change revisions, duplicate/no-op changes, subscription count, disposal and cancellation.
- Raw/generated/malformed/unknown keys, parent/custom/fallback cultures, schema rejection, placeholder multisets and formatting argument failures.
- Split UI/format culture, without modifying process defaults.
- Pack IDs, generated types/tokens, output/publish collisions, embedded resource lookup and explicit override precedence.
- SSR/activation/refresh/reconnect/new-circuit consistency and document language; no browser storage in prerender.
- Catalog loading/version/error handling; released WASM trimming/AOT/globalization behavior when that mode is implemented.
- Host preference action validation, bounded cultures and local return navigation; unchanged command/route/table and validation policy.

Use source inspection, builds and automated tests. Do not use Computer Use.

## Verification performed for this plan

The implementation and project files were inspected directly. Culture has no AGENTS.md, AGENTS.ensure.json or docs-ai in this directory; none were created because it was explicitly read-only. A Git check with the required filesystem access confirmed a clean checkout at e03775a (Add dynamic localization key binding). An earlier restricted Git check could not establish repository identity. No Culture source, project or dependency files were changed.

Only existing Release test DLLs were executed; no restore, rebuild or source/project modification occurred. SDK 10.0.401 and VSTest 18.7.0 ran:

| Existing artifact | Result | Coverage limit |
|---|---:|---|
| Essential.Culture.Test/bin/Release/net10.0-windows/Essential.Culture.Test.dll | 28 passed | Inspected source has 8 Core, 10 WPF and 10 Avalonia tests; no Web isolation coverage. |
| Essential.Culture.Generator.Test/bin/Release/net10.0/Essential.Culture.Generator.Test.dll | 11 passed | Existing key/facade/configuration tests. |
| Essential.Culture.WinUI.Test/bin/Release/net10.0-windows10.0.19041.0/Essential.Culture.WinUI.Test.dll | 4 passed | Marker encoding/extraction only. |
| Total | 43 passed, 0 failed, 0 skipped | No Server/SSR/WASM adapter exists to test. |

Initial sandboxed invocations could not connect to testhost within 90 seconds and aborted before assertions. Running the existing artifacts with permitted test-process communication succeeded. Results were directed to the OS temporary directory. DLL timestamps were 2026-08-27, later than sampled runtime/generator source timestamps, but timestamps do not prove source-to-binary identity. These are existing-artifact results, not a fresh build or verification of proposed APIs.

## Manual acceptance checklist for future implementation

- Open independent sessions with different supported languages; switch one repeatedly. The other retains labels, formatting and accessible names.
- Select UI zh-CN and formatting pt-BR. Inspect numbers/dates, table output and native inputs; invariant HTML/wire values remain valid.
- Load a translated deep link with scripts delayed. Compare SSR, document lang, activation, refresh and reconnect. No language flash or preference loss.
- Navigate, open/close dialogs and copy controls, then switch language. Current controls refresh once; disposed controls receive no updates.
- Resolve library/application catalogs with identical raw key names under different IDs. Confirm distinct translations, literal overrides and missing-token fallback.
- Run Gallery with Framework plus Design and Native with Framework alone/no Culture. Verify existing navigation, commands, validation and record selection.
- For a future WASM/Auto host, inspect the Release published output, offline/failed loads, storage, server/client activation and all supported globalization data.
