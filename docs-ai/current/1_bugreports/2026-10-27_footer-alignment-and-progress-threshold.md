# Footer alignment and presentation progress threshold

Recorded 2026-10-27, America/Sao_Paulo (UTC-03:00). Follow-up to [standard branding and structured rows](2026-10-27_standard-brand-button-and-structured-menu-rows.md). Local candidate: **1.4.4-preview.shell.20261027.4**.

## Causes and implementation

PresentationFooter was naturally sized by padding and text, so scaling padding alone would not increase its overall baseline by half. Design now defines a responsive minimum height from the former default `90px + title size + 14px + 17px×1.55 + 48px`, multiplied by 1.5. Title size remains clamp(64px,9vw,112px). The default single-line baseline grows from roughly 242–290px to 364–436px; wrapping and additional actions may grow further. Foreground content aligns to the bottom with 48px inset.

The existing header-layout controller projects its actual contentBounds into each owned, external-to-main presentation footer. Framework applies those logical insets to the foreground container, following header/page resizing, direction and footer replacement. The decorative watermark stays outside this container. Without script, the standard configured-width ContentContainer remains usable. Observer writes are idempotent, and disposal/removal clears only owned footer layout state.

PresentationProgress previously chose the last band whose top reached the sticky-header boundary plus one pixel. This is not a full-visibility ratio, but it delays full-height scene selection until almost complete arrival. A directory click did not protect its destination from intermediate scroll updates.

Manual tracking now uses the upper-third line of the header-aware clipped viewport: incoming full-height scenes occupy about two thirds of usable space at activation. This uses section starts, not whole-section intersection ratios, so tall/short sections remain supported. Clicking selects the destination immediately throughout the existing bounded navigation lifecycle. Wheel, touch, pointer or keyboard interruption releases it and updates from actual geometry immediately. Existing settling correction, focus, reduced-motion, footer exclusion and cleanup remain in place.

## Verification and limits

The focused mock-DOM progress checks pass **31/31**, including asynchronous intermediate scroll positions, both navigation directions, interruption, visual viewport/clipping, reduced motion and final/short/tall sections. No browser or Computer Use is involved. Full-suite and physical interaction acceptance remain separate.

The header/footer geometry mocks pass **6/6**, covering shared edges, responsive changes, RTL, replacement/removal and observer cleanup. Producer 12-project and consumer 17-project Release builds pass with zero warnings/errors; six .4 packages were packed and the consumer restores five. Eleven library and two host JS/MJS files pass syntax checks, and Gallery multilingual JSON parses. C# regressions compiled; the full suite was not run.

After the consumer's unified-entry restart, eight readiness responses return HTTP 200 and Debug Web dependencies select .4. Five library browser resources, including both modified layout/progress modules, and the host culture script match the saved HTTP response bytes. Four public SSR pages retain 4/3/3/3 scenes and an external-to-main footer with separate watermark/foreground. The consumer's ignored `health-4.json`, `runtime-assets-4.json` and `runtime-markup-4.json` record delivery evidence; final browser geometry remains manual acceptance. No public publication or cloud deployment was performed.

## Manual acceptance

Verify larger bottom-aligned footer content at wide/mobile widths, exact shared header/content edges, unchanged full-width watermark, long-content growth and bottom inset. Verify immediate directory destination selection, manual two-thirds transitions in both directions, interruption without delayed snapping, multi-row headers, reduced motion, and progress hiding when actual scenes give way to the footer. These are user-run checks.
