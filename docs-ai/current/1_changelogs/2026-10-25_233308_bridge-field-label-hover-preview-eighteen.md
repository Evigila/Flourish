# Keep Field label and input hover continuous

Recorded: 2026-10-25 23:33:08 America/Sao_Paulo (UTC-03:00).

Design's Field label now owns an empty, absolutely positioned ::before box spanning the label-to-control gap. Native label-associated hover remains active while crossing that area. The box is restricted to direct Field labels and their column, keeping right-side actions and lower validation surfaces outside the bridge. Label box measurements, input position and existing access-form action spacing are retained. The existing gap role drives ordinary Field/Search spacing as well as the bridge; default remains 7px. No hover delay, new JavaScript controller, markup wrapper or public parameter was added.

The [HTML hover rule](https://html.spec.whatwg.org/multipage/semantics-other.html#selector-hover) includes the control associated with a hovered label. The former grid gap was outside the label and control hit boxes, causing the intervening exit/re-entry. A generated label box extends the existing native interaction without styling the entire Field as hovered. ::before leaves the required-marker ::after contract available.

Root package version is `1.4.3-preview.portals.18`. Six local packages passed metadata/internal-dependency/required-asset validation. Consumer cache archives and packaged CSS match source output. Existing package identities and external dependencies remain; no public publication or running-app restart occurred.

Verification: Framework 474/474; palette Node 35/35 including two new default/custom-gap geometry regressions; existing access-form spacing DOM 4/4; Colligere Release build (17 projects), zero warnings/errors; final Web: 2605 passed, 0 environment-dependent skips, zero failures. A first consumer run found five old branding assertions still requiring the formerly unframed portal; these were corrected to the user-approved framed layout and the full suite rerun. Real pointer movement and visual zoom checks remain manual, per project policy. Evidence: ../Colligere/artifacts/field-hover-20261025/.
