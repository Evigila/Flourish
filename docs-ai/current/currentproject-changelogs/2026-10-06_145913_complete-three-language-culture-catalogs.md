# Complete the Blazor three-language Culture catalogs

## Request and preceding commit

The user requested a commit of the accumulated Blazor control/Gallery repairs, followed by verification of the single-Culture.json/access-key workflow and maintenance of en-US, zh-CN and pt-BR translations. Commit f05c65a (Refactor Blazor controls and fix Gallery scenarios) was created before this localization follow-up. This record describes the subsequent working-tree changes; no second commit or public publication is implied.

## Findings and changes

- Gallery already used one Localization/Culture.json as both the Essential generator AdditionalFile and the embedded application catalog. Its 144 existing keys had English/Chinese translations but no Brazilian Portuguese translations.
- Added Brazilian Portuguese to all existing Gallery keys while preserving their English/Chinese values. Added 11 three-language access-method keys and replaced literal labels/actions/help in NavigationChoicesSample with generated key lookups. The current Gallery catalog has 155 complete three-language keys.
- Framework's 121 control keys already had all three translations. Renamed its physical Localization/Texts.json to Localization/Culture.json and updated the project resource paths. Contents and the embedded logical name Flourish.Blazor.Texts.json remain identical; this does not add an Essential dependency or another generator input.
- Enabled pt-BR in Gallery's supported session/UI languages and the existing language picker, and corrected the localization registration example. Formatting already supported the three languages. The default remains zh-CN.
- Added build/Test-CultureCatalogs.ps1 for catalog invariants and build/Test-GalleryCulture.ps1 for actual loopback HTTP localization checks. Updated active Web/bridge guides and the explanatory architecture tree.

The single-source rule applies per owning project. Gallery and Framework register different catalog identities. Generated access keys remain stable tokens; scoped render-time lookup selects translations. Framework deliberately keeps provider-neutral TextReference tokens rather than depending on Essential's generator. The JSON/key model is consistent with Essential desktop targets; the server's request/circuit scope and renderer lifecycle differ from desktop process/window localization.

The bridge neither automatically translates literal Razor text nor registers catalogs or persists user language preferences. Live language changes remain circuit-local; reload follows the existing cookie/Accept-Language initialization. Technical guides, API metadata and sample literals outside the maintained catalogs remain an explicit later key-extraction boundary. Desktop/Core culture files were not changed. Business values, native navigation URLs, query keys, access behavior and formatting transport were preserved.

## Verification

- Isolated Gallery Release build: zero warnings and zero errors.
- Catalog integrity: 3,036 checks passed for 155 Gallery and 121 Framework keys, including exact language coverage, duplicates, nonempty text, format placeholders and line breaks.
- Actual three-language Gallery HTTP verification: 43 checks passed for language negotiation, generated application keys, choices/method actions, Framework defaults, number/date formatting, request isolation and cookie precedence.
- Actual Gallery navigation regression: 191 checks passed.
- Blazor component regression: 356/356 passed.
- Blazor Culture bridge regression: 12/12 passed.
- Git whitespace validation passed. No new external dependency, Computer Use test, preference endpoint, package publication or Git push was performed.

Evidence: artifacts/culture-three-build.log, culture-three-catalogs.log, culture-three-http.log, culture-three-navigation.log, culture-three-components.log and culture-three-bridge.log. The temporary verification server used 127.0.0.1:5268; the user's running 5188 instance was not replaced. Separate launcher and concurrent SplitButton/layout edits were preserved and are not attributed to this localization work.

## User-run acceptance

1. Restart Gallery so the current sources and embedded catalogs load, then switch en-US, zh-CN and pt-BR on the home and language/localization pages.
2. Visit /examples/display/access-methods, switch methods repeatedly and change language; check translated labels/actions and retained selection.
3. Keep selected controls/form values while changing language and format culture; check Brazilian number/date formatting independently of Chinese UI text.
4. Open a second independent browser profile, change only one circuit's language and confirm the other remains unchanged. Reload and confirm initialization follows its cookie/header rather than assuming live selection was persisted.

HTTP/source/component checks do not replace interactive visual acceptance. The complete declared catalogs do not establish that every hardcoded Gallery technical paragraph is translated.
