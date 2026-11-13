# Generic Blazor assets and page-loading repair

## Scope and supersession

The user clarified that Flourish is a universal framework and optional design library. It must not know a consuming application's pages, routes, business DTOs, CSS-isolation IDs or scripts. This supersedes the consumer compatibility/route opt-in proposal in [the original diagnosis](2026-10-03_blazor-page-load-resource-fanout.md). The original measurements remain historical evidence.

This repair changes reusable UI ownership, asset delivery, component initialization and Gallery host logging. Authentication, authorization, session rules, business persistence and remote queries remain host responsibilities. No external dependency, font, runtime service or middleware was added.

## Evidence and cause

The supplied single-page-open log contained 110 CSS responses and 594 Information records. Modular runtime imports caused a multi-stage stylesheet discovery graph and included consumer page styles. Request Information logging amplified the development workload. The document itself returned in 223.8245 ms; CSS median/P95/max were 529.5313/930.0849/1282.6880 ms. Parallel duration sums are not elapsed browser loading time. Debugger/log-output amplification is a plausible contributor, not a separately measured percentage.

No repeated navigation or business request loop was present in that capture. Unused sheet initialization and repeated table measurement were secondary component costs; they do not by themselves explain the root capture.

## Repair

- Business pages, real isolated page/layout CSS, role palette mappings and four application scripts return to the consuming host. Library resources contain generic controls, surfaces, grids, foundations and tokens only. No compatibility entrypoint remains.
- Data contracts and roots use neutral names: Primitives.DataTable, DataColumn, DataFilter, DataSorter, DataPagination, TablePurpose and CardField.Title/Identifier/Metric. FormSurface replaces business-named surface components; a host can supply its own CssClass. Gallery demonstrates generic composition at /patterns.
- An installed-SDK-only MSBuild task flattens local CSS imports in their original cascade order and rebases local URLs. Missing files, cycles, escapes and unsupported imports fail the build. Modular source CSS is not exposed as development, publish or package assets.
- Framework and Design each publish one primary CSS asset. Framework has no Design dependency. Functional layout/accessibility CSS accompanies Framework; native control presentation remains usable without Design.
- Generated assets participate in SDK fingerprints, endpoint caching and precompression. Both stable and fingerprint aliases retain gzip/Brotli in independent NuGet consumers. Only bundle compression alternatives are added to each package; internal CSS sources are excluded.
- Gallery and the native fixture filter routine ASP.NET request Information events at the host, retain startup Information plus Warning/Error, short-circuit static endpoints, and load asset-map URLs. Development does not redirect HTTP without a configured HTTPS endpoint. Library registration does not alter a third party's logging policy.
- Never-opened sheets do not mount child forms or import browser behavior. First open initializes them; later closes retain child contents to preserve drafts.
- Table measurement caches widths and invalidates for relevant content, columns, visibility, fonts and viewport changes. Manual widths and Home restoration remain supported. Observer/listener disposal is retained.

## Actual package and HTTP verification

Five existing 1.1.0 packages were rebuilt locally; no public feed was published. Fresh offline PackageReference consumers independently restored, built and published Framework-only and Framework-plus-Design modes without sibling ProjectReferences.

| Asset | Original bytes | gzip wire bytes | Brotli wire bytes |
| --- | ---: | ---: | ---: |
| Framework CSS | 65,625 | 10,491 | 9,007 |
| Design CSS | 153,862 | 23,807 | 19,842 |
| Styled total | 219,487 | 34,298 | 28,849 |

All eight stable/fingerprint × gzip/Brotli requests returned the requested Content-Encoding, smaller wire data and identical decompressed CSS. Matching ETags returned 304; fingerprint endpoints advertised max-age=31536000, immutable. No runtime @import remained. Framework package contains one CSS, two CSS compression alternatives and 13 generic JS modules; Design contains one CSS and its two alternatives.

Development HTTP checks produced two CSS links for Gallery and one for the native host. Isolated cold document requests were 251.06 ms and 148.37 ms respectively. These are HTTP measurements, not browser paint/interactivity measurements, and are not directly equivalent to the user's VS debugger session. Development hot reload cache/compression defaults were preserved.

## Regression verification

- Blazor child solution Debug and Release: zero warnings/errors.
- Console/renderer suite: 49/49 passed, including two actual sheet lifetime checks.
- CSS parser: 21 checks; independent SDK/project/package/incremental/publish and real HTTP compression fixture: 62 checks.
- Browser-module suites: table/data 12, controls DOM 15, interaction origin 5; existing host column, portal, scroll and spreadsheet regressions passed.
- Consuming Web suite: 1595 passed, zero failed, 85 environment-gated skips, total 1680.
- Independent native/styled package builds: zero warnings/errors.
- Source/package ownership scan and git diff --check passed.

UI class renaming temporarily touched twelve host rate-limit policy references. All were restored to the original security identifiers before final verification. The three endpoint files and policy expectation test have no changes from this repair; HTTP assertions were not relaxed.

## Limitations and manual regression

No Computer Use was used for this repair. The user's running VS service on 5188 was preserved; stop it, rebuild and restart before measuring. Browser cold/warm elapsed time and focus/hover/press visuals still need the [manual checklist](../blazor-manual-tests.md). Verify two library CSS requests, quiet ordinary request logs, first/reopened sheet draft/focus behavior, table automatic/manual sizing, native operation without Design, host-specific palettes/layouts and unchanged login/registration behavior.

This intentionally removes business-named extracted APIs instead of adding compatibility aliases. Consumers must use neutral APIs and keep their business adapters/styles. The repository's unrelated uncommitted work is preserved. No Git commit is included.

## Evidence

External task artifacts: C:/Users/Evigila/.codex/visualizations/2026/10/03/01a0ff44-995d-7090-af88-8835158b1571/

- generic-validation/published-http-verification-final.json
- generic-validation/http-verification.json
- generic-validation/web/generic-boundary-web-verified.trx
- generic-validation/packages/ and package-consumers/{Native,Styled}/
- css-bundle-checks/css-assets-e1d674d743534d96bad528be792ccc2f/
- ui-css-generic-backup/manifest.json and ui-css-generic-report.json


## Subsequent development transport correction

The user then reported a dynamic module fetch failure. The prior build-time Brotli addition produced HTTP 200/br responses containing gzip bytes under the installed Development reload handler. That package/compression implementation is superseded by [the development module repair](2026-10-03_development-module-compression.md): keep normal build gzip, preserve the CSS fingerprint expression in package props, and let Publish generate Brotli. The new regression decodes actual Development CSS/JS and passes 194 checks; actual Gallery module verification passes 56 responses. Historical Production measurements above remain valid evidence, while current packages carry one gzip alternative per CSS and generate Brotli only on publish.
