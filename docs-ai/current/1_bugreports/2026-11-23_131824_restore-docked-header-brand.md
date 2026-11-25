# Restore the brand when a workspace heading leaves the top bar

Recorded on 2026-11-23 at 13:18:24 America/Sao_Paulo (UTC−03:00).

The normal ShellHeader fitter measured the start group while the dock CSS hid the brand. It stored a width intended for the docked state and reused it on return. The fixed-width logo occupied the space while the brand label shrank to zero. The intrinsic measurement skipped the rendered zero-width label, so it could not recover its required width. The dock also started at the edge of the launcher group, outside the normal brand inset.

The fitter preserves the expanded header fit while docked and reruns when the title returns to the page. Rendered labels still contribute their text width when clipped. The dock reserves the brand's inline padding and border so its title starts where the expanded logo starts. The same origin applies with the launcher hidden and mirrors with text direction.

The layout regression covers docking, an initially clipped brand name and remeasurement on return. CSS checks cover the shared inset and the elevated header baseline. All 347 non-browser JavaScript suite checks and 29 ActionMenu controller checks passed. The six local 1.4.11-preview.4 packages passed package verification. The visual transition remains a manual acceptance check.
