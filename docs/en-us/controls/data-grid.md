---
title: DataGrid
description: Present tabular data with Flourish styling and native WPF behavior.
---

# DataGrid

`DataGrid` inherits from the native WPF `DataGrid`. Use the standard `ItemsSource`, `Columns`, selection, sorting, editing, and scrolling properties; Flourish supplies the themed surface, separators, Regular typography, and first-column emphasis.

## Define rows and columns

Bind `ItemsSource` to any collection accepted by WPF. Keep `AutoGenerateColumns="True"` to derive columns from public item properties, or set it to `False` and declare native column types explicitly.

```xml
<flourish:DataGrid
  AutoGenerateColumns="False"
  ItemsSource="{Binding Members}">
  <flourish:DataGrid.Columns>
    <DataGridTextColumn
      Header="Property"
      Binding="{Binding Name}" />
    <DataGridTextColumn
      Width="*"
      Header="Function"
      Binding="{Binding Description}" />
  </flourish:DataGrid.Columns>
</flourish:DataGrid>
```

The control does not replace WPF's item or column collections. Native properties such as `CanUserSortColumns`, `SelectionMode`, `IsReadOnly`, and `HeadersVisibility` remain available.

## Flourish properties

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `RowCount` | `int` | `0` | Read-only number of data rows; excludes the native new-item placeholder. |
| `ColumnCount` | `int` | `0` | Read-only number of explicit or automatically generated columns. |
| `FirstColumnForeground` | `Brush` | Primary foreground | Colors cells in the first displayed column. |

Set `FirstColumnForeground` when the leading field needs a different semantic color. Headers and cells otherwise inherit the active Flourish theme and use Regular font weight.

## Mouse-wheel scrolling

The grid consumes mouse-wheel input while its internal vertical viewport can move in the requested direction. At the top or bottom boundary, outward wheel input continues to the containing [PageBody](page-body.md) or Flourish [ScrollViewer](scroll-viewer.md). A grid with no internal scroll range also leaves the wheel available to the page.

Do not wrap `DataGrid` in another scroll viewer; nested ownership can break boundary scrolling and virtualization.

## Related content

- [Card](card.md)
- [PageBody](page-body.md)
- [WPF DataGrid documentation](https://learn.microsoft.com/dotnet/desktop/wpf/controls/datagrid)
- [DataGrid API](xref:ArkheideSystem.Flourish.Controls.DataGrid)
