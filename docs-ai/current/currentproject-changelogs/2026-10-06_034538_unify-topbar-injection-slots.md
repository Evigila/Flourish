# Unify top bar injection slots and offer labels

Local timestamp: 2026-10-06 03:45:38, America/Sao_Paulo.

## Implementation

Replace the group-only ShellHeader patch with one chrome-scoped f-topbar-slot contract. ShellHeader Services/LeadingActions/EndActions and ApplicationShell start/center/end tracks all use it. Direct controls, native form children and InlineActions members override document-flow alignment and block margins at stronger specificity while preserving variants, logos, identities, hidden transport fields and menu geometry. Gallery demonstrates authentication-state action changes; regressions cover all current Button variants and injection forms.

OfferCard's plan title becomes its production label rather than a forced h3. Update both appearance/behavior selectors and retain list semantics, safe text, target IDs, active rotation and the hidden decorative duplicate. Consumers can compose a single-title Pricing page without a host hiding rule or an alternate renderer. Update the active API guide.

## Verification and boundaries

Complete Release gate passes with 333 Blazor checks, current JS/CSS gates, six local package candidates and 95 isolated NuGet-consumer checks. Colligere's current package, build and UI regressions pass; its two known nonvisual order-contract failures remain. See [the bug report and manual acceptance checks](../bugfix-reports/2026-10-06_topbar-injection-alignment.md).

No new public API family, compatibility path, external dependency, publishing, commit/push, database operation or user-process termination was introduced. Physical pixel alignment remains user-tested after loading the current library assets.
