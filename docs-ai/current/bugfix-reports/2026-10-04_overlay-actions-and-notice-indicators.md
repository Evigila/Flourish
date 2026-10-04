# Overlay divider, draft action and notice badge corrections

## Symptoms and cause

Decorative horizontal borders divided all overlays. The primitive draft's save/close area used a full-width filled form grid because the sheet lacked a footer slot. Information/Warning Notice retained the earlier merged Primary paint; NoticeTrigger's rectangular text badge and40px native minimum height prevented a body-size circle.

## Mitigation

Remove overlay dividers, add optional right-aligned Actions below the primitive sheet's scrolling body and use ordinary draft action buttons. Preserve mounting/retained-draft/Busy/close lifetimes. Add centralized Info text and Warning background with independent dark values and readable alias ink. Replace badge text with a named17px circle and decorative severity glyph; keep unique aria-describedby, notes and hover/focus explanations.

## Evidence

Zero-warning/error builds and102 .NET checks pass, including lazy/retained Actions, glyph accessibility and description encoding.27 controls DOM,8 row-action and9 palette audits pass. Headless installed Edge opens every overlay, observes zero divider widths, checks desktop/320px footer alignment and performs a real local save/Busy/reopen cycle. It verifies exact Light/Dark Notice paint, unchanged Success/Error, five17px named circles, hover/focus explanation and live severity changes without browser errors. All78 guides remain routable; API audit has543 rows and254 non-null defaults.

## Limits and regression acceptance

Manually test keyboard, focus return, close veto, omitted Actions, narrow wrapping, System mode, high contrast, zoom, screen readers and supported engines. Dark-mode circles use the readable existing Surface/Primary/Danger roles; Light Info is white/blue. No business transaction, package or dependency changed. Existing history and unrelated working changes are preserved.