# Compact heading scroll feedback and module verification

## Symptoms and cause

On short pages, especially Home, scrolling beyond 96px collapsed the heading. The reduced heading height also reduced the document's scrollable range. The browser clamped scrollTop below the 24px expansion threshold, so the heading expanded again. The restored range triggered another scroll event and repeated the cycle. Threshold hysteresis alone cannot prevent this geometry feedback.

IconSupport previously centered both content groups inside equal halves, leaving a large gap between the icon and text. The Design button display selector could also override the Framework grid display for action cells, so correcting alignment required keeping the equal-row layout effective in both layers.

During testing, ApplicationShell reported a failed dynamically imported fingerprinted shell module. The original localhost:5188 service was no longer listening when its response was inspected; its HTTP status, compression and transitive-import failure cannot be established from the exception alone. A newly introduced standalone heading helper also required a matching host static-asset manifest, which made validation through temporary build outputs insufficient for a concurrently running VS host.

## Correction

Framework shell.js measures the compact document before paint. If the remaining scroll range is below 24px, it immediately restores the expanded heading and previous scroll position. A WeakMap remembers that document geometry so queued events do not retry the same unsuitable collapse. Content/viewport changes and navigation resets allow a new attempt. No spacer or permanent blank region is added. Long pages retain the 96px/24px behavior, 38px compact title and icon-only back action.

The heading functions live in the existing shell.js asset; patterns/surfaces.js imports that existing module. The temporary standalone helper was removed. The normal solution outputs used by VS were rebuilt, including their static asset manifests and fingerprinted module endpoints. No retry loop, swallowed import exception or manual fingerprint URL was introduced. A running host still needs a restart and full browser reload after rebuilding to use a coherent asset map.

UniformGrid and UniformGridButton retain equal icon/text halves. The icon aligns to the bottom of its half with 4px padding and text to the top with 4px padding. The total central gap is 8px. A Framework selector keeps this grid display effective when the optional Design button rules are present. Existing automatic IconSupport selection and explicit overrides remain unchanged.

## Evidence and regression

- Normal fourteen-project Flourish.slnx build: zero warnings and errors. An old ignored WinUI Gallery obj cache still referenced its pre-rename path; removing that cache allowed normal regeneration without source changes.
- Blazor checks: 105 passed. All Node check scripts passed, including five new heading cases for clamping, queued events, thresholds, changed geometry and resets.
- Headless installed Edge exercised Home at widths 1440, 1000 and 760px with a 110px expanded scroll range. Wider layouts reject unsuitable compaction and settle after one rollback; the narrower layout retains a usable 40px compact range. Real scrolling to the top expands the title, and idle observation shows no repeated mutations.
- Long Button pages retain the compact title/back layout and expand at the top. Passive and action grid cells keep equal halves and a measured 4px + 4px join gap. Local NavigationSurface actions and its module dependency work. No browser exceptions were observed.
- The normal Gallery host serves fingerprinted shell, controls and surfaces modules with correct decoded content for identity, gzip and Brotli requests. All 77 guide routes retain five sections and default-value columns. The Framework-only host works without Design.

## Limits and manual acceptance

The original old-hash request was unavailable, so this does not prove whether that response was absent, stale or failed through a dependency. Current normal output and the rendered module graph are verified. After restarting VS Gallery, fully reload localhost:5188 and verify no import exception. Scroll Home slowly around the collapse threshold, resize, navigate to a long page and return to the top. Inspect grid cells with short and wrapped text in both themes. Follow blazor-manual-tests.md for other browsers, zoom, keyboard and accessibility acceptance. Computer Use was not used.
