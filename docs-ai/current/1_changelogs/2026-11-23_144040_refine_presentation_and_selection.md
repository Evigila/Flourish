# Refine presentation layout and selection feedback

Recorded on 2026-11-23 at 14:40:40 America/Sao_Paulo (UTC−03:00).

ShellHeader uses the theme accent for the brand and the docked title at the same 18px default size. Document-flow presentation bands subtract the measured sticky header from their viewport minimum. The existing header observer takes wrapping into account, and the presentation container reduces outer and inner horizontal gutters to 80 percent. Fixed-host access panels and footer bands keep their own layout rules.

The default chrome and table headers share silver #EBEBE6. The light neutral hover role shares the secondary navigation beige #F5F1EA. Persistent neutral selection uses charcoal #3C3932 with white text. Ordinary dropdowns use the selected row highlight. The table column-visibility menu keeps its checkmark without persistent charcoal paint. Keyboard and form selection semantics continue to use the original controls. The classic system select fallback remains browser-native.

All 380 static and DOM regression checks passed. The six local 1.4.11-preview.6 packages passed dependency and asset verification. Browser acceptance covers responsive bands and the two dropdown selection cues.
