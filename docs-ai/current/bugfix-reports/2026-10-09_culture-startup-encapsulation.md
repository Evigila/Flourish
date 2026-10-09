# Culture startup and preference encapsulation

## Symptoms and cause

Gallery Program registered Essential catalogs, request-localization options, a separate text bridge and a separate preference writer. Gallery also owned language/format selectors, browser-error handling and the save-then-apply sequence. The optional Culture extension only adapted text lookup, leaving its integration responsibilities scattered across the consumer and Framework.

The user requested three Gallery service entry points: AddFlourishFramework, AddFlourishDesign and AddScoped<RecordStore>. The user also explicitly confirmed that appsettings.Flourish.json holds shared defaults while personal choices persist independently in each browser. This redesign supersedes the initial same-day persistence implementation; its report and append-only records remain historical evidence.

## Implemented boundaries

Gallery now passes builder.Configuration to AddFlourishFramework and configures Culture inside the framework callback. Its only Culture startup data is the application catalog identity/resource. Project identity, menus, routes, command parser and RecordStore remain Gallery concerns. Normal exception/status handling, antiforgery and endpoint mapping remain host responsibilities.

Framework loads optional appsettings.Flourish.json below existing host configuration sources. Business settings, environment and command-line overrides retain priority. ConfigurationManager uses its configured file provider; other IConfiguration inputs receive a merged settings snapshot. Framework registers Interactive Server Razor services in an ASP.NET host, while service-only consumers can register its services without constructing a host. Its provider-neutral ConfigureServices callback supplies resolved configuration to optional integrations after shell configuration freezes.

The Culture extension owns embedded library catalog loading, application catalog loading, Essential registration, request initialization, cookie negotiation, browser persistence, renderer subscriptions and LanguagePicker. An internal IStartupFilter installs request localization; cookie, Accept-Language and configured defaults restore both culture identifiers before SSR. Essential remains the single dictionary/formatting core. Framework has no reference to Culture or Essential.

CultureBuilder validates resources, catalog identities, supported/default cultures and retention before freezing. CultureSession implements ITextProvider directly and exposes the necessary application text and preference operations. SelectAsync and SelectFormatAsync serialize the complete persistence/apply transaction. Formatting-only selection reads the latest UI culture after entering the queue, preventing a concurrent operation from restoring an old UI language. Storage, connection and cancellation failures retain the current pair and permit retry. Module import is deferred until interaction; disposal waits for queued transactions.

LanguagePicker composes existing SelectBox, Dialog and Notice. Its options come from configured cultures, with native names; FormatCulture selects formatting independently. Failure strings and usage metadata belong to the extension's three-language catalog. Gallery adds its complete navigation/API/sample registration. There are 80 Framework components and one extension component, with 81 Gallery guides, 721 API rows and 328 documented defaults.

The old AddFlourishCulture, AddFlourishPreferences, independent CultureTextProvider, Framework BrowserPreferences, old Framework browser-module path and Gallery-local LanguagePicker are removed without aliases. Gallery pages import the extension's LocalizedComponentBase, and App reads document language through ITextProvider. Application key-generation metadata remains a necessary application build concern; Gallery does not call Essential runtime APIs.

## Dependency and packaging consequences

The existing Razor SDK supports the extension's reusable component and static asset. Its same-repository dependency changes from Abstract to Framework: Culture extension → Framework → Abstract → Core. Design remains optional. No external package, external version, package identity or release version changes.

Gallery references the same-repository extension project so local source edits cannot be hidden by a cached same-version package. Isolated package consumers still validate the NuGet boundary. Pack/publish ordering now places Framework before its Culture extension. Six local candidate packages were created in artifacts/culture-redesign-packages; none were published.

## Evidence and regression results

- Release builds of Framework tests, Gallery, Culture tests and the production aggregate: zero compiler warnings/errors.
- Framework console suite: 436/436 checks, including file/default precedence, generic registration ordering/freeze and Design configuration overrides.
- Gallery suite: 8,275 checks, including 474 actual post-event checks and 227 ChangeLog checks. Both actual picker modes, failure UI, retry, state retention and subscription disposal passed.
- Culture extension: 21 checks covering registration, catalog identity, scope isolation, HTTP initialization, lazy interop, concurrent transactions, invalid selections, failure/retry, disconnection and disposal. The first run exposed an uncaught JSDisconnectedException; the catch now names it explicitly, and import/write disconnection regressions pass.
- Browser cookie module: four Node checks passed.
- Catalog and consumer-boundary checks: 22,320 checks passed across 1,552 Gallery, 286 Framework and six Culture keys. ChangeLog validation passed 91 checks.
- Real loopback HTTP: eight preference/asset checks, 583 culture/asset checks and 240 navigation checks passed. The new extension asset was delivered correctly with identity and gzip requests. An initial invocation used BaseUrl instead of the older scripts' BaseUri parameter; corrected invocations passed.
- All six candidate packages passed dependency and asset verification. Four fresh isolated consumers passed 136 checks: FrameworkOnly, MetaNative, MetaDesign and MetaCulture. This includes Framework without Culture activation, automatic Razor registration, catalog generation/loading, scoped text, negotiated SSR and published assets.

Temporary HTTP hosts were stopped after validation. Package-consumer evidence is under artifacts/package-consumers/2f11b247461042af8727ff95ddff40a0. No Computer Use or browser acceptance run was performed.

## Manual acceptance and limits

1. Change UI language in the top bar, navigate between pages, then refresh. Verify the selection, titles, menus, dialog captions and table captions remain consistent.
2. Keep Chinese UI and select Brazilian number/date formats on the localization page. Verify formatted samples and tables use Brazilian formatting, including after refresh.
3. Quickly alternate language and format selections. Verify the final formatting choice does not restore an earlier UI language.
4. Block site cookies and attempt a change. Verify the standard error dialog appears, the previous choice remains, and retry succeeds after storage is allowed.
5. Open a second browser profile and verify its preference is independent. Changing either browser must not rewrite appsettings.Flourish.json or appsettings.json.
6. Change framework defaults and restart the application. Verify new visitors receive them and existing browser choices still take precedence. Verify environment/command-line settings override file defaults.

This source change targets SSR and Interactive Server. It does not establish WASM/Interactive Auto support. Theme/palette, keyed table/chart presentation and optional navigation preferences remain separate persistence candidates. Retired source APIs require consumer updates; current public tags retain their historical implementations. No Git commit, tag or publication was performed.
