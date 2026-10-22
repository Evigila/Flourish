# Match standard access identity and introduce decorative hero media

Recorded: 2026-10-21 18:42:52, America/Sao_Paulo (UTC-03:00). Exact local candidate: **1.4.3-preview.portals.8**. Earlier packages and append-only records remain intact.

Compact LogoDisplayer retains horizontal geometry and image-size behavior but now inherits ordinary Account title/context typography. The 32/24px main-title and 18px context overrides are removed; the shared name uses clamp(56px,8vw,80px) and the shared context uses its 28px role with 12px margin. Bounded operation layout and explicit 96px images remain independent.

PresentationBand and PresentationHero add optional BackgroundImageUrl. Band renders a decorative image layer before the foreground container; Hero forwards the actual value without duplicating its band, h1 or progress entry. Framework owns absolute cover/layering and pointer transparency; Design owns Canvas/Surface/Primary semantic gradients, with a more visible desktop right edge and denser narrow-screen veil. Missing images retain the original Tone, and omitted values preserve ordinary markup. Dotted, height, side content and native actions remain independent. Gallery demonstrates the API with an existing local SVG and translated guidance. No external package/media dependency is added.

Verified final source and packages:

- Complete Blazor solution Release build: zero warnings/errors. Framework 458/458; catalog 83 components, 761 rows, 352 documented defaults. Gallery 8541 includes its 474 post-event and 359 ChangeLog subsets.
- Focused AccessLayout/media/identity and palette Node group: 36/36 (10+26, including imported page-body checks). Preview identity/stable-publication guard: 12/12. Six exact .8 packages passed identity, dependency and required-asset verification.
- Initial and diagnostic Framework runs were 456/458 because two new assertions required literal alt="". Actual output used legal minimized empty alt and preserved URL encoding and accessible labels. Final independent attribute checks retain all source/ARIA/form/progress/side-slot protections. Production did not change to satisfy those failures. Palette guards permit only the exact standard media gradients; other color/font/theme ownership checks remain.

See [implementation, evidence and manual acceptance](../1_bugreports/2026-10-21_product-hero-media-and-access-titles.md). Logs are under ignored artifacts/hero-media-8; final Framework evidence is framework-tests-final.log. Consumer-specific Web/SVG results are recorded in the linked Colligere report. No Computer Use, normal AppHost restart, database/cloud operation, Git commit/push or public publication occurred. Current inventories are refreshed separately; audit state remains unchanged.
