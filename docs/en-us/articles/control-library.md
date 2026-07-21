---
title: Control library
description: A brief introduction to the Flourish control library and links to dedicated control documentation.
---

# Control library

Flourish provides explicit WPF custom controls for a consistent theme, layout, typography, and interaction language across application pages, Shell extension regions, dialogs, and independently hosted windows. They do not replace the default appearance of WPF base controls; reference the `http://schemas.arkheide.system/flourish` XAML namespace to opt into a Flourish control.

Design guidance, properties, defaults, and examples live in the dedicated [Controls documentation](../controls/index.md):

- [Chunk and ChunkHero](../controls/chunk.md) define the required full-width page hierarchy, leading hero, and section spacing.
- [Paragraph](../controls/paragraph.md) presents several consistently spaced and indented text paragraphs.
- [Presenter](../controls/presenter.md) combines copy and supporting controls with images, icon groups, or other presentation content.
- The [Card family](../controls/card.md) includes text-only `Card`, single-icon `IconCard`, and compact `ListCard` rows with one `ActionBody` control.
- [OutputCard](../controls/output-card.md) appends operation messages to a compact Small-text scrolling viewport.
- The [Button family](../controls/button.md) includes `Button`, `IconButton`, `CardButton`, and `WindowCaptionButton`.
- [DataGrid](../controls/data-grid.md), [Overlay](../controls/overlay.md), and [ScrollViewer](../controls/scroll-viewer.md) document tabular, floating, and scrolling behavior.

All other existing controls remain available through their current APIs. See the [Controls API](xref:ArkheideSystem.Flourish.Controls) for the complete type list and member signatures, and the [design principles](../conception/index.md) for page-wide selection and composition rules.
