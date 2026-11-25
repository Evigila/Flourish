# Standardize shell header spacing and brand accent

Recorded on 2026-11-23 at 14:59:30 America/Sao_Paulo (UTC−03:00).

ShellHeader applies the theme accent to the brand name when AccentBrand is enabled. Context labels and docked page titles inherit ordinary chrome ink.

The header uses its existing 12px control spacing for the launcher and brand, the Back and title cluster, and the docked action group and trailing controls. Back keeps the launcher gap and starts before the docked title. With no Back control, the title aligns to the expanded logo. The width check includes the actual gap before choosing a stacked action row.

All 382 static and DOM checks passed. The six 1.4.11-preview.7 local packages passed dependency and asset verification. Browser spacing acceptance remains manual.
