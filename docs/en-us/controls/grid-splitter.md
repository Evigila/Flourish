---
title: GridSplitter
description: Resize adjacent Grid regions with a thin Flourish interaction highlight and continuous layout updates.
---

# GridSplitter

`FlourishGridSplitter` inherits the native WPF `GridSplitter` layout model. Use it between adjacent `Grid` rows or columns when the user should be able to change their relative sizes. Flourish supplies a narrow centered highlight inside its transparent pointer surface.

## Resize columns

Place the splitter in an `Auto` column between the definitions it controls. Define minimum widths on the content columns so neither region can be collapsed beyond its usable size.

```xml
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*" MinWidth="160" />
    <ColumnDefinition Width="Auto" />
    <ColumnDefinition Width="2*" MinWidth="240" />
  </Grid.ColumnDefinitions>

  <local:EditorPane />
  <flourish:FlourishGridSplitter
    Grid.Column="1"
    ResizeBehavior="PreviousAndNext"
    ResizeDirection="Columns" />
  <local:PreviewPane Grid.Column="2" />
</Grid>
```

Use the same structure with rows by placing the splitter in an `Auto` row and setting `ResizeDirection="Rows"`.

## Interaction

Dragging updates the affected `GridLength` values continuously. Hover, keyboard focus, and dragging use the same thin highlight; Flourish does not switch to a wider preview overlay. This keeps the visible divider consistent with the final layout while the transparent input surface remains large enough for pointer interaction.

The surrounding `Grid` owns minimum and maximum constraints. Set `ResizeDirection` and `ResizeBehavior` explicitly for application layouts so both the axis and the affected definitions are clear.

## Layout roles

`Variant="Standard"` is the general-purpose role. `Variant="NavigationPane"` is reserved for the edge of the Flourish Shell navigation pane: it selects horizontal column resizing, shell-edge alignment, cursor, and layer defaults. Both roles use the same interaction-surface width and thin live-resize highlight.

Align a neighboring viewport and splitter through their Grid columns. Do not use negative ScrollBar margins to conceal spacing: that can move the pointer target outside its viewport, clip the bar, or create overlap that changes with scaling.

## Related content

- [ScrollViewer and ScrollBar](scroll-viewer.md) describes the standard viewport and scroll-bar geometry.
- The [FlourishGridSplitter API](xref:ArkheideSystem.Flourish.Controls.FlourishGridSplitter) lists the Flourish-specific members.
- The [WPF GridSplitter documentation](https://learn.microsoft.com/dotnet/desktop/wpf/controls/how-to-resize-rows-with-a-gridsplitter) explains native Grid sizing behavior.
