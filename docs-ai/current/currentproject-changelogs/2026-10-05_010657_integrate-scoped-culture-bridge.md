# Integrate scoped Culture bridge

Recorded at 2026-10-05T01:06:57-03:00 (America/Sao_Paulo).

## Request and result

The user authorized local Flourish.Blazor integration testing and analysis of the existing Culture middle layer. The inspected WPF bridge uses process-wide static state and Dispatcher/Singleton lifetimes. A parallel optional Extension.Culture.Blazor now implements the neutral Abstract ITextProvider with scoped Essential.Culture.Blazor; the WPF implementation is preserved.

## Changes

- Added TextReference/ITextProvider contracts, explicit lambda-builder overloads and immutable outer label-reference metadata. Existing Shared DTOs and ordinary strings remain intact, avoiding a dependency cycle and implicit token guessing.
- Added Framework literal fallback and renderer-safe text refresh/disposal. Shell/menu/navigation/default ARIA, heading back accessibility, Dialog close, DataTable defaults/format selection and DisplayBoard copy captions use the provider. Explicit text/culture overrides remain authoritative.
- Added independent embedded Framework/Gallery catalogs, generated Gallery keys, request-language initialization, a standard top-bar picker, bilingual Home and /framework/localization. No persistence endpoint, business behavior or desktop migration was added.
- Added seven console regression checks and build/Test-CultureIntegration.cjs for repeatable installed headless-browser verification. Replaced the active proposed Culture guide with the actual integration and remaining-rollout status; the earlier plan is superseded and traceable in Git/history.
- Extension added its optional package, twelve checks, package/CI entries and guide. Only stale WPF sibling project paths were corrected. Existing desktop dependency-version defaults were not upgraded.

## Verification

- Full Flourish.slnx Release/TreatWarningsAsErrors build: 0 warnings, 0 errors.
- Flourish Blazor: 118/118 checks, including immutable configuration, duplicate record/catalog identity, explicit old-default overrides, background refresh, disposal and two separate users.
- Extension bridge: 12/12 checks. Existing WPF extension: 5/5 tests. Extension solution build and bridge pack passed.
- Published Gallery headless Edge: English/Chinese SSR, two independent circuits, home/PageTitle/chrome refresh, Brazilian formatting, table captions, dialog/copy text, third-navigation state, existing menu command routing and reload restoration. No browser exceptions or failed framework assets.
- Framework-only Native SSR: HTTP 200 with fallback text and no Design/Culture markup. Full Native interaction remains a manual acceptance boundary.
- Framework package has only Abstract/Shared dependencies; extension package has only Abstract/Culture.Blazor. No loose translation JSON in Gallery publish output. No Git commit, push or NuGet publication was performed.

## Resolved validation issues and limits

ParameterView uses its explicit enumerator rather than LINQ. The new demo's required Field.Id was supplied. Restore does not consistently apply ProjectReference.AdditionalProperties, so the new bridge/test independently detect available sibling source only when local flags are unset; explicit package-mode flags still win. Browser checks wait for asynchronous routing/disclosure rendering and account for two existing table pagers rather than treating immediate old DOM as product defects.

Only the bilingual pilot and stated standard defaults are translated. Other Gallery guides and primitive defaults, host cookie/preference persistence, client/WASM validation and public package availability remain separate work. The Extension directory currently has no .git metadata; its new files are saved locally, not silently initialized as a repository. Earlier uncommitted Flourish branding changes and prior Essential.Culture work were preserved.
