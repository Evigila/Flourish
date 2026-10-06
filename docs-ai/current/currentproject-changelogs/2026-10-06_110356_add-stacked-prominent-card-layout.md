# Stacked prominent Card layout

The personalized-license Pricing scene now supports vertically arranged copy and actions through Card.Stacked. This is an option on the existing production Card, not another callout renderer or a consumer stylesheet. Prominent remains opt-in; Stacked defaults to false and does not alter ordinary cards.

## Implementation

Stacked prominent cards render paragraph copy first and actual action controls afterward, so source, visual and keyboard order agree. Framework CSS supplies the column layout and removes the horizontal action/copy flex bases. Existing Design role colors, H1 paragraph typography, natural height growth and action semantics remain unchanged. Empty action slots are omitted. Horizontal prominent cards retain their original action-before-copy order and responsive layout.

Gallery's Card sample exposes the option and /examples/display/pricing uses it for personalized support only. ParameterMeaning and ComponentCatalog describe the default and intended behavior. Colligere consumes this parameter directly and retains the Elevated disabled Solicitar action, original support copy and unchanged free-start callout.

## Verification

artifacts/stacked-card-release-2026-10-06.log records successful Release preparation: 351/351 Blazor checks, 90 exported components, 726 parameter rows, 320 documented defaults, 63 configured Node checks and the CSS gates. Six candidate packages and 99 isolated NuGet-consumer checks passed. Consumer evidence is artifacts/package-consumers/eee90dd24f314d4ea3ebdb6e9615978d/. No public upload, dependency upgrade, commit or push occurred.

The new executable Card regression verifies actual SSR order, a single real Elevated action, encoded copy, no heading generation, missing actions, long text and ordinary-card isolation. Framework-only layout rules retain vertical flow without fixed-height clipping or a new skin.

## Manual acceptance

Check Gallery Card with Prominent and Stacked independently enabled/disabled, long copy, missing actions, narrow/wide viewports and both themes. Verify /examples/display/pricing personalized support has text above its action while free-start retains its horizontal wide-screen arrangement. Check keyboard focus and natural wrapping. Browser and pixel-level acceptance remains user-run; no Computer Use was performed.

AI-owned technical guidance and this append-only record were maintained with the write-page workflow. Existing work, human documentation and prior change records were preserved.
