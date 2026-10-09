# Align FormActions with uniform-grid variants

- Shared the default Elevated palette between UniformGrid and FormActions, preserving explicit FilledElevated and Danger children without a new public variant API.
- Replaced legacy Primary/border-colored container paint with transparent gaps, restored the shared outer group elevation and inset keyboard focus outlines, and retained the existing equal-width columns, clipped 16px group corners, 96px minimum child height and narrow-window adaptation.
- Danger still has no individual item shadow; the group owns its shared outside elevation. Busy, Disabled, submit transport and scenario callbacks keep their existing behavior.
- Preserved the existing Gallery previews and scenario, and added a three-column FilledElevated/default Elevated/Danger preview. Updated production and API guidance, localized the default label, and added a concise 1.1.4-preview ChangeLog entry.
- Added FormActions rendering and complete-import CSS regressions covering the default, explicit variants, group paint/elevation, keyboard focus and geometry.

Verification reported by the parent agent: zero-warning/error Blazor and Gallery Release builds; 429/429 Blazor checks; 7,808 Gallery checks, including 171 event examples and 195 ChangeLog assertions; 19 focused Node tests; 22,169 culture, 67 ChangeLog and 21 CSS bundle checks; git diff --check clean. See [the source diagnosis and manual checklist](../bugfix-reports/2026-10-09_form-actions-grid-variants.md). No Computer Use, browser UI test, dependency change, Git commit, tag or publication was performed. This supplements the earlier UGB variant record without editing append-only history.
