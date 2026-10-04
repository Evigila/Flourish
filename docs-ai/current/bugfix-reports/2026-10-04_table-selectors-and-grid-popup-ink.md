# Table dropdown divergence and selected grid option contrast

## Symptoms and cause

DataTable's search/page-size selects bypassed standard SelectBox hooks, while display retained a private summary skin. Native picker options did not share the existing menu option geometry. The item count separately framed its total, and pager text had only 6px right padding. EditingGrid selection inverted the closed editor as intended, but native options inherited that foreground onto a Surface popup.

## Mitigation

Use standard native select hooks and shared dropdown trigger/panel/option styles, retaining original control semantics and callbacks. Render one localized range span with no inner total border; append locale-owned Total and ItemRangeFormat captions. Set pager total padding to 12px. Give grid options/optgroups and enhanced pickers independent Text/Surface ink, including selected options; retain selected collapsed editor inversion.

## Evidence and regression checks

Both builds pass with zero warnings/errors. 101 .NET checks, 27 control DOM checks, 8 row-menu checks and 9 palette audits pass. Installed headless Edge confirms real scoped filtering, zero/populated counts, synchronized top/bottom page-size changes, page reset, native popup geometry, transparent idle display rows and visibility toggles. Real selected grid edits maintain closed-cell inversion and readable options in both themes. Dark picker, 320px containment and no browser exceptions pass. A separate CSS fixture verifies 52 selected/unselected, native/enhanced and Light/Dark combinations. Existing historical records are unchanged.

## Limits and manual checks

Native platform pickers vary between engines. Manually verify keyboard, disabled/readonly states, hover/press, screen readers, zoom and supported browsers. No data source, business operation, editor kind or host callback was changed. Temporary verification hosts use independent ports and do not stop the user's VS process.