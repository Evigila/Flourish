using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using WpfDataGrid = System.Windows.Controls.DataGrid;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A themed WPF data grid that preserves the native items, columns, selection, editing, and
/// automatic-column-generation contracts.
/// </summary>
public class DataGrid : WpfDataGrid
{
    private static readonly DependencyPropertyKey RowCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(RowCount), typeof(int), typeof(DataGrid), new FrameworkPropertyMetadata(0));

    private static readonly DependencyPropertyKey ColumnCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ColumnCount), typeof(int), typeof(DataGrid), new FrameworkPropertyMetadata(0));

    /// <summary>Identifies the read-only <see cref="RowCount" /> dependency property.</summary>
    public static readonly DependencyProperty RowCountProperty = RowCountPropertyKey.DependencyProperty;

    /// <summary>Identifies the read-only <see cref="ColumnCount" /> dependency property.</summary>
    public static readonly DependencyProperty ColumnCountProperty = ColumnCountPropertyKey.DependencyProperty;

    /// <summary>Identifies the <see cref="FirstColumnForeground" /> dependency property.</summary>
    public static readonly DependencyProperty FirstColumnForegroundProperty = DependencyProperty.Register(
        nameof(FirstColumnForeground),
        typeof(Brush),
        typeof(DataGrid),
        new FrameworkPropertyMetadata(CreateDefaultFirstColumnForeground())
    );

    static DataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGrid), new FrameworkPropertyMetadata(typeof(DataGrid)));
    }

    /// <summary>Initializes a new instance of the <see cref="DataGrid" /> class.</summary>
    public DataGrid()
    {
        ((INotifyCollectionChanged)Items).CollectionChanged += OnItemsCollectionChanged;
        Columns.CollectionChanged += OnColumnsCollectionChanged;
        UpdateCounts();
    }

    /// <summary>Gets the number of data rows, excluding the native new-item placeholder.</summary>
    public int RowCount => (int)GetValue(RowCountProperty);

    /// <summary>Gets the number of columns, including automatically generated columns.</summary>
    public int ColumnCount => (int)GetValue(ColumnCountProperty);

    /// <summary>Gets or sets the foreground applied to cells in the first displayed column.</summary>
    public Brush FirstColumnForeground
    {
        get => (Brush)GetValue(FirstColumnForegroundProperty);
        set => SetValue(FirstColumnForegroundProperty, value);
    }

    private static Brush CreateDefaultFirstColumnForeground()
    {
        var brush = new SolidColorBrush(Color.FromRgb(0x0F, 0x6C, 0xBD));
        brush.Freeze();
        return brush;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) => UpdateRowCount();

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) =>
        SetValue(ColumnCountPropertyKey, Columns.Count);

    private void UpdateCounts()
    {
        UpdateRowCount();
        SetValue(ColumnCountPropertyKey, Columns.Count);
    }

    private void UpdateRowCount()
    {
        var count = Items.Cast<object?>().Count(item => !ReferenceEquals(item, CollectionView.NewItemPlaceholder));
        SetValue(RowCountPropertyKey, count);
    }
}
