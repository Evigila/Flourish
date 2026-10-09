using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

internal static class DataPresentation
{
    private static readonly DependencyProperty IsCellPresenterProperty = DependencyProperty.RegisterAttached("IsCellPresenter", typeof(bool), typeof(DataPresentation), new PropertyMetadata(false));
    internal static DataGrid Grid(bool editable = false)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false, IsReadOnly = !editable, CanUserAddRows = false,
            CanUserDeleteRows = false, CanUserReorderColumns = false, MinHeight = 150,
            SelectionUnit = editable ? DataGridSelectionUnit.Cell : DataGridSelectionUnit.FullRow,
            SelectionMode = editable ? DataGridSelectionMode.Extended : DataGridSelectionMode.Single,
            EnableRowVirtualization = true, EnableColumnVirtualization = true, HeadersVisibility = DataGridHeadersVisibility.Column,
            RowHeaderWidth = 0, GridLinesVisibility = DataGridGridLinesVisibility.Horizontal
        };
        grid.SetResourceReference(FrameworkElement.StyleProperty, "Flourish.DataGrid");
        return grid;
    }

    internal static DataGridColumn Column(TableColumn column, CultureInfo culture, DataTemplate? template = null, bool wrap = false, bool rowHeader = false)
    {
        var factory = new FrameworkElementFactory(typeof(ContentControl));
        factory.SetValue(IsCellPresenterProperty, true);
        factory.SetValue(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
        if (rowHeader) factory.SetValue(Control.FontWeightProperty, FontWeights.Bold);
        if (template is not null) factory.SetValue(ContentControl.ContentTemplateProperty, template);
        var binding = new MultiBinding { Converter = new CellConverter(column, culture, template, wrap) };
        binding.Bindings.Add(new Binding());
        // The path subscription refreshes native cells when a record implements INotifyPropertyChanged.
        if (!string.IsNullOrWhiteSpace(column.BindingPath)) binding.Bindings.Add(new Binding(column.BindingPath));
        factory.SetBinding(ContentControl.ContentProperty, binding);
        return new DataGridTemplateColumn
        {
            Header = column.Label, SortMemberPath = column.Key, Width = column.Width, MinWidth = 72,
            CanUserSort = column.Sortable, CellTemplate = new DataTemplate { VisualTree = factory },
            ClipboardContentBinding = new Binding { Converter = new DisplayConverter(column, culture) }
        };
    }

    internal static void RefreshCells(DependencyObject visual)
    {
        if (visual is ContentControl cell && (bool)cell.GetValue(IsCellPresenterProperty))
            BindingOperations.GetMultiBindingExpression(cell, ContentControl.ContentProperty)?.UpdateTarget();
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(visual); i++)
            RefreshCells(System.Windows.Media.VisualTreeHelper.GetChild(visual, i));
    }

    internal static Border Frame(UIElement child)
    {
        var frame = new Border { Child = child, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(16), ClipToBounds = true };
        frame.SetResourceReference(Border.BackgroundProperty, "Flourish.Brush.Surface");
        frame.SetResourceReference(Border.BorderBrushProperty, "Flourish.Brush.Border");
        return frame;
    }

    internal static TextBlock Text(string value, bool muted = false)
    {
        var text = new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap };
        text.SetResourceReference(TextBlock.ForegroundProperty, muted ? "Flourish.Brush.Muted" : "Flourish.Brush.Text");
        return text;
    }

    private sealed class CellConverter(TableColumn column, CultureInfo culture, DataTemplate? template, bool wrap) : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo ignored)
        {
            var value = values[0];
            if (value is null || value == DependencyProperty.UnsetValue) return null;
            var display = TableData.Display(value, column, culture);
            if (template is not null) return new TableCellContext(value, column, display);
            var text = new TextBlock
            {
                Text = display, ToolTip = display, Margin = new Thickness(16, 12, 16, 12),
                VerticalAlignment = VerticalAlignment.Center, TextTrimming = wrap ? TextTrimming.None : TextTrimming.CharacterEllipsis,
                TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap
            };
            text.SetResourceReference(TextBlock.ForegroundProperty, "Flourish.Brush.Text");
            return text;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
    private sealed class DisplayConverter(TableColumn column, CultureInfo culture) : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo ignored) => value is null ? string.Empty : TableData.Display(value, column, culture);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
