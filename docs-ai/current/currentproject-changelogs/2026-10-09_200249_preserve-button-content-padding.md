# Preserve Button padding as content grows

Local date: 2026-10-09 20:02:49 (America/Sao_Paulo).

The user observed compressed vertical spacing in the Portuguese account Button and clarified that the standard short-button height is not a requirement to equalize multiline buttons. Shared Blazor Design .f-button changes zero block padding to 10px while retaining its 48px minimum and natural height. Default single-line text/icon examples fit 48px; stacked and wrapped labels grow with their padding. Existing width, rendering, actions and specialized icon/menu/grid/navigation/table geometry remain in place. No new API or Gallery-specific override is introduced.

Updated Framework usage guidance, Gallery descriptions, three-language catalogs and 1.1.4-preview notes. Added one bounded production CSS cascade regression to the existing suite. The [bug report](../bugfix-reports/2026-10-09_button-content-height.md) explains the actual cause and manual acceptance checklist.

Verification: zero-warning/error Framework and Gallery Release builds; Framework 436/436; Gallery 8,283 (474 post-event and 231 ChangeLog); production CSS cascade 16/16; Culture catalogs 22,331; ChangeLog 94; generated Design bundle checked; git diff --check passes. No Computer Use testing, dependency/version/publication change or Git commit.
