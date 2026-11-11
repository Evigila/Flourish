# Remove inner UniformGrid shadow seams

2026-11-09 14:09:40 · America/Sao_Paulo · UTC−03:00.

UniformGrid and FormActions combined each cell's blurred box-shadow with a drop-shadow on the group. The transparent one-pixel grid gaps exposed both cell shadows and the group filter's interior silhouette, producing dark blurred seams between controls.

Design now makes group cells meet with zero grid gap. Their inline-end and block-end borders use the existing opaque border role as one-pixel flat separators. Grouped normal, hover, active and unavailable cells have no blurred box-shadow; elevated group variants and FormActions keep their existing outer drop-shadow. Group containers remain transparent and no placeholder or rectangular background was introduced, so unused tracks are not painted or independently shadowed.

Grouped unavailable cells keep their original opaque semantic fill and fade their direct content to .55 instead of applying .55 opacity to the entire cell. This preserves unavailable feedback without exposing the group shadow through the cell or its separators. Standalone UniformGridButton retains its original shadows, transparent borders and whole-control unavailable opacity. Hover/press semantic fills, busy spinner positioning, inset group keyboard focus, variants, sizing limits and native behavior are unchanged. All color choices use existing palette roles; no mixed or private colors are introduced.

Verification: 52/52 focused Node tests passed across the complete imported Framework/Design cascade and palette inventory. The state matrix covers light/dark themes, inherited/local variants, native button and link controls, hover/press/focus, disabled/busy, grouped and standalone usage. A geometry/source-alpha check verifies contiguous occupied cell boxes and an unpainted last-row empty track. CSS bundle passed 21 checks; scoped git diff --check passed. These are automated cascade/geometry checks, not a claim of browser visual acceptance.

Manual acceptance: compare two adjacent cells and a multirow group on light/dark themes; internal edges should be crisp and the outside should keep one shadow. Check an incomplete last row, a square grid, an information grid, FormActions, mixed variants, disabled and busy cells, keyboard focus and a standalone UniformGridButton. Also check a narrow viewport and forced-colors mode. No dependency/version change, maintained source-path addition, package, public release or commit was made by this implementation task.
