# Gallery prose and metadata localization

## Symptoms and cause

Internal H2 headings, explanatory paragraphs and component/sample labels remained Chinese while navigation and primary page titles changed language. The optional Culture bridge resolves explicit references; Section cannot infer a translation key from arbitrary Title or ChildContent strings. Static ComponentCatalog, ParameterMeaning, ComponentUsageCatalog and SampleFor metadata also captured untranslated prose. Explicit Chinese TableText overrides suppressed translated library defaults. Event handlers could retain a translated status snapshot, and annotation errors were literal Chinese messages.

This completed repair supersedes the open [coverage audit](2026-10-06_gallery-body-localization-coverage.md), preserving that historical diagnosis.

## Changes and ownership

- Gallery pages, shared guides and all 80 sample components resolve H2, paragraphs, actions, labels, accessibility captions, options, columns, chart labels, status/count/error text and documentation in the current request/circuit. Long descriptions render as paragraphs outside code/control background boards.
- Static Gallery metadata stores stable PurposeKey, VariantsKey, DescriptionKey and contextual prefix/owner tokens. Framework usage metadata carries TextReference values in the Flourish catalog, with English provider-neutral fallbacks. The former plain-string metadata contract is removed, and real callers/tests are updated.
- Gallery maintains one Culture.json with 1,483 complete en-US/zh-CN/pt-BR keys. Framework maintains one with 281 keys, including 160 usage descriptions and its existing 121 standard captions. Unused newly extracted sample keys were removed. No new dependency or alternate translator was added.
- Stored dynamic state uses access keys/arguments. Display projections rebuild without changing record/input values, selection, sort state or grid history. Code syntax, API/enum/namespace/icon identities, brands, language autonyms, font demonstrations and actual fixture/user data remain literal.
- Field passes optional MessageFormatter to production ValidationMessages. Gallery annotations store keys; display lookup updates already stored messages when language changes. Validation rules, field identities and EditContext originals remain intact. An omitted formatter retains ordinary host text; explicit Error passes through a supplied formatter. Language and context subscriptions are released.

## Verification and evidence

Release Blazor solution and Gallery compilation with TreatWarningsAsErrors passed with zero warnings/errors. Tests use --artifacts-path artifacts/gallery-body-validation, isolating intermediate files and outputs. All six original Gallery/Framework/Design asset manifests/bundles retain their pre-test SHA-256 values. Original start.bat/launch settings, CSS source, font/icon assets, theme configuration and dependency settings were not changed by this repair.

- build/Test-CultureCatalogs.ps1: 21,217 checks; duplicate/missing/empty keys, exactly three languages, placeholders, paragraph breaks and access-key caller coverage.
- Tests.Gallery.Flourish.Blazor: 7,234 actual render checks across 27 pages and all 80 guide/sample entries in en-US/zh-CN/pt-BR/en-US, real API rows, unresolved-token guards, disposal and already stored annotation errors.
- Tests.Flourish.Blazor: 373/373 component and inventory checks; 80 components, 697 API rows and 311 documented defaults.
- Tests.Flourish.Extensions.Culture.Blazor: 12/12 scope, catalog, fallback, formatting and lifetime checks.
- build/Test-GalleryCulture.ps1: 556 real loopback HTTP checks, including semantic H2/body text and valid identity/gzip/browser-negotiated responses for Framework/Design/scoped CSS, MaterialSymbolsOutlined font and Blazor startup script. Decoded payload hashes agree across encodings.
- build/Test-GalleryNavigation.ps1: 227 actual loopback navigation checks.

Evidence logs: artifacts/gallery-body-build.log, gallery-body-catalogs.log, gallery-body-render.log, gallery-body-http.log and gallery-body-navigation.log; resource baseline: artifacts/localization-normal-assets-before.json. Test-only server used port 5273 and is stopped after verification. No Computer Use or browser automation was used.

## Manual regression checks and limits

Start Gallery through the existing start.bat and switch English, Chinese and Brazilian Portuguese on Framework, Appearance, examples, input/selection and data controls. Review long Portuguese H2 wrapping, paragraph separation and code-background placement. Show a validation error before switching; the existing message should translate while the input remains. Retain a query, selected option, table sort/page and edited/undoable grid through switching. Review live chart/menu actions and confirm all icons/styles render. A second browser profile should retain its own language. Existing reload negotiation remains cookie then Accept-Language; this repair does not add preference persistence.

Automated HtmlRenderer/HTTP checks verify content and contracts, not physical browser geometry or end-to-end pointer interactions. Those remain user-run acceptance. Business data and code examples intentionally need not match the selected UI language.

ReleaseSettings.ConsoleTests now includes Tests.Gallery.Flourish.Blazor, and CheckScripts includes Test-CultureCatalogs.ps1. Test-Release executes these existing console/PowerShell gates without changing launch or asset configuration.

Final render verification includes 124 actual post-event checks for SplitButton export count/format/selection, SearchBox input/query/matches, EditingGrid stored errors/rejected save/undo/redo history and MultiSelectBox stable selection/new user labels. All state and component identities survive language changes. Total Gallery checks: 7,234.
