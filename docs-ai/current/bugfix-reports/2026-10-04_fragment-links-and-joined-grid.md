# Fragment links and joined grid regression

## Symptoms

The Button scenario's status action left its component guide for the home page. UniformGrid and UniformGridButton no longer had the earlier joined appearance; the recent sizing change applied the Square maximum to Rectangle too. The user also requested smaller corners facing the SplitButton gap.

## Causes and evidence

Gallery declares a root base href. A native anchor with href=#button-draft-status therefore resolves against that root rather than the current guide. Button and SplitButton forwarded fragment-only values unchanged; UniformGridButton composes Button and inherited the same behavior. The scenario source itself correctly uses a reusable local identifier.

The committed Design controls.css defines one rounded grid container. The later uniform-grid.css overrides introduced 16px gaps and independent 12px rounded/shadowed cards. A maximum-width token was applied to every shape, and UniformGrid emitted it for Rectangle as well. These changes altered the shared grid's appearance and rectangular layout instead of confining the cap to Square.

## Repair

- Button and SplitButton prefix fragment-only destinations with the current NavigationManager.Uri after removing its previous fragment. Path and query survive. Other destinations retain their original values; unavailable links still have no href. UniformGridButton inherits the correction. No Gallery route is hardcoded into reusable components or scenario source.
- Restored a joined grid container, thin Border dividers and shared outer radius/shadow. Internal cells have no independent rounded corners or shadows; hover remains clipped and keyboard focus is inset. Standalone cells retain their own skin.
- Square MaxCellSize now defaults to 280px. Only Square emits/uses the maximum token. Rectangle grows within available width. Automatic wrapping and explicit row/column/narrow settings remain available. The approved thirteen-role palette and four Variant values remain intact.
- Both SplitButton corners facing its existing 3px gap use a 3px radius. The four outer corners retain the normal radius. Standalone and navigation variants follow the same geometry without changing action, disclosure or selection behavior.

## Verification

Gallery and library-check builds completed with zero warnings/errors. All 96 library checks passed, including fragment-only Button/SplitButton/UniformGridButton targets with an existing path, query and hash; ordinary relative/external links; default/custom Square-only limits; disabled/busy link behavior and native callbacks. Palette checks passed 8/8 and mocked menu/modal DOM checks passed 24/24.

A hidden, isolated Edge headless automated test against a separate temporary Gallery service saved a draft and activated the status link. The resulting URL retained /controls/actions/buttonsample?mode=draft and gained #button-draft-status; the displayed saved count remained one. The same test applied delivered Framework/Design CSS to sizing fixtures at viewport widths 320, 760 and 1180px. Square cells remained 280px and wrapped; no grid exceeded its parent. Rectangle cells in a two-column fixture measured 146.5, 366.5 and 576.5px, confirming no Square cap. Internal corners/shadows were absent. Computed SplitButton radii were 12/3/3/12 and 3/12/12/3 for both standalone and selected navigation, with the existing 3px gap.

Evidence script: C:/Users/Evigila/AppData/Local/Temp/flourish-grid-fragment-624a8cfe546c4959826b268465152427.cjs. Matching service logs: C:/Users/Evigila/AppData/Local/Temp/flourish-control-http-624a8cfe546c4959826b268465152427.out.log and .err.log. The temporary service and headless browser were closed without stopping the user's VS process. No Computer Use tools were used.

## Limits and manual regression

The geometry checks cover controlled fixtures and Chromium/Edge, not every host composition or browser. Rebuild/restart the VS Gallery and refresh its document. Repeat the status action after saving, inspect both grid shapes and directory wrapping at wide/narrow widths, verify all grid variants in Light/Dark, and check SplitButton hover/focus/selection with separate main/disclosure actions. Existing explicit row-only mode intentionally distributes content by columns. No dependency was added and no business behavior changed.
