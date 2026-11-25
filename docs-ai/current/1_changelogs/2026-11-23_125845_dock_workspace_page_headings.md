# Dock workspace page headings in the application header

Recorded on 2026-11-23 at 12:58:45 America/Sao_Paulo (UTC−03:00).

When an expanded PageHeading leaves its NavigationSurface scroll region, ShellHeader shows a compact copy of the title in its brand and center area. The original parent link and Actions content appear next to it through the native top layer. Their reserved slots preserve scroll geometry and their DOM identity stays with the page. The header keeps its launcher and trailing controls, allows a second action row on narrow screens, and restores the brand and center area when the expanded heading returns. The title animates in from above when motion is enabled. Standalone content and the explicit Compact variant keep their existing behavior.

The header brand and docked title use 16px at the default body scale. ShellHeader supports icon-only identity with an accessible label and identity tooltip. The light palette restores the gray-beige secondary navigation surface `#F5F1EA`, preview `#EAE1D5` and press `#D3CBC0`.

All 63 focused heading, header and palette checks passed. All six local `1.4.11-preview.3` packages passed dependency and static asset verification.
