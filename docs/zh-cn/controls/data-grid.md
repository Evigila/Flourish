---
title: DataGrid
description: 使用 Flourish 样式展示表格数据，同时保留原生 WPF DataGrid 的数据、列、选择与编辑模型。
---

# DataGrid

`DataGrid` 保留 WPF 的 `ItemsSource`、`Columns`、选择、排序、编辑和滚动模型，并提供主题表面、分隔线、Regular 字重与首列强调色。

## 定义行与列

`ItemsSource` 可以绑定 WPF 支持的任意集合。保留 `AutoGenerateColumns="True"` 可根据数据项的公共属性自动生成列；若需要明确控制列，则将其设为 `False` 并声明原生列类型。

```xml
<flourish:DataGrid
  AutoGenerateColumns="False"
  ItemsSource="{Binding Members}">
  <flourish:DataGrid.Columns>
    <DataGridTextColumn
      Header="属性"
      Binding="{Binding Name}" />
    <DataGridTextColumn
      Width="*"
      Header="功能"
      Binding="{Binding Description}" />
  </flourish:DataGrid.Columns>
</flourish:DataGrid>
```

该控件不会替代 WPF 的数据项集合或列集合。`CanUserSortColumns`、`SelectionMode`、`IsReadOnly` 与 `HeadersVisibility` 等原生属性仍可直接使用。

## Flourish 属性

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `RowCount` | `int` | `0` | 只读数据行数；不包含原生的新建项占位行。 |
| `ColumnCount` | `int` | `0` | 只读列数，包含显式声明或自动生成的列。 |
| `FirstColumnForeground` | `Brush` | 主要前景色 | 设置首个显示列中单元格的颜色。 |

首列需要其他语义颜色时可设置 `FirstColumnForeground`。表头和单元格的其余颜色会跟随 Flourish 主题，并统一使用 Regular 字重。

## 页面滚轮协作

DataGrid 位于可滚动页面中时，不会无条件截留鼠标滚轮。只要内部内容在滚轮方向上仍可移动，DataGrid 会优先滚动自己的行；到达顶部或底部边界后，继续向外滚动的输入会交给外层 [PageBody](page-body.md) 或 ScrollViewer。DataGrid 本身没有可滚动范围时，页面可以直接响应滚轮。

因此通常不需要为 DataGrid 编写 `PreviewMouseWheel` 转发处理程序。保留标准滚动属性即可同时获得表格内部浏览和连续页面滚动。

## 相关内容

- [Card](card.md)
- [PageBody](page-body.md)
- [ScrollViewer](scroll-viewer.md)
- [WPF DataGrid 文档](https://learn.microsoft.com/dotnet/desktop/wpf/controls/datagrid)
