# Adopt Essential Culture 1.4.0 modules

Local date/time: 2026-10-10 00:02:26, America/Sao_Paulo. The request and implementation began on 2026-10-09 and verification crossed local midnight.

The user authorized updating Flourish.Extensions.Culture.Blazor and Culture.WPF to the new Essential release. Public package verification confirmed 1.4.0; the shared dependency pin and release dependency checks now use that version, with no Essential source-project substitution. Flourish's own package identities/VersionPrefix stay unchanged.

Blazor adds loaded-catalog registration and AddCatalogFiles over Essential.FromFiles, including retained-language policy and startup compatibility checks. WPF accepts loaded catalogs with isolated selection, delegates selectable cultures/change events to Essential and removes its independent library-caption parser. Existing persistence, formatting defaults and caller-owned provider lifetime remain supported.

Gallery translations now occupy seven functional multilingual modules, preserving original keys and strings except the approved introduction update. Generated keys/manifest drive both deployment and runtime loading. Updated Localization examples, module-aware validation/HTTP/package consumers and the concise 1.1.4-preview note. Fixed an embedded extension resource also being copied into host module paths. Active guides describe the new contracts while prior dated records remain untouched.

Verification: zero-warning/error builds and Gallery publish; Blazor bridge 26; Framework 436/436; Gallery 8,291; WPF bridge 11/11/native139/139; Node4/4; catalogs24,202/ChangeLog97; corrected publish HTTP8+583 and navigation240; six Blazor package contracts and143 isolated consumer checks; independent WPF consumer51 build+51 publish. Seven published Gallery modules match source, with no extra library catalog. git diff --check passes.

See [the report](../bugfix-reports/2026-10-10_essential-modules-bridge.md) for API details, initial compiler/deployment findings, artifact evidence and manual checks. No Computer Use, Git commit, tag or publication occurred. Candidate packages and fixtures remain ignored local verification artifacts.
