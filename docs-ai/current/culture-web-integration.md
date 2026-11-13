# Culture integration and preference boundaries

## Ownership and registration

The optional Blazor and WPF bridges consume the declared Essential.Culture 1.4.0 packages. Essential is an independent NuGet dependency, not a source-project reference. Framework retains neutral text contracts and has no Essential or optional Design dependency. Source version remains Flourish 1.4.7; historical dependency/publication observations are in archived guides and release records.

For ASP.NET hosts, register AddFlourishFramework(configuration, framework => ...) and call ConfigureCulture inside that builder callback. Optional AddFlourishDesign is separate. Gallery's existing registration configures project/header/navigation and its scoped RecordStore; host routing, exception/status handling, antiforgery and endpoints remain host-owned. Configuring a service-only library does not create a Web host.

ConfigureCulture loads the embedded Flourish/Culture caption catalogs, applies application configuration and explicit builder values, installs the scoped CultureSession/ITextProvider, and adds request-localization startup integration. Duplicate configuration fails rather than adding parallel sessions. The former AddFlourishCulture/AddFlourishPreferences and Gallery-owned language/preference implementations are retired, not compatibility paths.

## Catalog modules and generated resources

CultureBuilder supports embedded marker/resource catalogs, caller-owned streams, immutable loaded LocalizationCatalog and AddCatalogFiles(catalogId, paths, fallbackCulture, options). Files are explicitly selected; relative paths resolve against AppContext.BaseDirectory, while caller-supplied absolute paths remain supported. Essential eagerly validates missing/malformed modules, globally duplicate keys, fallback and retained-language policies. Do not implement a second merger or runtime directory scan.

Gallery owns seven functional multilingual modules: Shell, Pages, Components, Parameters, Samples, Access and ChangeLog. CultureModule items and Essential's existing generator supply typed keys plus the CultureResources deployed-file manifest. Consumers configure their namespace/auto-include/module DeploymentPath according to the actual generator contract. The generated class is internal to its consumer. Build/publish must carry the same explicit files that startup loads.

Library captions remain independently embedded. Immutable catalogs may be shared; mutable UI/format selection, providers and subscriptions remain request/circuit scoped. Application-selected default catalog, explicit UI choices, fallback and format settings are not interchangeable with brand or authorization state.

## Browser and SSR preferences

The bridge's startup filter performs request negotiation before SSR. Its provider order is the supported culture Cookie followed by Accept-Language, with configured defaults for absent context. Browser preferences persist the selected UI/format pair; defaults remain application configuration rather than being rewritten for each user.

LanguagePicker and CultureSession validate supported UI/format choices. Selection persistence succeeds before applying the scoped state; storage, cancellation or connection failure returns failure rather than showing an unsaved choice as committed. Queued changes, asynchronous disposal and circuit lifecycle preserve the latest valid state. TextReference resolves the requested catalog/token with its encoded fallback and current formatting culture.

Cookie retention, culture support policy, URL/host/path scope and deployment security remain actual adapter/host contracts. A preference cookie contains culture data, not credentials or authorization. Changing language does not select an organization, locale-specific tax rules or another user's state.

## Native WPF adapter

Use FrameworkBuilder.UseEssentialCulture with a caller-owned EssentialTextProvider or the overload exposing the created provider for disposal. Independent LocalizationCatalog/LocalizationContext inputs provide isolated selection. Static-facade consumers intentionally share that facade and must configure the complete catalog before constructing views/providers; its explicit format override is not the same as independent context state.

The provider adapts neutral native text contracts, follows supported available-culture/fallback policy, publishes changes and detaches subscriptions on disposal. Native framework captions use the shared loaded catalog rather than a competing dictionary. [Native WPF integration](wpf-native-integration.md) covers resources/window lifetime.

## Verification and remaining acceptance

Maintained bridge/catalog/generator/SSR/preference tests remain repeatable verification. Archived reports retain their exact test counts and older API versions; they do not prove a later restore or new host passed. Static SSR and Interactive Server have recorded coverage; WASM and Interactive Auto remain unverified rather than implicitly supported by this guide.

Manually check first SSR negotiation, refresh persistence, UI versus number/date formatting, explicit supported/fallback choices, browser failure/retry, multiple circuits/browsers, generated module deployment and disposal. On desktop check independent providers versus intentional static-facade sharing and owner-window teardown. No global locale, package publication or new runtime dependency changes here.
