---
title: Control library
description: A brief introduction to the Flourish control library and links to dedicated control documentation.
---

# Control library

Flourish provides explicit WPF custom controls for a consistent theme and interaction language across application pages, Shell extension regions, dialogs, and independently hosted windows. They do not replace the default appearance of WPF base controls; reference the `http://schemas.arkheide.system/flourish` XAML namespace to opt into a Flourish control.

Design guidance, properties, defaults, and examples now live in the dedicated [Controls documentation](../controls/index.md):

- [Chunk and ChunkHero](../controls/chunk.md) define page sections, page-leading hero regions, and their standard spacing.
- The [Button family](../controls/button.md) includes `Button`, `IconButton`, `CardButton`, and `WindowCaptionButton`.
- The [Card family](../controls/card.md) includes `Card` for longer information, single-control and immediately applied `ListCard` configuration rows, and presenter-aware `IconCard` surfaces.
- [OutputCard](../controls/output-card.md) appends raw messages, progress, completed results, and failures to a compact scrolling history that does not determine a peer layout's height.
- [Overlay](../controls/overlay.md) gives temporary hover details and strong floating content a shared surface and dismissal contract.

All other existing controls remain available through their current APIs. Dedicated documentation will be added after each corresponding control is refactored. See the [Controls API](xref:ArkheideSystem.Flourish.Controls) for the complete type list and member signatures.
