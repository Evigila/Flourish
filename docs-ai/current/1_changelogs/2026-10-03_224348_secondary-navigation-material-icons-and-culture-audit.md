# Gallery secondary navigation, Material Icons and Culture Web audit

The user requested secondary navigation for every Gallery category except Home, an external open-source icon font with Google Material Design preferred, and a source assessment of Essential.Culture before future Web integration. Implementation changes are limited to Flourish. Essential.Culture and Colligere were not modified.

## Changes

- Home remains primary-only. Framework has six secondary topics, Controls eight categories, Foundations six topics and Examples eight destinations. Each page renders its selected content instead of one large category page. The 73-entry component catalog and interactive demonstrations remain available across Controls categories.
- Multi-route pages clear optional route parameters before applying the next route. Returning from a child to its category landing route does not retain the previous topic. /appearance redirects to /foundations. Gallery error/not-found links now use Home terminology.
- Navigation configuration allows a primary landing route to appear once among that primary's own children. Other duplicate-route validation remains intact. Secondary links now render their configured icons and preserve selected-page semantics.
- Installed the official Material Icons Outlined font and name map from Google revision 737e3324305806514d7909874fa1818ae1808232. Framework owns the functional font/CSS and embedded map; Design only owns appearance/size roles. There is no runtime Google CDN request or additional PackageReference.
- IconCatalog exposes 2195 official names and checks compatibility aliases. Icon renders mapped codepoints rather than visible ligature-name text. AppIcon and Glyph use the same rendering path while preserving their existing API roles. The Foundations icon page supports search and incremental display.
- Preserved the upstream Apache-2.0 license and pinned file hashes/source metadata. Framework's NuGet license expression is MIT AND Apache-2.0 to cover code and bundled assets.
- Recorded the read-only Culture audit in culture-web-readiness.md. Core parsing, validation, fallback and key generation are reusable, but the public static Localizer shares one server-process language. Core needs an instance/catalog loading API and Web needs request/circuit/client lifetimes, rerender/disposal, persistence and multi-catalog distribution before safe multi-user integration.
- Essential.Culture.Wpf is already used by Gallery.WPF; this does not establish Web support. Flourish.Core's separate localization contract must be mapped or preserved during any future integration. No Culture dependency or proposed adapter API was implemented in this task.
- Refreshed the directory map in the user's current canonical file, currentproject-architecture.md. The user moved the previous architecture.md contents there during this task; architecture.md and its former pointer were not recreated. AGENTS reading order now follows the canonical file. The earlier append-only record retains the historical layout as it existed then.
- Updated active implementation notes and the manual acceptance checklist. Existing user skill deletions remain untouched; earlier uncommitted category/README cleanup remains in the worktree.

## Verification

- Root Flourish.slnx build passed with zero warnings/errors. Final Gallery rebuild after the icon-page parameter repair also passed with zero warnings/errors.
- Blazor console checks: 53/53 passed, including primary/secondary landing selection, duplicate validation, embedded icon names, aliases and decorative rendering semantics.
- CSS parser checks: 21 passed. Development/Production SDK/static-asset integration checks: 194 passed. No JS implementation changed, so unchanged browser-module suites were not rerun.
- All 29 distinct Program-configured navigation addresses returned HTTP 200. Home omitted the secondary rail; every other route rendered the rail and selected its exact configured child. Framework and Foundations topic samples rendered only their own sections. The legacy /appearance route resolved to Foundations.
- The served font returned font/otf, 339168 decoded bytes and SHA-256 b63fa9edd75e3c20328e04ad31dcc38ce76411f3f9ea1a1ff87f49e5ba874b05. The public CSS preserved the normalized relative font URL without imports; the icon directory reported all 2195 names.
- The first HTTP pass found a Gallery-only error: the new icon page supplied Columns=3 to FactList, whose supported values are 1 and 2. It was corrected to two columns, rebuilt and successfully rechecked. No FactList library contract was expanded.
- Framework packed successfully. The inspected archive includes the font, LICENSE.txt and source.json, and has the MIT AND Apache-2.0 expression. NuGet's missing-README best-practice notice is expected after the user's authorized README removal; no README was recreated.
- Independent directory-map validation found 781 files, 126 directories and the root, with no missing/extra/duplicate nodes or empty explanations. docs/docs-ai and generated state retain the map's explicit omission rules.
- Git diff whitespace check passed. No Computer Use testing occurred. Temporary HTTP hosts were stopped; visual, keyboard and interactive route acceptance remains in blazor-manual-tests.md.
- Essential.Culture tests were inspected, not executed. Its 43 Fact tests cover desktop/core/generator behavior, with no Web integration coverage found. Server isolation and WASM readiness conclusions are source-derived and require new integration tests during implementation.

## Next implementation boundary

Recommended sequence: Essential.Culture Core instance/catalog API, then a Web/Blazor adapter, then Gallery verification and an optional Flourish runtime text bridge, then Colligere adoption. Retain Framework-only use without a required Culture dependency and avoid translating singleton startup labels once in Program.

No Git commit or public package publication was performed in this task.
