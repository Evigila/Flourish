# Add watermark and restore editing layout

Extend the existing EmptyState with passive title-only Watermark, keeping Standard defaults and rejecting interactive composition. Update the contract, catalog/defaults, localized descriptions, Gallery and four checks. DataTable owns bulk-action end placement; Gallery uses one ordinary Concluir command. Restore EditingGrid's historical framed 65vh scroll area, 74px headers, 220/280px automatic width floors and dirty identity cue without another rendering engine. Preserve current 48px cells, resizing, formatted/focused editors, selection, clipboard and validation.

Release test/Gallery builds passed with zero warnings/errors; console checks passed 382/382 and Node layout tests 19/19. [The report](../bugfix-reports/2026-10-06_watermark-and-editing-layout.md) distinguishes host-supplied labels, library layout loss, partial historical restoration and the public-package heading-collapse mismatch. The release verification guide records the unpublished queue.

No version change, package publication, consumer upgrade, Computer Use, Aspire interruption or commit occurred. Current public 1.1.1 remains unchanged; queued source fixes must not be confused with active consumer behavior.
