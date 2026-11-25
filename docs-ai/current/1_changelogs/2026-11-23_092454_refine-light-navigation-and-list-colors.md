# Refine light navigation and list colors

Recorded on 2026-11-23 at 09:24:54 America/Sao_Paulo (UTC−03:00).

The default light secondary navigation uses standard beige `#F5F5DC`. Unselected controls on the beige rail and silver chrome use white preview and press feedback. The white page, popups and nested navigation retain visible tinted feedback. Existing selection and dark navigation roles retain their state- and mode-specific colors.

Standard DataTable and ListView column headers use the dark light-mode primary role, `#3C3932` by default, with white text in either theme. The action column uses the same background. Sort labels retain the header ink in every state, and focus outlines and resize marks use that ink for contrast. Row labels and responsive card labels retain their own surfaces.

All 37 targeted palette checks passed. The local `1.4.11-preview.1` package set built and all six packages passed dependency and asset verification. Source package defaults remain at `1.4.10`.
