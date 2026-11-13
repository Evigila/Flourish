# Simplify access pages and remove nested surfaces

Local timestamp: 2026-10-06 13:01:01, America/Sao_Paulo.

## Cause and reference composition

Gallery's standalone login and account routes shared an emphasized AccessPanel, whose optional Design paint adds a gray rounded background. An outer AccessFormSurface wrapped the complete route content, and the login form contained a second AccessFormSurface. Design paints each form surface with its own border, background and radius, producing nested decorative panels.

The reference Colligere composition was verified in src/Colligere.Web/Components/Pages/Login.razor and Components/Layout/LoginLayout.razor. Its ordinary login uses the production Surface-tone dotted AccessSurface, a plain AccessPanel, LogoDisplayer above the form and exactly one AccessFormSurface inside the credential form. No Colligere files or authentication contracts were changed.

## Implementation

AccessExamples removes the emphasized panel and whole-page form-surface wrapper. The login form retains its single production AccessFormSurface; account-selection cards retain their record meaning and are no longer inside an extra decorative form surface. The language/theme controls use AccessActions directly without a redundant visible section heading. Login now has a concise localized title and simulate-sign-in label, no technical brand description and one short local-demo note. English and Chinese remember/result/connection/prefill labels and result feedback were shortened.

DisplayExamples' access overview also removes the emphasized gray layer. Its navigation-only content now uses AccessActions rather than a form surface. AccessPanelSample no longer offers the emphasized-plus-form-surface combination; its ordinary panel, one form surface and Wide option remain. AccessMethodExample uses the same concise localized demo note; NavigationChoicesSample retains one form surface per method while shortening explanations and action labels. AccessFormSurfaceSample, LogoDisplayerSample and AccessSurface catalog guidance were also condensed, preserving the sole-main and default-versus-local-identity distinctions.

Shared Gallery DisplayBoard staging remains appropriate for component documentation; it was not treated as an independent login-page wrapper. No library CSS/API or host skin was introduced. Field validation, selected demonstration outcomes, busy locking, password clearing, fixture operations, native navigation and local-only submission remain governed by the existing implementation.

## Verification

Final Gallery Release build succeeded in artifacts/access-login-verification with zero warnings and zero errors. Existing Blazor regressions passed 351/351, including access-example validation, accepted/rejected/unavailable states, remembered fixture semantics, cancellation and account actions. Evidence: artifacts/access-login-regressions.log. The changed-file whitespace check passes. The final additional edits after those regressions only shortened presentation strings; the final build confirms their Razor/catalog compilation.

These checks verify code and behavior, not browser geometry. The running Gallery was not stopped; restart it using the normal project workflow before visual acceptance.

## Manual acceptance

- Open the login through SplitButton and directly: the brand stays above one form surface, with no gray emphasized wrapper or nested form border.
- Compare the ordinary Colligere login structure in wide/narrow layouts and both themes. Check email/password fields, concise note, simulate-sign-in SplitButton and language/theme controls.
- Exercise required/invalid fields and all three demonstration outcomes; check busy/disabled behavior, retry, reset, password clearing and remember-email fixtures.
- Check account selection, remove/clear/restore, unavailable fixtures and returning to login; cards should not gain a whole-page decorative wrapper.
- Inspect access overview, access-method organization/account/pass choices, and AccessPanel/AccessFormSurface/LogoDisplayer/AccessSurface details. Explanations should be concise and form panels should not stack inside emphasized panels.
- Verify Chinese/English switching, SplitButton menu links, appearance controls and narrow-screen layouts.

No Computer Use, real authentication/network integration, dependency change, process termination, package publication, Git commit or push occurred. Existing refactor/layout changes, historical records and human documentation were preserved.