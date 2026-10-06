# Complete Gallery prose and metadata localization

## Result

Internal H2, body prose, component/API/scenario descriptions and all 80 example UIs now resolve the current en-US, zh-CN or pt-BR catalog while rendering. Gallery contains 1,483 complete keys and Framework 281. Static metadata carries keys/TextReference values, dynamic status retains keys/arguments, and explicit Chinese table overrides are removed. Data/API/code identities remain unchanged.

Field/ValidationMessages now expose optional MessageFormatter for display-only localization of stored annotation errors. Validation state and rules remain intact; scoped language and EditContext subscriptions are released. The plain-string documentation metadata contract is replaced without compatibility aliases. No CSS, font/icon, launch, dependency or Colligere changes belong to this repair.

## Validation

Isolated Release compilation passed with zero warnings/errors. Catalog checks: 21,217. Real Gallery render checks: 7,110, covering 27 pages and all 80 guides plus stored errors. Component checks: 373/373; Culture bridge: 12/12; HTTP localization/assets: 556; HTTP navigation: 227. Six normal resource manifests/bundles retained their SHA-256 baselines. Manual language/wrapping/state acceptance remains user-run.

See [the detailed cause, scope and regression report](../bugfix-reports/2026-10-06_gallery-prose-localization.md). The earlier open body-coverage audit is superseded by this implementation. No Git commit or publication is implied.
