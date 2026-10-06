# Gallery navigation choice literal key

Local timestamp: 2026-10-06 14:44:32, America/Sao_Paulo.

## Symptoms and confirmed scope

The user reported no response on the first navigation attempt and an ActiveKey ArgumentException on a subsequent attempt. They clarified the route as /examples/display/access-methods and the sidebar item as access-method selection, not saved-account selection.

An independent pre-fix Gallery instance returned HTTP 500 for the default access-method page and choice=account, with the same NavigationChoices.OnParametersSet exception. /examples/display/accounts returned HTTP 200 and did not contain NavigationChoices. Evidence: artifacts/gallery-navigation-before.json. No browser automation or physical click timing was used; server rendering failure is confirmed, but the precise first/second-click browser sequence was not independently measured.

## Cause

NavigationChoicesSample supplied ActiveKey="ActiveKey". Razor treats a plain string-valued component attribute as a literal, so the control received the word ActiveKey instead of the getter's organization/account/pass value. No choice has that key. The control correctly rejected it.

The API example similarly used ActiveKey="ActiveMethod". Existing library tests only rendered the control with dictionary parameters, so they did not exercise the compiled Gallery sample or catch its Razor binding mistake.

A bounded audit found nine more runtime mistakes with the same cause: remote DataTable column/query, AccessBrand and ShellHeader logo paths, an AccessBrand title ID, and two EditForm names. API examples for DataSearch query/column, progress status and CodeBlock text also passed property names as literals.

## Fix and boundaries

The NavigationChoices sample now supplies ActiveKey="@ActiveKey"; its API example uses @ActiveMethod. Existing query normalization selects an available fixture key and defaults missing, unknown or ambiguous query values to organization. The library's strict key validation is unchanged.

Ten runtime string bindings across six Gallery samples now use explicit Razor expressions. Six dynamic string attributes in the copied API examples are corrected as well. No library contract, native GET destination, authentication protocol, account data, dependency or business rule changed. Samples remain fictional.

build/Test-GalleryNavigation.ps1 runs against an explicitly supplied loopback Gallery instance. It checks actual compiled Razor endpoint responses and markup: current native links, retained panels, query variants/fallbacks, repeated requests, both guide instances, saved-account/login routes, real logo URLs, unique form-handler values and remote search values. It does not substitute a fake NavigationChoices renderer.

## Evidence and limitations

- Isolated Gallery Release build: zero warnings/errors, artifacts/gallery-navigation-build.log.
- Actual Gallery HTTP regression: 191 checks, artifacts/gallery-navigation-http-final.log.
- Existing Blazor C# regressions: 356/356, artifacts/gallery-navigation-components.log. These retain rejection of unknown/disabled ActiveKey values and unsafe destinations.
- Initial HTTP regression evidence remains in gallery-navigation-http.log; its logo assertion assumed an unencoded plus sign in SVG data URLs. Final assertions decode HTML attribute encoding and use the actual shared search marker rather than an invented class.
- Diff whitespace check passes. No component count/default changes occurred.
- Only temporary independently started instances were stopped after verification; the user's 5188 instance was retained. No Computer Use, public publication, Git commit/push, dependency or database change occurred.

Server/SSR checks do not certify interactive circuit clicks or visual geometry. Restart the user's Gallery before manual acceptance so it loads the corrected sample assembly.

## Manual regression

1. Restart Gallery, enter /examples, and select access-method selection once. It must open normally without a blank/stale page or server exception.
2. Follow organization/account/pass links, refresh each URL, use back/forward and return to the page repeatedly. Exactly one choice/panel is active.
3. Check default, unknown and duplicate choice query values; they show the organization fixture. Open NavigationChoices' detail guide and confirm both preview and scene render.
4. Confirm saved-account/login entries still work. Check the corrected logo samples, form examples and remote DataTable search for the intended values.
