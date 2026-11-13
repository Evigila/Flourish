# Notice semantics and standard layout controls

## Symptoms and cause

Primitives.StatusNotice retained legacy Information and Warning backgrounds mapped to Primary. Notice already used Surface with Info text for Information and Warning background with Warning ink for Warning. Separate severity rules allowed the two controls to diverge; legacy Subtle aliases also disagreed between stylesheets.

Four Gallery shell/layout samples rendered their own inputs, submit controls or navigation actions instead of composing library controls. AccessSurface's access-form class additionally repainted descendants and increased label weight, so replacing its elements alone would still bypass standard appearance. The Foundations theme chooser and palette actions used FormActions, applying form-action geometry to ordinary theme controls. Icons placed a small set of source links at the end rather than introducing its source library first.

## Correction

Design controls.css supplies one shared severity rule for each Notice / StatusNotice pair. Both consume the same foreground/background variables and application palette. Information uses Surface / Info text, Warning uses Warning background / Warning ink, Success uses Primary / Surface, Error uses Danger / Surface and Subtle uses Display board / Muted. layout.css no longer resets the shared severity variables. Framework markup, severity enums, Role and Announce behavior remain unchanged.

AccessSurface uses FormLayout, Field, TextBox and Button inside its native form, without access-form repainting. NavigationGuard and InteractionBoundary use standard labeled TextBox controls; NavigationGuard's test link uses Underline Button. NavigationSurface uses a real PrimaryNavigationItem and standard secondary Button actions while keeping scene navigation local. PageContent/PageContents keep the semantic li/a children required by their public API.

The theme chooser uses an explicit three-column, one-row Rectangle UniformGrid with UniformGridButton. Palette actions use ordinary Filled/Outlined Button. Icons starts with verified official repository, Apache 2.0 license, browser and usage-documentation links through Underline Button. No font, package or dependency changed.

## Evidence and regression

Eight-project Blazor build: zero warnings/errors. Existing Blazor checks: 105/105; catalog: 77 guides, 546 parameter rows and 253 defaults. Palette checks: 10/10, including shared severity rules and absence of later CSS overrides. Controls DOM checks: 38/38. All 77 guide routes and the Framework-only host pass HTTP checks.

Isolated installed headless Edge compares all five rendered Notice/StatusNotice colors in Light/Dark and exercises severity callbacks and alert/status roles. It verifies the three-cell theme row, System/Light/Dark changes, apply/reset callbacks, 320px containment, four source links and icon search, native required/email rejection and valid submission, boundary lock/unlock/save, local navigation without URL changes and NavigationGuard cancel/save/leave behavior. No browser exceptions. Temporary servers use separate dynamic ports and stop only their own process.

## Limits and manual acceptance

Headless Edge does not establish screen-reader behavior, native refresh prompts, other browser engines or zoom acceptance. Follow blazor-manual-tests.md for those checks and official external-link opening. No Computer Use or Colligere changes were made. Public library signatures and framework/design dependency direction remain intact.
