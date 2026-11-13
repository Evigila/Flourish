# Restore progress highlight and improve ring clearance

## Request and cause

The user reported the missing slanted white ProgressBar highlight and a ProgressRing arc too close to its text. Earlier color convergence had replaced the original 105-degree white/transparent gradient with a full preview-colored layer. The old 52px SVG, radius 22 and stroke 4 provided a 40px inner diameter, less than the actual 47.30px width of a bold body-size 100% label.

## Changes

Restore the original gradient angle, transparent ends and 2.4-second moving highlight with 47% white from the existing light Surface value. Primary remains the solid fill. Foundation now declares that white once as --f-surface-light; Light Surface and the highlight reuse it. The fifteen-role palette is unchanged; audits narrowly allow the requested progress alpha effect.

Set ProgressRing's Design container to 72px, SVG to 68px, stroke to 4.5 units and both drawing radii to 23. The viewBox/center and typed progress APIs remain. Effective inner diameter is 54.27px, with 6.97px combined horizontal clearance around the rendered 100% label. Initial 68/64px sizing failed that clearance check and was refined from actual rendering.

Fix the existing reduced-motion unknown-fill specificity conflict by explicitly stopping its animation and transform. Unknown progress remains a static track-contained block and does not invent a numeric value. No package or business behavior changed.

## Verification

Gallery/test builds have zero warnings/errors. 102 .NET checks and 9 palette/style audits pass. Isolated installed headless Edge verifies the gradient and distinct real animation samples, identical white highlight in Light/Dark, real bound 40% progress, honest unknown state and reduced-motion static presentation. It measures final ring geometry and 100% clearance and exercises pause/resume, unknown, reduced motion and 320px containment without browser errors. Git whitespace check passes.

## Manual acceptance and limits

Use blazor-manual-tests.md for full sweeps, 0/100% endpoints, narrow windows, zoom, both themes, reduced motion, screen readers and supported engines. The new ring is taller than a narrow toolbar slot; current Gallery uses it in content, and third-party toolbar hosts should apply their contextual size. No Computer Use, user VS-process interruption, Colligere edit or Git commit occurred.