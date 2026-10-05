# Optional Blazor Culture integration

This extension connects the independent Flourish text contract to Essential.Culture.Blazor. The existing WPF extension remains a separate Windows package with its existing public API and lifetime.

## Dependency boundary

Flourish.Extensions.Culture.Blazor targets net10.0 and references only Flourish.Blazor.Abstract and Essential.Culture.Blazor, plus the ASP.NET shared framework required by the current Server adapters. It does not reference Framework, Design, WPF or application business code. This implementation supports static SSR and Interactive Server; pure WASM and Interactive Auto are not verified.

The package is Arkheide.Flourish.Extensions.Culture.Blazor 1.1.0. It uses the same-repository Abstract version and the root EssentialCultureVersion=1.3.0 dependency. Culture 1.3.0 and Flourish 1.1.0 are verified local package outputs, not a claim of public NuGet availability.

## Host configuration

Register catalogs and a scoped Culture session through AddCultureBlazor. Register Flourish normally and then connect the optional provider:

```csharp
builder.Services.AddCultureBlazor(options => options
    .AddCatalog("App", appCatalog)
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US", "zh-CN"));

builder.Services.AddFlourishFramework();
builder.Services.AddFlourishCulture();
```

The bridge also works before Framework registration because Framework's default provider uses TryAdd. Repeated AddFlourishCulture calls leave one scoped ITextProvider descriptor. Explicit bridge registration replaces existing text-provider registrations; later explicit replacement remains a host choice.

AddFlourishCulture does not register or reconfigure Culture, catalogs, language defaults, Framework, Design, cookie endpoints or persistence. Missing Culture registration fails DI validation.

## Text and events

ITextProvider.Get accepts a TextReference with explicit catalog ID, token and optional fallback. Catalog IDs prevent collisions between equal generated token strings in different assemblies. Existing translations use the selected scope's UI and formatting culture. Missing keys return FallbackText when supplied, otherwise the original Token. When arguments are supplied, fallback text uses the scoped formatting culture, consistently with the default Flourish provider. Missing catalogs and composite-format errors are not hidden.

Culture and FormatCulture delegate to the scoped Culture service. Changed add/remove accessors directly forward subscriptions; the bridge holds no permanent subscriber or dispatcher. Consumer components own renderer dispatch and unsubscribe when disposed. No static Localizer or ambient thread culture is changed.

Immutable catalogs may be host singletons. Text providers and Culture selections are request/circuit scoped. Applications must keep immutable references in singleton shell configuration and resolve them while rendering. Route, command, identity and user-entered content remain outside localization.

## Local verification

Both maintained extensions reference Flourish projects and Essential.Culture NuGet packages at 1.3.0. Only the Blazor bridge belongs to this first six-package Core/Blazor release. A sibling Essential/artifacts/packages folder supplies a local feed when present; no external source project is selected. See [NuGet release and integration](nuget-release-integration.md) for the current scope.

The independent console verification project uses actual DI scopes and catalogs without adding a test package. Run:

```powershell
dotnet restore tests/Tests.Flourish.Extensions.Culture.Blazor/Tests.Flourish.Extensions.Culture.Blazor.csproj
dotnet run --project tests/Tests.Flourish.Extensions.Culture.Blazor -c Release
```

Checks cover independent users, catalog identity, fallback, explicit formatting, unsubscribe, registration order, repeated registration, scoped validation, absent catalogs, invalid arguments and unchanged ambient culture. Build warnings are treated as errors during verification. The WPF tests are now Tests.Flourish.Extensions.Culture.WPF in the Flourish root and extension solutions.

## Manual acceptance

- Open two independent browser profiles and switch one language; the other stays unchanged.
- Verify first-render text agrees with the negotiated request culture.
- Switch language while on a nested navigation page; labels change while routing and selection remain intact.
- Verify menu, table, accessibility captions and application text use the intended catalog.
- Dispose a localized component and confirm it receives no subsequent updates.
- Verify cookie reload persistence through the host endpoint, independently of live circuit switching.
- Start without the optional extension and confirm Framework remains usable.

## Verified local results

Historical verification before the unified package release: Release builds with TreatWarningsAsErrors passed for the combined solution. All 12 Blazor bridge checks and all five existing WPF extension tests passed against local sibling sources. That earlier packing produced Arkheide.Flourish.Extensions.Culture.Blazor.1.0.0 with exactly two package dependencies: Arkheide.Essential.Culture.Blazor 1.3.0 and Arkheide.Flourish.Blazor.Abstract 1.1.0. The package also declares Microsoft.AspNetCore.App, with no Framework, Design or WPF dependency.

The current Blazor release solution includes this console verification and it is executed explicitly by Test-Release.ps1. CI restores Essential 1.3.0 from NuGet without checking out sibling source. Publish Essential first, then the six Flourish packages under a matching v1.1.0 tag, only after separately authorized publication. Former eight-package/culture-v source-checkout instructions are superseded. NuGet emits its advisory about the intentionally omitted README; no README is created.
