---
title: PageBody
description: Use PageBody as the scrolling root that enforces the HeaderChunk and Chunk hierarchy of a Flourish page.
---

# PageBody

`PageBody` is the root content container for a navigated Flourish page. It combines a Flourish [ScrollViewer](scroll-viewer.md), the standard page margin, and a vertical section stack. Declare sections directly without adding another `ScrollViewer` or `StackPanel`.

```xml
<Page
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish">
  <flourish:PageBody>
    <flourish:HeaderChunk
      Title="Reports"
      Content="Review generated reports."
      PresenterMode="Split"
      PresenterPosition="Right" />

    <flourish:Chunk Title="Available reports">
      <flourish:Card Content="Choose a report to continue." />
    </flourish:Chunk>
  </flourish:PageBody>
</Page>
```

Direct XAML content is added to `Children` and arranged vertically in declaration order. The collection enforces these rules:

- Every direct child is either `HeaderChunk` or `Chunk`.
- At most one `HeaderChunk` is allowed.
- When present, `HeaderChunk` is the first child.
- Cards, Presenters, Documents, panels, and other content belong inside a `Chunk.Body` or `HeaderChunk.Body`, never directly under `PageBody`.

A standard content page uses one leading `HeaderChunk` followed by one or more `Chunk` sections. The collection permits temporarily constructing a page without a header, but does not permit placing one after another section.

## Scrolling and centered content

Horizontal scrolling is disabled and vertical scrolling is automatic by default. Because `PageBody` inherits the Flourish `ScrollViewer` behavior, nested Flourish scroll surfaces consume wheel input while they can move and let boundary input continue scrolling the page.

When the Shell enables `UseCenterContent`, it limits and centers the section stack while keeping the scrolling viewport full width. The vertical scroll bar therefore remains at the edge of the Shell content area. In narrower viewports, the body uses the available width.

Do not assign the inherited `Content` property. `PageBody` uses it for its internal section stack, and the Flourish Shell may wrap that stack while applying its page-width constraint. Add or remove sections through `Children` instead.

## Related features

- [Chunk](chunk.md) defines the `HeaderChunk` and `Chunk` hierarchy inside PageBody.
- [ScrollViewer](scroll-viewer.md) documents inherited scrolling behavior.
- [Shell configuration](../articles/shell-configuration.md#customize-content-alignment) configures centered page width.
- The [PageBody API](xref:ArkheideSystem.Flourish.Controls.PageBody) lists all members.
