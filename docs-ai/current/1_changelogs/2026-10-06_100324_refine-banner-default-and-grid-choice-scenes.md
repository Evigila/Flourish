# Refine banner defaults and grid choice scenes

Local timestamp: 2026-10-06 10:03:24, America/Sao_Paulo.

## Implementation

PresentationBand's single default minimum is now 800px instead of 720px. PresentationHero and AccessSurface inherit it; CSS fallbacks, FullHeight, Gallery parameter/usage text and real rendering/CSS regressions were updated together. Explicit custom minima, natural content growth and opt-in dots remain intact. Desktop window heights and chart coordinate values are unrelated and unchanged.

Rectangular UniformGrid previously shrank with fit-content and a column cap derived from twice MaxCellHeight. It now fills container width and distributes equal flexible columns; the twice-height value is only the automatic-column wrapping threshold. Explicit Columns, Rows-only flow and NarrowColumns use the same shape-aware CSS. Square retains fit-content, aspect 1:1 and MaxCellSize width/height limits. Centered positions rather than resizes the grid. Rectangle height/content overflow behavior and standard paint remain unchanged; no new parameter or compatibility renderer exists.

UniformGridButton's production usage guidance now includes forms and finite multi-step choices. A normal pure-choice step has only grid/buttons and their text, without a separate heading, explanation or dropdown. Actual forms, required business confirmation and error/status feedback remain separate responsibilities. Gallery /examples/display/wizard exercises localized fictional purpose/plan choices, back/restart and disabled alternatives using existing production controls; it makes no network/purchase/provisioning call. Source catalogs and API guidance were synchronized, not replaced by another visual specification.

## Verification

artifacts/banner-wizard-grid-release-2026-10-06.log records successful full Release preparation: Core 367, Blazor 350/350, bridge 12, 90 exported controls with 725 parameter rows/319 defaults, 63 configured Node checks, embedded controller assertions and CSS gates. Palette checks pass 18/18. The additional real grid regression covers both shapes, Centered on/off, four automatic/explicit layout combinations and narrow-column settings; CSS tests reject restored rectangle width caps and preserve Square constraints.

Six local packages are verified, including dependencies/assets; 99 isolated actual NuGet-consumer checks pass across FrameworkOnly, MetaNative, MetaDesign and MetaCulture. Evidence: artifacts/package-consumers/43a438af5bf147b58bc1f8ffe14eeea3. Package authoring still reports the existing missing-README advisory; no README or dependency was added. Framework DLL SHA256 agrees across library output, restored consumer package and Colligere Release output: 6B4F19F273BA0DD8D978A316D017BB3CF2672EF6AA745DF35E9F7DC2AD2803E3.

Colligere's complete Release build and nine Node files pass. Seven actual-event wizard regressions and existing UI tests pass; its full application suite retains the two previously documented order-contract failures and 87 environment-gated skips. Source/package/SSR/CSS assertions are not physical-browser geometry certification.

## Manual acceptance and scope

Check default banners and full-height access below/above 800px viewport height; explicit minima, dots and long/narrow content must retain natural growth. Compare Rectangle at wide/narrow widths, explicit columns, rows-only/automatic layout and Centered with Square's capped equal sides. Test wizard choice clicks, back/restart, disabled alternatives and live Gallery language/theme switching. Confirm consumers need no width/height skin patch.

No Computer Use, consumer-instance restart, database/security change, public NuGet upload, deployment, commit or push occurred. Generated local candidates were rebuilt by the existing guarded release script; prior content-addressed consumer caches were retained. Existing source changes/history/human docs were preserved. Active technical guidance and this append-only AI record were maintained with the write-page workflow.
