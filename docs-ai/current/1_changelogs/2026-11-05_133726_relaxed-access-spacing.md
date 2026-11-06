# Opt-in top spacing for access pages

2026-11-05 13:37:26 · America/Sao_Paulo · UTC−03:00.

AccessLayout can now display its brand and form lower on larger screens. The optional RelaxedTopSpacing flag applies 80px top padding at viewport widths of at least 761px and heights of at least 640px. Other viewports use the existing touch and short-screen spacing. The flag defaults to false and adds no dependency. The Gallery guide includes a working example and a parameter description in all three cultures.

Manual checks: compare a wide service login with a standard AccessLayout, then narrow the window and reduce its height. Confirm the service identity, login methods and submit action remain reachable.
