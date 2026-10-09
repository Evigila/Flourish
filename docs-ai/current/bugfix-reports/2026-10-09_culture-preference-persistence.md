# Culture preference persistence and framework configuration

## Symptoms and cause

Gallery's LanguagePicker and Localization format selector called the scoped Essential service's SetCulture only. That service correctly isolates each request/circuit, but its memory does not survive a page refresh. Request negotiation already read Cookie then Accept-Language; the controls never wrote a culture cookie, so the next request restored headers/defaults instead of the chosen language and number/date format.

Core already defines a process-level file store using appsettings.Flourish.json. The current Blazor Framework and optional Culture bridge do not register that store. Reusing its singleton localization/preferences would conflate different Web visitors. This correction does not connect or change the separate desktop services.

## Correction and storage boundary

Added opt-in Framework AddFlourishPreferences, BrowserPreferenceOptions and scoped BrowserPreferences. SaveCultureAsync normalizes UI/format identifiers and serializes writes through one lazy library JS module. The module writes the standard .AspNetCore.Culture pair, preserving independent UI and format choices. It uses the application base path, a default 365-day lifetime, SameSite=Lax and Secure on HTTPS, and checks that the browser accepted the write. Invalid inputs and failed writes remain visible as exceptions; subsequent attempts can retry. No JS runs during prerendering, and disconnected module disposal is safe.

Both real Gallery controls await the saved pair before applying the same pair to the scoped localization service. The format handler captures its UI culture before awaiting, so another change during the write cannot make the rendered state differ from the saved state. The existing request middleware restores cookie values before SSR, preserving the initial language and formatting on refresh. Direct SetCulture remains intentionally memory-only; host integrations use the explicit save-then-apply sequence. The optional Culture text bridge remains storage-independent.

Gallery now explicitly loads appsettings.Flourish.json for framework defaults: UI/format language, supported cultures, preference lifetime and startup colors/theme/font. Environment and command-line values retain higher priority. appsettings.json is an empty object available to business/host configuration. Web personal choices live in each browser's cookie and never rewrite either shared server JSON file. This preserves visitor isolation; a server JSON file represents common defaults, while per-account synchronization would require host-owned identity and storage. Defaults are loaded at startup and require restart after editing the file.

Updated the executable registration/usage examples, three-language scope explanation and 1.1.4-preview ChangeLog. Release verification includes the Node regression and requires the new packaged JS asset. Added a loopback HTTP regression script; its default configuration expectations target Gallery's checked-in defaults.

## Other persistence candidates

| Preference | Current behavior | Recommendation |
| --- | --- | --- |
| UI language and number/date format | Scoped runtime selection | Completed together in this milestone. |
| System/Light/Dark theme | Scoped AppearanceService resets on refresh | Highest next priority; save the selection, including System itself, rather than a resolved OS color mode. |
| User-selected Primary/Accent palette | Apply colors changes scoped runtime values | Persist an explicit user override; Restore default should remove the override and use configured defaults. |
| Table/chart presentation | TablePreferences retains keyed sort only inside a scope; other display state remains control-owned | Opt-in persistence with stable keys for sort, column/series visibility, order, widths and page size. Do not claim these fields are already stored. |
| Navigation expansion | ApplicationShell holds instance-local state | Optional, with stable route identities and cleanup for changed navigation. |
| Font, motion, smooth scrolling, centered width | Host/component configuration or system behavior, without a common personal-settings UI | Keep defaults configurable; persist only explicit future user settings, not temporary sample toggles. |

Business drafts, searches, selected records, credentials and domain/session state are owned by the consuming application rather than generic framework preferences. These candidate implementations are not included in this language/format milestone.

## Verification and limits

- Blazor and Gallery Release builds: zero warnings and errors.
- Blazor executable suite: 443/443, including ten preference checks for retention, scope isolation, laziness, normalized pairs, base paths, concurrent ordering, retry/disposal and fresh request-cookie restoration.
- Gallery executable suite: 8,197 checks, including 464 post-event and 223 ChangeLog checks. Real language/format handlers verify save-before-apply, independent formats, failed writes and capture during an in-flight save. Test scopes now dispose asynchronously to release the browser writer correctly.
- Node cookie suite: 4/4 against an isolated simulated browser, covering flags, lifetime/path, separate browsers, latest choice, blocked storage and invalid arguments.
- Actual loopback HTTP suite: 8 checks for configured defaults, header selection, saved UI/format precedence on first/repeated requests, independent visitors, invalid-cookie fallback and usable library JS delivery. Sandbox network/key-directory restrictions prevented the first attempt; the same built application and script passed under normal local permissions. The temporary hidden host was closed afterward; no application security/runtime configuration was changed for testing.
- Culture catalogs: 22,071 checks. ChangeLog: 88 checks. Release test/asset registrations and diff whitespace passed. The pre-existing browser integration script now expects persistence; its browser run remains manual and was not executed here.

No Computer Use, dependency update, package version change, commit, release tag or publication occurred. Cookie retention depends on browser policy, private browsing and user clearing site data. The scope is one browser/application, not account synchronization across devices.

## Manual regression checklist

1. Select each supported language from the top bar. Refresh and close/reopen the browser; verify the heading, navigation, page title and picker restore the saved language.
2. On Localization, choose Chinese UI with Brazilian number/date formatting. Refresh directly on that URL; both selectors and numeric/date examples should retain their independent choices on the initial frame.
3. Use a second browser profile with another language. Changing one profile must not alter the other. Clear the culture cookie and confirm normal browser-language/default negotiation resumes.
4. Change languages repeatedly during navigation and formatting changes. The final visible UI/format pair should match the pair after refreshing. Block cookies and verify a failed save does not silently apply an unsaved selection.
5. Change framework defaults in appsettings.Flourish.json, restart Gallery and test with the preference cookie cleared. Confirm the defaults apply and appsettings.json remains unchanged. Do not expect current theme/palette changes to persist until their separate follow-up is implemented.

The design follows Microsoft's [request-culture cookie contract](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/select-language-culture?view=aspnetcore-10.0#cookierequestcultureprovider) and [interactive HttpContext limitations](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/httpcontext?view=aspnetcore-10.0). Browser writes avoid modifying response headers after interactive rendering starts.

## Same-task UI failure follow-up

Final review identified that service-level error propagation alone would let a failed browser write terminate a real Interactive Server event circuit. The controls now catch JS/disconnection/cancellation failures, preserve the previous pair and show a translated existing Dialog with Notice in LanguagePicker, or an inline Notice on the format page. Successful retries clear the error. The service itself continues propagating failures to its caller; it does not silently claim persistence.

The real control regressions no longer catch the write exception outside the handlers. They verify dialog/warning markup, old-value retention, continued interaction, successful retry and subscription release. Final Gallery Release build passed with zero warnings/errors, and the executable suite passed 8,202 checks, including 469 post-event and 223 ChangeLog checks. Updated three-language error messages bring final catalog validation to 22,096; ChangeLog remains 88 checks. Blazor 443/443, Node 4/4 and the eight protocol/asset HTTP checks remain the verified storage baseline. Review found no further actionable issues. No Computer Use or browser visual run was performed.
