# Standard access titles and image-backed product heroes

Date: 2026-10-21, America/Sao_Paulo (UTC-03:00). Implemented and automatically verified in local **1.4.3-preview.portals.8**. This follows [the .7 anchored-popup record](../1_changelogs/2026-10-21_180910_unify-anchored-popup-placement.md), whose results remain historical. Physical browser layout, cropping and contrast are manual acceptance.

## Cause and implementation

LogoSize controls image dimensions only. Compact separately forced a 32px main title, 24px on short screens and an 18px context with a 4px margin. Those typography overrides are removed. The horizontal Compact layout, zero bottom margin, default image fallback and explicit instance LogoSize remain; title and context now inherit the same ordinary Account identity rules: clamp(56px,8vw,80px), and the standard 28px context with 12px separation. Bounded AccessLayout and its overflow behavior remain unchanged.

PresentationBand and PresentationHero each add optional string BackgroundImageUrl. Hero forwards the actual parameter into its one Band. A nonblank value renders one decorative f-presentation-band-media wrapper before ContentContainer, with aria-hidden and an empty-alt image using asynchronous decoding. Framework owns absolute cover geometry, pointer transparency, isolation and the foreground z-order. The image contributes no height or progress entry and does not become a heading, action, form or landmark. Null/empty/whitespace values preserve ordinary Band output. The existing Tone remains the fallback if media cannot load.

Design owns the fixed gradient veil for Canvas, Surface and Primary using only each active semantic role. The left 52% stays solid for copy, 76% retains 92% role opacity, and the desktop right edge fades to 20% so artwork remains visible. At widths up to 760px the right edge retains 78% to protect full-width copy. No mask color or opacity parameter, host skin or external media service is introduced. FullHeight, MinHeight, Dotted, SideContent and IncludeInProgress retain their independent contracts; the consumer disables Dotted only on its product heroes and leaves access pages dotted.

Gallery uses its existing local SVG to demonstrate the media parameter; usage, parameter, sample and aggregate preview change descriptions are localized. Colligere supplies a separate locally authored abstract architectural SVG through the public parameter for its four product roots. No third-party package or media dependency is added.

## Final library verification

- Complete Blazor solution restore and final Release build: zero warnings/errors. An earlier production aggregate build passed before the final desktop-mask adjustment; the complete solution build certifies the final mask.
- Framework **458/458**. Catalog: 83 components, 761 rows, 352 documented defaults. Gallery **8541**, including its 474 post-event and 359 ChangeLog subsets, not additional totals.
- Focused Node group **36/36**: AccessLayout/media/identity 10 plus palette 26, already including the imported page-body checks. Preview identity/publication fence **12/12**. Six-package identity/dependency/required-asset verification passed for the exact .8 feed; previous package identities were not overwritten.
- Two initial Framework runs were 456/458 because new tests expected the literal alt="" spelling. Captured HtmlRenderer output uses the equivalent minimized alt attribute; encoded src, aria-hidden and the Hero accessible label were correct. Final tests independently verify exact encoded source, present empty alt in either legal form, aria-hidden and absence of lazy/interactive attributes. Forms, Tone, progress, title and side-slot assertions remain. Production did not change for this correction; initial, diagnostic and final logs are separate.
- The earlier palette guard rejected every new alpha expression in presentation.css. Its update permits only the exact three-Tone standard media gradients and retains other raw-color/font/theme/host-skin guards. No production change was made to bypass that guard.

Logs are in ignored artifacts/hero-media-8; final Framework results are in framework-tests-final.log. Consumer restore/build/Web and anonymous SVG delivery results are maintained in [the Colligere report](C:/Users/RC_Auditoria/source/Repos/Colligere/docs-ai/current/1_bugreports/2026-10-21_product-hero-media-and-access-titles.md). No Computer Use, normal AppHost restart, database/cloud operation, Git commit/push or public package publication occurred. Audit state is unchanged.

## Manual acceptance

Compare Account and service login identity sizes, then switch service methods without moving the brand/navigation region. Check narrow/short/zoomed viewports for reachable actions. Inspect the four product heroes in light/dark modes and three languages: no dotted board, visible artwork, readable foreground and working buttons. Confirm progress/footer behavior, image cropping, image-failure fallback, and dotted backgrounds after entering login. Replace only the host image URL/asset when final artwork is available; visual contrast still needs acceptance against that artwork.
